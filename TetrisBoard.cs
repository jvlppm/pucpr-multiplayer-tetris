using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaProjectTest
{
    class TetrisBoard : DrawableGameComponent
    {
        Texture2D _squareSprite;
        SpriteBatch _spriteBatch;

        Point Location;
        TimeSpan CurrentTickTime;
        int Level;

        TetrisGameState State;
        TimeSpan _gravityTickTime;

        public TetrisBoard(Game game)
            : base(game)
        {
            State = TetrisGameState.NewGameState();
            UpdateLevel();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _squareSprite = Game.Content.Load<Texture2D>("PieceBlock");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            _gravityTickTime += gameTime.ElapsedGameTime;

            if (_gravityTickTime > CurrentTickTime)
            {
                State = State.Tick();
                UpdateLevel();
                _gravityTickTime -= CurrentTickTime;
            }
            base.Update(gameTime);
        }

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

        void UpdateLevel()
        {
            Level = State.Rows / 10;
            var tick = Math.Pow((0.8 - ((Level - 1) * 0.007)), (Level - 1));
            CurrentTickTime = TimeSpan.FromSeconds(tick);
        }
    }
}
