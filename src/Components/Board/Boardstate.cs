using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using static Stolon.StolonGame;

using Point = Microsoft.Xna.Framework.Point;

#nullable enable

namespace Stolon
{
    public struct BoardState
    {
        public class SearchTargetCollection : IDictionary<string, SearchTarget>
        {
            private Dictionary<string, SearchTarget> dictionary;

            public SearchTargetCollection(SearchTarget[]? searchTargets = null)
            {
                dictionary = searchTargets == null ? DefaultWinTargets.ToDictionary(kp => kp.Key, kp => kp.Value) : searchTargets.ToDictionary(s => s.Id);
            }

            public SearchTarget this[string key] { get => ((IDictionary<string, SearchTarget>)dictionary)[key]; set => ((IDictionary<string, SearchTarget>)dictionary)[key] = value; }
            public ICollection<string> Keys => ((IDictionary<string, SearchTarget>)dictionary).Keys;
            public ICollection<SearchTarget> Values => ((IDictionary<string, SearchTarget>)dictionary).Values;
            public int Count => ((ICollection<KeyValuePair<string, SearchTarget>>)dictionary).Count;
            public bool IsReadOnly => ((ICollection<KeyValuePair<string, SearchTarget>>)dictionary).IsReadOnly;
            public void Add(string key, SearchTarget value) => ((IDictionary<string, SearchTarget>)dictionary).Add(key, value);
            public void Add(KeyValuePair<string, SearchTarget> item) => ((ICollection<KeyValuePair<string, SearchTarget>>)dictionary).Add(item);
            public void Clear() => ((ICollection<KeyValuePair<string, SearchTarget>>)dictionary).Clear();
            public bool Contains(KeyValuePair<string, SearchTarget> item) => ((ICollection<KeyValuePair<string, SearchTarget>>)dictionary).Contains(item);
            public bool ContainsKey(string key) => ((IDictionary<string, SearchTarget>)dictionary).ContainsKey(key);
            public void CopyTo(KeyValuePair<string, SearchTarget>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, SearchTarget>>)dictionary).CopyTo(array, arrayIndex);
            public IEnumerator<KeyValuePair<string, SearchTarget>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, SearchTarget>>)dictionary).GetEnumerator();
            public bool Remove(string key) => ((IDictionary<string, SearchTarget>)dictionary).Remove(key);
            public bool Remove(KeyValuePair<string, SearchTarget> item) => ((ICollection<KeyValuePair<string, SearchTarget>>)dictionary).Remove(item);
            public bool TryGetValue(string key, [MaybeNullWhen(false)] out SearchTarget value) => ((IDictionary<string, SearchTarget>)dictionary).TryGetValue(key, out value);
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dictionary).GetEnumerator();

            public static ReadOnlyDictionary<string, SearchTarget> DefaultWinTargets => new ReadOnlyDictionary<string, SearchTarget>(new Dictionary<string, SearchTarget>(new SearchTarget[]
            {
                new SearchTarget("vertical4r", new Point[]
                {
                    new Point(0, 1),
                    new Point(0, 2),
                    new Point(0, 3),
                }),
                new SearchTarget("horizontal4r", new Point[]
                {
                    new Point(1, 0),
                    new Point(2, 0),
                    new Point(3, 0),
                }),
                new SearchTarget("diagonal4rDD", new Point[]
                {
                    new Point(1, 1),
                    new Point(2, 2),
                    new Point(3, 3),
                }),
                new SearchTarget("diagonal4rDU", new Point[]
                {
                    new Point(1, -1),
                    new Point(2, -2),
                    new Point(3, -3),
                }),
            }.Select(st => new KeyValuePair<string, SearchTarget>(st.Id, st))));
        }

        public Point Dimensions => dimensions;
        public Tile[,] Tiles => tiles;
        public Player[] Players => players;
        public SearchTargetCollection WinSearchTargets => winSearchTargets;
        public int CurrentPlayerID { get; private set; }
        public Player CurrentPlayer => Players[CurrentPlayerID];

        public int NextPlayer => CurrentPlayerID == 0 ? 1 : 0; //NONPOLY

        private readonly Tile[,] tiles;
        private readonly Player[] players;
        private readonly Point dimensions;
        private readonly SearchTargetCollection winSearchTargets;

        public BoardState(Tile[,] tiles, Player[] players, SearchTarget[] searchTargets, int currentPlayer = 0) : this(tiles, players, new SearchTargetCollection(searchTargets), currentPlayer) { }
        public BoardState(Tile[,] tiles, Player[] players, SearchTargetCollection searchTargets, int currentPlayer = 0)
        {
            this.tiles = tiles;
            this.players = players;
            dimensions = new Point(tiles.GetLength(0), tiles.GetLength(1));
            this.winSearchTargets = searchTargets;
            CurrentPlayerID = currentPlayer;
        }
        public void GoNextPlayer()
        {
            CurrentPlayerID = NextPlayer;
        }
        public Move[] GetUniqueMoves()
        {
            List<Move> toret = new List<Move>();
            for (int i = 0; i < Tiles.GetLength(0); i++)
            {
                
            }
            return toret.ToArray();
        }
        public BoardState DeepCopy()
        {
            Tile[,] tiles2 = new Tile[tiles.GetLength(0), tiles.GetLength(1)];
            for (int x = 0; x < dimensions.X; x++)
            {
                for (int y = 0; y < dimensions.Y; y++)
                {
                    tiles2[x, y] = tiles[x, y].Clone();
                }
            }
            return new BoardState(tiles2, players, winSearchTargets, CurrentPlayerID);
        }
        public Tile GetTile(Point p) => tiles[p.X, p.Y];
        public bool Search(int targetPlayer, SearchTargetCollection? targets = null)
        {
            targets ??= winSearchTargets;

            foreach (string targetKey in targets.Keys)
            {
                SearchTarget target = targets[targetKey];
                bool toret = Search(targetPlayer, target);
                if (toret) return true;
            }
            return false;
        }
        public bool Search(int targetPlayer, SearchTarget target)
        {
            Instance.DebugStream.WriteLine("\tsearching board for " + target.Id + "..");

            for (int x = 0; x < dimensions.X; x++)
            {
                for (int y = 0; y < dimensions.Y; y++)
                {
                    int playerid = tiles[x, y].GetOccupiedByPlayerID();
                    if (playerid == -1) continue;

                    int score = 0;
                    for (int i = 0; i < target.Nodes.Length; i++)
                    {
                        Tile tile;
                        try
                        {
                            tile = GetTile(target.Nodes[i] + new Point(x, y));
                        }
                        catch { continue; } // for when out of range.
                        if (tile.GetOccupiedByPlayerID() == targetPlayer)
                        {
                            score++;
                        }
                    }
                    if (score == target.Nodes.Length)
                    {
                        Instance.DebugStream.WriteLine("\t\tfound.");
                        return true;
                    }
                }
            }
            return false;
        }

        public int SearchAny(SearchTargetCollection? targets = null)
        {
            Instance.DebugStream.WriteLine("searching board for any searchtarget..");
            for (int i = 0; i < 2; i++)
            {
                if (Search(i, targets))
                {
                    Instance.DebugStream.WriteLine("found any for player " + i + ".");
                    Instance.DebugStream.Succes();
                    return i;
                }
            }
            Instance.DebugStream.Fail();
            return -1;
        }
        public int GetPlayerID(Player player) => players.GetFirstIndexWhere(p => p.Equals(player));
        public static bool Alter(ref BoardState board, Tile overridenTile)
        {
            board.Tiles[overridenTile.TiledPosition.X, overridenTile.TiledPosition.Y].Attributes = overridenTile.Attributes;
            board.Tiles[overridenTile.TiledPosition.X, overridenTile.TiledPosition.Y].TileType = overridenTile.TileType;
            return true;
        }
        public static bool Alter(ref BoardState board, Move move, bool? nextPlayer = null)
        {
            bool autoNext;

            if (move.Origin.X >= board.Dimensions.X) throw new Exception();
            if (move.Origin.Y >= board.Dimensions.Y) throw new Exception();

            HashSet<TileAttributeBase> toadd = new HashSet<TileAttributeBase>()
            {
                TileAttribute.TileAttributes["Player" + board.CurrentPlayerID + "Occupied"],
                TileAttribute.Get<TileAttribute.TileAttributeSolid>(),
            };
            toadd.UnionWith(board.Tiles[move.Origin.X, move.Origin.Y].Attributes);

            autoNext = Alter(ref board, new Tile(new Point(move.Origin.X, move.Origin.Y), null, toadd).Simulate(board));

            nextPlayer ??= autoNext;
            if (nextPlayer.Value) board.GoNextPlayer();
            return nextPlayer.Value;
        }
    }
}