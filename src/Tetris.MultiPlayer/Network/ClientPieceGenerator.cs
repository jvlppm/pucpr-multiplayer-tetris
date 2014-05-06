using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.MultiPlayer.Network
{
    class ClientPieceGenerator : IPieceGenerator
    {
        int _playerIndex;
        public ClientPieceGenerator(int playerIndex)
        {
            _playerIndex = playerIndex;
        }

        public Model.Piece GetPiece()
        {
            throw new NotImplementedException();
        }
    }
}
