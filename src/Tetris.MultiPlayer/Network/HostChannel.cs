using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Network
{
    class HostChannel
    {
        static List<Piece> AllPieces = Pieces.All.ToList();
        public readonly NetworkSession Session;
        public readonly HostPieceRandomizer _pieceRandomizer;
        public readonly LocalNetworkGamer Me;

        TaskCompletionSource<bool> _clientReadyCompletion;

        public HostChannel(NetworkSession session, HostPieceRandomizer randomizer)
        {
            if (!session.IsHost)
                throw new InvalidOperationException();

            Session = session;
            Me = Session.LocalGamers[0];
            _pieceRandomizer = randomizer;
            _clientReadyCompletion = new TaskCompletionSource<bool>();
        }

        public Task WaitClientReadyAsync()
        {
            return _clientReadyCompletion.Task;
        }

        public async void Listen(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                while (Me.IsDataAvailable)
                {
                    NetworkGamer requester;
                    var reader = new PacketReader();
                    Me.ReceiveData(reader, out requester);

                    switch (reader.ReadChar())
                    {
                        case 'P':
                            var quantity = reader.ReadByte();
                            var pieces = Enumerable.Range(0, quantity).Select(i =>
                                    _pieceRandomizer.RemoteClientGenerators[requester.Id].GetPiece().Result);

                            var response = pieces.Select(p => AllPieces.IndexOf(p));

                            var writer = new PacketWriter();
                            writer.Write('p');
                            writer.Write(quantity);
                            foreach (var pIndex in response)
                                writer.Write((byte)pIndex);

                            Me.SendData(writer, SendDataOptions.Reliable);
                            _clientReadyCompletion.SetResult(true);
                            break;
                    }
                }

                Session.Update();
                await TaskEx.Delay(TimeSpan.FromMilliseconds(15));
            }
        }
    }
}
