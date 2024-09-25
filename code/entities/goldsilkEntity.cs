using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib.XNA;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System;
using System.Runtime.Versioning;
using System.Reflection.Metadata;
using AsitLib;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using System.Diagnostics;
using System.Collections;
using MonoGame.Extended;
using AsitLib.Collections;
using MonoGame.Extended.Content;

using static Stolon.StolonGame;
using RectangleF = MonoGame.Extended.RectangleF;
using System.Reflection;
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;
using System.Threading.Tasks;
using AsitLib.FormConsole;
using System.Collections.Concurrent;

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
        private const int winScore = 20;

        public GoldsilkCom(GoldsilkEntity source) : base(source)
        {
            player = null!;
        }

        public override void DoMove(Board board)
        {
            // player = GetPlayer(board);
            // board.State.Alter(Search(board.State, 5), player);
            // BoardState.Alter(board, player)
            // board.EndMove();
        }
        public Move Search(BoardState state, int maxDepth)
        {

            Move[] uniqueMoves = state.GetUniqueMoves();
            Move bestMove = Move.Invalid;

            Instance.DebugStream.WriteLine("\t[s]initializing parallel alpha-beta for best move of player: " + player + " with id: " + state.GetPlayerID(player));
            Instance.DebugStream.WriteLine("\tnumber of uniquemoves: " + uniqueMoves.Length);

            object lockObj = new object();

            ConcurrentDictionary<Move, MinMaxResult> results = new ConcurrentDictionary<Move, MinMaxResult>();


            Parallel.ForEach(uniqueMoves, (move) =>
            {
                BoardState bCopy = state.DeepCopy(); // copy (and alter) board
                BoardState.Alter(ref bCopy, move); // the last true indicates its goes to next player btw, should probably remove for readability but there is something nice about mehtods with way to may parameters.
                MinMaxResult result = -AlphaBeta(bCopy, 0, int.MinValue, int.MaxValue, maxDepth);
                results[move] = result;

                //lock (lockObj)
                //{
                //    Instance.DebugStream.WriteLine("\t\tmove #? has score: " + result.Score + ", current best score: " + bestScore);
                //    results.Add(move, result);
                //}

            });

            Instance.DebugStream.WriteLine("\tdumping found results..");
            foreach (var item in results.OrderBy(p => p.Key.Origin.X)) // jt.. this does not order the actual enumerable.. pls dont forget
            {
                Instance.DebugStream.WriteLine("\t\t result: " + item.Value.ToString() + "");
                Instance.DebugStream.WriteLine("\t\t\t linked to move: " + item.Key.ToString() + "");
            }

            Instance.DebugStream.WriteLine("\tgetting move(s) with highest score..");
            int highestScore = int.MinValue;
            foreach (var p in results) highestScore = Math.Max(p.Value.Score, highestScore);
            Dictionary<Move, MinMaxResult> highestScoreMoves = new Dictionary<Move, MinMaxResult>(results.Where(p => p.Value.Score == highestScore)); // get moves with highest score.
            Instance.DebugStream.WriteLine("\t\tnumber of moves with highest score: " + highestScoreMoves.Count);

            if (highestScore < 0)
            {
                Instance.DebugStream.WriteLine("\t\tGuaranteed loss if other plays perfectly detected, searching again but dumber in order to avoid the COM prematurelly taking the loss.");
                if (maxDepth == 1) return results.Keys.First();
                else return Search(state, (int)(maxDepth / 2f));
            }

            if (highestScoreMoves.Count == 1)
            {
                Instance.DebugStream.WriteLine("\t\tthere is only one competing move, assigning that one to \"bestMove\".");
                bestMove = highestScoreMoves.First().Key;
            }
            else
            {
                Instance.DebugStream.WriteLine("\t\tmultiple competing moves found, refining further. [depth]");
                int leastDeep = int.MaxValue;
                foreach (var p in highestScoreMoves) leastDeep = Math.Min(p.Value.Depth, leastDeep);
                List<Move> bestMoves = highestScoreMoves.WhereSelect(p => (p.Key, p.Value.Depth == leastDeep)).ToList(); // get move(s) with shortests path.
                Instance.DebugStream.WriteLine("\t\t\tnumber of moves with highest score and lowest depth: " + bestMoves.Count);

                if (bestMoves.Count == 1)
                {
                    Instance.DebugStream.WriteLine("\t\t\tthere is only one competing move, assigning that one to \"bestMove\".");
                }
                else
                {
                    Instance.DebugStream.WriteLine("\t\t\tmultiple competing moves found, refining further. [distance to middle]"); // todo
                }
                bestMove = bestMoves[0];
            }



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
