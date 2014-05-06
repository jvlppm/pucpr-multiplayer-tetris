using Microsoft.Xna.Framework.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Model;

namespace Tetris.MultiPlayer.Network
{
    class ClientPieceRandomizer
    {
        ClientChannel _channel;
        Queue<Task<Piece>> _myPieces;
        Dictionary<byte, Queue<Task<Piece>>> _playerPieces;

        public ClientPieceRandomizer(ClientChannel channel)
        {
            _channel = channel;
            var players = _channel.Session.LocalGamers.OfType<LocalNetworkGamer>();
            var playerIds = players.Select(p => p.Id).ToArray();

            _myPieces = new Queue<Task<Piece>>();
            _playerPieces = Enumerable.Range(0, playerIds.Length).ToDictionary(
                i => playerIds[i],
                i => new Queue<Task<Piece>>());
        }

        public IPieceGenerator GetGenerator()
        {
            return new PieceGenerator(() =>
            {
                if (_myPieces.Count <= 0)
                    EnqueueNextPieces();
                return _myPieces.Dequeue();
            });
        }

        public IPieceGenerator GetGenerator(byte playerId)
        {
            return new PieceGenerator(() => GetPiece(playerId));
        }

        Task<Piece> GetPiece(byte playerId)
        {
            if (!_playerPieces.ContainsKey(playerId))
                throw new InvalidOperationException();

            if (_playerPieces[playerId].Count <= 0)
                EnqueueNextPieces();

            return _playerPieces[playerId].Dequeue();
        }

        void EnqueueNextPieces()
        {
            int fetchSize = 7;
            var fetchPieces = Enumerable.Range(0, fetchSize)
                                .Select(i => new TaskCompletionSource<Piece>())
                                .ToArray();

            foreach (var fetch in fetchPieces)
            {
                _myPieces.Enqueue(fetch.Task);
                foreach (var p in _playerPieces.Values)
                    p.Enqueue(fetch.Task);
            }

            _channel.GetNextPieces(fetchSize).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    foreach (var fetch in fetchPieces)
                        fetch.SetException(t.Exception);
                    return;
                }
                if (t.IsCanceled)
                {
                    foreach (var fetch in fetchPieces)
                        fetch.SetCanceled();
                    return;
                }

                for (int i = 0; i < fetchSize; i++)
                    fetchPieces[i].SetResult(t.Result[i]);
            });
        }
    }
}
