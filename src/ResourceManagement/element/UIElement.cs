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

namespace Stolon
{
    public interface IGraphicElementParent
    {
        public Vector2 Position { get; }
    }
    public class GraphicElement : Stolon.IDrawable, ICloneable
    {
        private Vector2 scale;

        public virtual GameTexture Texture { get; protected set; }
        public Vector2 Position { get; set; } //relative to source
        public virtual Vector2 Scale { get => scale; set => scale = value; }
        public virtual Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        public IGraphicElementParent Source { get; }

        public string Name => Texture.Name.Split("\\").Last();

        public virtual float Height => (Texture.Height * Scale.Y);
        public virtual float Width => (Texture.Width * Scale.X);


        public ReadOnlyCollection<GameTexture> Textures => textures.ToList().AsReadOnly();
        protected GameTexture[] textures;

        //public UIElement(IIntraraComponent window, InTexture[] textures, Vector2 position, Vector2 scale)
        //{
        //    Texture = textures[0];
        //    Position = position;
        //    Source = window;

        //    for (int i = 0; i < textures.Length; i++)
        //    {
        //        AddTexture(i, textures[i]);
        //    }
        //    this.scale = scale;
        //}
        public GraphicElement(IGraphicElementParent source, GameTexture texture) : this(source, texture, Vector2.Zero, Vector2.One) { }
        public GraphicElement(IGraphicElementParent source, GameTexture texture, Vector2 position, Vector2 scale, int textureSlots = 10)
        {
            Texture = texture;
            Position = position;
            Source = source;

            textures = new GameTexture[textureSlots];
            textures[0] = texture;

            this.scale = scale;
            //DrawHandles.Add(new DrawMirror<UIElement>(this));
        }

        public virtual void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds, SpriteEffects effects = SpriteEffects.None)
        {
            //DebugStream.WriteLine(Scale);
            spriteBatch.Draw(Texture, Position + Source.Position, null, Color.White, 0, new Vector2(0), Scale, SpriteEffects.None, 1f);
        }
        public virtual bool IsHovered(MouseState mouseState) => IsHovered(mouseState.Position);
        public virtual bool IsHovered(Point mousepos)
        {
            return Bounds.Contains(mousepos);
        }
        public virtual bool IsClicked(MouseState mouseState)
        {
            return IsHovered(mouseState) && mouseState.LeftButton == ButtonState.Pressed;
        }
        public virtual GraphicElement AddTexture(int index, GameTexture texture)
        {
            if(texture == null)
            {
                textures[index] = null;
                return this;
            }
            textures[index] = texture;
            return this;
            if (texture.Bounds.SamePrintAs(Texture.Bounds)) textures[index] = texture;
            else throw new InvalidOperationException();
            return this;
        }
        public virtual void SetTexture(int index)
        {
            Texture = textures[index];
        }
        public void CastTexturesTo(GraphicElement element2)
        {
            if (element2.Texture.Height == Texture.Height && element2.Texture.Width == Texture.Width)
                element2._SetTextures(textures.Copy());
            else throw new InvalidOperationException("dimensions do not match.");
        }
        //public UIElement AddInverted(int textureIndex)
        //{

        //}


        internal void _SetTextures(GameTexture[] textures)
        {
            this.textures = textures;
        }
        public object Clone() => new GraphicElement(Source, Texture, Position, Scale, textures.Length);
    }
}
