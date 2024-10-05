using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using AsitLib;
using static Stolon.StolonGame;

using Math = System.Math;

#nullable enable

namespace Stolon
{
    public class GoldsilkEntity : SLEntity
    {
        public override SLComputer Computer => computer;
        public override Texture2D Splash => Instance.Textures.GetReference("textures\\splash\\goldsilk"); // unrelevant in first ver
        public override string? Description => "This shoulden't be readable in the current verion.";

        private GoldsilkCom computer;

        public GoldsilkEntity() : base("goldsilk", "Opponent", "O")
        {
            computer = new GoldsilkCom(this);
        }

        public override DialogueInfo GetReaction(PrimitiveReactOption reactOption)
        {
            return new DialogueInfo(this, "Lets goooo");
        }
    }
    public class GoldsilkCom : SLComputer
    {
        public struct MinMaxResult
        {
            public int Score { get; }
            public int Depth { get; }
            public MinMaxResult(int score, int depth)
            {
                Score = score;
                Depth = depth;
            }

            public MinMaxResult InvertScore() => new MinMaxResult(-Score, Depth);
            public override string ToString() => "{score: " + Score + ", depth: " + Depth + "}";


            public static MinMaxResult operator -(MinMaxResult result) => result.InvertScore();
        }

        private Player player;

        public GoldsilkCom(GoldsilkEntity source) : base(source)
        {
            player = null!;
        }

        public override void DoMove(Board board)
        {
            player = GetPlayer(board);
            BoardState.Alter(ref board.State, Search(board.State, 2), true);
        }
        public Move Search(BoardState state, int maxDepth)
        {
            Move[] uniqueMoves = state.GetUniqueMoves();
            Move bestMove = Move.Invalid;

            Instance.DebugStream.WriteLine("\t[s]initializing parallel alpha-beta algorithm for best move for computer of id:" + Source!.Id);
            Instance.DebugStream.WriteLine("\tnumber of uniquemoves: " + uniqueMoves.Length);

            if (bestMove.Equals(Move.Invalid)) // this could only be true if uniquemoves is empty.
            {
                throw new Exception("No valid move found");
            }

            Instance.DebugStream.Succes(2);
            return bestMove;
        }

        private MinMaxResult AlphaBeta(BoardState state, int depth, int alpha, int beta, int maxDepth)
        {
            //Move[] uniqueMoves = state.GetUniqueMoves();

            //int winner = state.Search();
            //if (winner == state.CurrentPlayerID) return new MinMaxResult(winScore, depth);
            //if (winner >= 0) return -new MinMaxResult(winScore, depth);
            //if (uniqueMoves.Length == 0) return new MinMaxResult(0, depth);

            //if (depth == maxDepth) return new MinMaxResult(15, depth);

            //Player mmPlayer = state.Players[state.CurrentPlayerID];

            //MinMaxResult result;
            //MinMaxResult bestResult = new MinMaxResult(int.MinValue, depth);
            //foreach (Move move in uniqueMoves)
            //{
            //    BoardState bCopy = state.DeepCopy();
            //    if (!BoardState.Alter(ref bCopy, move, true)) throw new Exception("alter unsuccessful");

            //    result = -AlphaBeta(bCopy, depth + 1, -beta, -alpha, maxDepth);
            //    if (result.Score > bestResult.Score) bestResult = result;

            //    alpha = Math.Max(alpha, result.Score);
            //    beta = Math.Max(beta, result.Score);

            //    if (bestResult.Score == winScore) break;
            //    if (alpha >= beta) break;
            //}
            //return bestResult;

            throw new NotImplementedException();
        }

    }
}
