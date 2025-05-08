using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SizeF = MonoGame.Extended.SizeF;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Stolon
{
    public static class CompactibilityConversionExtensions
    {
        public static SizeF ToSizeF(this Point point) => new SizeF(point.X, point.Y);
        public static SizeF ToSizeF(this Vector2 vector) => new SizeF(vector.X, vector.Y);

        public static Point ToPoint(this PointF point) => new Point((int)point.X, (int)point.Y);
        //public static Point ToPoint(this Vector2 vector) => new Point((int)vector.X, (int)vector.Y);
        public static Point ToPoint(this SizeF size) => new Point((int)size.Width, (int)size.Height);

        public static PointF ToPointF(this Vector2 vector) => new PointF(vector.X, vector.Y);
        public static PointF ToPointF(this Point point) => new PointF(point.X, point.Y);

        public static Color ToColor(this System.Drawing.Color color) => new Color(color.R, color.G, color.B, color.A);

        public static System.Drawing.Color ToSystemColor(this Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

        public static Vector2 ToVector(this PointF point) => new Vector2(point.X, point.Y);
        //public static Vector2 ToVector(this Size2 size) => new Vector2(size.Width, size.Height);
        public static Vector2 ToVector(this SizeF size) => new Vector2(size.Width, size.Height);
        //public static Vector2 ToVector(this Point2 point) => new Vector2(point.X, point.Y);

        public static Rectangle ToRectangle(this Point point) => new Rectangle(Point.Zero, point);
    }
}
