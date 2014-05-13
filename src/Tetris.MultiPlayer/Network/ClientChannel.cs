using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Network
{
    class ClientChannel
    {
        /// Solicitar 3 peças: P -> p 3 0 7 5

        public readonly NetworkSession Session;
        public readonly LocalNetworkGamer Me;
        public readonly NetworkGamer Host;

        TaskCompletionSource<Piece[]> _getPieceRequest;

        public ClientChannel(NetworkSession session)
        {
            if (session.IsHost)
                throw new InvalidOperationException();
            Session = session;
            Me = Session.LocalGamers[0];
            Host = Session.AllGamers.FirstOrDefault((NetworkGamer g) => g.IsHost);
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
                        case 'p':
                            if (_getPieceRequest != null)
                            {
                                _getPieceRequest.TrySetResult(Enumerable.Range(0, (int)reader.ReadByte()).Select(i => Pieces.All[(int)reader.ReadByte()]).ToArray());
                                _getPieceRequest = null;
                            }
                            break;

                        case 'H':
                            break;
                    }
                }

                Session.Update();
                await TaskEx.Delay(TimeSpan.FromMilliseconds(15));
            }
        }

        public Task<Piece[]> GetNextPieces(int count)
        {
            _getPieceRequest = new TaskCompletionSource<Piece[]>();
            Me.SendData(new byte[] { (byte)'P' }, SendDataOptions.Reliable, Host);
            Session.Update();

            return _getPieceRequest.Task;
        }
    }
}
