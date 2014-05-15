using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Components;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Network
{
    class ClientChannel: TetrisChannel
    {
        public readonly NetworkGamer Host;

        TaskCompletionSource<Piece[]> _getPieceRequest;

        public ClientChannel(NetworkSession session)
            : base(session)
        {
            if (session.IsHost)
                throw new InvalidOperationException();

            Host = Session.AllGamers.Single((NetworkGamer g) => g.IsHost);
        }

        protected override void OnMessage(NetworkGamer sender, PacketReader reader)
        {
            switch (reader.ReadChar())
            {
                case 'p':
                    if (_getPieceRequest != null)
                    {
                        var pieceCount = (int)reader.ReadByte();
                        var pieceIds = Enumerable.Range(0, pieceCount).Select(i => (int)reader.ReadByte()).ToArray();

                        _getPieceRequest.TrySetResult(pieceIds.Select(i => Pieces.All[i]).ToArray());
                        _getPieceRequest = null;
                    }
                    break;
            }
        }

        /// <summary>
        /// Solicita as próximas peças ao servidor
        /// </summary>
        /// <param name="count">Quantas peças devem ser geradas</param>
        /// <returns></returns>
        public Task<Piece[]> GetNextPieces(int count)
        {
            _getPieceRequest = new TaskCompletionSource<Piece[]>();
            Me.SendData(new byte[] { (byte)'P', (byte)count }, SendDataOptions.Reliable, Host);
            Session.Update();

            return _getPieceRequest.Task;
        }
    }
}
