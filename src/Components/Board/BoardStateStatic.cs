using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using Microsoft.Xna.Framework;
using static Stolon.BoardState;
using static Stolon.StolonGame;

using Point = Microsoft.Xna.Framework.Point;

#nullable enable

namespace Stolon
{
    public partial class BoardState
    {
        /// <summary>
        /// Get the default starting <see cref="BoardState"/>.
        /// </summary>
        /// <param name="players">The players participating.</param>
        /// <returns>The default starting <see cref="BoardState"/>.</returns>
        public static BoardState GetDefault(Player[] players) => new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection());
        /// <summary>
        /// Validate a <see cref="BoardState"/>.
        /// </summary>
        /// <param name="state">The <see cref="BoardState"/> to validate.</param>
        /// <returns>A value indicating if the gives <see cref="BoardState"/> is valid or not.</returns>
        public static bool Validate(BoardState state)
        {
            return true;
        }
    }
}
