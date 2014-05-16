using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Components;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Activities
{
    class LocalGamePlayActivity : GamePlayActivity
    {
        Random _random;

        public LocalGamePlayActivity(Game game)
            : base(game)
        {
            _random = new Random(Environment.TickCount);
        }

        protected override async Task InitializePlayerBoards()
        {
            var p1BoardLocation = new Point(80, 100);
            var p2BoardLocation = new Point(800 - 260 - 80, 100);

            var randomizer = new PieceRandomizer(2);

            var getP1GameState = TetrisGameState.NewGameState(randomizer.GetGenerator(0));
            var getP2GameState = TetrisGameState.NewGameState(randomizer.GetGenerator(1));

            PlayerBoards = new List<BaseTetrisBoard>
            {
                new LocalTetrisBoard(new PlayerInput(PlayerIndex.One)) { Location = p1BoardLocation, State = await getP1GameState },
                new LocalTetrisBoard(new PlayerInput(PlayerIndex.Two)) { Location = p2BoardLocation, State = await getP2GameState }
            };

            foreach(var board in PlayerBoards)
                board.LinesCleared += LinesCleared;
        }

        void LinesCleared(object sender, LinesClearedEventArgs e)
        {
            var board = (LocalTetrisBoard)sender;
            if (e.Lines <= 1)
                return;

            foreach (var b in PlayerBoards)
            {
                if (b != board && b.HasState && !b.State.IsFinished)
                    b.State = b.State.MoveLinesUp(e.Lines, _random.Next(10)).Result;
            }
        }
    }
}
