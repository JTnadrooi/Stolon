using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Specialized;
using static Stolon.StolonGame;

#nullable enable

namespace Stolon
{
    public abstract class TileAttributeBase
    {
        public string Id;

        public TileAttributeBase(string id)
        {
            Id = id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Id.Equals(Id);
        }

        public override string ToString()
        {
            return Id;
        }
    }
    public static class TileAttribute
    {
        public static FrozenDictionary<string, TileAttributeBase> TileAttributes { get; }

        public static HashSet<TileAttributeBase> DefaultAttributes { get; }


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
            };
        }
        //public static void Register<T>(T attribute) where T : TileAttributeBase => TileAttributes.Add(attribute.Id, attribute);

        public static bool IsOccupiedByPlayer(this Tile tile) => tile.HasAttribute((TileAttributeBase)TileAttributes["Player0Occupied"], (TileAttributeBase)TileAttributes["Player1Occupied"]);
        public static int GetOccupiedByPlayerID(this Tile tile)
        {
            if (tile.HasAttribute((TileAttributeBase)TileAttributes["Player0Occupied"])) return 0;
            if (tile.HasAttribute((TileAttributeBase)TileAttributes["Player1Occupied"])) return 1;
            return -1;
        }
        public static HashSet<TileAttributeBase> GetPlayerAttributes(int playerID) => new HashSet<TileAttributeBase>()
            {
                (TileAttributeBase)TileAttribute.TileAttributes["Player" + playerID + "Occupied"],
                TileAttribute.Get<TileAttribute.TileAttributeSolid>(),
            }; // dont staticify!!
        public static bool HasGravity(this Tile tile) => tile.HasAttribute<TileAttributeGravDown>() || tile.HasAttribute<TileAttributeGravUp>();
        public static bool IsSolid(this Tile tile) => tile.HasAttribute<TileAttributeSolid>();

        public static HashSet<TileAttributeBase> ReplaceAttribute<TFrom, TTo>(this HashSet<TileAttributeBase> attributes) where TFrom : TileAttributeBase where TTo : TileAttributeBase
        {
            Instance.DebugStream.WriteLine("Attempting the replacement of attribute " + GetName<TFrom>() + " to " + GetName<TTo>());
            if (!attributes.Remove(Get<TFrom>())) throw new Exception();
            if (!attributes.Add(Get<TTo>())) throw new Exception();
            return attributes;
        }

        public static string GetName<TTileAttribute>() where TTileAttribute : TileAttributeBase
            => typeof(TTileAttribute).Name.Replace("TileAttribute", string.Empty);
        public static TileAttributeBase Get<TTileAttribute>() where TTileAttribute : TileAttributeBase
            => (TileAttributeBase)TileAttributes[GetName<TTileAttribute>()];
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
        /// Fall ender?
        /// </summary>
        public sealed class TileAttributeSolid : TileAttributeBase
        {
            public TileAttributeSolid() : base("Solid") { }
        }
    }
}
