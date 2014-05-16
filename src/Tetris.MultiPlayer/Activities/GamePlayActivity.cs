using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Components;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Activities
{
    abstract class GamePlayActivity : Activity
    {
        Random Random = new Random(Environment.TickCount);

        public static SoundEffect Begin;
        public static SoundEffect Return;

        SpriteFont BigFont, HeaderFont, DefaultFont;
        Texture2D Background;

        int Winner;
        protected IEnumerable<BaseTetrisBoard> PlayerBoards;

        public GamePlayActivity(Game game)
            : base(game)
        {

        }

        protected override void Initialize()
        {
            Begin = Content.Load<SoundEffect>("scifi_laser_gun-003");
            Return = Content.Load<SoundEffect>("scifi_laser_echo-002");

            Background = Content.Load<Texture2D>("Background");
            BigFont = Content.Load<SpriteFont>("BigFont");
            HeaderFont = Content.Load<SpriteFont>("HeaderFont");
            DefaultFont = Content.Load<SpriteFont>("DefaultFont");

            base.Initialize();
        }

        protected abstract Task InitializePlayerBoards();

        protected async override System.Threading.Tasks.Task RunActivity()
        {
            var init = InitializePlayerBoards();

            foreach (var board in PlayerBoards)
                board.LoadContent(Content);

            await init;

            Begin.Play();
            await base.RunActivity();
            Return.Play();
        }

        // Valida vencedor, e atualiza boards
        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (PlayerBoards == null)
                return;

            if (Winner <= 0)
            {
                if (PlayerBoards.All(b => b.HasState && b.State.IsFinished))
                {
                    var winner = PlayerBoards.OrderBy(b => !b.HasState? 0 : b.State.Points).LastOrDefault();
                    if (winner != null)
                        Winner = PlayerBoards.IndexOf(winner) + 1;
                }
                else
                {
                    var remaining = PlayerBoards.Where(b => !b.HasState || !b.State.IsFinished).ToArray();
                    var last = remaining.Length == 1 ? remaining[0] : null;
                    if (last != null && last.HasState && PlayerBoards.All(b => b == last || (b.HasState && b.State.Points < last.State.Points)))
                        Winner = PlayerBoards.IndexOf(last) + 1;
                }
            }

            foreach (var board in PlayerBoards)
                board.Update(gameTime);
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(Background, SpriteBatch.GraphicsDevice.Viewport.Bounds, Color.White);

            if (Winner > 0)
            {
                var winnerText = "Player " + Winner + " Wins.";
                var textSize = BigFont.MeasureString(winnerText);
                SpriteBatch.DrawString(BigFont, winnerText, new Vector2((Viewport.Width - textSize.X) / 2, 400), Color.Black);
            }

            if (PlayerBoards != null)
            {
                var boardWidth = Viewport.Width / PlayerBoards.Count();
                int p = 0;
                foreach(var board in PlayerBoards)
                {
                    string playerText = "Player " + (p + 1);
                    var pSize = HeaderFont.MeasureString(playerText);

                    SpriteBatch.DrawString(HeaderFont, playerText, new Vector2(boardWidth * p + (boardWidth - pSize.X) / 2, 24), Color.Black);

                    string keysText = board.Title;
                    var kSize = DefaultFont.MeasureString(keysText);

                    SpriteBatch.DrawString(DefaultFont, keysText, new Vector2(boardWidth * p + (boardWidth - kSize.X) / 2, 56), Color.Black);

                    p++;
                }

                foreach (var board in PlayerBoards)
                    board.Draw(SpriteBatch, gameTime);
            }

            SpriteBatch.End();
        }
    }
}
