using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;

namespace Tetris.MultiPlayer.Model
{
    struct TetrisGameState
    {
        public static SoundEffect Move;
        public static SoundEffect Solidified;
        public static SoundEffect Cleared;
        public static SoundEffect End;

        public static void LoadContent(ContentManager content)
        {
            Move = content.Load<SoundEffect>("beep");
            Solidified = content.Load<SoundEffect>("menu_click-001");
            Cleared = content.Load<SoundEffect>("menu_sweep-001");
            End = content.Load<SoundEffect>("scifi_laser_echo-001");
        }

        public static async Task<TetrisGameState> NewGameState(IPieceGenerator generator)
        {
            var getP1 = generator.GetPiece();
            var getP2 = generator.GetPiece();

            await getP1;
            await getP2;

            return new TetrisGameState(generator, 0, 0, new MovablePiece(getP1.Result, 0, new Microsoft.Xna.Framework.Point(5, 0)), getP2.Result, new Color[20, 10], 0);
        }

        public TetrisGameState(IPieceGenerator generator, int rows, int points, MovablePiece current, Piece next, Color[,] grid, uint sequence)
        {
            PieceGenerator = generator;
            Level = rows / 10;
            Rows = rows;
            Points = points;
            CurrentPiece = current;
            NextPiece = next;
            Grid = grid;
            IsFinished = !ValidPosition(current, grid);
            if (IsFinished)
                End.Play();

            Sequence = sequence;
        }

        public readonly IPieceGenerator PieceGenerator;
        public readonly int Level;
        public readonly int Rows;
        public readonly int Points;
        public readonly MovablePiece CurrentPiece;
        public readonly Piece NextPiece;
        public readonly bool IsFinished;
        public readonly Color[,] Grid;
        public readonly uint Sequence;

        public bool TryToLowerPiece(out TetrisGameState nextGameState)
        {
            var nextPiecePosition = new MovablePiece(CurrentPiece.Piece, CurrentPiece.Rotation,
                new Point(CurrentPiece.Position.X, CurrentPiece.Position.Y + 1));

            return TrySetCurrentPiece(nextPiecePosition, out nextGameState);
        }

        /*public async Task<TetrisGameState> Tick()
        {
            var nextPiece = new MovablePiece(CurrentPiece.Piece, CurrentPiece.Rotation,
                new Point(CurrentPiece.Position.X, CurrentPiece.Position.Y + 1));
            return await SetCurrentPiece(nextPiece, true);
        }*/

        public TetrisGameState MoveLeft()
        {
            Move.Play();
            var nextPiece = new MovablePiece(CurrentPiece.Piece, CurrentPiece.Rotation,
                new Point(CurrentPiece.Position.X - 1, CurrentPiece.Position.Y));
            TetrisGameState nextState;
            TrySetCurrentPiece(nextPiece, out nextState);
            return nextState;
        }

        public TetrisGameState MoveRight()
        {
            Move.Play();
            var nextPiece = new MovablePiece(CurrentPiece.Piece, CurrentPiece.Rotation,
                new Point(CurrentPiece.Position.X + 1, CurrentPiece.Position.Y));
            TetrisGameState nextState;
            TrySetCurrentPiece(nextPiece, out nextState);
            return nextState;
        }

        public TetrisGameState RotateClockwise()
        {
            Move.Play();
            var nextRotation = (CurrentPiece.Rotation - 1) % CurrentPiece.Piece.Shapes.Length;
            if (nextRotation < 0)
                nextRotation += CurrentPiece.Piece.Shapes.Length;
            var nextPiece = new MovablePiece(CurrentPiece.Piece, nextRotation, CurrentPiece.Position);
            TetrisGameState nextState;
            TrySetCurrentPiece(nextPiece, out nextState);
            return nextState;
        }

        public TetrisGameState RotateCounterClockwise()
        {
            Move.Play();
            var nextRotation = (CurrentPiece.Rotation + 1) % CurrentPiece.Piece.Shapes.Length;
            var nextPiece = new MovablePiece(CurrentPiece.Piece, nextRotation, CurrentPiece.Position);
            TetrisGameState nextState;
            TrySetCurrentPiece(nextPiece, out nextState);
            return nextState;
        }

        public async Task<TetrisGameState> MoveLinesUp(int count, int spaceLocation)
        {
            var grid = (Color[,])Grid.Clone();

            for (int l = 0; l < 20 - count; l++)
            {
                for (int c = 0; c < 10; c++)
                    grid[l, c] = grid[l + count, c];
            }

            for (int i = 0; i < count; i++)
            {
                for (int c = 0; c < 10; c++)
                    grid[20 - 1 - i, c] = c == spaceLocation ? Color.Transparent : Color.Gray;
            }

            if(ValidPosition(CurrentPiece, grid))
                return new TetrisGameState(PieceGenerator, Rows, Points, CurrentPiece, NextPiece, grid, Sequence);

            return await SolidifyCurrentPiece(grid);
        }

        //async Task<TetrisGameState> SetCurrentPiece(MovablePiece currentPiece, bool autoSolidify)
        //{
        //    if (ValidPosition(currentPiece, Grid))
        //        return new TetrisGameState(PieceGenerator, Rows, Points, currentPiece, NextPiece, Grid);

        //    if (autoSolidify)
        //        return await SolidifyCurrentPiece(Grid);

        //    return this;
        //}

        public bool TrySetCurrentPiece(MovablePiece currentPiece, out TetrisGameState nextState)
        {
            if (ValidPosition(currentPiece, Grid))
            {
                nextState = new TetrisGameState(PieceGenerator, Rows, Points, currentPiece, NextPiece, Grid, Sequence);
                return true;
            }

            nextState = this;
            return false;
        }

        static bool ValidPosition(MovablePiece livePiece, Color[,] grid)
        {
            var shape = livePiece.Shape.Data;
            for (int l = 0; l < 4; l++)
            {
                int checkY = livePiece.Position.Y + l - 1;
                for (int c = 0; c < 4; c++)
                {
                    int checkX = livePiece.Position.X + c - 2;
                    if (checkY < 0)
                        continue;
                    if (shape[l, c] && (checkX < 0 || checkX >= 10 || checkY < 0 || checkY >= 20 || grid[checkY, checkX] != Color.Transparent))
                        return false;
                }
            }
            return true;
        }

        public Task<TetrisGameState> SolidifyCurrentPiece()
        {
            return SolidifyCurrentPiece(Grid);
        }

        async Task<TetrisGameState> SolidifyCurrentPiece(Color[,] grid)
        {
            for (int l = 0; l < 4; l++)
            {
                for (int c = 0; c < 4; c++)
                {
                    var gridLine = CurrentPiece.Position.Y + l - 1;
                    if (gridLine >= 0 && CurrentPiece.Shape.Data[l, c])
                        grid[gridLine, CurrentPiece.Position.X + c - 2] = CurrentPiece.Piece.Color;
                }
            }

            int cleared = 0;
            for (int l = 20 - 1; l >= 0; l--)
            {
                bool removeLine = true;
                for (int c = 0; c < 10; c++)
                {
                    if (grid[l, c] == Color.Transparent)
                    {
                        removeLine = false;
                        break;
                    }
                }

                if (removeLine)
                {
                    cleared++;
                    for (int j = l - 1; j >= 0; j--)
                    {
                        for (int c = 0; c < 10; c++)
                            grid[j + 1, c] = grid[j, c];
                    }
                    for (int c = 0; c < 10; c++)
                        grid[0, c] = Color.Transparent;
                    l++;
                }
            }

            int points = CurrentPiece.Position.Y * 2;
            switch (cleared)
            {
                case 1: points += Level * 40 + 40; break;
                case 2: points += Level * 100 + 100; break;
                case 3: points += Level * 300 + 300; break;
                case 4: points += Level * 1200 + 1200; break;
            }

            if(cleared > 0)
                Cleared.Play();
            Solidified.Play();

            var nextPiece = await PieceGenerator.GetPiece();

            return new TetrisGameState(PieceGenerator, Rows + cleared, Points + points, new MovablePiece(NextPiece, 0, new Point(5, 0)), nextPiece, grid, Sequence + 1);
        }
    }
}
