using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace XnaProjectTest.Components
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
        readonly Keys _moveRight, _moveLeft, _moveDown, _rotateCW, _rotateCCW;
        KeyboardState _state;
        KeyboardState? _oldState;

        public PlayerInput(Keys moveRight, Keys moveLeft, Keys moveDown, Keys rotateCW, Keys rotateCCW)
        {
            _moveRight = moveRight;
            _moveLeft = moveLeft;
            _moveDown = moveDown;
            _rotateCW = rotateCW;
            _rotateCCW = rotateCCW;
        }

        public bool IsPressed(InputButton button)
        {
            return _state.IsKeyDown(ToKey(button));
        }

        public bool WasPressed(InputButton button)
        {
            return _oldState != null && _oldState.Value.IsKeyDown(ToKey(button));
        }

        Keys ToKey(InputButton button)
        {
            switch (button)
            {
                case InputButton.Left: return _moveLeft;
                case InputButton.Right: return _moveRight;
                case InputButton.Down: return _moveDown;

                case InputButton.RotateCW: return _rotateCW;
                case InputButton.RotateCCW: return _rotateCCW;
            }

            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            _oldState = _state;
            _state = Keyboard.GetState();
        }
    }
}
