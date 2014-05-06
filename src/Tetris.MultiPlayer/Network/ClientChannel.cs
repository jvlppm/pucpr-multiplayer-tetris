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
        public ClientChannel(NetworkSession session)
        {
            if (session.IsHost)
                throw new InvalidOperationException();
            Session = session;
        }

        public void Listen(CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Task<Piece[]> GetNextPieces(int count)
        {
            throw new NotImplementedException();
        }
    }
}
