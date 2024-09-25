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

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Main player class, 
    /// </summary>
    public sealed class Player : ICloneable
    {
        public string Name { get; }

        public SLComputer? Computer { get; }

        public bool IsComputer => Computer != null;

        public Player(string name, SLComputer? computer = null)
        {
            Name = name;
            Computer = computer;
        }
        public override bool Equals(object? obj) => (Player?)obj != null && ((Player)obj).Name == Name;
        public override string? ToString() => Name;
        public override int GetHashCode() => Name.GetHashCode();

        public object Clone() => new Player(Name, Computer);
    }
}
