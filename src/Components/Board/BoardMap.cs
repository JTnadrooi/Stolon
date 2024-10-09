using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.XNA;
using MonoGame.Extended;
using static Stolon.StolonGame;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Math = System.Math;
using RectangleF = MonoGame.Extended.RectangleF;

#nullable enable

namespace Stolon
{
    public class UniqueMoveBoardMap
    {
        public int InitialUniqueMoveCount { get; }

        private Move[] moves;
        int w = 8;

        public UniqueMoveBoardMap()
        {
            InitialUniqueMoveCount = 16;
            moves = new Move[InitialUniqueMoveCount];
            for (int i = 0; i < InitialUniqueMoveCount; i++)
            {
                moves[i] = new Move(GetMovePos(i));
            }
        }

        public bool TryGetMove(int moveIndex, BoardState state, out Move? move)
        {
            move = IsValid(moveIndex, state) ? GetMove(moveIndex) : null;
            return move != null;
        }
        public bool IsValid(int moveIndex, BoardState state)
        {
            return !state.GetTile(GetMovePos(moveIndex)).IsOccupiedByPlayer();
        }
        public Point GetMovePos(int moveIndex)
        {
            return new Point(moveIndex % w, moveIndex >= w ? 4 : 3);
        }
        public Move GetMove(int moveIndex)
        {
            return moves[moveIndex];
        }
        public List<Move> GetAllMoves(BoardState state)
        {
            List<Move> toret = new List<Move>();
            for (int i = 0; i < InitialUniqueMoveCount; i++)
            {
                if (TryGetMove(i, state, out Move? move)) toret.Add(move!.Value);
            } 
            return toret;
        }
    }
}