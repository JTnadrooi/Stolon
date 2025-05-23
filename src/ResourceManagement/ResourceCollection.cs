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

namespace STOLON
{
    public interface IResourceCollection<TContent> : IDisposable, IReadOnlyDictionary<string, TContent>
    {
        public ContentManager ContentManager { get; }
        public TContent GetReference(string path);
        public void UnLoadAll();
        public void UnLoad(string path);
        public void Add(TContent resource, string? newName = null);
    }
    public abstract class ResourceCollection<TContent> : IResourceCollection<TContent>
    {
        protected readonly Dictionary<string, TContent> dictionary = new Dictionary<string, TContent>();
        private bool _disposedValue;

        public ContentManager ContentManager { get; }

        public IEnumerable<string> Keys => dictionary.Keys;
        public IEnumerable<TContent> Values => dictionary.Values;
        public int Count => dictionary.Count;
        public TContent this[string key] => GetReference(key);

        public ResourceCollection(ContentManager contentManager, Func<string, TContent?> loader)
        {
            string[] files = Directory.GetFiles(contentManager.RootDirectory, "*", SearchOption.AllDirectories);
            if (files.Length == 0) throw new Exception("No initial content found.");

            foreach (string file in files)
            {
                STOLON.Debug.Log("found file: " + file);
                string toLoad = file[(contentManager.RootDirectory.Length + 1)..].Split('.')[..^1].ToJoinedString();
                STOLON.Debug.Log(">loading resource with id/key: " + toLoad);
                TContent? loaderResult = loader(toLoad);
                if (loaderResult != null) dictionary[toLoad] = loaderResult;
                STOLON.Debug.Success();
            }
            ContentManager = contentManager;

        }

        public virtual void Add(TContent resource, string? newName = null)
        {
            if (newName == null) throw new ArgumentNullException(nameof(newName));
            dictionary.Add(newName, resource);
        }

        public virtual TContent GetReference(string path)
        {
            if (!dictionary.TryGetValue(path, out var item))
                throw new KeyNotFoundException($"Resource '{path}' not found.");
            return item;
        }

        public virtual void UnLoad(string path)
        {
            if (dictionary[path] is IDisposable disposable)
                disposable.Dispose();
            dictionary.Remove(path);
        }

        public virtual void UnLoadAll()
        {
            foreach (var item in dictionary.Values)
                if (item is IDisposable disposable)
                    disposable.Dispose();
            dictionary.Clear();
        }

        public bool ContainsKey(string key) => dictionary.ContainsKey(key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TContent value) => dictionary.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, TContent>> GetEnumerator() => dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue && disposing)
            {
                UnLoadAll();
                _disposedValue = true;
            }
        }

        ~ResourceCollection() => Dispose(disposing: false);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    
}
