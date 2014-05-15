using Jv.Games.Xna.Async.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using System;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Helpers;
using Tetris.MultiPlayer.Model;
using Tetris.MultiPlayer.Network;

namespace Tetris.MultiPlayer.Components
{
    class RemoteTetrisBoard : BaseTetrisBoard
    {
        MutexAsync _updateMutex;
        AsyncContext _updateContext;

        new TetrisGameState State
        {
            get
            {
                if (base.State == null)
                    throw new InvalidOperationException();
                return base.State.Value;
            }
            set
            {
                var oldRows = base.State == null ? 0 : base.State.Value.Rows;
                base.State = value;
                /*UpdateLevel(value.Level + 1);
                var clearedRows = value.Rows - oldRows;
                if (clearedRows > 0)
                    FireLinesCleared(clearedRows);*/
            }
        }

        int _lastPieceHeight;
        uint _currentPieceId;
        public readonly byte PlayerId;

        //public event LinesClearedEventHandler LinesCleared;

        public RemoteTetrisBoard(TetrisChannel channel, byte playerId)
        {
            PlayerId = playerId;

            _updateMutex = new MutexAsync();
            _updateContext = new AsyncContext();

            channel.RemotePieceMoved += channel_RemotePieceMoved;
            channel.RemotePieceSolidified += channel_RemotePieceSolidified;
        }

        void channel_RemotePieceSolidified(object sender, PieceEventArgs args)
        {
            if (PlayerId != args.Player.Id)
                return;

            var mutexWait = _updateMutex.WaitAsync();
            _updateContext.Post((Action)async delegate
            {
                using (var disp = await mutexWait)
                {
                    if (_currentPieceId != args.PieceSequence)
                        throw new NotImplementedException();

                    TetrisGameState nextState;
                    if (!State.TrySetCurrentPiece(new MovablePiece(State.CurrentPiece.Piece, args.PieceRotation, args.PieceLocation), out nextState))
                        throw new NotImplementedException();

                    State = await nextState.SolidifyCurrentPiece();
                    _currentPieceId++;
                    _lastPieceHeight = 0;
                }
            });
        }

        void channel_RemotePieceMoved(object sender, PieceEventArgs args)
        {
            if (PlayerId != args.Player.Id)
                return;

            var mutexWait = _updateMutex.WaitAsync();
            _updateContext.Post((Action)async delegate
            {
                using (var disp = await mutexWait)
                {
                    if (_currentPieceId != args.PieceSequence || args.PieceLocation.Y < _lastPieceHeight)
                        return;

                    _lastPieceHeight = args.PieceLocation.Y;

                    TetrisGameState nextState;
                    if (!State.TrySetCurrentPiece(new MovablePiece(State.CurrentPiece.Piece, args.PieceRotation, args.PieceLocation), out nextState))
                        throw new NotImplementedException();

                    State = nextState;
                }
            });
        }

        #region Update

        public override void Update(GameTime gameTime)
        {
            _updateContext.Update(gameTime);
        }
        #endregion
    }
}
