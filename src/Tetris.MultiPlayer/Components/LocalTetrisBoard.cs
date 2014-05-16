using Jv.Games.Xna.Async.Core;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Helpers;
using Tetris.MultiPlayer.Model;
using Tetris.MultiPlayer.Network;

namespace Tetris.MultiPlayer.Components
{
    class LocalTetrisBoard : BaseTetrisBoard
    {
        MutexAsync _updateMutex;
        AsyncContext _updateContext;

        TimeSpan CurrentTickTime;
        TimeSpan _gravityTickTimeCount;

        bool _updating;

        public IPlayerInput PlayerInput;
        public event PieceEventHandler PreviewPieceMove, PreviewPieceSolidify;

        MovablePiece? _oldPieceState;

        public LocalTetrisBoard(IPlayerInput playerInput)
        {
            PlayerInput = playerInput;

            _updateMutex = new MutexAsync();
            _updateContext = new AsyncContext();
        }

        #region Update

        public override void Update(GameTime gameTime)
        {
            _updateContext.Send(SyncUpdate, gameTime);
            _updateContext.Update(gameTime);
        }

        protected async void SyncUpdate(GameTime gameTime)
        {
            if (_updating || !HasState || State.IsFinished)
                return;

            _updating = true;

            using (await _updateMutex.WaitAsync())
            {
                _gravityTickTimeCount += gameTime.ElapsedGameTime;

                PlayerInput.Update(gameTime);

                TetrisGameState nextState = State;

                if (IsPressing(InputButton.Left))
                    nextState = nextState.MoveLeft();
                if (IsPressing(InputButton.Right))
                    nextState = nextState.MoveRight();
                if (PlayerInput.IsPressed(InputButton.Down))
                    _gravityTickTimeCount += TimeSpan.FromTicks(gameTime.ElapsedGameTime.Ticks * 4);
                if (IsPressing(InputButton.RotateCW))
                    nextState = nextState.RotateClockwise();
                if (IsPressing(InputButton.RotateCCW))
                    nextState = nextState.RotateCounterClockwise();

                if (_oldPieceState == null || !_oldPieceState.Equals(nextState.CurrentPiece))
                {
                    if (PreviewPieceMove != null)
                    {
                        PreviewPieceMove(this, new PieceEventArgs
                        {
                            PieceLocation = nextState.CurrentPiece.Position,
                            PieceRotation = nextState.CurrentPiece.Rotation,
                            PieceSequence = nextState.Sequence
                        });
                    }
                    _oldPieceState = nextState.CurrentPiece;
                }

                if (_gravityTickTimeCount > CurrentTickTime)
                {
                    TetrisGameState finalState;
                    if (!nextState.TryToLowerPiece(out finalState))
                    {
                        if (PreviewPieceSolidify != null)
                        {
                            PreviewPieceSolidify(this, new PieceEventArgs
                            {
                                PieceLocation = nextState.CurrentPiece.Position,
                                PieceRotation = nextState.CurrentPiece.Rotation,
                                PieceSequence = nextState.Sequence
                            });
                        }

                        nextState = await nextState.SolidifyCurrentPiece();
                    }
                    else nextState = finalState;

                    _gravityTickTimeCount -= CurrentTickTime;
                    if (_gravityTickTimeCount < TimeSpan.Zero)
                        _gravityTickTimeCount = TimeSpan.Zero;
                }

                State = nextState;
                UpdateLevel(State.Level);
            }

            _updating = false;
        }

        void UpdateLevel(int level)
        {
            var tick = Math.Pow((0.8 - ((level - 1) * 0.007)), (level - 1));
            CurrentTickTime = TimeSpan.FromSeconds(tick);
        }
        #endregion

        bool IsPressing(InputButton button)
        {
            return PlayerInput.IsPressed(button) && !PlayerInput.WasPressed(button);
        }

        public async Task Invoke(Func<Task> asyncAction)
        {
            using (await _updateMutex.WaitAsync())
                await asyncAction();
        }
    }
}
