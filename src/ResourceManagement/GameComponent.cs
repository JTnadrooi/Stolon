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

namespace Stolon
{
    /// <summary>
    /// A interface that provides a basic way to interact with <see cref="Intrara"/> component classes.
    /// </summary>
    public interface IGameComponent
    {
        public Vector2 Position { get; }
        /// <summary>
        /// Update this component so it computes all the calculations.
        /// </summary>
        /// <param name="elapsedMiliseconds">The miliseconds since last frame.</param>
        public void Update(int elapsedMiliseconds);
        /// <summary>
        /// Update this component so it draws all sub-drawables.
        /// </summary>
        /// <param name="elapsedMiliseconds">The miliseconds since last frame.</param>
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds);
        /// <summary>
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> featuring all the <see cref="GraphicElement"/> objects managed by this <see cref="IGameComponent"/>.
        /// </summary>
        public ReadOnlyDictionary<string, GraphicElement> Elements { get; }
    }
    public abstract class GameComponent : IGameComponent, IGraphicElementParent
    {
        public ReadOnlyDictionary<string, GraphicElement> Elements => new ReadOnlyDictionary<string, GraphicElement>(graphicElements);

        public virtual Vector2 Position { get; protected set; }

        public IGameComponent? Source { get; }

        protected GraphicElementCollection graphicElements;

        protected GameComponent(IGameComponent? source = null)
        {
            Position = Vector2.Zero;
            Source = source;
            graphicElements = new GraphicElementCollection(this);
        }
        public virtual void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {

        }
        public virtual void Update(int elapsedMiliseconds)
        {

        }
        protected virtual GraphicElement AddGraphicElement(GraphicElement element)
        {
            graphicElements.Add(element.Name, element);
            return element;
        }
        //protected virtual UIElement LoadElement(string name) => AddElement(new UIElement(this, GameContent.LoadTexture(name)));
    }
   
}
