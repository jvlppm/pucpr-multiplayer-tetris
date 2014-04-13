using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaProjectTest
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
        public TetrisGameState(int rows, int points, PieceInstance current, Piece next, Color[,] grid)
        {
            Rows = rows;
            Points = points;
            CurrentPiece = current;
            NextPiece = next;
            Grid = grid;
            IsFinished = ValidPosition(current, grid);
        }

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
            var nextRotation = (CurrentPiece.Rotation + 1) % CurrentPiece.Piece.Shapes.Length;
            var nextPiece = new PieceInstance(CurrentPiece.Piece, nextRotation, CurrentPiece.Position);
            return SetCurrentPiece(nextPiece, false);
        }

        public TetrisGameState RotateCounterClockwise()
        {
            var nextRotation = (CurrentPiece.Rotation - 1) % CurrentPiece.Piece.Shapes.Length;
            if (nextRotation < 0)
                nextRotation += CurrentPiece.Piece.Shapes.Length;
            var nextPiece = new PieceInstance(CurrentPiece.Piece, nextRotation, CurrentPiece.Position);
            return SetCurrentPiece(nextPiece, false);
        }

        TetrisGameState SetCurrentPiece(PieceInstance currentPiece, bool autoSolidify)
        {
            if (ValidPosition(currentPiece, Grid))
                return new TetrisGameState(Rows, Points, currentPiece, NextPiece, Grid);

            if (autoSolidify)
            {
                currentPiece = new PieceInstance(NextPiece, 0, new Point(5, 0));
                return new TetrisGameState(Rows, Points + 40, currentPiece, Pieces.Random(), SolidifyCurrentPiece());
            }

            return this;
        }

        static bool ValidPosition(PieceInstance livePiece, Color[,] grid)
        {
            var shape = livePiece.Shape.Data;
            for (int l = 0; l < 4; l++)
            {
                int checkY = livePiece.Position.Y + l - 1;
                if (checkY < 0) continue;
                for (int c = 0; c < 4; c++)
                {
                    int checkX = livePiece.Position.X + c - 2;
                    if (checkX < 0 || checkX >= 10) continue;
                    if (shape[l, c] && (checkY >= 20 || grid[checkY, checkX] != Color.Transparent))
                        return false;
                }
            }
            return true;
        }

        Color[,] SolidifyCurrentPiece()
        {
            var grid = (Color[,])Grid.Clone();
            for (int l = 0; l < 4; l++)
            {
                for (int c = 0; c < 4; c++)
                {
                    var gridLine = CurrentPiece.Position.Y + l - 1;
                    if (gridLine >= 0 && CurrentPiece.Shape.Data[l, c])
                        grid[gridLine, CurrentPiece.Position.X + c - 2] = CurrentPiece.Piece.Color;
                }
            }
            return grid;
        }

        public static TetrisGameState NewGameState()
        {
            return new TetrisGameState(0, 0, new PieceInstance(Pieces.Random(), 0, new Microsoft.Xna.Framework.Point(5, 0)), Pieces.Random(), new Color[20, 10]);
        }
    }
}
