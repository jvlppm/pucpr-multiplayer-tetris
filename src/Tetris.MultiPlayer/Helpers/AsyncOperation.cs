using Jv.Games.Xna.Async;
using Jv.Games.Xna.Async.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.MultiPlayer.Helpers
{
    class AsyncOperation : IAsyncOperation
    {
        TaskCompletionSource<bool> _tcs;
        Func<GameTime, bool> _update;

        public AsyncOperation(Func<GameTime, bool> update)
        {
            _tcs = new TaskCompletionSource<bool>();
            _update = update;
        }

        public void Cancel()
        {
            _tcs.TrySetCanceled();
        }

        public Task Task
        {
            get { return _tcs.Task; }
        }

        public bool Continue(GameTime gameTime)
        {
            if (!_update(gameTime))
            {
                _tcs.TrySetResult(false);
                return false;
            }
            return true;
        }
    }

    public static class AsyncOperationExtensions
    {
        public static ContextTaskAwaitable RunWhile(this AsyncContext context, Func<GameTime, bool> update)
        {
            return context.Run(new AsyncOperation(update));
        }
    }
}
