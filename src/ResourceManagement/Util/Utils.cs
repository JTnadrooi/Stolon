
using AsitLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
using Vector2 = Microsoft.Xna.Framework.Vector2;
#nullable enable

namespace STOLON
{
    public static class Utils
    {
        /// <summary>
        /// Copy a texture.
        /// </summary>
        /// <param name="texture">The texture to copy.</param>
        /// <param name="newName">The newName of the texture.</param>
        /// <param name="onlyPostFix">
        /// A value indicating if only the name of texture will be changed by <paramref name="newName"/>. If <see langword="false"/>,
        /// the entirety of the id/key will be changed.
        /// </param>
        /// <param name="collection">
        /// The <see cref="GameTextureCollection"/> this <see cref="GraphicsResource"/> will be added to. This is only relevant
        /// when <paramref name="lazyCopy"/> is set to <see langword="true"/>.
        /// </param>
        /// <param name="graphics">The <see cref="GraphicsDevice"/> that will manage the newly created <see cref="GraphicsResource"/>.</param>
        /// <param name="lazyCopy">
        /// If <see langword="true"/>, there will be a check performed to see if the <paramref name="newName"/> 
        /// is already contained in the given <paramref name="collection"/>. If yes, there will simply be a reference returned to that <see cref="GraphicsResource"/>.
        /// If no, the the resulting <see cref="Texture2D"/> will be added to the <paramref name="collection"/>.
        /// </param>
        /// <param name="action">
        /// The <see cref="Action{T}"/> that will be performed on the result of the 
        /// <see cref="Copy(Texture2D, string?, bool, GameTextureCollection?, GraphicsDevice?, bool, Action{Texture2D}?)"/>.
        /// </param>
        /// <returns></returns>
        public static GameTexture Copy(this GameTexture texture, GraphicsDevice graphicsDevice, GameTextureCollection collection,
            string? newName = null, bool onlyPostFix = true,
            bool lazyCopy = true, Action<GameTexture>? action = null)
        {
            action ??= new Action<GameTexture>(t => { });
            newName ??= texture.Name;
            newName = onlyPostFix ? (texture.Name.Split("\\")[..^1].ToJoinedString("\\") + "\\" + newName) : newName;

            if (collection.ContainsKey(newName) && lazyCopy)
            {
                return collection.GetReference(newName);
            }
            GameTexture texture2 = new GameTexture(texture.Palette, graphicsDevice, texture.Width, texture.Height);
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetColorData(data);
            texture2.SetColorData(data);
            texture2.Name = newName;
            texture2.Tag = texture.Tag;

            if (lazyCopy)
            {
                action.Invoke(texture2);
                collection.Add(texture2, newName);
            }

            return texture2;
        }
        public static string WrapText(string text, SpriteFont font, float maxLineWidth, float fontScale)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = font.MeasureString(" ").X * fontScale;

            foreach (string word in words)
            {
                Vector2 size = font.MeasureString(word) * fontScale;

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }
            return sb.ToString();
        }
        public static float Size(this Vector2 vector) => vector.X * vector.Y;
        public static int Size(this Point point) => point.X * point.Y;
        public static Texture2D ReplaceColor(this Texture2D texture, Color color1, Color color2) => ReplaceColor(texture, color1.ToSingleArray(), color2);
        public static Texture2D ReplaceColor(this Texture2D texture, Color[] color1s, Color color2)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                if (color1s.Any(c => c == data[i])) data[i] = color2;
            }
            texture.SetData(data);
            return texture;
        }
        public static GameTexture SetAllColor(this GameTexture texture, Color color)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetColorData(data);
            for (int i = 0; i < data.Length; i++) data[i] = data[i] == Color.Transparent ? Color.Transparent : color;
            texture.SetColorData(data);
            return texture;
        }
        public static char ConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard)
        {
            if (!TryConvertKeyboardInput(keyboard, oldKeyboard, out char key)) return (char)0;
            else return key;
        }
        public static float GetBrightness(this Color color, bool cheap = true)
        {
            float r1 = color.R / (float)byte.MaxValue;
            float g1 = color.G / (float)byte.MaxValue;
            float b1 = color.B / (float)byte.MaxValue;
            if (cheap) return 0.2126f * r1 + 0.7152f * g1 + 0.0722f * b1;
            else return MathF.Sqrt(0.299f * MathF.Pow(r1, 2f) + 0.587f * MathF.Pow(g1, 2) + 0.114f * MathF.Pow(g1, 2));
        }
        /// <summary>
        /// Tries to convert keyboard input to characters and prevents repeatedly returning the 
        /// same character if a key was pressed last frame, but not yet unpressed this frame.
        /// </summary>
        /// <param name="keyboard">The current KeyboardState</param>
        /// <param name="oldKeyboard">The KeyboardState of the previous frame</param>
        /// <param name="key">When this method returns, contains the correct character if conversion succeeded.
        /// Else contains the null, (000), character.</param>
        /// <returns>True if conversion was successful</returns>
        public static bool TryConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard, out char key)
        {
            Keys[] keys = keyboard.GetPressedKeys();
            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            if (keys.Length > 0 && !oldKeyboard.IsKeyDown(keys[0]))
            {
                switch (keys[0])
                {
                    //Alphabet keys
                    case Keys.A: if (shift) { key = 'A'; } else { key = 'a'; } return true;
                    case Keys.B: if (shift) { key = 'B'; } else { key = 'b'; } return true;
                    case Keys.C: if (shift) { key = 'C'; } else { key = 'c'; } return true;
                    case Keys.D: if (shift) { key = 'D'; } else { key = 'd'; } return true;
                    case Keys.E: if (shift) { key = 'E'; } else { key = 'e'; } return true;
                    case Keys.F: if (shift) { key = 'F'; } else { key = 'f'; } return true;
                    case Keys.G: if (shift) { key = 'G'; } else { key = 'g'; } return true;
                    case Keys.H: if (shift) { key = 'H'; } else { key = 'h'; } return true;
                    case Keys.I: if (shift) { key = 'I'; } else { key = 'i'; } return true;
                    case Keys.J: if (shift) { key = 'J'; } else { key = 'j'; } return true;
                    case Keys.K: if (shift) { key = 'K'; } else { key = 'k'; } return true;
                    case Keys.L: if (shift) { key = 'L'; } else { key = 'l'; } return true;
                    case Keys.M: if (shift) { key = 'M'; } else { key = 'm'; } return true;
                    case Keys.N: if (shift) { key = 'N'; } else { key = 'n'; } return true;
                    case Keys.O: if (shift) { key = 'O'; } else { key = 'o'; } return true;
                    case Keys.P: if (shift) { key = 'P'; } else { key = 'p'; } return true;
                    case Keys.Q: if (shift) { key = 'Q'; } else { key = 'q'; } return true;
                    case Keys.R: if (shift) { key = 'R'; } else { key = 'r'; } return true;
                    case Keys.S: if (shift) { key = 'S'; } else { key = 's'; } return true;
                    case Keys.T: if (shift) { key = 'T'; } else { key = 't'; } return true;
                    case Keys.U: if (shift) { key = 'U'; } else { key = 'u'; } return true;
                    case Keys.V: if (shift) { key = 'V'; } else { key = 'v'; } return true;
                    case Keys.W: if (shift) { key = 'W'; } else { key = 'w'; } return true;
                    case Keys.X: if (shift) { key = 'X'; } else { key = 'x'; } return true;
                    case Keys.Y: if (shift) { key = 'Y'; } else { key = 'y'; } return true;
                    case Keys.Z: if (shift) { key = 'Z'; } else { key = 'z'; } return true;

                    //Decimal keys
                    case Keys.D0: if (shift) { key = ')'; } else { key = '0'; } return true;
                    case Keys.D1: if (shift) { key = '!'; } else { key = '1'; } return true;
                    case Keys.D2: if (shift) { key = '@'; } else { key = '2'; } return true;
                    case Keys.D3: if (shift) { key = '#'; } else { key = '3'; } return true;
                    case Keys.D4: if (shift) { key = '$'; } else { key = '4'; } return true;
                    case Keys.D5: if (shift) { key = '%'; } else { key = '5'; } return true;
                    case Keys.D6: if (shift) { key = '^'; } else { key = '6'; } return true;
                    case Keys.D7: if (shift) { key = '&'; } else { key = '7'; } return true;
                    case Keys.D8: if (shift) { key = '*'; } else { key = '8'; } return true;
                    case Keys.D9: if (shift) { key = '('; } else { key = '9'; } return true;

                    //Decimal numpad keys
                    case Keys.NumPad0: key = '0'; return true;
                    case Keys.NumPad1: key = '1'; return true;
                    case Keys.NumPad2: key = '2'; return true;
                    case Keys.NumPad3: key = '3'; return true;
                    case Keys.NumPad4: key = '4'; return true;
                    case Keys.NumPad5: key = '5'; return true;
                    case Keys.NumPad6: key = '6'; return true;
                    case Keys.NumPad7: key = '7'; return true;
                    case Keys.NumPad8: key = '8'; return true;
                    case Keys.NumPad9: key = '9'; return true;

                    //Special keys
                    case Keys.OemTilde: if (shift) { key = '~'; } else { key = '`'; } return true;
                    case Keys.OemSemicolon: if (shift) { key = ':'; } else { key = ';'; } return true;
                    case Keys.OemQuotes: if (shift) { key = '"'; } else { key = '\''; } return true;
                    case Keys.OemQuestion: if (shift) { key = '?'; } else { key = '/'; } return true;
                    case Keys.OemPlus: if (shift) { key = '+'; } else { key = '='; } return true;
                    case Keys.OemPipe: if (shift) { key = '|'; } else { key = '\\'; } return true;
                    case Keys.OemPeriod: if (shift) { key = '>'; } else { key = '.'; } return true;
                    case Keys.OemOpenBrackets: if (shift) { key = '{'; } else { key = '['; } return true;
                    case Keys.OemCloseBrackets: if (shift) { key = '}'; } else { key = ']'; } return true;
                    case Keys.OemMinus: if (shift) { key = '_'; } else { key = '-'; } return true;
                    case Keys.OemComma: if (shift) { key = '<'; } else { key = ','; } return true;
                    case Keys.Space: key = ' '; return true;
                }
            }

            key = (char)0;
            return false;
        }
        public static Color[] GetColors(Bitmap source, bool unique = true)
        {
            if (unique)
            {
                HashSet<Color> uniqueColors = new HashSet<Color>();
                for (int y = 0; y < source.Height; y++)
                    for (int x = 0; x < source.Width; x++)
                        uniqueColors.Add(source.GetPixel(x, y).ToColor());
                return uniqueColors.ToArray();
            }
            else
            {
                List<Color> colorsList = new List<Color>();
                for (int y = 0; y < source.Height; y++)
                    for (int x = 0; x < source.Width; x++)
                        colorsList.Add(source.GetPixel(x, y).ToColor());
                return colorsList.ToArray();
            }
        }
        public static Microsoft.Xna.Framework.Rectangle ToRectangle(this RectangleF source)
        {
            return new Rectangle(source.Location.ToPoint(), source.Size.ToPointF().ToPoint());
        }
        //public static RectangleF ToRectangleF(this Rectangle source)
        //{
        //    return new RectangleF(source.Location.ToVector2().ToPointF(), source.Size.ToSizeF());
        //}
        //public static Point ToPoint(this Vector2 v)
        //{
        //    return new Point((int)v.X, (int)v.Y);
        //}
        public static string ToHex(this System.Drawing.Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
        public static string ToHex(this Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
        public static Rectangle OffsetPos(this Rectangle inRectanle, Point offset) => ChangePos(inRectanle, inRectanle.Location + offset);
        public static Rectangle ChangePos(this Rectangle inRectanle, Point newPos)
        {
            return new Rectangle(newPos, inRectanle.Size);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns>0 if none of the main buttons are pressed, 1 if left and -1 if right. if both are pressed 0 is returneed</returns>
        public static int GetMouseStateCoefficient(this MouseState state)
        {
            int toret = 0;
            if (state.LeftButton == ButtonState.Pressed) toret++;
            if (state.RightButton == ButtonState.Pressed) toret--;
            return toret;
        }
        public static bool IsMouseClicked(MouseState current, MouseState previous) => previous.LeftButton == ButtonState.Released && current.LeftButton == ButtonState.Pressed;
        public static Line[] ToLines(this Rectangle rectangle)
        {
            Line[] lines = new Line[4];

            Vector2 topLeft = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 topRight = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 bottomLeft = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 bottomRight = new Vector2(rectangle.Right, rectangle.Bottom);

            lines[0] = new Line(topLeft, bottomLeft);       //left
            lines[1] = new Line(topLeft, topRight);         //top
            lines[2] = new Line(topRight, bottomRight);     //right
            lines[3] = new Line(bottomLeft, bottomRight);   //bottom

            //DO NOT CHANGE ORDER EVER

            return lines;
        }
        public static bool SamePrintAs(this Rectangle first, Rectangle second)
        {
            return first.Height == second.Height && first.Width == second.Width;
        }
        public static bool SamePrintAs(this Texture2D firstTexture, Texture2D secondTexture) => firstTexture.Bounds.SamePrintAs(secondTexture.Bounds);
        public static Texture2D AsFlipped(Texture2D source, bool vertical, bool horizontal) //thx stack
        {
            Texture2D flipped = new Texture2D(source.GraphicsDevice, source.Width, source.Height);
            Color[] data = new Color[source.Width * source.Height];
            Color[] flippedData = new Color[data.Length];

            source.GetData(data);
            for (int x = 0; x < source.Width; x++)
                for (int y = 0; y < source.Height; y++)
                    flippedData[x + y * source.Width] = data[(horizontal ? source.Width - 1 - x : x) + ((vertical ? source.Height - 1 - y : y) * source.Width)];
            flipped.SetData(flippedData);

            return flipped;
        }
        public static Vector2 GetRandomVector(Vector2? max = null, Vector2? minSize = null)
        {
            max ??= new Vector2(float.MaxValue, float.MaxValue);
            minSize ??= Vector2.Zero;
            Random rnd = new Random();
            return new Vector2(rnd.Next((int)minSize.Value.X, (int)max.Value.X), rnd.Next((int)minSize.Value.Y, (int)max.Value.Y));
        }
        public static Texture2D GetPixel(GraphicsDevice graphics, string name = "pixel")
        {
            Texture2D pixel = new Texture2D(graphics, 1, 1);
            pixel.SetData(new Color[] { Color.White });
            pixel.Name = name;
            return pixel;
        }
    }
    public static class Centering
    {
        public static Vector2 TopLeft(Texture2D texture, Vector2 location, Vector2 scaling) => location;
        public static Vector2 TopRight(Texture2D texture, Vector2 location, Vector2 scaling) => location + new Vector2(-(texture.Width * scaling.X), 0);
        public static Vector2 BottomLeft(Texture2D texture, Vector2 location, Vector2 scaling) => location + new Vector2(0, -(texture.Height * scaling.Y));
        public static Vector2 BottomRight(Texture2D texture, Vector2 location, Vector2 scaling) => location + new Vector2(-(texture.Width * scaling.X), -(texture.Height * scaling.Y));

        public static Vector2 MiddleX(Texture2D texture, float y, float inX, Vector2 scaling) => MiddleX(texture.Width, y, inX, scaling);
        public static Vector2 MiddleX(int boxX, float y, float inX, Vector2 scaling) => new Vector2(inX * 0.5f - boxX * scaling.X * 0.5f, y);

        public static Vector2 MiddleY(Texture2D texture, float x, float inY, Vector2 scaling) => new Vector2(x, inY * 0.5f - texture.Height * scaling.Y * 0.5f);
        public static Vector2 MiddleY(int boxY, float x, float inY, Vector2 scaling) => new Vector2(x, inY * 0.5f - boxY * scaling.Y * 0.5f);

        public static Vector2 MiddleXY(Rectangle tocenter, Rectangle inXY, Vector2 scaling)
        {
            float x = MiddleX(tocenter.Width, 0, inXY.Width, scaling).X;
            float y = MiddleY(tocenter.Height, 0, inXY.Height, scaling).Y;
            return new Vector2(x, y) + inXY.Location.ToVector2();
        }
        public static Vector2 MiddleXY(Texture2D texture, Rectangle inXY, Vector2 scaling) => MiddleXY(texture.Bounds, inXY, scaling);
        public static Vector2 MiddleXY(Point dimensions, Rectangle inXY, Vector2 scaling) => MiddleXY(new Rectangle(Point.Zero, dimensions), inXY, scaling);

        public static Vector2 Get(Rectangle rectangle) => rectangle.Center.ToVector2();
        public static Vector2 Get(Texture2D texture) => texture.Bounds.Center.ToVector2();

        public static void OnPixel(ref Vector2 pos) => pos = pos.ToPoint().ToVector2();
    }
    public struct Line
    {
        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }
        public Line(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }
        public void Offset(Vector2 amount)
        {
            throw new NotImplementedException();
        }
        public bool IsNear(Vector2 vector, int threshold = 10)
        {
            Vector2 lineDirection = End - Start;
            float projectionLength = Vector2.Dot(vector - Start, lineDirection) / lineDirection.Length();

            if (projectionLength < 0) projectionLength = 0;
            else if (projectionLength > lineDirection.Length()) projectionLength = lineDirection.Length();

            return Vector2.Distance(vector, Start + projectionLength * Vector2.Normalize(lineDirection)) <= threshold;
        }
    }
    public enum Orgin
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }
}
