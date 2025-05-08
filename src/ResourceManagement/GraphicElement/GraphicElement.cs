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
#nullable enable

namespace Stolon
{
    public interface IGraphicElementParent
    {
        public Vector2 Position { get; }
    }
    public class GraphicElement : ICloneable
    {
        private Vector2 scale;

        public virtual GameTexture Texture { get; protected set; }
        public Vector2 Position { get; set; } //relative to parent
        public virtual Vector2 Scale { get => scale; set => scale = value; }
        public virtual Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        public IGraphicElementParent Source { get; }

        public string Name => Texture.Name.Split("\\").Last();

        public virtual float Height => (Texture.Height * Scale.Y);
        public virtual float Width => (Texture.Width * Scale.X);

        public ReadOnlyCollection<GameTexture> Textures => textures.ToList().AsReadOnly();
        protected GameTexture[] textures;

        public GraphicElement(IGraphicElementParent source, GameTexture texture) : this(source, texture, Vector2.Zero, Vector2.One) { }

        public GraphicElement(IGraphicElementParent source, GameTexture texture, Vector2 position, Vector2 scale, int textureSlots = 10)
        {
            Texture = texture;
            Position = position;
            Source = source;

            textures = new GameTexture[textureSlots];
            textures[0] = texture;

            this.scale = scale;
        }

        public virtual void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds, SpriteEffects effects = SpriteEffects.None)
        {
            spriteBatch.Draw(Texture, Position + Source.Position, null, Color.White, 0, new Vector2(0), Scale, SpriteEffects.None, 1f);
        }

        public virtual bool IsHovered(MouseState mouseState) => IsHovered(mouseState.Position);
        public virtual bool IsHovered(Point mousepos) => Bounds.Contains(mousepos);
        public virtual bool IsClicked(MouseState mouseState) => IsHovered(mouseState) && mouseState.LeftButton == ButtonState.Pressed;

        public virtual GraphicElement AddTexture(int index, GameTexture texture)
        {
            if (texture == null) throw new Exception();
            textures[index] = texture;
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

        internal void _SetTextures(GameTexture[] textures) => this.textures = textures;
        public GraphicElement SetPosition(Vector2 newPos, Orgin orgin = Orgin.TopLeft)
        {
            Position = orgin switch
            {
                Orgin.TopLeft => Centering.TopLeft(Texture, newPos, Scale),
                Orgin.TopRight => Centering.TopRight(Texture, newPos, Scale),
                Orgin.BottomLeft => Centering.BottomLeft(Texture, newPos, Scale),
                Orgin.BottomRight => Centering.BottomRight(Texture, newPos, Scale),
                _ => throw new Exception(),
            };
            return this;
        }
        public GraphicElement SetScale(float newScaling) => SetScale(new Vector2(newScaling));
        public GraphicElement SetScale(float newScalingX, float newScalingY) => SetScale(new Vector2(newScalingX, newScalingY));
        public GraphicElement SetScale(Vector2 newScaling)
        {
            Scale = newScaling;
            return this;
        }
        public object Clone() => new GraphicElement(Source, Texture, Position, Scale, textures.Length);
    }
}
