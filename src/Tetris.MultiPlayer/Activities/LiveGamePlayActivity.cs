using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using System;
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

        TetrisChannel _channel;
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
                _channel = channel;

                var getP1GameState = TetrisGameState.NewGameState(channel.HostGenerator);
                var getP2GameState = TetrisGameState.NewGameState(channel.GetClientGenerator(0));

                PlayerBoards = new List<BaseTetrisBoard>
                {
                    (_local = new LocalTetrisBoard(new PlayerInput(PlayerIndex.Two)) { Location = p1BoardLocation }),
                    (_remote = new RemoteTetrisBoard(channel, channel.Clients[0].Id) { Location = p2BoardLocation })
                };

                channel.Listen(CancelOnExit);
                _local.State = await getP1GameState;
                _remote.State = await getP2GameState;

                await channel.WaitClientReadyAsync();
            }
            else
            {
                var channel = new ClientChannel(_session);
                _channel = channel;

                var randomizer = new ClientPieceRandomizer(channel);
                var getP1GameState = TetrisGameState.NewGameState(randomizer.GetGenerator());
                var getP2GameState = TetrisGameState.NewGameState(randomizer.GetGenerator(channel.Host.Id));

                PlayerBoards = new List<BaseTetrisBoard>
                {
                    (_local = new LocalTetrisBoard(new PlayerInput(PlayerIndex.Two)) { Location = p1BoardLocation }),
                    (_remote = new RemoteTetrisBoard(channel, channel.Host.Id) { Location = p2BoardLocation })
                };

                channel.Listen(CancelOnExit);
                _local.State = await getP1GameState;
                _remote.State = await getP2GameState;
            }

            _local.PreviewPieceMove += (s, e) => _channel.NotifyPieceMoved(e);
            _local.PreviewPieceSolidify += (s, e) => _channel.NotifyPieceSolidified(e);
            _remote.LinesCleared += CreateLocalLines;
        }

        async void CreateLocalLines(object sender, LinesClearedEventArgs e)
        {
            if (!_local.HasState || _local.State.IsFinished || e.Lines <= 1)
                return;

            await _local.Invoke(async delegate
            {
                var random = new Random(Environment.TickCount);
                var gapLocation = random.Next(10);

                var state = _local.State;

                _channel.NotifyLinesCreated(new LinesCreatedEventArgs
                {
                    Count = e.Lines,
                    GapLocation = gapLocation,
                    PieceLocation = state.CurrentPiece.Position,
                    PieceRotation = state.CurrentPiece.Rotation,
                    PieceSequence = state.Sequence
                });
                _local.State = await state.MoveLinesUp(e.Lines, gapLocation);
            });
        }
    }
}
