using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaProjectTest
{
    class TetrisGameComponent : DrawableGameComponent
    {
        Texture2D _squareSprite;
        SpriteBatch _spriteBatch;

        public Point Location;

        TimeSpan CurrentTickTime;
        TimeSpan KeyTickTime;
        int Level;

        TetrisGameState State;
        TimeSpan _gravityTickTimeCount;

        IPlayerInput PlayerInput;
        Dictionary<InputButton, TimeSpan> PressTime;

        public TetrisGameComponent(Game game, IPlayerInput playerInput)
            : base(game)
        {
            PlayerInput = playerInput;
            PressTime = Enum.GetValues(typeof(InputButton)).OfType<InputButton>().ToDictionary(k => k, k => TimeSpan.Zero);

            State = TetrisGameState.NewGameState();
            UpdateLevel();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _squareSprite = Game.Content.Load<Texture2D>("PieceBlock");
            base.LoadContent();
        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            _gravityTickTimeCount += gameTime.ElapsedGameTime;

            bool forceTick = false;

            PlayerInput.Update(gameTime);
            foreach (var button in PressTime.Keys.ToArray())
                PressTime[button] += gameTime.ElapsedGameTime;

            if (Press(InputButton.Left))
                State = State.MoveLeft();
            if (Press(InputButton.Right))
                State = State.MoveRight();
            if (Press(InputButton.Down))
                forceTick = true;
            if (Press(InputButton.RotateCW))
                State = State.RotateClockwise();
            if (Press(InputButton.RotateCCW))
                State = State.RotateCounterClockwise();

            if (_gravityTickTimeCount > CurrentTickTime || forceTick)
            {
                State = State.Tick();
                UpdateLevel();
                _gravityTickTimeCount -= CurrentTickTime;
                if (_gravityTickTimeCount < TimeSpan.Zero)
                    _gravityTickTimeCount = TimeSpan.Zero;
            }
            base.Update(gameTime);
        }

        void UpdateLevel()
        {
            Level = State.Rows / 10;
            var tick = Math.Pow((0.8 - ((Level - 1) * 0.007)), (Level - 1));
            CurrentTickTime = TimeSpan.FromSeconds(tick);
            KeyTickTime = TimeSpan.FromSeconds(tick / 5);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            DrawCurrentPiece();
            //DrawNextPiece();
            DrawGrid();
            //DrawInfo();

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        void DrawCurrentPiece()
        {
            var currentPiece = State.CurrentPiece.Shape.Data;
            var piecePos = State.CurrentPiece.Position;

            for (int l = 0; l < 4; l++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (currentPiece[l, c])
                        _spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                Location.X + (piecePos.X + c - 2) * _squareSprite.Width,
                                Location.Y + (piecePos.Y + l - 1) * _squareSprite.Height
                            ), State.CurrentPiece.Piece.Color);
                }
            }
        }

        void DrawGrid()
        {
            for (int l = 0; l < 20; l++)
            {
                for (int c = 0; c < 10; c++)
                {
                    if (State.Grid[l, c] != Color.Transparent)
                    {
                        _spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                    Location.X + c * _squareSprite.Width,
                                    Location.Y + l * _squareSprite.Height
                                ), State.Grid[l, c]);
                    }
                }
            }
        }
        #endregion

        bool Press(InputButton button)
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
    }
}
