using Jv.Games.Xna;
using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Components;
using Tetris.MultiPlayer.Helpers;

namespace Tetris.MultiPlayer.Activities
{
    class StartScreenActivity : Activity<StartScreenActivity.Result>
    {
        public enum Result
        {
            Exit,
            Local,
            WaitAsHost,
            WaitAsPlayer
        }

        Reference<Color> _selectionColor, _defaultColor;
        public static Texture2D StartScreen;
        public static Texture2D LocalOponent, NetworkOponent;
        public static Texture2D WaitAsHost, WaitAsPlayer;

        Texture2D _option1, _option2;

        int _selection;
        TaskCompletionSource<int> _currentMenuCompletion;
        IMenuInput _menuInput;

        public StartScreenActivity(Game game)
            : base(game)
        {
            _menuInput = new MenuInput();
        }

        protected override void Initialize()
        {
            StartScreen = Content.Load<Texture2D>("StartScreen");
            LocalOponent = Content.Load<Texture2D>("LocalOponent");
            NetworkOponent = Content.Load<Texture2D>("NetworkOponent");
            WaitAsHost = Content.Load<Texture2D>("WaitAsHost");
            WaitAsPlayer = Content.Load<Texture2D>("WaitAsPlayer");

            base.Initialize();
        }

        protected async override Task<StartScreenActivity.Result> RunActivity()
        {
            DisplayMenu();
            return await base.RunActivity();
        }

        async void DisplayMenu()
        {
            AnimateSelection();
            while (true)
            {
                switch (await SelectOption(LocalOponent, NetworkOponent))
                {
                    case -1: Exit(Result.Exit); return;
                    case 0: Exit(Result.Local); return;
                    case 1:
                        if (await SignIn())
                        {
                            switch (await SelectOption(WaitAsHost, WaitAsPlayer))
                            {
                                case 0: Exit(Result.WaitAsHost); return;
                                case 1: Exit(Result.WaitAsPlayer); return;
                            }
                        }
                        break;
                }
            }
        }

        async void AnimateSelection()
        {
            _selectionColor = Color.Transparent;
            _defaultColor = Color.Transparent;

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

        Task<int> SelectOption(Texture2D option1, Texture2D option2)
        {
            _option1 = option1;
            _option2 = option2;

            _currentMenuCompletion = new TaskCompletionSource<int>();
            _selection = 0;

            var animations = TaskEx.WhenAll(
                (Task<TimeSpan>)DrawContext.Animate(TimeSpan.FromSeconds(1), _selectionColor, Color.White, CancelOnExit),
                (Task<TimeSpan>)DrawContext.Animate(TimeSpan.FromSeconds(1), _defaultColor, Color.White * 0.4f, CancelOnExit)
            );

            return _currentMenuCompletion.Task;
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(StartScreen, Viewport.Bounds, null, Color.White);
            if (_option1 != null && _option2 != null)
            {
                SpriteBatch.Draw(_option1, new Vector2((Viewport.Width - _option1.Width) / 2, Viewport.Height * 2.5f / 4), _selection == 0 ? _selectionColor : _defaultColor);
                SpriteBatch.Draw(_option2, new Vector2((Viewport.Width - _option2.Width) / 2, Viewport.Height * 2.5f / 4 + LocalOponent.Height), _selection == 1 ? _selectionColor : _defaultColor);
            }
            SpriteBatch.End();
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _menuInput.Update(gameTime);

            if (_currentMenuCompletion != null)
            {
                if (_menuInput.Press(MenuButton.Cancel))
                    _currentMenuCompletion.TrySetResult(-1);

                if (_menuInput.Press(MenuButton.Confirm))
                    _currentMenuCompletion.TrySetResult(_selection);
            }

            if (_menuInput.Press(MenuButton.Up) || _menuInput.Press(MenuButton.Down))
                _selection = ((int)_selection + 1) % 2;
        }

        async Task<bool> SignIn()
        {
            var tcs = new TaskCompletionSource<bool>();

            if (Gamer.SignedInGamers.Count <= 0)
            {
                Guide.ShowSignIn(1, false);
                await UpdateContext.RunWhile(gt => Guide.IsVisible);
            }
            return Gamer.SignedInGamers.Count > 0;
        }
    }
}
