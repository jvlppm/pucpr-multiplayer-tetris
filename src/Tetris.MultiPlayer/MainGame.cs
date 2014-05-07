using System;
using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Activities;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.GamerServices;

namespace Tetris.MultiPlayer
{
    class MainGame : Game
    {
        public GraphicsDeviceManager Graphics { get; private set; }

        Song GameSong;

        public MainGame()
        {
            Content.RootDirectory = "Content";
            Graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 800,
                PreferredBackBufferHeight = 520
            };

            Components.Add(new GamerServicesComponent(this));
        }

        protected override void Initialize()
        {
            base.Initialize();

            try
            {
                GameSong = Content.Load<Song>("Music");
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(GameSong);
            }
            catch { }

            this.Play(async activity =>
            {
                while (true)
                {
                    var result = await activity.Run(new StartScreenActivity(this));
                    switch (result)
                    {
                        case StartScreenActivity.Result.Exit:
                            return;
                        case StartScreenActivity.Result.Local:
                            await activity.Run<GamePlayActivity>();
                            break;
                        case StartScreenActivity.Result.WaitAsHost:
                        case StartScreenActivity.Result.WaitAsPlayer:
                            bool isHost = result == StartScreenActivity.Result.WaitAsHost;

                            var session = await activity.Run(new LobbyActivity(this, isHost));
                            if(session != null)
                                await activity.Run<NetworkGamePlayActivity>(session);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            });
        }
    }
}
