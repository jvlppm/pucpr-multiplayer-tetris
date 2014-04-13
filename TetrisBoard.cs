using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaProjectTest
{
    class TetrisBoard
    {
        Texture2D Square { get; set; }

        TetrisGameState State { get; set; }

        float _gravityTickTime;

        public TetrisBoard()
        {
            State = TetrisGameState.NewGameState();
        }
    }
}
