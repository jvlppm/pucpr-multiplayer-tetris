using Microsoft.Xna.Framework;
using System;

namespace Tetris.MultiPlayer.Model
{
    static class Pieces
    {
        static readonly Piece I = new Piece(Color.Cyan, new[]{
            new bool[4, 4]
            {
                {false, false, true, false},
                {false, false, true, false},
                {false, false, true, false},
                {false, false, true, false},
            },
            new bool[4, 4]
            {
                {false, false, false, false},
                {true, true, true, true},
                {false, false, false, false},
                {false, false, false, false},
            }});

        static readonly Piece J = new Piece(Color.Orange, new[]{
            new bool[4, 4]
            {
                {false, false, false, false},
                {false, true, true, true},
                {false, false, false, true},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, true, true},
                {false, false, true, false},
                {false, false, true, false},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, true, false, false},
                {false, true, true, true},
                {false, false, false, false},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, true, false},
                {false, false, true, false},
                {false, true, true, false},
                {false, false, false, false},
            }
        });

        static readonly Piece L = new Piece(Color.LightBlue, new[]{
            new bool[4, 4]
            {
                {false, false, false, false},
                {false, true, true, true},
                {false, true, false, false},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, true, false},
                {false, false, true, false},
                {false, false, true, true},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, false, true},
                {false, true, true, true},
                {false, false, false, false},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, true, true, false},
                {false, false, true, false},
                {false, false, true, false},
                {false, false, false, false},
            }
        });

        static readonly Piece O = new Piece(Color.Yellow, new[]{
            new bool[4,4]
            {
                {false, false, false, false},
                {false, true, true, false},
                {false, true, true, false},
                {false, false, false, false},
            }
        });

        static readonly Piece S = new Piece(Color.Green, new[]{
            new bool[4, 4]
            {
                {false, false, false, false},
                {false, false, true, true},
                {false, true, true, false},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, true, false},
                {false, false, true, true},
                {false, false, false, true},
                {false, false, false, false},
            }
        });

        static readonly Piece T = new Piece(Color.DarkRed, new[]{
            new bool[4, 4]
            {
                {false, false, false, false},
                {false, true, true, true},
                {false, false, true, false},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, true, false},
                {false, false, true, true},
                {false, false, true, false},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, true, false},
                {false, true, true, true},
                {false, false, false, false},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, true, false},
                {false, true, true, false},
                {false, false, true, false},
                {false, false, false, false},
            }
        });

        static readonly Piece Z = new Piece(Color.Pink, new[]{
            new bool[4, 4]
            {
                {false, false, false, false},
                {false, true, true, false},
                {false, false, true, true},
                {false, false, false, false},
            },
            new bool[4, 4]
            {
                {false, false, false, true},
                {false, false, true, true},
                {false, false, true, false},
                {false, false, false, false},
            }
        });

        public static readonly Piece[] All = new[] { I, L, J, O, S, Z, T };
    }
}
