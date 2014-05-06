using Jv.Games.Xna;
using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading.Tasks;

namespace Tetris.MultiPlayer.Activities
{
    class StartScreenActivity : Activity<StartScreenActivity.Result>
    {
        public enum Result
        {
            Exit,
            Local,
            Network
        }

        bool _ready;
        Reference<Color> _selectionColor;
        public static Texture2D StartScreen;
        public static Texture2D PressStart;

        public StartScreenActivity(Game game)
            : base(game)
        {
        }

        protected override void Initialize()
        {
            StartScreen = Content.Load<Texture2D>("StartScreen");
            PressStart = Content.Load<Texture2D>("PressStart");
            base.Initialize();
        }

        protected async override Task<StartScreenActivity.Result> RunActivity()
        {
            _selectionColor = Color.Transparent;
            await DrawContext.Animate(TimeSpan.FromSeconds(1), _selectionColor, Color.White);
            _ready = true;
            AnimateSelection();
            return await base.RunActivity();
        }

        async void AnimateSelection()
        {
            var blinkDuration = TimeSpan.FromSeconds(0.8f);
            var fadeOut = Color.White * 0.6f;
            var fadeIn = Color.White;

            try
            {
                while (true)
                {
                    await DrawContext.Animate(blinkDuration, _selectionColor, fadeOut, CancelOnExit);
                    await DrawContext.Animate(blinkDuration, _selectionColor, fadeIn, CancelOnExit);
                }
            }
            catch (OperationCanceledException) { }
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            SpriteBatch.Begin();
            // TODO: Add your drawing code here
            SpriteBatch.Draw(StartScreen, Viewport.Bounds, null, Color.White);
            SpriteBatch.Draw(PressStart, new Vector2((Viewport.Width - PressStart.Width) / 2, Viewport.Height * 3 / 4), _selectionColor);
            SpriteBatch.End();
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // TODO: Add your update logic here
            if (!_ready)
                return;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit(Result.Exit);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Enter) || Keyboard.GetState().IsKeyDown(Keys.Space))
                Exit(Result.Local);
        }
    }
}
