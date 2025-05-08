using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stolon
{
    public class GameFont
    {
        public string Name { get; }
        public SpriteFont SpriteFont { get; }
        public float Scale { get; }
        public Vector2 Dimensions { get; }
        public GameFont(string name, SpriteFont spriteFont, float scale = 1)
        {
            Name = name;
            SpriteFont = spriteFont;
            Dimensions = spriteFont.MeasureString("A");
        }
        public Vector2 FastMeasureString(string s) => Dimensions * Scale * s.Length;
        public override string ToString() => Name + " (Scale: " + Scale + ", Dimensions: " + Dimensions + ")";
        public static implicit operator SpriteFont(GameFont font) => font.SpriteFont;
    }
}
