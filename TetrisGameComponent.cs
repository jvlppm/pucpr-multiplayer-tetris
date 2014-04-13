using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaProjectTest
{
    class LinesClearedEventArgs : EventArgs
    {
        public readonly int Lines;

        public LinesClearedEventArgs(int lines)
        {
            Lines = lines;
        }
    }
    delegate void LinesClearedEventHandler(LinesClearedEventArgs e);

    class TetrisGameComponent : DrawableGameComponent
    {
        SpriteBatch _spriteBatch;
        Texture2D _squareSprite;
        SpriteFont _statsFont;

        public Point Location;

        TimeSpan CurrentTickTime;
        TimeSpan KeyTickTime;

        TetrisGameState State;
        TimeSpan _gravityTickTimeCount;

        IPlayerInput PlayerInput;
        Dictionary<InputButton, TimeSpan> PressTime;

        public event LinesClearedEventHandler LinesCleared;

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
            _statsFont = Game.Content.Load<SpriteFont>("DefaultFont");
            base.LoadContent();
        }

        #region Update
        public override void Update(GameTime gameTime)
        {
            if (State.IsFinished)
                return;

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
                var oldRows = State.Rows;
                State = State.Tick();
                UpdateLevel();
                var clearedRows = State.Rows - oldRows;
                if (clearedRows > 0)
                    FireLinesCleared(clearedRows);

                _gravityTickTimeCount -= CurrentTickTime;
                if (_gravityTickTimeCount < TimeSpan.Zero)
                    _gravityTickTimeCount = TimeSpan.Zero;
            }
            base.Update(gameTime);
        }

        void UpdateLevel()
        {
            var tick = Math.Pow((0.8 - ((State.Level - 1) * 0.007)), (State.Level - 1));
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
            DrawInfo();

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

        void DrawInfo()
        {
            _spriteBatch.DrawString(_statsFont, "Pontos:", new Vector2(Location.X + 200, Location.Y + 15), Color.Black);
            _spriteBatch.DrawString(_statsFont, State.Points.ToString("#,##0"), new Vector2(Location.X + 200, Location.Y + 30), Color.Black);

            _spriteBatch.DrawString(_statsFont, "Linhas:", new Vector2(Location.X + 200, Location.Y + 90), Color.Black);
            _spriteBatch.DrawString(_statsFont, State.Rows.ToString(), new Vector2(Location.X + 280, Location.Y + 90), Color.Black);

            _spriteBatch.DrawString(_statsFont, "Level:", new Vector2(Location.X + 200, Location.Y + 110), Color.Black);
            _spriteBatch.DrawString(_statsFont, State.Level.ToString(), new Vector2(Location.X + 280, Location.Y + 110), Color.Black);
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

        void FireLinesCleared(int lines)
        {
            if (LinesCleared != null)
                LinesCleared(new LinesClearedEventArgs(lines));
        }
    }
}
