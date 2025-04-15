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
        public (UIElementDrawData drawData, bool isHovered) GetElementDrawData(UIElement element, Vector2 UIOrgin, int index)
        {
            Vector2 elementPos = Centering.MiddleX(
                                Instance.Environment.FontDimensions.X * element.Text.Length,
                                index * (Instance.Environment.FontDimensions.Y + 2) * 1.5f + UIOrgin.Y,
                                Instance.VirtualDimensions.X, Vector2.One);

            Centering.OnPixel(ref elementPos);

            Rectangle elementBounds = new Rectangle(elementPos.ToPoint(), new Point(Instance.Environment.FontDimensions.X * element.Text.Length, Instance.Environment.FontDimensions.Y));
            string elementText = element.Text;

            string postPre = element.Id switch
            {
                "quit" => "x",
                "specialThanks" => "!",
                _ => ">",
            };
            bool elementIsHovered = elementBounds.Contains(SLMouse.VirualPosition);

            return (new UIElementDrawData(element.Id, elementIsHovered ? (postPre + " " + elementText + " " + postPre.Replace(">", "<")) : elementText, element.Type, elementPos + (elementIsHovered ? new Point(-Instance.Environment.FontDimensions.X * 2, 0) : Point.Zero).ToVector2(), Rectangle.Empty, false), elementIsHovered);
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
            //if (!path.HasValue)
            //{
            //    Order(uIElements, topParentID, drawDump, updateDump, uiOrgin, orderProvider, isMouseRelevant);
            //    return;
            //}

            //UIPath path;
            //var temp = openPaths.Where(p => p.TopID == topParentID);
            //if (temp.Count() > 1) throw new Exception();
            //else path = temp.First();

            Order(uIElements, path.UIElementID, drawDump, updateDump, uiOrgin, orderProvider, isMouseRelevant);
        }
        public static void Order(UIElement[] uIElements, string parentID, ICollection<UIElementDrawData> drawDump, IDictionary<string, UIElementUpdateData> updateDump,
            Vector2 uiOrgin, IOrderProvider orderProvider, bool isMouseRelevant = true)
        {
            uIElements = uIElements.Where(e => e.ChildOf == parentID).ToArray(); // slow
            for (int i = 0; i < uIElements.Length; i++)
            {
                //if (uIElements[i].ChildOf != parentID) continue; // works, this does not?
                var ret = orderProvider.GetElementDrawData(uIElements[i], uiOrgin, i);
                updateDump[uIElements[i].Id] = new UIElementUpdateData(ret.isHovered && isMouseRelevant, uIElements[i].Id);
                drawDump.Add(ret.drawData);
            }
        }
    }
}
