using Jv.Games.Xna.Async.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using System;
using System.Threading.Tasks;
using Tetris.MultiPlayer.Helpers;
using Tetris.MultiPlayer.Model;
using Tetris.MultiPlayer.Network;

namespace Tetris.MultiPlayer.Components
{
    class RemoteTetrisBoard : BaseTetrisBoard
    {
        MutexAsync _updateMutex;
        AsyncContext _updateContext;

        int _lastPieceHeight;
        public readonly byte PlayerId;

        public RemoteTetrisBoard(TetrisChannel channel, byte playerId)
        {
            PlayerId = playerId;

            _updateMutex = new MutexAsync();
            _updateContext = new AsyncContext();

            channel.RemoteLinesCreated += channel_RemoteLinesCreated;
            channel.RemotePieceMoved += channel_RemotePieceMoved;
            channel.RemotePieceSolidified += channel_RemotePieceSolidified;
            channel.Session.GamerLeft += Session_GamerLeft;
        }

        void channel_RemoteLinesCreated(object sender, LinesCreatedEventArgs args)
        {
            if (PlayerId != args.Player.Id)
                return;

            var mutexWait = _updateMutex.WaitAsync();
            _updateContext.Post((Action)async delegate
            {
                using (var disp = await mutexWait)
                {
                    var updatedPieceLocation = new MovablePiece(State.CurrentPiece.Piece, args.PieceRotation, args.PieceLocation);
                    var nextState = new TetrisGameState(State.PieceGenerator, State.Rows, State.Points, updatedPieceLocation, State.NextPiece, State.Grid, State.Sequence);

                    var oldRows = State.Rows;
                    State = await nextState.MoveLinesUp(args.Count, args.GapLocation);

                    _lastPieceHeight = 0;
                }
            });
        }

        void Session_GamerLeft(object sender, GamerLeftEventArgs e)
        {
            if (e.Gamer.Id == PlayerId)
                Title = "Player Left";
        }

        void channel_RemotePieceSolidified(object sender, PieceEventArgs args)
        {
            if (PlayerId != args.Player.Id)
                return;

            var mutexWait = _updateMutex.WaitAsync();
            _updateContext.Post((Action)async delegate
            {
                using (var disp = await mutexWait)
                {
                    if (args.PieceSequence < State.Sequence)
                        return;

                    while (args.PieceSequence > State.Sequence)
                    {
                        var currentPiece = new MovablePiece(State.NextPiece, args.PieceRotation, args.PieceLocation);
                        State = new TetrisGameState(State.PieceGenerator, State.Rows, State.Points, currentPiece, await State.PieceGenerator.GetPiece(), State.Grid, State.Sequence + 1);
                    }

                    var updatedPieceLocation = new MovablePiece(State.CurrentPiece.Piece, args.PieceRotation, args.PieceLocation);
                    var nextState = new TetrisGameState(State.PieceGenerator, State.Rows, State.Points, updatedPieceLocation, State.NextPiece, State.Grid, State.Sequence);

                    State = await nextState.SolidifyCurrentPiece();
                    _lastPieceHeight = 0;
                }
            });
        }

        void channel_RemotePieceMoved(object sender, PieceEventArgs args)
        {
            if (PlayerId != args.Player.Id)
                return;

            var mutexWait = _updateMutex.WaitAsync();
            _updateContext.Post((Action)async delegate
            {
                using (var disp = await mutexWait)
                {
                    if (State.Sequence != args.PieceSequence || args.PieceLocation.Y < _lastPieceHeight)
                        return;

                    _lastPieceHeight = args.PieceLocation.Y;

                    TetrisGameState nextState;
                    if (!State.TrySetCurrentPiece(new MovablePiece(State.CurrentPiece.Piece, args.PieceRotation, args.PieceLocation), out nextState))
                        Title = "Out of sync";
                    else Title = string.Empty;

                    State = nextState;
                }
            });
        }

        #region Update

        public override void Update(GameTime gameTime)
        {
            _updateContext.Update(gameTime);
        }
        #endregion
    }
}
