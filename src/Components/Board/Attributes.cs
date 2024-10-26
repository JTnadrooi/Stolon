using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Specialized;
using static Stolon.StolonGame;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// A base new <see cref="TileAttributeBase"/> objects
    /// </summary>
    public abstract class TileAttributeBase
    {
        /// <summary>
        /// The ID of the <see cref="TileAttributeBase"/>
        /// </summary>
        public string Id;
        /// <summary>
        /// Create a new <see cref="TileAttributeBase"/> with a set <see cref="Id"/>.
        /// </summary>
        /// <param name="id"></param>
        public TileAttributeBase(string id)
        {
            Id = id;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object? obj) => Id.Equals(Id);

        public override string ToString() => Id;
    }
    /// <summary>
    /// Provides some static methods for working with <see cref="TileAttributeBase"/> implementing classes.
    /// </summary>
    public static class TileAttribute
    {
        /// <summary>
        /// A <see cref="FrozenDictionary{TKey, TValue}"/> holding all registered <see cref="TileAttributeBase"/> objects.
        /// </summary>
        public static FrozenDictionary<string, TileAttributeBase> TileAttributes { get; }
        /// <summary>
        /// A <see cref="FrozenSet"/>
        /// </summary>
        public static FrozenSet<TileAttributeBase> DefaultAttributes { get; }


        static TileAttribute()
        {
            Dictionary<string, TileAttributeBase>  tileAttributes = new Dictionary<string, TileAttributeBase>();

            static void Register<T>(T attribute, Dictionary<string, TileAttributeBase> dic) where T : TileAttributeBase => dic.Add(attribute.Id, attribute);

            Register(new TileAttributePlayer0Occupied(), tileAttributes);
            Register(new TileAttributePlayer1Occupied(), tileAttributes);
            Register(new TileAttributeGravDown(), tileAttributes);
            Register(new TileAttributeGravUp(), tileAttributes);
            Register(new TileAttributeSolid(), tileAttributes);

            TileAttributes = tileAttributes.ToFrozenDictionary();

            DefaultAttributes = new HashSet<TileAttributeBase>()
            {
                Get<TileAttributeGravDown>(),
            }.ToFrozenSet();
        }
        /// <summary>
        /// Get if a <see cref="Tile"/> is occupied by any <see cref="Player"/>.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <returns>A value indicating if a <see cref="Tile"/> is occupied by any <see cref="Player"/>.</returns>
        public static bool IsOccupiedByPlayer(this Tile tile) => tile.HasAttribute((TileAttributeBase)TileAttributes["Player0Occupied"], (TileAttributeBase)TileAttributes["Player1Occupied"]);
        /// <summary>
        /// Gets the <strong>board relative id</strong> of the player occupying the <paramref name="tile"/> or <strong>-1</strong> if no player occupy the <paramref name="tile"/>.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <returns>The <strong>board relative id</strong> of the player occupying the <paramref name="tile"/> or <strong>-1</strong> if no player occupy the <paramref name="tile"/>.</returns>
        public static int GetOccupiedByPlayerID(this Tile tile)
        {
            if (tile.HasAttribute((TileAttributeBase)TileAttributes["Player0Occupied"])) return 0;
            if (tile.HasAttribute((TileAttributeBase)TileAttributes["Player1Occupied"])) return 1;
            return -1;
        }
        /// <summary>
        /// Get a new <see cref="HashSet{T}"/> containing a list of <see cref="TileAttributeBase"/> objects fit for indicating a <see cref="Tile"/> is occupied.
        /// </summary>
        /// <param name="playerID">The <strong>board-relative-id</strong> of the player to base the created <see cref="HashSet{T}"/> on.</param>
        /// <returns>A new <see cref="HashSet{T}"/> containing a list of <see cref="TileAttributeBase"/> objects fit for indicating a <see cref="Tile"/> is occupied.</returns>
        public static HashSet<TileAttributeBase> GetNewPlayerAttributes(int playerID) => new HashSet<TileAttributeBase>()
            {
                (TileAttributeBase)TileAttribute.TileAttributes["Player" + playerID + "Occupied"],
                TileAttribute.Get<TileAttribute.TileAttributeSolid>(),
            }; // keep new
        /// <summary>
        /// Get a value indicating if the <paramref name="tile"/> has gravity.
        /// </summary>
        /// <param name="tile">The <see cref="Tile"/> to check.</param>
        /// <returns>Returns a value indicating if the <paramref name="tile"/> has gravity.</returns>
        public static bool HasGravity(this Tile tile) => tile.HasAttribute<TileAttributeGravDown>() || tile.HasAttribute<TileAttributeGravUp>();
        /// <summary>
        /// Get a value indicating if the <paramref name="tile"/> is solid.
        /// </summary>
        /// <param name="tile">The <see cref="Tile"/> to check.</param>
        /// <returns>Returns a value indicating if the <paramref name="tile"/> is solid.</returns>
        public static bool IsSolid(this Tile tile) => tile.HasAttribute<TileAttributeSolid>();
        /// <summary>
        /// Replace a <see cref="TileAttributeBase"/> in a <see cref="HashSet{T}"/> of <see cref="TileAttributeBase"/> objects.
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="attributes">The <see cref="HashSet{T}"/> of attributes to alter.</param>
        /// <returns>The <paramref name="attributes"/> <see cref="HashSet{T}"/>.</returns>
        public static HashSet<TileAttributeBase> ReplaceAttribute<TFrom, TTo>(this HashSet<TileAttributeBase> attributes) where TFrom : TileAttributeBase where TTo : TileAttributeBase
        {
            Instance.DebugStream.WriteLine("Attempting the replacement of attribute " + GetName<TFrom>() + " to " + GetName<TTo>());
            if (!attributes.Remove(Get<TFrom>())) throw new Exception();
            if (!attributes.Add(Get<TTo>())) throw new Exception();
            return attributes;
        }
        /// <summary>
        /// Get the name of a <see cref="TileAttributeBase"/> type.
        /// </summary>
        /// <typeparam name="TTileAttribute">The <see cref="TileAttributeBase"/> type.</typeparam>
        /// <returns>The name of the <see cref="TileAttributeBase"/> type. <strong>Unique</strong>.</returns>
        public static string GetName<TTileAttribute>() where TTileAttribute : TileAttributeBase
            => typeof(TTileAttribute).Name.Replace("TileAttribute", string.Empty);
        /// <summary>
        /// Get a <see cref="TileAttributeBase"/> from the specified type.
        /// </summary>
        /// <typeparam name="TTileAttribute">The type of <see cref="TileAttributeBase"/> to get.</typeparam>
        /// <returns>A <see cref="TileAttributeBase"/> from the specified type.</returns>
        public static TileAttributeBase Get<TTileAttribute>() where TTileAttribute : TileAttributeBase
            => (TileAttributeBase)TileAttributes[GetName<TTileAttribute>()];
        /// <summary>
        /// Get a value indicating if a <paramref name="tile"/> has a specified <see cref="TileAttributeBase"/>.
        /// </summary>
        /// <typeparam name="TTileAttribute">The type of <see cref="TileAttributeBase"/> to check from.</typeparam>
        /// <param name="tile">The <see cref="Tile"/> to check.</param>
        /// <returns>A value indicating if a <paramref name="tile"/> has a specified <see cref="TileAttributeBase"/>.</returns>
        public static bool HasAttribute<TTileAttribute>(this Tile tile) where TTileAttribute : TileAttributeBase
            => tile.HasAttribute(Get<TTileAttribute>());
        /// <summary>
        /// Tile is occupied by player 0.
        /// </summary>
        public sealed class TileAttributePlayer0Occupied : TileAttributeBase
        {
            public TileAttributePlayer0Occupied() : base("Player0Occupied") { }
        }
        /// <summary>
        /// Tile is occupied by player 1.
        /// </summary>
        public sealed class TileAttributePlayer1Occupied : TileAttributeBase
        {
            public TileAttributePlayer1Occupied() : base("Player1Occupied") { }
        }
        /// <summary>
        /// Tile applies gravity DOWN.
        /// </summary>
        public sealed class TileAttributeGravDown : TileAttributeBase
        {
            public TileAttributeGravDown() : base("GravDown") { }
        }
        /// <summary>
        /// Tile applies gravity DOWN.
        /// </summary>
        public sealed class TileAttributeGravUp : TileAttributeBase
        {
            public TileAttributeGravUp() : base("GravUp") { }
        }
        /// <summary>
        /// Tile ends the fall of a tile.
        /// </summary>
        public sealed class TileAttributeSolid : TileAttributeBase
        {
            public TileAttributeSolid() : base("Solid") { }
        }
    }
}
