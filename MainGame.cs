using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using XnaProjectTest.Components;

namespace XnaProjectTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<TetrisGameComponent> PlayerBoards;
        SpriteFont BigFont;
        int Winner;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            PlayerBoards = new List<TetrisGameComponent>
            {
                new TetrisGameComponent(this, new PlayerInput(Keys.D, Keys.A, Keys.S, Keys.W, Keys.Q)) { Location = new Point(80, 0) },
                new TetrisGameComponent(this, new PlayerInput(Keys.Right, Keys.Left, Keys.Down, Keys.Up, Keys.Enter)) { Location = new Point(800 - 260 - 80, 0) }
            };

            foreach (var board in PlayerBoards)
            {
                Components.Add(board);
                board.LinesCleared += LinesCleared;
            }

            base.Initialize();
        }

        void LinesCleared(object sender, LinesClearedEventArgs e)
        {
            var board = (TetrisGameComponent)sender;
            if (e.Lines <= 1)
                return;

            foreach (var b in PlayerBoards)
            {
                if (b != board && !b.State.IsFinished)
                    b.MoveLinesUp(e.Lines);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            BigFont = Content.Load<SpriteFont>("BigFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
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
                    var last = remaining.Length == 1? remaining[0] : null;
                    if (last != null && PlayerBoards.All(b => b == last || b.State.Points < last.State.Points))
                        Winner = PlayerBoards.IndexOf(last) + 1;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            // TODO: Add your drawing code here
            if(Winner > 0)
            {
                var winnerText = "Player " + Winner + " Wins.";
                var textSize = BigFont.MeasureString(winnerText);
                spriteBatch.DrawString(BigFont, winnerText, new Vector2((800 - textSize.X) / 2, 400), Color.Black);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
