using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Components;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Network
{
    class PieceEventArgs : EventArgs
    {
        public NetworkGamer Player;
        public uint PieceSequence;
        public Point PieceLocation;
        public int PieceRotation;
    }

    delegate void PieceEventHandler(object sender, PieceEventArgs args);

    abstract class TetrisChannel
    {
        public readonly NetworkSession Session;
        public readonly LocalNetworkGamer Me;

        public event PieceEventHandler RemotePieceMoved;
        public event PieceEventHandler RemotePieceSolidified;

        public TetrisChannel(NetworkSession session)
        {
            Session = session;
            Me = Session.LocalGamers[0];
        }

        protected abstract void OnMessage(NetworkGamer sender, PacketReader reader);

        public async void Listen(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                while (Me.IsDataAvailable)
                {
                    NetworkGamer requester;
                    var reader = new PacketReader();
                    Me.ReceiveData(reader, out requester);

                    var messageType = reader.PeekChar();

                    switch (messageType)
                    {
                        case 'M':
                        case 'S':
                            reader.ReadChar();
                            var args = new PieceEventArgs {
                                Player = requester,
                                PieceSequence = reader.ReadUInt32(),
                                PieceRotation = reader.ReadInt32(),
                                PieceLocation = new Point(reader.ReadInt32(), reader.ReadInt32())
                            };

                            if (messageType == 'S' && RemotePieceSolidified != null)
                                RemotePieceSolidified(this, args);
                            else if (messageType == 'M' && RemotePieceMoved != null)
                                RemotePieceMoved(this, args);
                            break;

                        default:
                            OnMessage(requester, reader);
                            break;
                    }
                }

                Session.Update();
                await TaskEx.Delay(TimeSpan.FromMilliseconds(15));
            }
        }

        public void NotifyPieceMoved(PieceEventArgs args)
        {
            Notify('M', args, SendDataOptions.Reliable);
        }

        public void NotifyPieceSolidified(PieceEventArgs args)
        {
            Notify('S', args, SendDataOptions.ReliableInOrder);
        }

        void Notify(char code, PieceEventArgs args, SendDataOptions options)
        {
            var writer = new PacketWriter();
            writer.Write(code);
            writer.Write(args.PieceSequence);
            writer.Write(args.PieceRotation);
            writer.Write(args.PieceLocation.X);
            writer.Write(args.PieceLocation.Y);
            Me.SendData(writer, options);
        }
    }

    class HostChannel : TetrisChannel
    {
        static List<Piece> AllPieces = Pieces.All.ToList();
        public readonly HostPieceRandomizer PieceRandomizer;
        public readonly NetworkGamer[] Clients;
        public readonly IPieceGenerator HostGenerator;

        TaskCompletionSource<bool> _clientReadyCompletion;

        public HostChannel(NetworkSession session)
            : base(session)
        {
            if (!session.IsHost)
                throw new InvalidOperationException();

            var players = Session.AllGamers.OfType<NetworkGamer>();
            var playerIds = players.Where(i => !i.IsHost).Select(i => i.Id).ToArray();
            var randomizer = new HostPieceRandomizer(playerIds);

            Clients = Session.AllGamers.Where((NetworkGamer g) => g.Id != Me.Id).ToArray();

            PieceRandomizer = randomizer;
            _clientReadyCompletion = new TaskCompletionSource<bool>();
            HostGenerator = PieceRandomizer.HostGenerator;
        }

        public IPieceGenerator GetClientGenerator(int playerIndex)
        {
            return PieceRandomizer.RealClientGenerators[Clients.Skip(playerIndex).First().Id];
        }

        public Task WaitClientReadyAsync()
        {
            return _clientReadyCompletion.Task;
        }

        protected override void OnMessage(NetworkGamer sender, PacketReader reader)
        {
            switch(reader.ReadChar())
            {
                // Cliente solicitou peças
                case 'P':
                    var quantity = reader.ReadByte();

                    var pieces = Enumerable.Range(0, quantity).Select(i =>
                            PieceRandomizer.RemoteClientGenerators[sender.Id].GetPiece().Result);

                    var response = pieces.Select(p => AllPieces.IndexOf(p));

                    var writer = new PacketWriter();
                    writer.Write('p');
                    writer.Write(quantity);
                    foreach (var pIndex in response)
                        writer.Write((byte)pIndex);

                    Me.SendData(writer, SendDataOptions.ReliableInOrder, sender);
                    _clientReadyCompletion.TrySetResult(true);
                    break;
            }
        }
    }
}
