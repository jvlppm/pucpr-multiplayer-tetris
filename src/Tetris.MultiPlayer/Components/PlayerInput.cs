using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Tetris.MultiPlayer.Components
{
    enum InputButton
    {
        Left,
        Right,
        Down,
        RotateCW,
        RotateCCW,
    }

    interface IPlayerInput
    {
        void Update(GameTime gameTime);
        bool IsPressed(InputButton button);
        bool WasPressed(InputButton button);
    }

    class PlayerInput : IPlayerInput
    {
        enum SelectedInput
        {
            Keyboard, GamePad
        }

        readonly PlayerIndex PlayerIndex;
        IDictionary<InputButton, bool> _state;
        IDictionary<InputButton, bool> _oldState;
        SelectedInput _lastInputMode;

        public PlayerInput(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public bool IsPressed(InputButton button)
        {
            return _state[button];
        }

        public bool WasPressed(InputButton button)
        {
            return _oldState != null && _oldState[button];
        }

        public void Update(GameTime gameTime)
        {
            _oldState = _state;
            _state = GetState();
        }

        IDictionary<InputButton, bool> GetState()
        {
            var gamePad1 = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.IndependentAxes);
            var gamePad2 = GamePad.GetState(PlayerIndex.Two, GamePadDeadZone.IndependentAxes);
            var kbState = Keyboard.GetState();

            if (PlayerIndex == PlayerIndex.One)
            {
                if (gamePad1.IsConnected)
                {
                    _lastInputMode = SelectedInput.GamePad;
                    return GetState(gamePad1);
                }

                _lastInputMode = SelectedInput.Keyboard;
                return GetState(kbState, PlayerIndex.One);
            }

            if (gamePad2.IsConnected)
            {
                _lastInputMode = SelectedInput.GamePad;
                return GetState(gamePad2);
            }

            _lastInputMode = SelectedInput.Keyboard;
            return GetState(kbState, PlayerIndex.Two);
        }

        IDictionary<InputButton, bool> GetState(GamePadState state)
        {
            const float analogToDigital = 0.8f;

            var hDir = state.ThumbSticks.Left.X > 0 ? 1 : -1;
            var vDir = state.ThumbSticks.Left.Y > 0 ? 1 : -1;

            var horAnalog = Math.Abs(state.ThumbSticks.Left.X) <= analogToDigital ? 0 : hDir;
            var horDPad = (state.IsButtonDown(Buttons.DPadLeft) ? -1 : 0) + (state.IsButtonDown(Buttons.DPadRight) ? 1 : 0);

            var verAnalog = Math.Abs(state.ThumbSticks.Left.Y) <= analogToDigital ? 0 : -vDir;
            var verDPad = (state.IsButtonDown(Buttons.DPadUp) ? -1 : 0) + (state.IsButtonDown(Buttons.DPadDown) ? 1 : 0);

            var horMove = MathHelper.Clamp(horAnalog + horDPad, -1, 1);
            var verMove = MathHelper.Clamp(verAnalog + verDPad, -1, 1);

            return new Dictionary<InputButton, bool> {
              { InputButton.Left, horMove < 0 },
              { InputButton.Right, horMove > 0 },
              { InputButton.Down, verMove > 0 || state.Triggers.Right > analogToDigital },
              { InputButton.RotateCW, state.IsButtonDown(Buttons.RightShoulder) || state.IsButtonDown(Buttons.A) },
              { InputButton.RotateCCW, state.IsButtonDown(Buttons.LeftShoulder) || state.IsButtonDown(Buttons.X) },
            };
        }

        IDictionary<InputButton, bool> GetState(KeyboardState state, PlayerIndex index)
        {
            if (index == Microsoft.Xna.Framework.PlayerIndex.Two)
            {
                return new Dictionary<InputButton, bool> {
                  { InputButton.Left, state.IsKeyDown(Keys.Left) },
                  { InputButton.Right, state.IsKeyDown(Keys.Right) },
                  { InputButton.Down, state.IsKeyDown(Keys.Down) },
                  { InputButton.RotateCW, state.IsKeyDown(Keys.Up) },
                  { InputButton.RotateCCW, state.IsKeyDown(Keys.Enter) },
                };
            }

            return new Dictionary<InputButton, bool> {
                { InputButton.Left, state.IsKeyDown(Keys.A) },
                { InputButton.Right, state.IsKeyDown(Keys.D) },
                { InputButton.Down, state.IsKeyDown(Keys.S) },
                { InputButton.RotateCW, state.IsKeyDown(Keys.W) },
                { InputButton.RotateCCW, state.IsKeyDown(Keys.Q) },
            };
        }

        public override string ToString()
        {
            if(_lastInputMode == SelectedInput.GamePad)
                return string.Format("GamePad {0}: Directional + Left/Right Shoulders", (int)(PlayerIndex));

            if (PlayerIndex == Microsoft.Xna.Framework.PlayerIndex.One)
                return "Move: A, D - Speed: S - Rotate: W, Q";

            return "Move: Arrows - Rotate: Up, Enter";
        }
    }
}
