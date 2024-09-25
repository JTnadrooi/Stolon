using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib.XNA;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Text;
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

using static Stolon.StolonGame;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// A simple camera for 2D xna games.
    /// </summary>
    public class Camera
    {
        protected float zoom;
        protected float rotation;

        protected Matrix transform;
        protected Vector2 position;

        protected int viewportWidth;
        protected int viewportHeight;

        // the cameras zoom property. Higher
        // values will make sprites appear larger
        public float Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                // we want to ensure that the zoom
                // doesn't get too small with a simple
                // check. If we allow the zoom to be zero
                // the screen will appear blank. If we allow
                // it to be negative, sprites will be drawn
                // upside down!
                if (zoom < 0.1f) zoom = 0.1f;
            }
        }

        // the cameras rotation property. For
        // most simple games this can be ignored,
        // but could be useful for example, in a top
        // down shooter game.
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        // the cameras position in the game.
        // The camera will be centered on this
        // point.
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// the camera constructor, sets up the camera
        /// with some default values.
        /// </summary>
        /// <param name="initialPos">the cameras starting position in the game</param>
        public Camera(Vector2 initialPos)
        {
            zoom = 1f;
            rotation = 0.0f;
            position = initialPos;
        }

        /// <summary>
        /// moves the camera a specified amount.
        /// </summary>
        /// <param name="amount">the vector to add to the current position.</param>
        public void Move(Vector2 amount)
        {
            position += amount;
        }

        /// <summary>
        /// gets the cameras transformation matrix.
        /// </summary>
        /// <param name="graphicsDevice">the graphics device.</param>
        /// <returns>the transformation matrix</returns>
        public Matrix GetTransformation(GraphicsDevice graphicsDevice)
        {
            viewportWidth = graphicsDevice.Viewport.Width;
            viewportHeight = graphicsDevice.Viewport.Height;

            // we create a transformation matrix using the various properties of
            // of the camera. This matrix positions, rotates and scales everything in
            // the game world respective to the cameras properties, which gives us the illusion
            // that we have a camera that can move around, rotate and zoom in on our game world
            transform = Matrix.CreateTranslation(new Vector3(-position.X, -position.Y, 0)) *
                        Matrix.CreateRotationZ(Rotation) *
                        Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                        Matrix.CreateTranslation(new Vector3(viewportWidth * 0.5f, viewportHeight * 0.5f, 0));
            return transform;
        }

        /// <summary>
        /// Inverts the cameras transformation matrix.
        /// </summary>
        /// <returns>the inverse transformation matrix</returns>
        public Matrix InverseTransform()
        {
            //this is useful for when we want to negate
            //the cameras transformation matrix. If we
            //have a game that requires mouse input as well 
            //as a camera, then we would need this method 
            //to ensure that the mouses position is correct.
            //You would call this method as follows:
            //Vector2 mousePos = new Vector2(mousestate.X, mousestate.Y);
            //mousePos = Vector2.Transform(mousePos, ContactGame.camera.GetInverse()));
            //which would ensure the mouse position in the game is correct.

            Matrix inverse = Matrix.Invert(transform);
            return inverse;
        }
    }
    public class Camera2D
    {
        Point dimensions;

        float zoom;
        public Camera2D()
        {

            MinZoom = 0.1f;
            MaxZoom = 100f;
            Zoom = 1;
            Rotation = 0;
            this.dimensions = Instance.VirtualDimensions;
        }

        public Vector2 Position
        { get; set; }

        public float MaxZoom
        { get; set; }
        public float MinZoom
        { get; set; }
        public float Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
                if (zoom < MinZoom) zoom = MinZoom;
                if (zoom > MaxZoom) zoom = MaxZoom;
            }
        }
        public float Rotation
        { get; set; }

        public Matrix Projection
        {
            get
            {
                return Matrix.CreateOrthographicOffCenter(0, ScreenSize.X, ScreenSize.Y, 0, -1, 1);
            }
        }
        public Matrix View
        {
            get
            {
                return
                    Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateTranslation(ScreenSize.X / 2, ScreenSize.Y / 2, 0);

            }
        }
        public Rectangle ScreenRectangle
        {
            get
            {
                return new Rectangle(0, 0, dimensions.X, dimensions.Y);
            }
        }
        public Vector2 ScreenSize
        {
            get
            {
                return new Vector2(dimensions.X, dimensions.Y);
            }
        }
        public Rectangle VisibleArea
        {
            get
            {
                var r = Unproject(Vector2.Zero);
                return new Rectangle((int)r.X, (int)r.Y, (int)(ScreenSize.X / Zoom), (int)(ScreenSize.Y / Zoom));
            }
        }



        public Vector2 Unproject(Vector2 screenPosition)
        {
            return Position + (screenPosition - ScreenSize / 2) / Zoom;
        }
        public Vector2 Project(Vector2 worldPosition)
        {
            return (worldPosition - Position) * Zoom - ScreenSize / 2;
        }
        public override string ToString()
        {
            return String.Format("Camera, pos: {0} area: {1}", Position, VisibleArea);
        }
    }
}
