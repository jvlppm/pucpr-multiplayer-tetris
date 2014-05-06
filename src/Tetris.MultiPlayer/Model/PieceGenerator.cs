using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.MultiPlayer.Model
{
    class PieceGenerator : IPieceGenerator
    {
        Func<Task<Piece>> _getPiece;
        public PieceGenerator(Func<Task<Piece>> getPiece)
        {
            _getPiece = getPiece;
        }

        public Task<Piece> GetPiece()
        {
            return _getPiece();
        }
    }
}
