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
    public class GameTextureCollection : ResourceCollection<GameTexture>
    {
        private readonly GameTexture _pixel;

        public GameTexture Pixel => _pixel;

        public GameTextureCollection(ContentManager contentManager, bool debug = false) : base(contentManager, (toLoad) =>
        {
            try
            {
                GameTexture texture = new GameTexture(TexturePalette.Debug, contentManager.Load<Texture2D>(toLoad));
                if (debug)
                {
                    Color[] data = new Color[texture.Width * texture.Height];
                    texture.GetColorData(data);
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (!TexturePalette.Debug.Contains(data[i]) && data[i].A == 1)
                        {
                            Instance.DebugStream.Log("found DEBUG texture: " + texture.Name);
                            break;
                        }
                    }
                }
                return texture;
            }
            catch { return null; }
        })
        {

            _pixel = new GameTexture(TexturePalette.Empty, new Texture2D(contentManager.GetGraphicsDevice(), 1, 1));
            ((Texture2D)_pixel).SetData(new[] { Color.White });
        }

        public TContent HardLoad<TContent>(string path)
        {
            Instance.DebugStream.Log("hardloading path: " + path);
            return ContentManager.Load<TContent>(path);
        }

        public override void UnLoadAll()
        {
            base.UnLoadAll();
            _pixel.Dispose();
        }
    }
}
