using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using AsitLib;

using Math = System.Math;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework.Input;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Goldsilk hates the player.
    /// </summary>
    public class GoldsilkEntity : EntityBase
    {
        public override SLComputer Computer => _computer;
        public override Texture2D Splash => STOLON.Textures.GetReference("textures\\splash\\goldsilk"); // unrelevant in first ver
        public override string? Description => "This shoulden't be readable in the current verion.";

        private GoldsilkCom _computer;

        public GoldsilkEntity() : base("goldsilk", "Opponent", "O")
        {
            _computer = new GoldsilkCom(this);
        }

        public override DialogueInfo GetReaction(PrimitiveReactOption reactOption)
        {
            return new DialogueInfo(this, "Lets goooo");
        }
    }
    /// <summary>
    /// The computer <see cref="GoldsilkEntity"/> uses to play.
    /// </summary>
    public class GoldsilkCom : SLComputer
    {
        public GoldsilkCom(GoldsilkEntity source) : base(source) { }
        public override void DoMove(Board board)
        {
            int current = board.State.CurrentPlayerID;
            board.State.Alter(Search(board.State, board.UniqueMoveBoardMap, 3).Move, true);

            int ret = board.State.SearchAny();
            if (ret == current) // this makes it so when goldsilk finds a connect four right after the player does, she wins. This is a bug but I'm calling it a feature.
                STOLON.Environment.Overlayer.Activate("transition", null, () =>
                {
                    board.Reset();
                }, "4 Connected found for player " + board.GetPlayerTile(ret) + "!");
        }
        private static int negaCount = 0;
        public static NegamaxEndResult Search(BoardState state, UniqueMoveBoardMap map, int depth)
        {
            STOLON.Debug.Log(">[s]initializing parallel alpha-beta algorithm..");

            ConcurrentDictionary<int, TTEntry> tt = new ConcurrentDictionary<int, TTEntry>();
            List<Move> moves = map.GetAllMoves(state);
            int color = state.CurrentPlayerID == 1 ? 1 : -1;
            List<(int score, Move move)> negaMaxedMoves = new List<(int score, Move move)>();
            List<(int score, Move move)> evaluatedMoves = new List<(int score, Move move)>();
            Stopwatch stopwatch = Stopwatch.StartNew();

            negaCount = 0;
            object lockObj = new object();
            object copyLockObj = new object();

            Parallel.For(0, moves.Count, i =>
            {
                BoardState child = state.DeepCopy();
                Point sim = child.Alter(moves[i], true);

                int score = -Negamax(child, sim, map, tt, depth, -evalNum, evalNum, color);

                lock (lockObj)
                {
                    STOLON.Debug.Log("evaluated move " + moves[i] + ", winstate score: " + score + ".");
                    negaMaxedMoves.Add((score, moves[i]));
                }
            });

            STOLON.Debug.Log(">attemting further move ordering");

            negaMaxedMoves = negaMaxedMoves.Select((x, i) => new { Index = i, Value = x })
                .Where(x => x.Value.score == negaMaxedMoves.Select(t => t.score).Max())
                .Select(x => x.Value).ToList();
            foreach ((int score, Move move) moveTuple in negaMaxedMoves)
            {
                int score = MoveEvaluate(state, moveTuple.move, moveTuple.score);
                evaluatedMoves.Add((score, moveTuple.move));

                STOLON.Debug.Log("move " + moveTuple.move + " has an evaluated score of " + score + ".");
            }
            STOLON.Debug.Success();
            STOLON.Debug.Log(">attempting best move selection");
            (int score, Move move) bestItem = evaluatedMoves.Where(t => t.score == evaluatedMoves.Select(t => t.score).Max()).First();

            stopwatch.Stop();
            STOLON.Debug.Log("bestMove found with a score of: " + bestItem.score + " and move " + bestItem.move);
            STOLON.Debug.Success();
            STOLON.Debug.Success();
            return new NegamaxEndResult(bestItem.move, negaCount, (int)stopwatch.ElapsedMilliseconds);
        }
        public const int evalNum = 10000;
        public static int MoveEvaluate(BoardState state, Move move, int score)
        {
            STOLON.Debug.Log(">starting eval of move " + move + "..");

            int connectScore;
            int positionalScore;
            int randomScore;
            int outScore;

            Tile sim = move.ToTile(state.CurrentPlayerID, state).Simulate(state);
            state.Alter(move, true);

            connectScore = state.DeepSearchFrom(sim.TiledPosition, out _, null).Score;
            STOLON.Debug.Log("move has a connectScore of " + connectScore);

            int distanceFromCenter = (int)MathF.Abs((sim.TiledPosition.X - (state.Dimensions.X / 2f))); // more = bad
            int distanceFromGround = Math.Abs(move.Origin.Y - sim.TiledPosition.Y); // more = bad
            positionalScore = distanceFromCenter * distanceFromCenter * -1 * distanceFromGround * distanceFromGround;
            STOLON.Debug.Log("move has a positionalScore of " + positionalScore + ", xDelta: " + distanceFromCenter + ", yDelta: " + distanceFromGround);

            Random random = new Random();
            randomScore = random.Next(1, 40);
            STOLON.Debug.Log("move has a randomScore of " + randomScore);

            state.Undo();
            outScore = score + connectScore + positionalScore + randomScore;

            STOLON.Debug.Log("total: " + outScore);
            STOLON.Debug.Success();

            return outScore;
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

        public static int Negamax(BoardState node, Point sim, UniqueMoveBoardMap map, IDictionary<int, TTEntry> tt, int depth, int alpha, int beta, int color)
        {

            //int alphaOrig = alpha;
            //int stateCode = node.GetStateCode();
            //if (tt.TryGetValue(stateCode, out TTEntry ttEntry) && ttEntry.Depth >= depth)
            //{
            //    //Console.WriteLine(ttEntry);
            //    if (ttEntry.Flag == NodeState.EXACT) return ttEntry.Value;
            //    else if (ttEntry.Flag == NodeState.LOWERBOUND)
            //        alpha = Math.Max(alpha, ttEntry.Value);
            //    else beta = Math.Min(beta, ttEntry.Value); // UPPERBOUND


            //    if (alpha >= beta) return ttEntry.Value;
            //}

            negaCount++;
            if (node.SearchFrom(sim, null, true).Succes) return -evalNum;
            if (depth == 0) return 0;
            List<Move> moves = map.GetAllMoves(node);
            int value = -evalNum;
            for (int i = 0; i < moves.Count; i++)
            {
                sim = node.Alter(moves[i], true);
                value = Math.Max(value, -Negamax(node, sim, map, tt, depth - 1, -beta, -alpha, -color));
                node.Undo();

                alpha = Math.Max(alpha, value);
                if (alpha >= beta) break;
            }

            //ttEntry.Value = value;
            //if (value <= alphaOrig) ttEntry.Flag = NodeState.LOWERBOUND;
            //else if (value >= beta) ttEntry.Flag = NodeState.UPPERBOUND;
            //else ttEntry.Flag = NodeState.EXACT;
            //ttEntry.Depth = depth;

            //tt[stateCode] = ttEntry;

            return value;
        }

        public struct TTEntry
        {
            public NodeState Flag;
            public int Value;
            public int Depth;
            public TTEntry(NodeState flag, int value, int depth)
            {
                Flag = flag;
                Value = value;
                Depth = depth;
            }
            public override string ToString()
            {
                return $"Flag: {Flag}, Value: {Value}, Depth: {Depth}";
            }
        }
        public struct NegamaxEndResult
        {
            public Move Move { get; }
            public int Items { get; }
            public int TotalTimeElapsed { get; }
            public float MsAItem { get; }

            public NegamaxEndResult(Move move, int items, int totalTimeElapsed) : this(move, items, totalTimeElapsed, (items > 0) ? (float)totalTimeElapsed / items : 0) { }
            public NegamaxEndResult(Move move, int items, int totalTimeElapsed, float msAItem)
            {
                Move = move;
                Items = items;
                TotalTimeElapsed = totalTimeElapsed;
                MsAItem = msAItem;
            }

            public override string ToString()
            {
                return $"Move: {Move}, Items: {Items}, Total Time Elapsed: {TotalTimeElapsed} ms, Ms/Item: ~{MsAItem}";
            }
        }
        public enum NodeState
        {
            EXACT,
            LOWERBOUND,
            UPPERBOUND,
        }
    }
}
