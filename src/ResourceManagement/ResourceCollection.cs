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
    public interface IResourceCollection<TContent> : IDisposable, IReadOnlyDictionary<string, TContent>
    {
        public ContentManager ContentManager { get; }
        public TContent GetReference(string path);
        public void UnLoadAll();
        public void UnLoad(string path);
        public void Add(TContent resource, string? newName = null);
    }
    public class GameTextureCollection : IResourceCollection<GameTexture> 
    {
        private bool disposedValue;
        private GameTexture pixel;

        public IEnumerable<string> Keys => dictionary.Keys;
        public Dictionary<string, GameTexture> dictionary;
        public IEnumerable<GameTexture> Values => dictionary.Values;
        public int Count => dictionary.Count;
        public ContentManager ContentManager { get; }
        public GameTexture this[string key] => GetReference(key);

        public GameTextureCollection(ContentManager contentManager)
        {
            this.ContentManager = contentManager;
            dictionary = new Dictionary<string, GameTexture>();
            string[] files = Directory.GetFiles(contentManager.RootDirectory, "*", SearchOption.AllDirectories);
            if (files.Length == 0) throw new Exception("No initial content found.");
            foreach (string file in files)
            {
                Instance.DebugStream.WriteLine("[s]found file: " + file);
                string toLoad = file[(contentManager.RootDirectory.Length + 1)..].Split('.')[..^1].ToJoinedString();
                Instance.DebugStream.WriteLine("\tloading InTexture with id/key: " + toLoad);

                try
                {
                    dictionary.Add(toLoad, new GameTexture(TexturePalette.Debug, contentManager.Load<Texture2D>(toLoad)));
                    GameTexture inTexture = dictionary[toLoad];
                    Color[] data = new Color[inTexture.Width * inTexture.Height];
                    inTexture.GetColorData(data);
                    for (int i = 0; i < data.Length; i++)
                        if (!TexturePalette.Debug.Contains(data[i]) && data[i].A == 1)
                        {
                            Instance.DebugStream.WriteLine("found DEBUG texture: " + inTexture.Name);
                            break;
                        }
                }
                catch { }
            }
            pixel = new GameTexture(TexturePalette.Empty, new Texture2D(contentManager.GetGraphicsDevice(), 1, 1));
            ((Texture2D)pixel).SetData(new Color[] { Color.White });
        }
        public GameTexture Pixel => pixel;
        public GameTexture GetReference(string path)
        {
            GameTexture item = dictionary[path];
            if (item.Palette == null) Instance.DebugStream.WriteLine("found corrupted/partial texture.");
            return item;
        }
        public void Add(GameTexture resource, string? newName = null)
        {
            newName ??= resource.Name;
            dictionary.Add(newName, resource);
        }
        public TContent HardLoad<TContent>(string path)
        {
            Instance.DebugStream.WriteLine("hardloading path: " + path);
            return ContentManager.Load<TContent>(path);
        }
        public void UnLoad(string path)
        {
            dictionary[path].Dispose();
            dictionary.Remove(path);
        }
        public void UnLoadAll()
        {
            foreach (GameTexture texture in dictionary.Values) texture.Dispose();
            dictionary.Clear();
            pixel.Dispose();
        }
        public bool ContainsKey(string key) => dictionary.ContainsKey(key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out GameTexture value) => dictionary.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, GameTexture>> GetEnumerator() => dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dictionary).GetEnumerator();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                UnLoadAll();
                disposedValue = true;
            }
        }
        ~GameTextureCollection() => Dispose(disposing: false);
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
