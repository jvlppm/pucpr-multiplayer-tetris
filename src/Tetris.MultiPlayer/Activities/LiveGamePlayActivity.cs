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

        LocalTetrisBoard _local;
        RemoteTetrisBoard _remote;

        public NetworkGamePlayActivity(Game game, NetworkSession session)
            : base(game)
        {
            _session = session;
            CancelOnExit.Register(session.Dispose);
        }

        protected override async Task InitializePlayerBoards()
        {
            var players = _session.AllGamers.OfType<NetworkGamer>();

            var p1BoardLocation = new Point(80, 100);
            var p2BoardLocation = new Point(800 - 260 - 80, 100);

            if (_session.IsHost)
            {
                var channel = new HostChannel(_session);

                var getP1GameState = TetrisGameState.NewGameState(channel.HostGenerator);
                var getP2GameState = TetrisGameState.NewGameState(channel.GetClientGenerator(0));

                PlayerBoards = new List<BaseTetrisBoard>
                {
                    (_local = new LocalTetrisBoard(new PlayerInput(PlayerIndex.One)) { Location = p1BoardLocation }),
                    (_remote = new RemoteTetrisBoard(channel, channel.Clients[0].Id) { Location = p2BoardLocation })
                };

                channel.Listen(CancelOnExit);
                _local.State = await getP1GameState;
                _remote.State = await getP2GameState;
                await channel.WaitClientReadyAsync();

                _local.PreviewPieceMove += (s, e) => channel.NotifyPieceMoved(e);
                _local.PreviewPieceSolidify += (s, e) => channel.NotifyPieceSolidified(e);
            }
            else
            {
                var channel = new ClientChannel(_session);
                var randomizer = new ClientPieceRandomizer(channel);

                var getP1GameState = TetrisGameState.NewGameState(randomizer.GetGenerator());
                var getP2GameState = TetrisGameState.NewGameState(randomizer.GetGenerator(channel.Host.Id));

                PlayerBoards = new List<BaseTetrisBoard>
                {
                    (_local = new LocalTetrisBoard(new PlayerInput(PlayerIndex.One)) { Location = p1BoardLocation }),
                    (_remote = new RemoteTetrisBoard(channel, channel.Host.Id) { Location = p2BoardLocation })
                };

                channel.Listen(CancelOnExit);
                _local.State = await getP1GameState;
                _remote.State = await getP2GameState;

                _local.PreviewPieceMove += (s, e) => channel.NotifyPieceMoved(e);
                _local.PreviewPieceSolidify += (s, e) => channel.NotifyPieceSolidified(e);
            }
        }
    }
}
