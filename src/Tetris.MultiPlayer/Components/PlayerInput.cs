using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

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
        readonly Keys MoveRight, MoveLeft, MoveDown, RotateCW, RotateCCW;
        KeyboardState _state;
        KeyboardState? _oldState;

        public PlayerInput(Keys moveRight, Keys moveLeft, Keys moveDown, Keys rotateCW, Keys rotateCCW)
        {
            MoveRight = moveRight;
            MoveLeft = moveLeft;
            MoveDown = moveDown;
            RotateCW = rotateCW;
            RotateCCW = rotateCCW;
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
                case InputButton.Left: return MoveLeft;
                case InputButton.Right: return MoveRight;
                case InputButton.Down: return MoveDown;

                case InputButton.RotateCW: return RotateCW;
                case InputButton.RotateCCW: return RotateCCW;
            }

            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            _oldState = _state;
            _state = Keyboard.GetState();
        }

        public override string ToString()
        {
            return string.Format("Keys: {0}, {1}, {2} - {3}, {4}", MoveLeft, MoveRight, MoveDown, RotateCW, RotateCCW);
        }
    }
}
