using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Components
{
    class TetrisBoard
    {
        Texture2D _squareSprite;
        SpriteFont _statsFont;

        public Point Location;

        TimeSpan CurrentTickTime;
        TimeSpan KeyTickTime;

        public TetrisGameState State;
        TimeSpan _gravityTickTimeCount;

        IPlayerInput PlayerInput;
        Dictionary<InputButton, TimeSpan> PressTime;

        public event LinesClearedEventHandler LinesCleared;

        public TetrisBoard(IPlayerInput playerInput)
        {
            PlayerInput = playerInput;
            PressTime = Enum.GetValues(typeof(InputButton)).OfType<InputButton>().ToDictionary(k => k, k => TimeSpan.Zero);

            State = TetrisGameState.NewGameState();
            UpdateLevel();
        }

        public void LoadContent(ContentManager content)
        {
            _squareSprite = content.Load<Texture2D>("PieceBlock");
            _statsFont = content.Load<SpriteFont>("DefaultFont");
        }

        #region Update
        public void Update(GameTime gameTime)
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
        }

        void UpdateLevel()
        {
            var tick = Math.Pow((0.8 - ((State.Level - 1) * 0.007)), (State.Level - 1));
            CurrentTickTime = TimeSpan.FromSeconds(tick);
            KeyTickTime = TimeSpan.FromSeconds(tick / 5);
        }
        #endregion

        #region Draw
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //var color = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            //color.SetData(new[] { Color.Gray });
            //_spriteBatch.Draw(color, new Rectangle(Location.X, Location.Y, 260, 240), Color.White);

            DrawCurrentPiece(spriteBatch);
            DrawNextPiece(spriteBatch);
            DrawGrid(spriteBatch);
            DrawInfo(spriteBatch);
        }

        void DrawCurrentPiece(SpriteBatch spriteBatch)
        {
            var currentPiece = State.CurrentPiece.Shape.Data;
            var piecePos = State.CurrentPiece.Position;

            for (int l = 0; l < 4; l++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (currentPiece[l, c])
                        spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                Location.X + (piecePos.X + c - 2) * _squareSprite.Width,
                                Location.Y + (piecePos.Y + l - 1) * _squareSprite.Height
                            ), State.CurrentPiece.Piece.Color);
                }
            }
        }

        void DrawGrid(SpriteBatch spriteBatch)
        {
            for (int l = 0; l < 20; l++)
            {
                for (int c = 0; c < 10; c++)
                {
                    if (State.Grid[l, c] != Color.Transparent)
                    {
                        spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                    Location.X + c * _squareSprite.Width,
                                    Location.Y + l * _squareSprite.Height
                                ), State.Grid[l, c]);
                    }
                }
            }
        }

        void DrawInfo(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_statsFont, "Pontos:", new Vector2(Location.X + 180, Location.Y + 10), Color.Black);
            spriteBatch.DrawString(_statsFont, State.Points.ToString("#,##0"), new Vector2(Location.X + 200, Location.Y + 30), Color.Black);
            
            spriteBatch.DrawString(_statsFont, "Linhas:", new Vector2(Location.X + 180, Location.Y + 60), Color.Black);
            spriteBatch.DrawString(_statsFont, State.Rows.ToString(), new Vector2(Location.X + 200, Location.Y + 80), Color.Black);
            
            spriteBatch.DrawString(_statsFont, "Level:", new Vector2(Location.X + 180, Location.Y + 110), Color.Black);
            spriteBatch.DrawString(_statsFont, State.Level.ToString(), new Vector2(Location.X + 200, Location.Y + 130), Color.Black);
        }

        void DrawNextPiece(SpriteBatch spriteBatch)
        {
            if (State.IsFinished)
                return;

            spriteBatch.DrawString(_statsFont, "Próxima:", new Vector2(Location.X + 180, 155), Color.Black);
            var nextPieceShape = State.NextPiece.Shapes[0].Data;

            for (int l = 0; l < 4; l++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (nextPieceShape[l, c])
                        spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                Location.X + 180 + (c) * _squareSprite.Width,
                                Location.Y + 180 + (l) * _squareSprite.Height
                            ), State.NextPiece.Color);
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

        void FireLinesCleared(int lines)
        {
            if (LinesCleared != null)
                LinesCleared(this, new LinesClearedEventArgs(lines));
        }
        public void MoveLinesUp(int count)
        {
            State = State.MoveLinesUp(count, new Random(Environment.TickCount).Next(0, 10));
        }
    }
}
