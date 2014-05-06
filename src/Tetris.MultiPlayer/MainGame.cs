using System;
using Jv.Games.Xna.Async;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Activities;
using Microsoft.Xna.Framework.Media;

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
                    switch (await activity.Run(new StartScreenActivity(this)))
                    {
                        case StartScreenActivity.Result.Exit:
                            return;
                        case StartScreenActivity.Result.Local:
                            await activity.Run<LocalGamePlayActivity>();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            });
        }
    }
}
