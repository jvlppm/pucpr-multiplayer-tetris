using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Tetris.MultiPlayer.Components
{
    enum MenuButton
    {
        Up,
        Down,
        Confirm,
        Cancel,
    }

    interface IMenuInput
    {
        void Update(GameTime gameTime);
        bool IsPressed(MenuButton button);
        bool WasPressed(MenuButton button);
    }

    class MenuInput : IMenuInput
    {
        enum SelectedInput
        {
            Keyboard, GamePad
        }

        IDictionary<MenuButton, bool> _state;
        IDictionary<MenuButton, bool> _oldState;
        SelectedInput _lastInputMode;

        public bool IsPressed(MenuButton button)
        {
            return _state[button];
        }

        public bool WasPressed(MenuButton button)
        {
            return _oldState != null && _oldState[button];
        }

        public void Update(GameTime gameTime)
        {
            _oldState = _state;
            _state = GetState();
            if (_oldState == null)
                _oldState = _state;
        }

        IDictionary<MenuButton, bool> GetState()
        {
            var gamePad1 = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.IndependentAxes);
            var kbState = Keyboard.GetState();

            if (gamePad1.IsConnected)
            {
                _lastInputMode = SelectedInput.GamePad;
                return GetState(gamePad1);
            }

            _lastInputMode = SelectedInput.Keyboard;
            return GetState(kbState, PlayerIndex.One);
        }

        IDictionary<MenuButton, bool> GetState(GamePadState state)
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

            return new Dictionary<MenuButton, bool> {
              { MenuButton.Up, verMove < 0 || state.Triggers.Right < -analogToDigital },
              { MenuButton.Down, verMove > 0 || state.Triggers.Right > analogToDigital },
              { MenuButton.Confirm, state.IsButtonDown(Buttons.Start) || state.IsButtonDown(Buttons.A) },
              { MenuButton.Cancel, state.IsButtonDown(Buttons.Back) || state.IsButtonDown(Buttons.B) },
            };
        }

        IDictionary<MenuButton, bool> GetState(KeyboardState state, PlayerIndex index)
        {
            return new Dictionary<MenuButton, bool> {
                { MenuButton.Up, state.IsKeyDown(Keys.Up) },
                { MenuButton.Down, state.IsKeyDown(Keys.Down) },
                { MenuButton.Confirm, state.IsKeyDown(Keys.Enter) },
                { MenuButton.Cancel, state.IsKeyDown(Keys.Escape) },
            };
        }
    }

    static class MenuInputExtensions
    {
        public static bool Press(this IMenuInput input, MenuButton button)
        {
            return !input.WasPressed(button) && input.IsPressed(button);
        }

        public static bool Release(this IMenuInput input, MenuButton button)
        {
            return input.WasPressed(button) && !input.IsPressed(button);
        }
    }
}
