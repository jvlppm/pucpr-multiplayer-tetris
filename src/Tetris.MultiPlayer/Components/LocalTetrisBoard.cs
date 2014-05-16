using Jv.Games.Xna.Async.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
        TimeSpan KeyTickTime;
        TimeSpan _gravityTickTimeCount;
        Dictionary<InputButton, TimeSpan> PressTime;

        bool _updating;

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

        public IPlayerInput PlayerInput;
        public event LinesClearedEventHandler LinesCleared;
        public event PieceEventHandler PreviewPieceMove, PreviewPieceSolidify;

        MovablePiece? _oldPieceState;

        public LocalTetrisBoard(IPlayerInput playerInput)
        {
            PlayerInput = playerInput;
            PressTime = Enum.GetValues(typeof(InputButton)).OfType<InputButton>().ToDictionary(k => k, k => TimeSpan.Zero);

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
            if (_updating || base.State == null || base.State.Value.IsFinished)
                return;

            var state = State;
            _updating = true;

            using (await _updateMutex.WaitAsync())
            {
                _gravityTickTimeCount += gameTime.ElapsedGameTime;

                bool forceTick = false;

                PlayerInput.Update(gameTime);
                foreach (var button in PressTime.Keys.ToArray())
                    PressTime[button] += gameTime.ElapsedGameTime;

                if (IsPressing(InputButton.Left))
                    State = state.MoveLeft();
                if (IsPressing(InputButton.Right))
                    State = state.MoveRight();
                if (IsPressing(InputButton.Down))
                    forceTick = true;
                if (IsPressing(InputButton.RotateCW))
                    State = state.RotateClockwise();
                if (IsPressing(InputButton.RotateCCW))
                    State = state.RotateCounterClockwise();

                if(_oldPieceState == null || !_oldPieceState.Equals(State.CurrentPiece))
                {
                    if (PreviewPieceMove != null)
                    {
                        PreviewPieceMove(this, new PieceEventArgs
                        {
                            PieceLocation = State.CurrentPiece.Position,
                            PieceRotation = State.CurrentPiece.Rotation,
                            PieceSequence = State.Sequence
                        });
                    }
                    _oldPieceState = State.CurrentPiece;
                }

                if (_gravityTickTimeCount > CurrentTickTime || forceTick)
                {
                    var oldRows = state.Rows;
                    TetrisGameState nextState;
                    if (!state.TryToLowerPiece(out nextState))
                    {
                        if (PreviewPieceSolidify != null)
                        {
                            PreviewPieceSolidify(this, new PieceEventArgs
                            {
                                PieceLocation = state.CurrentPiece.Position,
                                PieceRotation = state.CurrentPiece.Rotation,
                                PieceSequence = state.Sequence
                            });
                        }

                        //notify
                        nextState = await state.SolidifyCurrentPiece();
                    }

                    State = nextState;

                    var clearedRows = nextState.Rows - oldRows;
                    if (clearedRows > 0)
                        FireLinesCleared(clearedRows);

                    _gravityTickTimeCount -= CurrentTickTime;
                    if (_gravityTickTimeCount < TimeSpan.Zero)
                        _gravityTickTimeCount = TimeSpan.Zero;
                }

                UpdateLevel(State.Level);
            }

            _updating = false;
        }

        void UpdateLevel(int level)
        {
            var tick = Math.Pow((0.8 - ((level - 1) * 0.007)), (level - 1));
            CurrentTickTime = TimeSpan.FromSeconds(tick);
            KeyTickTime = TimeSpan.FromSeconds(tick / 5);
        }
        #endregion

        bool IsPressing(InputButton button)
        {
            if (!PlayerInput.IsPressed(button))
            {
                PressTime[button] = TimeSpan.Zero;
                return false;
            }

            if (!PlayerInput.WasPressed(button))
                return true;

            if (PressTime[button] > KeyTickTime)
            {
                PressTime[button] -= KeyTickTime;
                return true;
            }
            return false;
        }

        void FireLinesCleared(int lines)
        {
            if (LinesCleared != null)
                LinesCleared(this, new LinesClearedEventArgs(lines));
        }

        public async Task Invoke(Func<Task> asyncAction)
        {
            using (await _updateMutex.WaitAsync())
                await asyncAction();
        }
    }
}
