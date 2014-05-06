using Microsoft.Xna.Framework;
using System.Linq;

namespace Tetris.MultiPlayer.Model
{
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
}
