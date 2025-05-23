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

#nullable enable


namespace Stolon
{
    public class GameTexture
    {
        public ITexturePalette Palette => _palette;
        public string Name { get => _texture.Name; set => _texture.Name = value; }
        public object Tag { get => _texture.Tag; set => _texture.Tag = value; }
        public int Width => _texture.Width;
        public int Height => _texture.Height;
        public Rectangle Bounds => _texture.Bounds;
        public bool IsDisposed => _texture.IsDisposed;

        private ITexturePalette _palette;
        private Texture2D _texture;
        private bool _disposedValue;

        public GameTexture(ITexturePalette palette, Texture2D texture)
        {
            if (palette == null || texture == null) throw new Exception();
            this._palette = palette;
            this._texture = texture;
        }
        public GameTexture(ITexturePalette palette, GraphicsDevice graphicsDevice, int width, int height, string name = "")
        {
            _texture = new Texture2D(graphicsDevice, width, height);
            _texture.Name = name;
            if (palette == null) throw new Exception();
            this._palette = palette;
        }
        public void GetColorData(Color[] data) => _texture.GetData(data);
        public void SetColorData(Color[] data) => _texture.SetData(data);
        public GameTexture ApplyPalette(ITexturePalette newPalette, bool lazy = true)
        {
            STOLON.Debug.Log(">[s]applying palette \"" + newPalette.Name + "\" to \"" + Name + "\" with palette; \"" + _palette.Name + "\".");
            if (lazy) STOLON.Debug.Log("lazy is enabled.");
            if (_palette.Colors.Count != _palette.Colors.Count) throw new Exception();
            if (lazy && _palette.PaletteEquals(newPalette))
            {
                STOLON.Debug.Log("lazy replacement succes.");
                return this;
            }
            else STOLON.Debug.Log("lazy replacement invalid, this palette: " + _palette.Name + ", other: " + newPalette.Name + ".");

            Dictionary<Color, Color> colorMap = new Dictionary<Color, Color>(_palette.Size);
            for (int i = 0; i < _palette.Size; i++) colorMap[_palette.Colors[i]] = newPalette.Colors[i];

            Color[] data = new Color[_texture.Width * _texture.Height];

            GetColorData(data);
            for (int i = 0; i < data.Length; i++)
                if (colorMap.TryGetValue(data[i], out Color newColor)) data[i] = newColor;
            SetColorData(data);

            _palette = newPalette;
            STOLON.Debug.Success();
            return this;
        }
        public GameTexture InvertColors()
        {
            return ApplyPalette(_palette.AsInverted(), true);
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
            if (!_disposedValue)
            {
                if (disposing) _texture.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public static implicit operator Texture2D(GameTexture t) => t._texture;
        public static explicit operator GameTexture(Texture2D t) => new GameTexture(TexturePalette.Debug, t);
    }
}
