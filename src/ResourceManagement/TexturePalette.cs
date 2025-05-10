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

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using System.Diagnostics;
using System.Collections;
using AsitLib.Collections;
using System.Xml;
#nullable enable

namespace Stolon
{
    public interface ITexturePalette
    {
        public ReadOnlyCollection<Color> Colors { get; }
        public string Name { get; }
        public int Size => Colors.Count;
    }
    
    public static class TexturePalette
    {
        public enum BloomAlgorithm
        {
            Nadrooi,
        }
        public static (float strenght, float threshold) GetBloomConfig(this TexturePalette4 palette, BloomAlgorithm algorithm, bool cheap = true, float bloomMult = 0.85f)
        {
            float c1Bright = palette.GetBrightness(0, cheap);
            float c2Bright = palette.GetBrightness(1, cheap);
            float threshold = 0f;
            float strenght = 0f;

            switch (algorithm)
            {
                case BloomAlgorithm.Nadrooi:
                    float delta12 = c1Bright - c2Bright;
                    float deltaPow = delta12 * delta12;
                    threshold = c2Bright - deltaPow;
                    strenght = ((1 - c1Bright) * (1 / c1Bright) + delta12 - deltaPow) * bloomMult;
                    break;
                default: throw new Exception("what");
            }
            return (strenght, threshold);
        }
        public static string ToColorString(this ITexturePalette? palette)
        {
            if (palette == null) return "NULL";
            return palette.Colors.ToJoinedString(" || ");
        }
        public static ITexturePalette ToPalette(IEnumerable<Color> colors, string name) => new DynamicTexturePalette(name, colors.ToArray());
        public static ITexturePalette AsInverted(this ITexturePalette palette) => new DynamicTexturePalette(palette.Name + "-inverted", palette.Colors.Reverse().ToArray());
        /// <summary>
        /// Get the brightness of a color in the <see cref="ITexturePalette"/>.
        /// </summary>
        /// <param name="palette">The palette.</param>
        /// <param name="colorIndex">The 0-based index of the color in the <paramref name="palette"/>.</param>
        /// <param name="cheap">If a cheap, less accurate calculation will be used.</param>
        /// <returns>The brightness of a <see cref="Color"/> on the range from 0 to 1.</returns>
        public static float GetBrightness(this ITexturePalette palette, int colorIndex, bool cheap = true) => palette.Colors[colorIndex].GetBrightness(cheap);
        public static float GetBrightness(this ITexturePalette palette, bool cheap = true)
        {
            float overall = 0f;
            for (int i = 0; i < palette.Size; i++)
                overall += palette.GetBrightness(i, cheap);
            return overall / palette.Size;
        }
        public static bool PaletteEquals(this ITexturePalette palette1, ITexturePalette palette2) => palette1.Colors.SequenceEqual(palette2.Colors);
        public static ITexturePalette Empty => new DynamicTexturePalette();
        public static TexturePalette4 Debug => new TexturePalette4("debugPalette", new Color(0, 255, 251), new Color(255, 251, 0), new Color(255, 0, 4), new Color(0, 4, 255));
    }
    public readonly struct DynamicTexturePalette : ITexturePalette
    {
        public ReadOnlyCollection<Color> Colors => colors.ToList().AsReadOnly();

        public string Name { get; }

        private readonly Color[] colors;

        public DynamicTexturePalette()
        {
            colors = Array.Empty<Color>();
            Name = "emptyDynamicTexturePalette";
        }
        public DynamicTexturePalette(string name, params Color[] colors)
        {
            this.colors = colors.Copy();
            Name = name;
        }
    }
    public readonly struct TexturePalette4 : ITexturePalette
    {
        public Color Color1 => colors[0]; //lightest
        public Color Color2 => colors[1];
        public Color Color3 => colors[2];
        public Color Color4 => colors[3];

        public ReadOnlyCollection<Color> Colors => colors.ToList().AsReadOnly();

        public string Name { get; }

        private readonly Color[] colors;

        public TexturePalette4(string name, Bitmap bitmap, bool unique = true)
        {
            colors = new Color[4];
            Name = name;
            Array.Copy(StolonStatic.GetColors(bitmap, unique), 0 , colors, 0, colors.Length);
        }
        public TexturePalette4(string name, params Color[] colors)
        {
            Name = name;
            this.colors = new Color[4];
            //this.colors = colors.Copy();
            Array.Copy(colors, 0, this.colors, 0, this.colors.Length);
            ArrayHelpers.SetSize(ref colors, 4);
        }
        public bool Contains(Color color) => colors.Contains(color);
        // Custom comparer to sort colors by brightness
        //public static int CompareColorsByBrightness(Color color1, Color color2)
        //{
        //    // Calculate perceived brightness for each color
        //    double brightness1 = CalculatePerceivedBrightness(color1);
        //    double brightness2 = CalculatePerceivedBrightness(color2);

        //    // Compare brightness values
        //    return brightness1.CompareTo(brightness2);
        //}

        //// Function to calculate the perceived brightness of a color
        //public static double CalculatePerceivedBrightness(Color color)
        //{
        //    // Perceived brightness formula
        //    return color.R +  color.G +  color.B;
        //}
    }
    //public class ColorCollection16 : IReadOnlyCollection<Color>
    //{
    //    public int Count => colors.Length;
    //    public ReadOnlyCollection<Color> Colors => Array.AsReadOnly(colors);

    //    private Color[] colors;

    //    public ColorCollection16(Bitmap source, bool unique = true)
    //    {
    //        colors = AsitGame.GetColors(source, unique);
    //        AsitEnumerable.SetSize(ref colors, 16);
    //    }

    //    public ColorCollection16(Color[] colors)
    //    {
    //        if (colors.Length > 16) throw new InvalidOperationException();
    //        this.colors = colors.Copy();
    //    }

    //    public Palette4 GetPalette(int startIndex) => GetPalette(startIndex, startIndex + 1, startIndex + 2, startIndex + 3);
    //    public Palette4 GetPalette(params int[] indexes) 
    //        => indexes.Length == 4 ? new Palette4("colorCOLL", indexes.Select(i => colors[i]).ToArray()) : throw new InvalidOperationException();
    //    public override string ToString() => "{" + colors.ToJoinedString(", ") + "}";

    //    public IEnumerator<Color> GetEnumerator() => ((IEnumerable<Color>)colors).GetEnumerator();
    //    IEnumerator IEnumerable.GetEnumerator() => colors.GetEnumerator();
    //}
}
