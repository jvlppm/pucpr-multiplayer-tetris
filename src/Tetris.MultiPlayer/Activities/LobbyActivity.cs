using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
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

            return base.RunActivity();
        }

        void WaitAsHost(CancellationToken cancellationToken)
        {
            var session = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 2);
            EventHandler<GamerJoinedEventArgs> gamerJoined = null;
            bool validSession = false;
            gamerJoined = (s, e) =>
            {
                if (e.Gamer.IsHost)
                    return;
                validSession = true;
                Exit(session);
            };
            session.GamerJoined += gamerJoined;

            cancellationToken.Register(() =>
            {
                if (!validSession)
                    session.Dispose();
                session.GamerJoined -= gamerJoined;
            });
        }

        async void WaitAsPlayer(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var host = NetworkSession.Find(NetworkSessionType.SystemLink, 1, null).FirstOrDefault();
                if(host != null)
                {
                    Exit(NetworkSession.Join(host));
                    return;
                }

                await TaskEx.Delay(TimeSpan.FromSeconds(0.5));
            }
        }

        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            _menuInput.Update(gameTime);

            if (_menuInput.Press(MenuButton.Cancel))
                Exit(null);
        }
    }
}
