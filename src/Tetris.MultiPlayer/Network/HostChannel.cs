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

        public HostChannel(NetworkSession session, HostPieceRandomizer randomizer)
        {
            if (!session.IsHost)
                throw new InvalidOperationException();

            Session = session;
            Me = Session.LocalGamers[0];
            _pieceRandomizer = randomizer;
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
                            // TODO: Dado está chegando aqui, falta responder
                            var quantity = reader.ReadByte();
                            var pieces = Enumerable.Range(0, quantity).Select(i =>
                                    _pieceRandomizer.RemoteClientGenerators[requester.Id].GetPiece().Result);

                            var response = pieces.Select(p => AllPieces.IndexOf(p));

                            var writer = new PacketWriter();
                            writer.Write('P');
                            writer.Write(quantity);
                            foreach (var pIndex in response)
                                writer.Write(pIndex);

                            Me.SendData(writer, SendDataOptions.Reliable);
                            break;
                    }
                }

                Session.Update();
                await TaskEx.Delay(TimeSpan.FromMilliseconds(15));
            }
        }
    }
}
