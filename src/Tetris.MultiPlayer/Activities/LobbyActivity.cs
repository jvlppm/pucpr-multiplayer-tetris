using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Components;

namespace Tetris.MultiPlayer.Activities
{
    class LobbyActivity : Activity<NetworkSession>
    {
        bool _waitAsHost;
        MenuInput _menuInput;
        Texture2D Background;
        SpriteFont BigFont;
        string _message;
        Vector2 _messagePosition;

        public LobbyActivity(Game game, bool waitAsHost)
            : base(game)
        {
            _waitAsHost = waitAsHost;
            _menuInput = new MenuInput();
        }

        protected override Task<NetworkSession> RunActivity()
        {
            if (_waitAsHost)
                WaitAsHost(CancelOnExit);
            else
                WaitAsPlayer(CancelOnExit);

            AnimateMessage(CancelOnExit);

            return base.RunActivity();
        }

        async void AnimateMessage(CancellationToken cancellation)
        {
            TimeSpan delay = TimeSpan.FromSeconds(0.5);
            int count = 0;
            string message = "Waiting";

            var textSize = BigFont.MeasureString(message);
            _messagePosition = new Vector2((Viewport.Width - textSize.X), (Viewport.Height - textSize.Y)) / 2;

            while(!cancellation.IsCancellationRequested)
            {
                _message = "Waiting" + new string('.', count = (count + 1) % 4);
                await TaskEx.Delay(delay);
            }
        }

        protected override void Initialize()
        {
            Background = Content.Load<Texture2D>("Background");
            BigFont = Content.Load<SpriteFont>("BigFont");
            base.Initialize();
        }

        async void WaitAsHost(CancellationToken cancellationToken)
        {
            var session = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 2);
            EventHandler<GamerJoinedEventArgs> gamerJoined = null;
            bool validSession = false;
            gamerJoined = (s, e) =>
            {
                if (e.Gamer.IsHost)
                    return;

                session.GameStarted += delegate
                {
                    validSession = true;
                    Exit(session);
                };
                session.StartGame();
            };
            session.GamerJoined += gamerJoined;

            cancellationToken.Register(() =>
            {
                if (!validSession)
                    session.Dispose();
                session.GamerJoined -= gamerJoined;
            });

            while(!cancellationToken.IsCancellationRequested)
            {
                await TaskEx.Delay(TimeSpan.FromMilliseconds(200));
                session.Update();
            }
        }

        async void WaitAsPlayer(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var host = await Task.Factory.StartNew(() => NetworkSession.Find(NetworkSessionType.SystemLink, 1, null).FirstOrDefault());
                if(host != null)
                {
                    var session = NetworkSession.Join(host);
                    session.GameStarted += delegate { Exit(session); };

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        session.Update();
                        await TaskEx.Delay(TimeSpan.FromMilliseconds(200));
                    }
                    return;
                }

                await TaskEx.Delay(TimeSpan.FromSeconds(1));
            }
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(Background, SpriteBatch.GraphicsDevice.Viewport.Bounds, Color.White);
            SpriteBatch.DrawString(BigFont, _message, _messagePosition, Color.Black);
            SpriteBatch.End();
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _menuInput.Update(gameTime);

            if (_menuInput.Press(MenuButton.Cancel))
                Exit(null);
        }
    }
}
