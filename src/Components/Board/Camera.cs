﻿using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

#nullable enable

namespace STOLON
{
    public class Camera2D
    {
        private Point _dimensions;
        private float _zoom;

        public Camera2D()
        {
            _dimensions = STOLON.Instance.VirtualDimensions;

            MinZoom = 0.1f;
            MaxZoom = 100f;
            Zoom = 1;
            Rotation = 0;
        }

        public Vector2 Position { get; set; }
        public float MaxZoom { get; set; }
        public float MinZoom { get; set; }
        public float Rotation { get; set; }
        public Matrix Projection => Matrix.CreateOrthographicOffCenter(0, ScreenSize.X, ScreenSize.Y, 0, -1, 1);
        public Rectangle ScreenRectangle => new Rectangle(0, 0, _dimensions.X, _dimensions.Y);
        public Vector2 ScreenSize => new Vector2(_dimensions.X, _dimensions.Y);
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                if (_zoom < MinZoom) _zoom = MinZoom;
                if (_zoom > MaxZoom) _zoom = MaxZoom;
            }
        }
        public Matrix View => Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                                Matrix.CreateScale(Zoom) *
                                Matrix.CreateRotationZ(Rotation) *
                                Matrix.CreateTranslation(ScreenSize.X / 2, ScreenSize.Y / 2, 0);

        public Vector2 Unproject(Vector2 screenPosition) => Position + (screenPosition - ScreenSize / 2) / Zoom;
        public Vector2 Project(Vector2 worldPosition) => (worldPosition - Position) * Zoom - ScreenSize / 2;
        public override string ToString() => string.Format("Camera, pos: {0} area: {1}", Position, GetVisibleArea());
        public Rectangle GetVisibleArea()
        {
            Vector2 r = Unproject(Vector2.Zero);
            return new Rectangle((int)r.X, (int)r.Y, (int)(ScreenSize.X / Zoom), (int)(ScreenSize.Y / Zoom));
        }

    }
    public static class CameraStatic
    {
        public static Vector2 PixelLock(this Vector2 v, Camera2D camera2D)
        {
            return v;
        }
    }
}
