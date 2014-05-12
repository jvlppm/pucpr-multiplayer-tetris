using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tetris.MultiPlayer.Network
{
    public static class Extensions
    {
        public static async void AutoUpdate(this NetworkSession session, TimeSpan delay, CancellationToken cancellation)
        {
            while(!cancellation.IsCancellationRequested)
            {
                session.Update();
                await TaskEx.Delay(delay);
            }
        }
    }
}
