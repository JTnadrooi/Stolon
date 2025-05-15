using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    /// A class to allow objects to state a way of ordering <see cref="UIElement"/> objects.
    /// </summary>
    public interface IOrderProvider
    {
        /// <summary>
        /// Cast a <see cref="UIElement"/> to a <see cref="UIElementDrawData"/> object.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to convert.</param>
        /// <param name="UIOrgin">The orgin of the drawing UI.</param>
        /// <param name="index">The index of the current <see cref="UIElement"/>.</param>
        /// <returns>A <see cref="Tuple{T1, T2}"/> holding both the newly created <see cref="UIElementDrawData"/> and a value indicating if the <see cref="UIElement"/> is hovered or not.</returns>
        public (UIElementDrawData drawData, bool isHovered) GetElementDrawData(UIElement element, Vector2 UIOrgin, int index);
    }

    /// <summary>
    /// The <see cref="IOrderProvider"/> that orders the main menu.
    /// </summary> 
    public class MenuOrderProvider : IOrderProvider
    {
        private GameFont _font;
        private bool _capitalise = true;
        public MenuOrderProvider()
        {
            //_font = Instance.Fonts["fonts\\smollerMono"];
            //_font = Instance.Fonts["fonts\\monogram"];
            _font = Instance.Fonts[MEDIUM_FONT_ID];
            //_font = Instance.Fonts["fonts\\fixedsys"];
        }
        public (UIElementDrawData drawData, bool isHovered) GetElementDrawData(UIElement element, Vector2 UIOrgin, int index)
        {
            Vector2 elementPos = Centering.MiddleX((int)_font.FastMeasure(element.Text).X,
                                index * (_font.Dimensions.Y * 2 + 2) + UIOrgin.Y,
                                Instance.VirtualDimensions.X, Vector2.One);
            Centering.OnPixel(ref elementPos);

            Rectangle elementBounds = new Rectangle(elementPos.ToPoint(), new Point((int)_font.FastMeasure(element.Text).X, (int)_font.Dimensions.Y));
            string elementText = element.Text;
            if(_capitalise) element.Text = element.Text.ToUpper();

            string postPre = element.Id switch
            {
                "quit" => "x",
                "specialThanks" => "!",
                _ => ">",
            };
            bool elementIsHovered = elementBounds.Contains(SLMouse.VirualPosition);

            return (new UIElementDrawData(element.Id, elementIsHovered 
                ? (postPre + " " + elementText + " " + postPre.Replace(">", "<")) 
                :  elementText, _font.Name, element.Type, elementPos + (elementIsHovered ? new Point(-(int)_font.FastMeasure(2).X, 0) : Point.Zero).ToVector2(), Rectangle.Empty, false), 
                elementIsHovered);
        }
    }

    /// <summary>
    /// A static list of the most common <see cref="IOrderProvider"/> objects.
    /// </summary>
    public static class OrderProviders
    {
        static OrderProviders()
        {
            Menu = new MenuOrderProvider();
        }
        /// <summary>
        /// The <see cref="IOrderProvider"/> that orders the main menu.
        /// </summary>
        public static IOrderProvider Menu { get; }
    }

    /// <summary>
    /// Provides methods for ordering <see cref="UIElement"/> objects. (Casting them to <see cref="UIElementDrawData"/> or/and <see cref="UIElementUpdateData"/>.
    /// </summary>
    public static class UIOrdering
    {
        public static void Order(UIElement[] uIElements, UIPath path, ICollection<UIElementDrawData> drawDump, IDictionary<string, UIElementUpdateData> updateDump,
            Vector2 uiOrgin, IOrderProvider orderProvider, bool isMouseRelevant = true)
        {
            Order(uIElements, path.UIElementID, drawDump, updateDump, uiOrgin, orderProvider, isMouseRelevant);
        }
        public static void Order(UIElement[] uIElements, string parentID, ICollection<UIElementDrawData> drawDump, IDictionary<string, UIElementUpdateData> updateDump,
            Vector2 uiOrgin, IOrderProvider orderProvider, bool isMouseRelevant = true)
        {
            int orderIndex = 0;
            for (int i = 0; i < uIElements.Length; i++)
            {
                UIElement element = uIElements[i];
                if (element.ChildOf != parentID) continue;

                var ret = orderProvider.GetElementDrawData(element, uiOrgin, orderIndex++);
                updateDump[element.Id] = new UIElementUpdateData(ret.isHovered && isMouseRelevant, element.Id);
                drawDump.Add(ret.drawData);
            }
        }
    }
}
