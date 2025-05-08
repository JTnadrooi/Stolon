using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsitLib.XNA;
using Betwixt;
using MonoGame.Extended;
using static Stolon.StolonGame;
using static Stolon.UIElement;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using RectangleF = MonoGame.Extended.RectangleF;
using static Stolon.UserInterface;
using AsitLib;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Xna.Framework.Media;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Text.RegularExpressions;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Reprecents a button or textplane in the UI. Add new elements to the <see cref="UserInterface"/> using the <see cref="UserInterface.AddElement(UIElement)"/> method.
    /// </summary>
    public class UIElement
    {
        /// <summary>
        /// What type the <see cref="UIElement"/> is. When <see cref="Listen"/>, "collisions" with the mouse will be calculated for its hitbox.
        /// </summary>
        public enum UIElementType
        {
            /// <summary>
            /// Its relevant when this <see cref="UIElement"/> gets clicked.
            /// </summary>
            Listen,
            /// <summary>
            /// Its not relevant when this <see cref="UIElement"/> gets clicked.
            /// </summary>
            Ignore,
        }
        public bool IsTop => ChildOf == topId;
        public string ClickSoundID { get; }
        public CachedAudio ClickSound => AudioEngine.AudioLibrary[ClickSoundID];
        public const string topId = "_";
        /// <summary>
        /// The type of the <see cref="UIElement"/>.
        /// </summary>
        public UIElementType Type { get; }
        /// <summary>
        /// The text in this <see cref="UIElement"/>.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The ID of the <see cref="UIElement"/>.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// The order of this <see cref="UIElement"/>. From top to bottom. Yet to be implemented.
        /// </summary>
        public string? Order { get; }
        /// <summary>
        /// The <see cref="UIElement.Id"/> of the <see cref="UIElement"/> this is a child of. 
        /// </summary>
        public string ChildOf { get; }

        public object?[] DrawArguments { get; }

        public UIElement(string id, string childOf, string? text = null, UIElementType type = UIElementType.Listen, string? order = null, string? clickSoundId = null, params object?[] drawArgs)
        {
            Text = text ?? id;
            Type = type;
            Id = id;
            Order = order;
            ChildOf = childOf;
            DrawArguments = drawArgs;
            ClickSoundID = clickSoundId ?? "select3";
        }
        /// <summary>
        /// Get the bounds of a <see cref="UIElement"/>, this also is its hitbox.
        /// </summary>
        /// <param name="elementPos">The position of the <see cref="UIElement"/>.</param>
        /// <param name="fontDimensions">The dimentions of the used font. <i>See: <see cref="StolonEnvironment.FontDimensions"/>.</i></param>
        /// <param name="clearance">The clearance between the text and the bounds. <i>(Or margin for the CSS enjoyers)</i></param>
        /// <param name="supportMultiline"></param>
        /// <param name="fontId"></param>
        /// <param name="posOffsetX"></param>
        /// <param name="posOffsetY"></param>
        /// <returns>The bounds of a <see cref="UIElement"/>.</returns>
        public Rectangle GetBounds(Point elementPos, Point fontDimensions, int clearance = DefaultRectangleClearance, bool supportMultiline = false, string fontId = "", int posOffsetX = 0, int posOffsetY = 0)
            => GetBounds(elementPos, Text, fontDimensions, clearance, supportMultiline, fontId, posOffsetX, posOffsetY);

        public static UIPath GetSelfPath(string id)
        {
            IEnumerable<string> GetListPath(string idForSearch)
                => idForSearch == topId ? idForSearch.ToSingleArray() : GetListPath(Instance.Environment.UI.UIElements[idForSearch].ChildOf).Concat(idForSearch.ToSingleArray());
            return new UIPath(GetListPath(id).ToArray()[1..]);
        }
        public static UIPath GetParentPath(string id) => new UIPath(GetSelfPath(id).Segments.ToArray()[..^1]);
        /// <summary>
        /// Get the bounds of a <see cref="UIElement"/>, this also is its hitbox.
        /// </summary>
        /// <param name="elementPos">The position of the <see cref="UIElement"/>.</param>
        /// <param name="fontDimensions">The dimentions of the used font. <i>See: <see cref="StolonEnvironment.FontDimensions"/>.</i></param>
        /// <param name="clearance">The clearance between the text and the bounds. <i>(Or padding for the CSS enjoyers)</i></param>
        /// <param name="supportMultiline"></param>
        /// <param name="fontId"></param>
        /// <param name="posOffsetX"></param>
        /// <param name="posOffsetY"></param>
        /// <returns>The bounds of a <see cref="UIElement"/>.</returns>
        public static Rectangle GetBounds(Point elementPos, string text, Point fontDimensions, int clearance = DefaultRectangleClearance, bool supportMultiline = false, string fontId = "", int posOffsetX = 0, int posOffsetY = 0)
        {
            Point offset = new Point(posOffsetX, posOffsetY) + fontId switch
            {
                _ => new Point(-0, -1),
            };

            Point boundsPos = elementPos + new Point(-clearance, -clearance) + offset;

            int rectangeSizeX = fontDimensions.X * text.Length + clearance * 2;
            int rectangeSizeY = fontDimensions.Y + clearance * 2;

            return new Rectangle(boundsPos, new Point(rectangeSizeX, rectangeSizeY));
        }


        public override string ToString()
        {
            return "{" + $"Id={Id}, Type={Type}, Text={Text}, Order={Order}, ChildOf={ChildOf}" + "}";
        }

        public const int DefaultRectangleClearance = 2;
        public const int LongDefaultRectangleClearance = 5; // might delete later
    }
    /// <summary>
    /// The data element relevant for draw methods.
    /// </summary>
    public struct UIElementDrawData
    {
        /// <summary>
        /// The position of the "drawdataified" <see cref="UIElement"/>.
        /// </summary>
        public Vector2 Position { get; }
        /// <summary>
        /// If a bounding <see cref="RectangleF"/> must be drawn.
        /// </summary>
        public bool DrawRectangle { get; }
        /// <summary>
        /// The bounding rectangle to draw.
        /// </summary>
        public RectangleF Rectangle { get; }
        /// <summary>
        /// The type of the <see cref="UIElement"/>. Sometimes relevant for drawing.
        /// </summary>
        public UIElementType Type { get; }
        /// <summary>
        /// The text to draw inside the <see cref="Rectangle"/>.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// The <see cref="UIElement.Id"/> of the source <see cref="UIElement"/>.
        /// </summary>
        public string Id { get; }
        public string FontName { get; }
        /// <summary>
        /// Create a new <see cref="UIElementDrawData"/> object.
        /// </summary>
        /// <param name="sourceId">The source <see cref="UIElement.Id"/>.</param>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <param name="rectangle"></param>
        /// <param name="drawRectangle"></param>
        public UIElementDrawData(string sourceId, string text, string fontName, UIElementType type, Vector2 position, RectangleF rectangle, bool drawRectangle)
        {
            Position = position;
            Type = type;
            Text = text;
            Rectangle = rectangle;
            DrawRectangle = drawRectangle;
            Id = sourceId;
            FontName = fontName;
        }
        public override string ToString()
        {
            return "{pos: " + Position + ", text: " + Text + ", rectangle: " + Rectangle + "}";
        }
    }
    /// <summary>
    /// The data element relevant for update methods. <i>(Knowing when an <see cref="UIElement"/> is clicked.)</i>
    /// </summary>
    public struct UIElementUpdateData
    {
        /// <summary>
        /// A value indicating if the source <see cref="UIElement"/> is hovered by the mouse.
        /// </summary>
        public bool IsHovered { get; }
        /// <summary>
        /// A value indicating if the source <see cref="UIElement"/> is pressed by the mouse.
        /// </summary>
        public bool IsPressed => IsHovered && SLMouse.CurrentState.LeftButton == ButtonState.Pressed;
        /// <summary>
        /// A value indicating if the source <see cref="UIElement"/> is clicked by the mouse.
        /// </summary>
        public bool IsClicked => IsPressed && SLMouse.PreviousState.LeftButton == ButtonState.Released;
        /// <summary>
        /// The <see cref="UIElement.Id"/> of the source <see cref="UIElement"/>.
        /// </summary>
        public string SourcID { get; }
        public string ClickSoundID => Instance.UserInterface.UIElements[SourcID].ClickSoundID;
        public CachedAudio ClickSound => AudioEngine.AudioLibrary[ClickSoundID];

        /// <summary>
        /// Create a new <see cref="UIElementUpdateData"/> with the propeties <see cref="IsHovered"/> and <see cref="SourcID"/> set.
        /// </summary>
        /// <param name="isHovered">A value indicating if the source <see cref="UIElement"/> is hovered by the mouse.</param>
        /// <param name="sourcID">The <see cref="UIElement.Id"/> of the source <see cref="UIElement"/>.</param>
        public UIElementUpdateData(bool isHovered, string sourcID)
        {
            IsHovered = isHovered;
            SourcID = sourcID;
        }
    }
}
