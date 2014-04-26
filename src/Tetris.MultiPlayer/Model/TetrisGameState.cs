using Microsoft.Xna.Framework;
using System.Linq;

namespace Tetris.MultiPlayer.Model
{
    struct PieceShape
    {
        public PieceShape(bool[,] shape)
            : this()
        {
            Data = shape;
            LeftWidth = 2;
            RightWidth = 1;

            for (int i = 0; i < 2; i++)
            {
                bool emptyCol = true;
                for (int j = 0; j < 4; j++)
                {
                    if (shape[j, i])
                    {
                        emptyCol = false;
                        break;
                    }
                }

                if (emptyCol)
                {
                    LeftWidth--;
                }
            }

            for (int i = 3; i > 2; i--)
            {
                bool emptyCol = true;
                for (int j = 0; j < 4; j++)
                {
                    if (shape[j, i])
                    {
                        emptyCol = false;
                        break;
                    }
                }

                if (emptyCol)
                {
                    RightWidth--;
                }
            }
        }

        public bool[,] Data { get; private set; }
        public int LeftWidth { get; private set; }
        public int RightWidth { get; private set; }
    }

    struct Piece
    {
        public Piece(Color color, bool[][,] shapes)
            : this(color, shapes.OfType<bool[,]>().Select(b => new PieceShape(b)).ToArray())
        {
        }
        public Piece(Color color, params PieceShape[] shapes)
        {
            Color = color;
            Shapes = shapes;
        }

        public readonly Color Color;
        public readonly PieceShape[] Shapes;
    }

    struct PieceInstance
    {
        public PieceInstance(Piece piece, int rotation, Point position)
        {
            Piece = piece;
            Rotation = rotation % piece.Shapes.Length;
            Position = position;
        }

        public readonly Piece Piece;
        public readonly int Rotation;
        public readonly Point Position;

        public PieceShape Shape { get { return Piece.Shapes[Rotation]; } }
    }

    struct TetrisGameState
    {
        public static TetrisGameState NewGameState(IPieceGenerator generator)
        {
            return new TetrisGameState(generator, 0, 0, new PieceInstance(generator.GetPiece(), 0, new Microsoft.Xna.Framework.Point(5, 0)), generator.GetPiece(), new Color[20, 10]);
        }

        public TetrisGameState(IPieceGenerator generator, int rows, int points, PieceInstance current, Piece next, Color[,] grid)
        {
            PieceGenerator = generator;
            Level = rows / 10;
            Rows = rows;
            Points = points;
            CurrentPiece = current;
            NextPiece = next;
            Grid = grid;
            IsFinished = !ValidPosition(current, grid);
        }

        public readonly IPieceGenerator PieceGenerator;
        public readonly int Level;
        public readonly int Rows;
        public readonly int Points;
        public readonly PieceInstance CurrentPiece;
        public readonly Piece NextPiece;
        public readonly bool IsFinished;
        public readonly Color[,] Grid;

        public TetrisGameState Tick()
        {
            var nextPiece = new PieceInstance(CurrentPiece.Piece, CurrentPiece.Rotation,
                new Point(CurrentPiece.Position.X, CurrentPiece.Position.Y + 1));
            return SetCurrentPiece(nextPiece, true);
        }

        public TetrisGameState MoveLeft()
        {
            var nextPiece = new PieceInstance(CurrentPiece.Piece, CurrentPiece.Rotation,
                new Point(CurrentPiece.Position.X - 1, CurrentPiece.Position.Y));
            return SetCurrentPiece(nextPiece, false);
        }

        public TetrisGameState MoveRight()
        {
            var nextPiece = new PieceInstance(CurrentPiece.Piece, CurrentPiece.Rotation,
                new Point(CurrentPiece.Position.X + 1, CurrentPiece.Position.Y));
            return SetCurrentPiece(nextPiece, false);
        }

        public TetrisGameState RotateClockwise()
        {
            var nextRotation = (CurrentPiece.Rotation - 1) % CurrentPiece.Piece.Shapes.Length;
            if (nextRotation < 0)
                nextRotation += CurrentPiece.Piece.Shapes.Length;
            var nextPiece = new PieceInstance(CurrentPiece.Piece, nextRotation, CurrentPiece.Position);
            return SetCurrentPiece(nextPiece, false);
        }

        public TetrisGameState RotateCounterClockwise()
        {
            var nextRotation = (CurrentPiece.Rotation + 1) % CurrentPiece.Piece.Shapes.Length;
            var nextPiece = new PieceInstance(CurrentPiece.Piece, nextRotation, CurrentPiece.Position);
            return SetCurrentPiece(nextPiece, false);
        }

        public TetrisGameState MoveLinesUp(int count, int spaceLocation)
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
                return new TetrisGameState(PieceGenerator, Rows, Points, CurrentPiece, NextPiece, grid);

            return SolidifyCurrentPiece(grid);
        }

        TetrisGameState SetCurrentPiece(PieceInstance currentPiece, bool autoSolidify)
        {
            if (ValidPosition(currentPiece, Grid))
                return new TetrisGameState(PieceGenerator, Rows, Points, currentPiece, NextPiece, Grid);

            if (autoSolidify)
                return SolidifyCurrentPiece(Grid);

            return this;
        }

        static bool ValidPosition(PieceInstance livePiece, Color[,] grid)
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

        TetrisGameState SolidifyCurrentPiece(Color[,] grid)
        {
            //var grid = (Color[,])Grid.Clone();
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

            MainGame.Solidified.Play();

            return new TetrisGameState(PieceGenerator, Rows + cleared, Points + points, new PieceInstance(NextPiece, 0, new Point(5, 0)), PieceGenerator.GetPiece(), grid);
        }
    }
}
