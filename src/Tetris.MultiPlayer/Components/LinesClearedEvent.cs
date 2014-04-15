using System;

namespace Tetris.MultiPlayer.Components
{
    class LinesClearedEventArgs : EventArgs
    {
        public readonly int Lines;

        public LinesClearedEventArgs(int lines)
        {
            Lines = lines;
        }
    }

    delegate void LinesClearedEventHandler(object sender, LinesClearedEventArgs e);
}
