using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tetris.MultiPlayer.Network
{
    /*[StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TetrisStateInfo
    {
        public int Rows;
        public int Points;
        public byte X, Y;
        public byte CurrentPieceIndex;
        public byte NextPieceIndex;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 200)]
        public byte[] BoardState;
    }

    public class TetrisStateEventArgs : EventArgs
    {
        public TetrisStateInfo HostInfo { get; private set; }
        public TetrisStateInfo ClientInfo { get; private set; }

        public TetrisStateEventArgs(TetrisStateInfo hostInfo, TetrisStateInfo clientInfo)
        {
            HostInfo = hostInfo;
            ClientInfo = clientInfo;
        }
    }

    public delegate void TetrisStateEventHandler(object sender, TetrisStateEventArgs args);*/
}
