using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
