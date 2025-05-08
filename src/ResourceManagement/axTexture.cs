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
using MonoGame.Extended;
using AsitLib.Collections;

using MonoGame.Extended.Content;
#nullable enable

namespace Stolon
{
    public class AxTexture
    {
        public IAxPalette Palette => palette;
        public string Name { get => texture.Name; set => texture.Name = value; }
        public object Tag { get => texture.Tag; set => texture.Tag = value; }
        public int Width => texture.Width;
        public int Height => texture.Height;
        public Rectangle Bounds => texture.Bounds;
        public bool IsDisposed => texture.IsDisposed;

        private IAxPalette palette;
        private Texture2D texture;
        private bool disposedValue;

        public AxTexture(IAxPalette palette, Texture2D texture)
        {
            if (palette == null || texture == null) throw new Exception();
            this.palette = palette;
            this.texture = texture;
        }
        public AxTexture(IAxPalette palette, GraphicsDevice graphicsDevice, int width, int height, string name = "")
        {
            texture = new Texture2D(graphicsDevice, width, height);
            texture.Name = name;
            if (palette == null) throw new Exception();
            this.palette = palette;
        }
        public void GetColorData(Color[] data) => texture.GetData(data);
        public void SetColorData(Color[] data) => texture.SetData(data);
        public AxTexture ApplyPalette(IAxPalette newPalette, bool lazy = true)
        {
            //    DebugStream.WriteLine("\t\t[s]applying palette \"" + newPalette.Name + "\" to \"" + Name + "\" with palette; \"" + palette.Name + "\".");
            //    if (lazy) DebugStream.WriteLine("\t\t\tlazy is enabled.");
            if (palette.Colors.Count != palette.Colors.Count) throw new Exception();
            if (lazy && palette.PaletteEquals(newPalette))
            {
                //DebugStream.WriteLine("\t\t\tlazy replacement succes.");
                return this;
            }
            else
            {
                //DebugStream.WriteLine("\t\t\tlazy replacement invalid, this palette: " + palette.Name + ", other: " + newPalette.Name + ".");
            }

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
            //DebugStream.Succes(3);
            return this;
        }
        public AxTexture InvertColors()
        {
            return ApplyPalette(palette.AsInverted(), true);
        }
        public static AxTexture GetPixel(GraphicsDevice graphicsDevice, string name = "pixel")
        {
            AxTexture pixel = new AxTexture(AxPalette.ToPalette(Color.White.ToSingleArray(), "solidWhite"), graphicsDevice, 1, 1);
            pixel.SetColorData(new Color[] { Color.White });
            pixel.Name = name;
            return pixel;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                texture.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~InTexture()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public static implicit operator Texture2D(AxTexture t) => t.texture;
        public static explicit operator AxTexture(Texture2D t) => new AxTexture(AxPalette.Debug, t);
    }
}
