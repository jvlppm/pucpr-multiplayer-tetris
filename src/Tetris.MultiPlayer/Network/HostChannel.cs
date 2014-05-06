using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Network
{
    class HostChannel
    {
        static List<Piece> AllPieces = Pieces.All.ToList();
        public readonly NetworkSession Session;
        public readonly HostPieceRandomizer _pieceRandomizer;

        public HostChannel(NetworkSession session, HostPieceRandomizer randomizer)
        {
            if (!session.IsHost)
                throw new InvalidOperationException();

            Session = session;
            _pieceRandomizer = randomizer;
        }

        public void Listen(CancellationToken cancellation)
        {
            while (true)
            {
                foreach (var player in Session.LocalGamers)
                {
                    if (player.IsDataAvailable)
                    {
                        NetworkGamer requester;
                        var reader = new PacketReader();
                        player.ReceiveData(reader, out requester);

                        switch (reader.ReadChar())
                        {
                            case 'P':
                                var quantity = reader.ReadByte();
                                var pieces = Enumerable.Range(0, quantity).Select(i =>
                                        _pieceRandomizer.RemoteClientGenerators[player.Id].GetPiece().Result);

                                var response = pieces.Select(p => AllPieces.IndexOf(p));

                                var writer = new PacketWriter();
                                writer.Write('P');
                                writer.Write(quantity);
                                foreach (var pIndex in response)
                                    writer.Write(pIndex);

                                player.SendData(writer, SendDataOptions.Reliable);
                                break;
                        }
                    }
                }
            }
        }
    }
}
