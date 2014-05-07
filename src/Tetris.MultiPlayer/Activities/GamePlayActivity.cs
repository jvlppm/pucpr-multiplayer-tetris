using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Components;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Activities
{
    class GamePlayActivity : Activity
    {
        public static SoundEffect Begin;
        public static SoundEffect Return;

        SpriteFont BigFont, HeaderFont, DefaultFont;
        Texture2D Background;

        int Winner;
        protected List<TetrisBoard> PlayerBoards;

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

        protected async override System.Threading.Tasks.Task RunActivity()
        {
            if (PlayerBoards == null)
            {
                var randomizer = new PieceRandomizer(2);
                var p1GameState = TetrisGameState.NewGameState(randomizer.GetGenerator(0));
                var p2GameState = TetrisGameState.NewGameState(randomizer.GetGenerator(1));

                PlayerBoards = new List<TetrisBoard>
                {
                    new TetrisBoard(await p1GameState, new PlayerInput(PlayerIndex.One)) { Location = new Point(80, 100) },
                    new TetrisBoard(await p2GameState, new PlayerInput(PlayerIndex.Two)) { Location = new Point(800 - 260 - 80, 100) }
                };
            }

            foreach (var board in PlayerBoards)
            {
                board.LoadContent(Content);
                board.LinesCleared += LinesCleared;
            }

            Begin.Play();
            await base.RunActivity();
            Return.Play();
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

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

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
                var boardWidth = Viewport.Width / PlayerBoards.Count;
                for (var p = 0; p < PlayerBoards.Count; p++)
                {
                    var board = PlayerBoards[p];
                    string playerText = "Player " + (p + 1);
                    var pSize = HeaderFont.MeasureString(playerText);

                    SpriteBatch.DrawString(HeaderFont, playerText, new Vector2(boardWidth * p + (boardWidth - pSize.X) / 2, 24), Color.Black);

                    string keysText = board.PlayerInput.ToString();
                    var kSize = DefaultFont.MeasureString(keysText);

                    SpriteBatch.DrawString(DefaultFont, keysText, new Vector2(boardWidth * p + (boardWidth - kSize.X) / 2, 56), Color.Black);
                }

                foreach (var board in PlayerBoards)
                    board.Draw(SpriteBatch, gameTime);
            }

            SpriteBatch.End();
        }
    }
}
