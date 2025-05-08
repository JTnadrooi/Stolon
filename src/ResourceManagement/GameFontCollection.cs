using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Linq;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing;
using System;
using System.Runtime.Versioning;
using System.Reflection.Metadata;
using AsitLib;
using System.Windows;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;
using AsitLib.Collections;
using System.Diagnostics.CodeAnalysis;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using System.IO;
using MonoGame.Extended.Content;
using Microsoft.Xna.Framework.Content;
using static Stolon.StolonGame;
#nullable enable

namespace Stolon
{
    public class GameFontCollection : ResourceCollection<GameFont>
    {
        public GameFontCollection(ContentManager contentManager, bool debug = false) : base(contentManager, (toLoad) =>
        {
            try
            {
                float scale = toLoad switch
                {
                    "fonts\\fiont" => 0.5f,
                    _ => 1f,
                };
                GameFont font = new GameFont(toLoad, contentManager.Load<SpriteFont>(toLoad));
                return font;
            }
            catch { return null; }
        })
        {
            Console.WriteLine(dictionary["fonts\\fiont"].ToString());
        }
    }
}
