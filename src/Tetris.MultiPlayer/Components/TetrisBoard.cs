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

namespace Tetris.MultiPlayer.Components
{
    class LocalTetrisBoard : BaseTetrisBoard
    {
        TimeSpan CurrentTickTime;
        TimeSpan KeyTickTime;
        TimeSpan _gravityTickTimeCount;
        Dictionary<InputButton, TimeSpan> PressTime;

        bool _updating;
        MutexAsync _updateMutex;

        new TetrisGameState? State
        {
            get { return base.State; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                var oldRows = base.State == null? 0 : base.State.Value.Rows;
                base.State = value;
                UpdateLevel(value.Value.Level + 1);

                var clearedRows = value.Value.Rows - oldRows;
                if (clearedRows > 0)
                    FireLinesCleared(clearedRows);
            }
        }

        public IPlayerInput PlayerInput;
        public event LinesClearedEventHandler LinesCleared;

        public LocalTetrisBoard(TetrisGameState state, IPlayerInput playerInput)
        {
            PlayerInput = playerInput;
            PressTime = Enum.GetValues(typeof(InputButton)).OfType<InputButton>().ToDictionary(k => k, k => TimeSpan.Zero);
            State = state;
        }

        #region Update

        protected override async void SyncUpdate(GameTime gameTime)
        {
            var currentState = State;
            if (currentState == null)
                return;
            var state = currentState.Value;

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

                if (_gravityTickTimeCount > CurrentTickTime || forceTick)
                {
                    var oldRows = state.Rows;
                    State = await state.Tick();
                    var clearedRows = state.Rows - oldRows;
                    if (clearedRows > 0)
                        FireLinesCleared(clearedRows);

                    _gravityTickTimeCount -= CurrentTickTime;
                    if (_gravityTickTimeCount < TimeSpan.Zero)
                        _gravityTickTimeCount = TimeSpan.Zero;
                }
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
        /*public async void MoveLinesUp(int count)
        {
            using (await _updateMutex.WaitAsync())
                State = await State.MoveLinesUp(count, new Random(Environment.TickCount).Next(0, 10));
        }*/
    }
}
