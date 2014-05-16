using Jv.Games.Xna.Async.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using Tetris.MultiPlayer.Helpers;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Components
{
    abstract class BaseTetrisBoard
    {
        Texture2D _squareSprite, _boardBackground;
        SpriteFont _statsFont;

        TetrisGameState? _state;

        public string Title = string.Empty;
        public Point Location;

        public event LinesClearedEventHandler LinesCleared;

        public bool HasState
        {
            get { return _state != null; }
        }

        public TetrisGameState State
        {
            get
            {
                if (_state == null)
                    throw new InvalidOperationException();
                return _state.Value;
            }
            set
            {
                var oldRows = _state == null ? 0 : _state.Value.Rows;
                _state = value;
                var clearedRows = _state.Value.Rows - oldRows;
                if (clearedRows > 0 && LinesCleared != null)
                    LinesCleared(this, new LinesClearedEventArgs(clearedRows));
            }
        }

        public void LoadContent(ContentManager content)
        {
            TetrisGameState.LoadContent(content);

            _squareSprite = content.Load<Texture2D>("PieceBlock");
            _boardBackground = content.Load<Texture2D>("BoardBackground");
            _statsFont = content.Load<SpriteFont>("DefaultFont");
        }

        #region Update

        public abstract void Update(GameTime gameTime);
        #endregion

        #region Draw
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_boardBackground, new Vector2(Location.X, Location.Y), Color.White);

            TetrisGameState? state = State;
            if (state == null)
                return;

            var gridOffset = new Vector2(1, 1);

            DrawCurrentPiece(state.Value, spriteBatch, gridOffset);
            DrawGrid(state.Value, spriteBatch, gridOffset);

            DrawNextPiece(state.Value, spriteBatch);
            DrawInfo(state.Value, spriteBatch);
        }

        void DrawCurrentPiece(TetrisGameState state, SpriteBatch spriteBatch, Vector2 position)
        {
            var currentPiece = state.CurrentPiece.Shape.Data;
            var piecePos = state.CurrentPiece.Position;

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
                            ), state.CurrentPiece.Piece.Color);
                    }
                }
            }
        }

        void DrawGrid(TetrisGameState state, SpriteBatch spriteBatch, Vector2 position)
        {
            for (int l = 0; l < 20; l++)
            {
                for (int c = 0; c < 10; c++)
                {
                    if (state.Grid[l, c] != Color.Transparent)
                    {
                        spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                    Location.X + position.X + c * _squareSprite.Width,
                                    Location.Y + position.Y + l * _squareSprite.Height
                                ), state.Grid[l, c]);
                    }
                }
            }
        }

        void DrawInfo(TetrisGameState state, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_statsFont, "Pontos:", new Vector2(Location.X + 165, Location.Y + 10), Color.Black);
            spriteBatch.DrawString(_statsFont, state.Points.ToString("#,##0").PadLeft(13), new Vector2(Location.X + 165, Location.Y + 30), Color.Black);

            spriteBatch.DrawString(_statsFont, "Linhas:", new Vector2(Location.X + 165, Location.Y + 90), Color.Black);
            spriteBatch.DrawString(_statsFont, state.Rows.ToString().PadLeft(5), new Vector2(Location.X + 228, Location.Y + 90), Color.Black);

            spriteBatch.DrawString(_statsFont, "Level:", new Vector2(Location.X + 165, Location.Y + 110), Color.Black);
            spriteBatch.DrawString(_statsFont, (state.Level + 1).ToString().PadLeft(5), new Vector2(Location.X + 228, Location.Y + 110), Color.Black);
        }

        void DrawNextPiece(TetrisGameState state, SpriteBatch spriteBatch)
        {
            if (state.IsFinished)
                return;

            spriteBatch.DrawString(_statsFont, "Próxima:", new Vector2(Location.X + 165, Location.Y + 155), Color.Black);
            var nextPieceShape = state.NextPiece.Shapes[0].Data;

            for (int l = 0; l < 4; l++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (nextPieceShape[l, c])
                        spriteBatch.Draw(_squareSprite,
                            new Vector2(
                                Location.X + 180 + (c) * _squareSprite.Width,
                                Location.Y + 180 + (l) * _squareSprite.Height
                            ), state.NextPiece.Color);
                }
            }
        }
        #endregion
    }
}
