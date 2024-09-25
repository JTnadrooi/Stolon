using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using AsitLib;
using AsitLib.Collections;
using AsitLib.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Content;
using static Stolon.StolonGame;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using RectangleF = MonoGame.Extended.RectangleF;

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
        public static Dictionary<string, TileAttributeBase> TileAttributes { get; }

        public static HashSet<TileAttributeBase> DefaultAttributes { get; }

        static TileAttribute()
        {
            TileAttributes = new Dictionary<string, TileAttributeBase>();

            Register(new TileAttributePlayer0Occupied());
            Register(new TileAttributePlayer1Occupied());
            Register(new TileAttributeGravDown());
            Register(new TileAttributeGravUp());
            Register(new TileAttributeSolid());

            DefaultAttributes = new HashSet<TileAttributeBase>()
            {
                Get<TileAttributeGravDown>(),
            };
        }

        public static void Register<T>(T attribute) where T : TileAttributeBase => TileAttributes.Add(attribute.Id, attribute);
        public static bool IsOccupiedByPlayer(this Tile tile) => tile.HasAttribute(TileAttributes["Player0Occupied"], TileAttributes["Player1Occupied"]);
        public static int GetOccupiedByPlayerID(this Tile tile)
        {
            if (!tile.IsOccupiedByPlayer()) return -1;
            if (tile.HasAttribute(TileAttributes["Player0Occupied"])) return 0;
            if (tile.HasAttribute(TileAttributes["Player1Occupied"])) return 1;
            throw new Exception();
        }
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
            => TileAttributes[GetName<TTileAttribute>()];
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
