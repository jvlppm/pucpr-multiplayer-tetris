using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.MultiPlayer.Components
{
    class GameScreen
    {
        SpriteFont BigFont, HeaderFont, DefaultFont;
        Texture2D Background;

        int Winner;
        List<TetrisBoard> PlayerBoards;

        public GameScreen()
        {
            PlayerBoards = new List<TetrisBoard>
            {
                new TetrisBoard(new PlayerInput(PlayerIndex.One)) { Location = new Point(80, 100) },
                new TetrisBoard(new PlayerInput(PlayerIndex.Two)) { Location = new Point(800 - 260 - 80, 100) }
            };

            foreach (var board in PlayerBoards)
                board.LinesCleared += LinesCleared;
        }

        public void LoadContent(ContentManager content)
        {
            foreach (var board in PlayerBoards)
                board.LoadContent(content);

            Background = content.Load<Texture2D>("Background");
            BigFont = content.Load<SpriteFont>("BigFont");
            HeaderFont = content.Load<SpriteFont>("HeaderFont");
            DefaultFont = content.Load<SpriteFont>("DefaultFont");
        }

        void LinesCleared(object sender, LinesClearedEventArgs e)
        {
            var board = (TetrisBoard)sender;
            if (e.Lines <= 1)
                return;

            foreach (var b in PlayerBoards)
            {
                if (b != board && !b.State.IsFinished)
                    b.MoveLinesUp(e.Lines);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (Winner <= 0)
            {
                if (PlayerBoards.All(b => b.State.IsFinished))
                {
                    var winner = PlayerBoards.OrderBy(b => b.State.Points).LastOrDefault();
                    if (winner != null)
                        Winner = PlayerBoards.IndexOf(winner) + 1;
                }
                else
                {
                    var remaining = PlayerBoards.Where(b => !b.State.IsFinished).ToArray();
                    var last = remaining.Length == 1 ? remaining[0] : null;
                    if (last != null && PlayerBoards.All(b => b == last || b.State.Points < last.State.Points))
                        Winner = PlayerBoards.IndexOf(last) + 1;
                }
            }

            foreach (var board in PlayerBoards)
                board.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(Background, spriteBatch.GraphicsDevice.Viewport.Bounds, Color.White);

            if (Winner > 0)
            {
                var winnerText = "Player " + Winner + " Wins.";
                var textSize = BigFont.MeasureString(winnerText);
                spriteBatch.DrawString(BigFont, winnerText, new Vector2((800 - textSize.X) / 2, 400), Color.Black);
            }

            var boardWidth = 800 / PlayerBoards.Count;
            for (var p = 0; p < PlayerBoards.Count; p++ )
            {
                var board = PlayerBoards[p];
                string playerText = "Player " + (p + 1);
                var pSize = HeaderFont.MeasureString(playerText);

                spriteBatch.DrawString(HeaderFont, playerText, new Vector2(boardWidth * p + (boardWidth - pSize.X) / 2, 24), Color.Black);

                string keysText = board.PlayerInput.ToString();
                var kSize = DefaultFont.MeasureString(keysText);

                spriteBatch.DrawString(DefaultFont, keysText, new Vector2(boardWidth * p + (boardWidth - kSize.X) / 2, 56), Color.Black);
            }

            foreach (var board in PlayerBoards)
                board.Draw(spriteBatch, gameTime);
        }
    }
}
