using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        TaskCompletionSource<Piece[]> _getPieceRequest;

        public event TetrisStateEventHandler TetrisStateChanged;

        public ClientChannel(NetworkSession session)
        {
            if (session.IsHost)
                throw new InvalidOperationException();
            Session = session;
            Me = Session.LocalGamers[0];
            Host = Session.AllGamers.FirstOrDefault((NetworkGamer g) => g.IsHost);
        }

        static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof(T));
            handle.Free();
            return stuff;
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
                                var pieceCount = (int)reader.ReadByte();
                                var pieceIds = Enumerable.Range(0, pieceCount).Select(i => (int)reader.ReadByte()).ToArray();

                                _getPieceRequest.TrySetResult(pieceIds.Select(i => Pieces.All[i]).ToArray());
                                _getPieceRequest = null;
                            }
                            break;

                        case 't':
                            var buffer = new byte[Marshal.SizeOf(typeof(TetrisStateInfo))];
                            reader.Read(buffer, 0, buffer.Length);
                            var hostState = ByteArrayToStructure<TetrisStateInfo>(buffer);
                            reader.Read(buffer, 0, buffer.Length);
                            var localState = ByteArrayToStructure<TetrisStateInfo>(buffer);

                            if (TetrisStateChanged != null)
                                TetrisStateChanged(this, new TetrisStateEventArgs(hostState, localState));
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
            Me.SendData(new byte[] { (byte)'P', (byte)count }, SendDataOptions.Reliable, Host);
            Session.Update();

            return _getPieceRequest.Task;
        }
    }
}
