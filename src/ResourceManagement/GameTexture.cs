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
    public class GameTexture
    {
        public ITexturePalette Palette => palette;
        public string Name { get => texture.Name; set => texture.Name = value; }
        public object Tag { get => texture.Tag; set => texture.Tag = value; }
        public int Width => texture.Width;
        public int Height => texture.Height;
        public Rectangle Bounds => texture.Bounds;
        public bool IsDisposed => texture.IsDisposed;

        private ITexturePalette palette;
        private Texture2D texture;
        private bool disposedValue;

        public GameTexture(ITexturePalette palette, Texture2D texture)
        {
            if (palette == null || texture == null) throw new Exception();
            this.palette = palette;
            this.texture = texture;
        }
        public GameTexture(ITexturePalette palette, GraphicsDevice graphicsDevice, int width, int height, string name = "")
        {
            texture = new Texture2D(graphicsDevice, width, height);
            texture.Name = name;
            if (palette == null) throw new Exception();
            this.palette = palette;
        }
        public void GetColorData(Color[] data) => texture.GetData(data);
        public void SetColorData(Color[] data) => texture.SetData(data);
        public GameTexture ApplyPalette(ITexturePalette newPalette, bool lazy = true)
        {
            Instance.DebugStream.WriteLine("\t\t[s]applying palette \"" + newPalette.Name + "\" to \"" + Name + "\" with palette; \"" + palette.Name + "\".");
            if (lazy) Instance.DebugStream.WriteLine("\t\t\tlazy is enabled.");
            if (palette.Colors.Count != palette.Colors.Count) throw new Exception();
            if (lazy && palette.PaletteEquals(newPalette))
            {
                Instance.DebugStream.WriteLine("\t\t\tlazy replacement succes.");
                return this;
            }
            else Instance.DebugStream.WriteLine("\t\t\tlazy replacement invalid, this palette: " + palette.Name + ", other: " + newPalette.Name + ".");

            Color[] data = new Color[texture.Width * texture.Height];

            GetColorData(data);
            for (int i = 0; i < data.Length; i++)
                for (int i2 = 0; i2 < palette.Size; i2++)
                    if (data[i] == palette.Colors[i2])
                    {
                        data[i] = newPalette.Colors[i2];
                        break; //this line cost me a hour
                    }
            SetColorData(data);

            palette = newPalette; //copy
            Instance.DebugStream.Succes(3);
            return this;
        }
        public GameTexture InvertColors()
        {
            return ApplyPalette(palette.AsInverted(), true);
        }
        public static GameTexture GetPixel(GraphicsDevice graphicsDevice, string name = "pixel")
        {
            GameTexture pixel = new GameTexture(TexturePalette.ToPalette(Color.White.ToSingleArray(), "solidWhite"), graphicsDevice, 1, 1);
            pixel.SetColorData(new Color[] { Color.White });
            pixel.Name = name;
            return pixel;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) texture.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public static implicit operator Texture2D(GameTexture t) => t.texture;
        public static explicit operator GameTexture(Texture2D t) => new GameTexture(TexturePalette.Debug, t);
    }
}
