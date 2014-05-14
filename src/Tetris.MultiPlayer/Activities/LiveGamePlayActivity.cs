using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Components;
using Tetris.MultiPlayer.Model;
using Tetris.MultiPlayer.Network;

namespace Tetris.MultiPlayer.Activities
{
    class NetworkGamePlayActivity : GamePlayActivity
    {
        NetworkSession _session;

        public NetworkGamePlayActivity(Game game, NetworkSession session)
            : base(game)
        {
            _session = session;
        }

        protected async override Task RunActivity()
        {
            var players = _session.AllGamers.OfType<NetworkGamer>();

            var p1BoardLocation = new Point(80, 100);
            var p2BoardLocation = new Point(800 - 260 - 80, 100);

            if (_session.IsHost)
            {
                var playerIds = players.Where(i => !i.IsHost).Select(i => i.Id).ToArray();
                var randomizer = new HostPieceRandomizer(playerIds);
                var channel = new HostChannel(_session, randomizer);
                channel.Listen(CancelOnExit);

                await channel.WaitClientReadyAsync();

                var p1GameState = TetrisGameState.NewGameState(randomizer.HostGenerator);
                var p2GameState = TetrisGameState.NewGameState(randomizer.RealClientGenerators[playerIds[0]]);

                PlayerBoards = new List<TetrisBoard>
                {
                    new TetrisBoard(await p1GameState, new LocalPlayerInput()) { Location = p1BoardLocation },
                    new TetrisBoard(await p2GameState, new RemotePlayerInput()) { Location = p2BoardLocation }
                };
            }
            else
            {
                var channel = new ClientChannel(_session);
                channel.Listen(CancelOnExit);
                var randomizer = new ClientPieceRandomizer(channel);

                channel.TetrisStateChanged += (s, e) =>
                {
                    //randomizer clear queue, add infos
                };

                

                var p1GameState = TetrisGameState.NewGameState(randomizer.GetGenerator());
                var p2GameState = TetrisGameState.NewGameState(randomizer.GetGenerator(channel.Host.Id));

                PlayerBoards = new List<TetrisBoard>
                {
                    new TetrisBoard(await p1GameState, new LocalPlayerInput()) { Location = p1BoardLocation },
                    new TetrisBoard(await p2GameState, new RemotePlayerInput()) { Location = p2BoardLocation }
                };
            }

            await base.RunActivity();
        }
    }
}
