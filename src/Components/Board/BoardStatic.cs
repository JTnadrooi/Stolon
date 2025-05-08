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
using System.Diagnostics;
using System.Xml.Linq;
using MonoGame.Extended.Tiled;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// The representor of the board in the STOLON environment.
    /// </summary>
    public partial class Board : AxComponent
    { 
           public static Board MainInstance => StolonGame.Instance.Scene.Board;
    }
}
