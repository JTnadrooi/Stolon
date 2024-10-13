using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using AsitLib.IO;
using static Stolon.BoardState;
using static Stolon.StolonGame;

using Point = Microsoft.Xna.Framework.Point;

#nullable enable

namespace Stolon
{
    public class BoardState
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

            public static ReadOnlyDictionary<string, SearchTarget> DefaultWinTargets { get; }

            static SearchTargetCollection()
            {
                DefaultWinTargets = new ReadOnlyDictionary<string, SearchTarget>(new Dictionary<string, SearchTarget>(new SearchTarget[]
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
        }

        public Point Dimensions => dimensions;
        public Tile[,] Tiles => tiles;
        public Player[] Players => players;
        public SearchTargetCollection WinSearchTargets => winSearchTargets;
        public int CurrentPlayerID { get; private set; }
        public int TileCount { get; set; }
        public Player CurrentPlayer => Players[CurrentPlayerID];

        public Stack<UndoObj> undoStack;
        public Collection<UndoObj> undoSet;

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
            undoStack = new Stack<UndoObj>();
            undoSet = new Collection<UndoObj>();
        }
        public void GoNextPlayer()
        {
            CurrentPlayerID = NextPlayer;
        }
        public int DistanceFromCenter(int x) => Math.Min(x, 8 / 2);
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
            BoardState toret = new BoardState(tiles2, players, winSearchTargets, CurrentPlayerID)
            {
                TileCount = this.TileCount,
            };
            return toret;
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
        public int SearchAny(SearchTargetCollection? targets = null)
        {
            for (int i = 0; i < 2; i++)
            {
                if (Search(i, targets))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Search(int targetPlayer, SearchTarget target)
        {
            //Console.WriteLine(targetPlayer);
            for (int x = 0; x < dimensions.X; x++)
            {
                for (int y = 0; y < dimensions.Y; y++)
                {
                    //Console.WriteLine(new Point(x, y) == new Point(4, 4));
                    int playerid = tiles[x, y].GetOccupiedByPlayerID();
                    if (playerid == -1 || playerid != targetPlayer) continue;
                    {
                        //Console.WriteLine(new Point(x, y) + " " + playerid + " not");
                    }
                    if (SearchFrom(new Point(x, y), target, false, playerid).Succes)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public SquaredSearchData DeepSearchFrom(Point pos, out int outPlayerId, SearchTargetCollection? targets = null)
        {
            targets ??= winSearchTargets;
            int playerid = tiles[pos.X, pos.Y].GetOccupiedByPlayerID();
            int score = 0;
            outPlayerId = playerid;
            if (playerid == -1)
            {
                return SquaredSearchData.False;
            }

            foreach (string targetKey in targets.Keys)
            {
                SearchTarget target = targets[targetKey];
                SearchData toret = SearchFrom(pos, target, true, playerid);
                score += toret.Score * toret.Score;
                if (toret.Succes) return new SquaredSearchData(score, toret.Succes);
            }
            return new SquaredSearchData(score, false);
        }

        public SearchData SearchFrom(Point pos, SearchTargetCollection? targets = null, bool twotry = false) => SearchFrom(pos, out _, targets, twotry);
        public SearchData SearchFrom(Point pos, out int outPlayerId, SearchTargetCollection? targets = null, bool twotry = false)
        {
            targets ??= winSearchTargets;
            int playerid = tiles[pos.X, pos.Y].GetOccupiedByPlayerID();
            int score = 0;
            outPlayerId = playerid;
            if (playerid == -1)
            {
                return SearchData.False;
            }

            foreach (string targetKey in targets.Keys)
            {
                SearchTarget target = targets[targetKey];
                SearchData toret = SearchFrom(pos, target, twotry, playerid);
                score += toret.Score;
                if (toret.Succes) return new SearchData(score, toret.Succes);
            }
            return new SearchData(score, false);
        }

        public SearchData SearchFrom(Point pos, SearchTarget target, bool twotry = false, int occuPlayerId = -1)
        {
            occuPlayerId = occuPlayerId == -1 ? tiles[pos.X, pos.Y].GetOccupiedByPlayerID() : occuPlayerId;
            if (occuPlayerId == -1) return SearchData.False;
            //Console.WriteLine();

            //Console.WriteLine("nodes: " + target.Nodes.ToJoinedString(", "));
            //Console.WriteLine("revnodes: " + target.InvertedNodes.ToJoinedString(", "));

            int SearchFromInternallly(Point[] nodes, Point newPos)
            {
                //Console.WriteLine("reversing with orgin: " + newPos);
                int score = 0;
                if (occuPlayerId == -1) return 0;

                for (int i = 0; i < nodes.Length; i++)
                {
                    Tile tile;
                    try
                    {
                        //Console.WriteLine("trying: " + (nodes[i] + newPos));
                        tile = GetTile(nodes[i] + newPos);
                    }
                    catch { continue; }
                    if (tile.GetOccupiedByPlayerID() == occuPlayerId) score++;
                }
                return score;
            }

            Point[] nodes = target.Nodes;

            int score = SearchFromInternallly(nodes, pos);

            if (score == nodes.Length) return new SearchData(score, true);
            else if (twotry)
            {
                int siscore = SearchFromInternallly(target.InvertedNodes, nodes[score - 1] + pos);
                return new SearchData(siscore, siscore == target.InvertedNodes.Length);
            }

            return new SearchData(score, false);
        }

        public int GetPlayerID(Player player) => players.GetFirstIndexWhere(p => p.Equals(player));
        public bool Alter(Tile overridenTile)
        {
            Tiles[overridenTile.TiledPosition.X, overridenTile.TiledPosition.Y].Attributes = overridenTile.Attributes;
            Tiles[overridenTile.TiledPosition.X, overridenTile.TiledPosition.Y].TileType = overridenTile.TileType;
            return true;
        }
        public Point Alter(Move move, bool nextPlayer = false)
        {

            if (move.Origin.X >= Dimensions.X) throw new Exception();
            if (move.Origin.Y >= Dimensions.Y) throw new Exception();

            //HashSet<TileAttributeBase> toadd = new HashSet<TileAttributeBase>()
            //{
            //    (TileAttributeBase)TileAttribute.TileAttributes["Player" + CurrentPlayerID + "Occupied"],
            //    TileAttribute.Get<TileAttribute.TileAttributeSolid>(),
            //};
            //toadd.UnionWith(Tiles[move.Origin.X, move.Origin.Y].Attributes);

            //Tile sim = new Tile(new Point(move.Origin.X, move.Origin.Y), null, toadd).Simulate(this);
            Tile sim = move.ToTile(CurrentPlayerID, Tiles[move.Origin.X, move.Origin.Y].Attributes).Simulate(this);


            Alter(sim);
            undoStack.Push(new UndoObj(sim, nextPlayer));
            undoSet.Add(undoStack.Peek());

            if (nextPlayer) GoNextPlayer();

            TileCount++;

            return sim.TiledPosition;
        }
        public int GetStateCode()
        {
            int value = 0;
            //int v2 = 0;
            foreach (var item in undoSet)
            { 
                //value = HashCode.Combine(item.GetHashCode(), value);
                value += item.GetHashCode();
            }
            value = HashCode.Combine(TileCount, CurrentPlayerID, value);
            return value;
        }
        public void Undo()
        {
            UndoObj undoObj = undoStack.Pop();
            undoSet.Remove(undoObj);

            TileCount--;
            if (undoObj.NextPlayer) CurrentPlayerID = CurrentPlayerID == 0 ? 1 : 0;

            undoObj.Sim.Attributes.Remove((TileAttributeBase)TileAttribute.TileAttributes["Player" + CurrentPlayerID + "Occupied"]);
            undoObj.Sim.Attributes.Remove(TileAttribute.Get<TileAttribute.TileAttributeSolid>());

            Alter(new Tile(undoObj.Sim.TiledPosition, undoObj.Sim.TileType, undoObj.Sim.Attributes));
        }
        public struct UndoObj
        {
            public Tile Sim { get; }
            public bool NextPlayer { get; }
            public UndoObj(Tile sim, bool nextPlayer)
            {
                Sim = sim;
                NextPlayer = nextPlayer;
            }
            public override int GetHashCode()
            {
                return Sim.GetHashCode();
            }
        }

        public struct SearchData
        {
            public int Score { get; }
            public bool Succes { get; }
            public SearchData(int score, bool succes)
            {
                Score = score;
                Succes = succes;
            }

            public override string ToString() => $"Score: {Score}, Success: {Succes}";

            static SearchData()
            {
                False = new SearchData(0, false);
            }
            public static SearchData False { get; }
        }


        public struct SquaredSearchData
        {
            public int Score { get; }
            public bool Succes { get; }
            public SquaredSearchData(int score, bool succes)
            {
                Score = score;
                Succes = succes;
            }

            public override string ToString() => $"[SQUARED] Score: {Score}, Success: {Succes}";

            static SquaredSearchData()
            {
                False = new SquaredSearchData(0, false);
            }
            public static SquaredSearchData False { get; }
        }
        //public struct DeepSearchData
        //{
        //    public int Score { get; }
        //    public bool Succes { get; }

        //}
    }
}