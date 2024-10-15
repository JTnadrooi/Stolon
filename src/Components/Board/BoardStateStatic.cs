using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using AsitLib.IO;
using Microsoft.Xna.Framework;
using static Stolon.BoardState;
using static Stolon.StolonGame;

using Point = Microsoft.Xna.Framework.Point;

#nullable enable

namespace Stolon
{
    public partial class BoardState
    {
        public static BoardState GetDefault(Player[] players) => new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection());
        public static bool Validate(BoardState state)
        {
            return true;
        }
    }
}
