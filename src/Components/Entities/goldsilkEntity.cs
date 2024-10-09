using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using AsitLib;
using static Stolon.StolonGame;

using Math = System.Math;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Input;
using System.Xml.Linq;

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

        private Player player;
        private int playerId;

        public GoldsilkCom(GoldsilkEntity source) : base(source)
        {
            player = null!;
        }

        public override void DoMove(Board board)
        {
            //BoardState.Alter(ref board.State, Search(board.State, 2), true);
        }
        public static Move Search(BoardState state, UniqueMoveBoardMap map)
        {
            Move bestMove = Move.Invalid;

            Instance.DebugStream.WriteLine("\t[s]initializing parallel alpha-beta algorithm..");

            //if (bestMove.Equals(Move.Invalid)) // this could only be true if uniquemoves is empty.
            //{
            //    throw new Exception("No valid move found");
            //}
            Stopwatch a2 = new Stopwatch();
            a2.Start();

            List<Move> moves = map.GetAllMoves(state);
            Console.WriteLine(a2.ElapsedMilliseconds + "aa");

            a2.Stop();
            //for (int i = 0; i < moves.Count; i++)
            //{
            //    Stopwatch a = new Stopwatch();
            //    a.Start();

            //    BoardState child = state.DeepCopy();
            //    Console.WriteLine(a.ElapsedMilliseconds + "a");
            //    child.Alter(moves[i]);
            //    Console.WriteLine(a.ElapsedMilliseconds + "b");

            //    //Console.WriteLine(-Negamax(child, map, 4, int.MinValue, int.MaxValue, -1));
            //    Console.WriteLine(-Negamax(child, map, 3, -100, 100, -1));
            //    Console.WriteLine(a.ElapsedMilliseconds + "c");
            //    Console.WriteLine(moves[i]);

            //    a.Stop();
            //}
            Parallel.For(0, moves.Count, i =>
            {
                Stopwatch a = new Stopwatch();
                a.Start();

                // Each thread gets its own copy of the board state
                BoardState child = state.DeepCopy();
                //Console.WriteLine(a.ElapsedMilliseconds + "a");
                child.Alter(moves[i]);
                //Console.WriteLine(a.ElapsedMilliseconds + "b");

                // Perform the Negamax calculation in parallel
                Console.WriteLine(-Negamax(child, map, 3, -100, 100, -1) + " move: " + moves[i]);
                //Console.WriteLine(a.ElapsedMilliseconds + "c");
                Console.WriteLine();

                a.Stop();
            });

            Instance.DebugStream.Succes(1);
            return bestMove;
        }

        public static int Evaluate(BoardState state, int playerId, int? searchRes = null)
        {
            int score = 0;

            int searchAny = searchRes ?? state.SearchAny();

            if (searchAny == playerId) return 100;
            else if (searchAny != -1) return -100;

            //score += -state.TileCount / 2;

            //Console.WriteLine(stopwatch.ElapsedMilliseconds);
            return score;
        }
        public struct MinMaxResult
        {
            public int Score { get; }
            public Move Move { get; }
            public MinMaxResult(int score, Move move)
            {
                Score = score;
                Move = move;
            }

            public MinMaxResult InvertScore() => new MinMaxResult(-Score, Move);
            public override string ToString() => "{score: " + Score + ", move:" + Move + "}";
            public static MinMaxResult operator -(MinMaxResult result) => result.InvertScore();
        }
        public static int count = 0;
        //public static MinMaxResult AlphaBeta(BoardState state, Move move, UniqueMoveBoardMap map, int depth, int alpha, int beta, bool max)
        //{
        //    count++;
        //    List<Move> moves = map.GetAllMoves(state);
        //    int searchResult = state.SearchAny();
        //    //Console.WriteLine(depth + "m" + searchResult);

        //    // Terminating condition. i.e 
        //    // leaf node is reached
        //    if (searchResult != -1)
        //    {
        //        int eva = GoldsilkCom.Evaluate(state, state.CurrentPlayerID) * (max ? 1 : -1);

        //        Console.WriteLine(eva);
        //        return new MinMaxResult(eva, move);
        //    }
        //    if (moves.Count == 0) Console.WriteLine("HUH");
        //    if (depth == 3)
        //    {
        //        return new MinMaxResult(0, Move.Invalid);
        //        //return new MinMaxResult(GoldsilkCom.Evaluate(state, playerId), move);
        //    }

        //    if (max)
        //    {
        //        int best = int.MinValue;
        //        Move bestmove = Move.Invalid;
        //        foreach (var item in moves)
        //        {
        //            BoardState newcopy = state.DeepCopy();
        //            BoardState.Alter(ref newcopy, item, true);

        //            MinMaxResult val = AlphaBeta(newcopy, item, map, depth + 1, alpha, beta, !max);
        //            best = Math.Max(best, val.Score);

        //            if (val.Score > best)
        //            {
        //                best = val.Score;
        //                bestmove = val.Move;
        //            }
        //            alpha = Math.Max(alpha, best);
        //            // Alpha Beta Pruning
        //            if (beta <= alpha) break;
        //        }
        //        return new MinMaxResult(best, bestmove);
        //    }
        //    else
        //    {
        //        int best = int.MaxValue;
        //        Move bestmove = Move.Invalid;
        //        foreach (var item in moves)
        //        {
        //            BoardState newcopy = state.DeepCopy();
        //            BoardState.Alter(ref newcopy, item, true);

        //            MinMaxResult val = AlphaBeta(newcopy, item, map, depth + 1, alpha, beta, !max);
        //            best = Math.Min(best, val.Score);

        //            if (val.Score < best)
        //            {
        //                best = val.Score;
        //                bestmove = val.Move;
        //            }
        //            beta = Math.Min(beta, best);
        //            // Alpha Beta Pruning
        //            if (beta <= alpha) break;
        //        }
        //        return new MinMaxResult(best, bestmove);
        //    }
        //}


        //public static int Negamax(BoardState node, UniqueMoveBoardMap map, int depth, int alpha, int beta, int color)
        //{
        //    count++;

        //    int searchResult = node.SearchAny(); // -1 for no winner found, otherwise it hold the id of the winner.
        //    if (searchResult != -1 || depth == 0)
        //    {
        //        int eva = GoldsilkCom.Evaluate(node, node.CurrentPlayerID, searchResult) * color;
        //        return eva;
        //    }

        //    List<Move> moves = map.GetAllMoves(node);
        //    int value = -100;
        //    for (int i = 0; i < moves.Count; i++)
        //    {
        //        BoardState child = node.DeepCopy();
        //        child.Alter(moves[i]);

        //        value = Math.Max(value, -Negamax(child, map, depth - 1, -beta, -alpha, -color));

        //        alpha = Math.Max(alpha, value);
        //        if (alpha >= beta) break;
        //    }
        //    return value;
        //}
        public static int Negamax(BoardState node, UniqueMoveBoardMap map, int depth, int alpha, int beta, int color)
        {
            count++;

            int searchResult = node.SearchAny(); // -1 for no winner found, otherwise it hold the id of the winner.
            if (searchResult != -1 || depth == 0)
            {
                int eva = GoldsilkCom.Evaluate(node, node.CurrentPlayerID, searchResult) * color;
                return eva;
            }

            List<Move> moves = map.GetAllMoves(node);
            int value = -100;
            for (int i = 0; i < moves.Count; i++)
            {
                node.Alter(moves[i]);
                value = Math.Max(value, -Negamax(node, map, depth - 1, -beta, -alpha, -color));
                node.Undo();

                alpha = Math.Max(alpha, value);
                if (alpha >= beta) break;
            }
            return value;
        }
    }
}
