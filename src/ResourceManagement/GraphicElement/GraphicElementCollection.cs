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

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;
using System.Collections;
#nullable enable

namespace STOLON
{
    /// <summary>
    /// A collection of U
    /// </summary>
    public class GraphicElementCollection : IDictionary<string, GraphicElement>
    {
        private IDictionary<string, GraphicElement> _elements;
        public IGraphicElementParent Source { get; }

        public GraphicElementCollection(IGraphicElementParent source) : this(source, new Dictionary<string, GraphicElement>()) { }
        public GraphicElementCollection(IGraphicElementParent source, IDictionary<string, GraphicElement> elements)
        {
            this._elements = elements;
            Source = source;
        }
        public ReadOnlyDictionary<string, GraphicElement> AsReadOnly() => new ReadOnlyDictionary<string, GraphicElement>(_elements);
        public GraphicElement AddElement(GraphicElement element)
        {
            if (element.Source != Source) throw new Exception();
            _elements.Add(element.Name, element);
            return element;
        }
        public GraphicElement AddGraphicElement(GameTextureCollection textures, string textureKey, string? newName = null)
        {
            GraphicElement element = new GraphicElement(Source, textures.GetReference("textures\\" + textureKey));
            _elements.Add(newName ?? textureKey.Split("\\").Last(), element);
            return element;
        }
        public void MergeWith(GraphicElement[] elements)
        {
            foreach (GraphicElement element in elements)
                AddElement(element);
        }

        public GraphicElement this[string key] { get => _elements[key]; set => _elements[key] = value; }
        public ICollection<string> Keys => _elements.Keys;
        public ICollection<GraphicElement> Values => _elements.Values;
        public int Count => _elements.Count;
        public bool IsReadOnly => false;
        public void Add(string key, GraphicElement value) => _elements.Add(key, value);
        public void Add(KeyValuePair<string, GraphicElement> item) => _elements.Add(item.Key, item.Value);
        public void Clear() => _elements.Clear();
        public bool Contains(KeyValuePair<string, GraphicElement> item) => _elements.Contains(item);
        public bool ContainsKey(string key) => _elements.ContainsKey(key);
        public void CopyTo(KeyValuePair<string, GraphicElement>[] array, int arrayIndex) => ((IDictionary<string, GraphicElement>)_elements).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<string, GraphicElement>> GetEnumerator() => _elements.GetEnumerator();
        public bool Remove(string key) => _elements.Remove(key);
        public bool Remove(KeyValuePair<string, GraphicElement> item) => _elements.Remove(item.Key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out GraphicElement value) => _elements.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_elements).GetEnumerator();

    }
}
