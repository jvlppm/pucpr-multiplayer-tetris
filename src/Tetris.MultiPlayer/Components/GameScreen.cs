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
        SpriteFont BigFont;

        int Winner;
        List<TetrisBoard> PlayerBoards;

        public GameScreen()
        {
            PlayerBoards = new List<TetrisBoard>
            {
                new TetrisBoard(new PlayerInput(Keys.D, Keys.A, Keys.S, Keys.W, Keys.Q)) { Location = new Point(80, 0) },
                new TetrisBoard(new PlayerInput(Keys.Right, Keys.Left, Keys.Down, Keys.Up, Keys.Enter)) { Location = new Point(800 - 260 - 80, 0) }
            };

            foreach (var board in PlayerBoards)
                board.LinesCleared += LinesCleared;
        }

        public void LoadContent(ContentManager content)
        {
            foreach (var board in PlayerBoards)
                board.LoadContent(content);

            BigFont = content.Load<SpriteFont>("BigFont");
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
            if (Winner > 0)
            {
                var winnerText = "Player " + Winner + " Wins.";
                var textSize = BigFont.MeasureString(winnerText);
                spriteBatch.DrawString(BigFont, winnerText, new Vector2((800 - textSize.X) / 2, 400), Color.Black);
            }

            foreach (var board in PlayerBoards)
                board.Draw(spriteBatch, gameTime);
        }
    }
}
