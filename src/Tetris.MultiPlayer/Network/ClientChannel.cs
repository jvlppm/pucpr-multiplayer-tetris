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
        public readonly NetworkSession Session;
        public readonly LocalNetworkGamer Me;
        public readonly NetworkGamer Host;

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
            var tcs = new TaskCompletionSource<Piece[]>();
            Me.SendData(new byte[] { (byte)'P' }, SendDataOptions.Reliable, Host);
            Session.Update();

            return tcs.Task;
        }
    }
}
