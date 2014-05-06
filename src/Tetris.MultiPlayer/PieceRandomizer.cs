using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer
{
    interface IPieceGenerator
    {
        Task<Piece> GetPiece();
    }

    class PieceRandomizer
    {
        readonly static Task CompletedTask;

        static PieceRandomizer()
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            CompletedTask = tcs.Task;
        }

        static Random _rng;
        static Random RNG { get { return _rng ?? (_rng = new Random(Environment.TickCount)); } }

        Queue<Piece>[] _playerPieces;

        public PieceRandomizer(int players)
        {
            _playerPieces = Enumerable.Range(0, players).Select(i => new Queue<Piece>()).ToArray();
        }

        public IPieceGenerator GetGenerator(int playerIndex)
        {
            return new PieceGenerator(() => GetPiece(playerIndex));
        }

        async Task<Piece> GetPiece(int playerIndex)
        {
            if (playerIndex >= _playerPieces.Length)
                throw new InvalidOperationException();

            if (_playerPieces[playerIndex].Count <= 0)
                EnqueueNextPieces();

            return _playerPieces[playerIndex].Dequeue();
        }

        void EnqueueNextPieces()
        {
            var nextPieces = Pieces.All.OrderBy(p => RNG.Next())
                                       .ToArray();

            foreach (var randomPiece in nextPieces)
                foreach (var pQ in _playerPieces)
                    pQ.Enqueue(randomPiece);
        }
    }
}
