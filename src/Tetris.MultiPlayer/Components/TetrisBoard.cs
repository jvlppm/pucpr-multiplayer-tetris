using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Tetris.MultiPlayer.Model;
using Tetris.MultiPlayer.Helpers;

namespace Tetris.MultiPlayer.Components
{
    class TetrisBoard
    {
        bool _updating;

        Texture2D _squareSprite, _boardBackground;
        SpriteFont _statsFont;
        MutexAsync _updateMutex;

        public Point Location;

        TimeSpan CurrentTickTime;
        TimeSpan KeyTickTime;

        public TetrisGameState State;
        TimeSpan _gravityTickTimeCount;

        public IPlayerInput PlayerInput;
        Dictionary<InputButton, TimeSpan> PressTime;

        public event LinesClearedEventHandler LinesCleared;

        public TetrisBoard(TetrisGameState state, IPlayerInput playerInput)
        {
            _updateMutex = new MutexAsync();

            PlayerInput = playerInput;
            PressTime = Enum.GetValues(typeof(InputButton)).OfType<InputButton>().ToDictionary(k => k, k => TimeSpan.Zero);

            State = state;
            UpdateLevel();
        }

        public void LoadContent(ContentManager content)
        {
            TetrisGameState.LoadContent(content);

            _squareSprite = content.Load<Texture2D>("PieceBlock");
            _boardBackground = content.Load<Texture2D>("BoardBackground");
            _statsFont = content.Load<SpriteFont>("DefaultFont");
        }

        #region Update

        //TODO: UpdateContext
        public async Task Update(GameTime gameTime)
        {
            if (_updating || State.IsFinished)
                return;

            _updating = true;

            using (await _updateMutex.WaitAsync())
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
                    var oldRows = State.Rows;
                    State = await State.Tick();
                    UpdateLevel();
                    var clearedRows = State.Rows - oldRows;
                    if (clearedRows > 0)
                        FireLinesCleared(clearedRows);

                    _gravityTickTimeCount -= CurrentTickTime;
                    if (_gravityTickTimeCount < TimeSpan.Zero)
                        _gravityTickTimeCount = TimeSpan.Zero;
                }
            }

            _updating = false;
        }

        void UpdateLevel()
        {
            var level = State.Level + 1;
            var tick = Math.Pow((0.8 - ((level - 1) * 0.007)), (level - 1));
            CurrentTickTime = TimeSpan.FromSeconds(tick);
            KeyTickTime = TimeSpan.FromSeconds(tick / 5);
        }
        #endregion

        #region Draw
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_boardBackground, new Vector2(Location.X, Location.Y), Color.White);

            var gridOffset = new Vector2(1, 1);

            DrawCurrentPiece(spriteBatch, gridOffset);
            DrawGrid(spriteBatch, gridOffset);

            DrawNextPiece(spriteBatch);
            DrawInfo(spriteBatch);
        }

        void DrawCurrentPiece(SpriteBatch spriteBatch, Vector2 position)
        {
            var currentPiece = State.CurrentPiece.Shape.Data;
            var piecePos = State.CurrentPiece.Position;

            for (int l = 0; l < 4; l++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (currentPiece[l, c])
                    {
                        var rowPos = piecePos.Y + l - 1;
                        if (rowPos < 0 || rowPos >= 20) continue;

                        var colPos = piecePos.X + c - 2;
                        if (colPos < 0 || colPos >= 10) continue;

                        spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                Location.X + position.X + colPos * _squareSprite.Width,
                                Location.Y + position.Y + rowPos * _squareSprite.Height
                            ), State.CurrentPiece.Piece.Color);
                    }
                }
            }
        }

        void DrawGrid(SpriteBatch spriteBatch, Vector2 position)
        {
            for (int l = 0; l < 20; l++)
            {
                for (int c = 0; c < 10; c++)
                {
                    if (State.Grid[l, c] != Color.Transparent)
                    {
                        spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                    Location.X + position.X + c * _squareSprite.Width,
                                    Location.Y + position.Y + l * _squareSprite.Height
                                ), State.Grid[l, c]);
                    }
                }
            }
        }

        void DrawInfo(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_statsFont, "Pontos:", new Vector2(Location.X + 165, Location.Y + 10), Color.Black);
            spriteBatch.DrawString(_statsFont, State.Points.ToString("#,##0").PadLeft(13), new Vector2(Location.X + 165, Location.Y + 30), Color.Black);

            spriteBatch.DrawString(_statsFont, "Linhas:", new Vector2(Location.X + 165, Location.Y + 90), Color.Black);
            spriteBatch.DrawString(_statsFont, State.Rows.ToString().PadLeft(5), new Vector2(Location.X + 228, Location.Y + 90), Color.Black);

            spriteBatch.DrawString(_statsFont, "Level:", new Vector2(Location.X + 165, Location.Y + 110), Color.Black);
            spriteBatch.DrawString(_statsFont, (State.Level + 1).ToString().PadLeft(5), new Vector2(Location.X + 228, Location.Y + 110), Color.Black);
        }

        void DrawNextPiece(SpriteBatch spriteBatch)
        {
            if (State.IsFinished)
                return;

            spriteBatch.DrawString(_statsFont, "Próxima:", new Vector2(Location.X + 165, Location.Y + 155), Color.Black);
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
        public async Task MoveLinesUp(int count)
        {
            using (await _updateMutex.WaitAsync())
                State = await State.MoveLinesUp(count, new Random(Environment.TickCount).Next(0, 10));
        }
    }
}
