using Microsoft.Xna.Framework;

namespace Tetris.MultiPlayer.Model
{
    struct MovablePiece
    {
        public MovablePiece(Piece piece, int rotation, Point position)
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
}
