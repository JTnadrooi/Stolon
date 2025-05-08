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

using MonoGame.Extended;
using static Stolon.StolonGame;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Math = System.Math;
using RectangleF = MonoGame.Extended.RectangleF;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Provides a way to get the unique <see cref="Move"/> structs from any given board in a performant manner.
    /// </summary>
    public class UniqueMoveBoardMap
    {
        /// <summary>
        /// The initial count of unique <see cref="Move"/>'s
        /// </summary>
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
        /// <summary>
        /// Try to get a unique <see cref="Move"/>.
        /// </summary>
        /// <param name="moveIndex">The index of the <see cref="Move"/> to try get.</param>
        /// <param name="state">The current <see cref="BoardState"/>.</param>
        /// <param name="move">The <see langword="out"/> unique <see cref="Move"/>.</param>
        /// <returns></returns>
        public bool TryGetMove(int moveIndex, BoardState state, out Move? move)
        {
            move = IsValid(moveIndex, state) ? GetMove(moveIndex) : null;
            return move != null;
        }
        /// <summary>
        /// Get a value indicating if a <see cref="Move"/> with the specified index is still valid.
        /// </summary>
        /// <param name="moveIndex">The index of the <see cref="Move"/> to check.</param>
        /// <param name="state">The current <see cref="BoardState"/>.</param>
        /// <returns>A value indicating if a <see cref="Move"/> with the specified index is still valid.</returns>
        public bool IsValid(int moveIndex, BoardState state)
        {
            return !state.GetTile(GetMovePos(moveIndex)).IsOccupiedByPlayer();
        }
        /// <summary>
        /// Get the orgin of a <see cref="Move"/> with the <see cref="UniqueMoveBoardMap"/> relevant index.
        /// </summary>
        /// <param name="moveIndex">The index to get the <see cref="Move"/> orgin from.</param>
        /// <returns>The orgin of a <see cref="Move"/> with the <see cref="UniqueMoveBoardMap"/> relevant index.</returns>
        public Point GetMovePos(int moveIndex)
        {
            return new Point(moveIndex % w, moveIndex >= w ? 4 : 3);
        }
        /// <summary>
        /// Get the <see cref="Move"/> with the <see cref="UniqueMoveBoardMap"/> relevant index.
        /// </summary>
        /// <param name="moveIndex">The index to get the <see cref="Move"/> from.</param>
        /// <returns>The <see cref="Move"/> with the <see cref="UniqueMoveBoardMap"/> relevant index.</returns>
        public Move GetMove(int moveIndex)
        {
            return moves[moveIndex];
        }
        /// <summary>
        /// Get a <see cref="List{T}"/> holding all legal and unique moves.
        /// </summary>
        /// <param name="state">The current <see cref="BoardState"/>.</param>
        /// <returns>A <see cref="List{T}"/> holding all legal and unique moves.</returns>
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