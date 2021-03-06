using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.Linq;
using Tetris.MultiPlayer.Components;

namespace Tetris.MultiPlayer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameScreen gameScreen;

        public static SoundEffect Move;
        public static SoundEffect Solidified;
        public static SoundEffect Cleared;

        public static SoundEffect Begin;
        public static SoundEffect End;
        public static SoundEffect Return;

        Texture2D StartScreen;
        Texture2D PressStart;
        float _startCount, _startFade;
        bool _playing, _startFadingOut;

        Song GameSong;

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
            gameScreen = new GameScreen();

            base.Initialize();
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
            StartScreen = Content.Load<Texture2D>("StartScreen");
            PressStart = Content.Load<Texture2D>("PressStart");

            GameSong = Content.Load<Song>("Music");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(GameSong);

            Move = Content.Load<SoundEffect>("beep");
            Solidified = Content.Load<SoundEffect>("menu_click-001");
            Cleared = Content.Load<SoundEffect>("menu_sweep-001");
            Begin = Content.Load<SoundEffect>("scifi_laser_gun-003");
            End = Content.Load<SoundEffect>("scifi_laser_echo-001");
            Return = Content.Load<SoundEffect>("scifi_laser_echo-002");

            gameScreen.LoadContent(Content);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (_playing)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Return.Play();

                    gameScreen = new GameScreen();
                    gameScreen.LoadContent(Content);
                    _playing = false;
                    _startCount = 0;
                    _startFade = 0;
                }
                else
                    gameScreen.Update(gameTime);

            }
            else
            {
                if (_startCount <= 1)
                    return;

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();

                if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    _playing = true;
                    Begin.Play();
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

            if(_playing)
                gameScreen.Draw(spriteBatch, gameTime);
            else
            {
                _startCount += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_startCount >= 1)
                {
                    if (_startFadingOut)
                    {
                        _startFade -= (float)gameTime.ElapsedGameTime.TotalSeconds / 2;
                        if (_startFade < 0.6f)
                            _startFadingOut = false;
                    }
                    else
                    {
                        _startFade += (float)gameTime.ElapsedGameTime.TotalSeconds * 2;
                        if (_startFade >= 1)
                            _startFadingOut = true;
                    }
                }
                else
                    _startFade = _startCount;

                spriteBatch.Draw(StartScreen, graphics.GraphicsDevice.Viewport.Bounds, null, Color.White);
                spriteBatch.Draw(PressStart, new Vector2((graphics.GraphicsDevice.Viewport.Width - PressStart.Width) / 2, graphics.GraphicsDevice.Viewport.Height * 3 / 4), Color.White * _startFade);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
