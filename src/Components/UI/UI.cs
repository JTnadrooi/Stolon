using AsitLib;
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using static Stolon.UIElement;
using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// The user interface for the <see cref="GameEnvironment"/>.
    /// </summary>
    public class UserInterface : GameComponent
    {
        private List<UIElementDrawData> _drawData;
        public List<UIElementDrawData> DrawData => _drawData;

        public const int LINE_WIDTH = 2;
        private Dictionary<string, UIElement> _AllUIElements;
        private Dictionary<string, UIElementUpdateData> _updateData;

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing all the <see cref="UIElementDrawData"/> objects from all the <see cref="UIElement"/> objects refreched AFTER the UI update.
        /// </summary>
        public Dictionary<string, UIElementUpdateData> UIElementUpdateData => _updateData;
        /// <summary>
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing all <see cref="UIElement"/> added via the <see cref="AddElement(UIElement)"/> method.
        /// </summary>
        public ReadOnlyDictionary<string, UIElement> UIElements => new ReadOnlyDictionary<string, UIElement>(_AllUIElements);

        public const string TITLE_PARENT_ID = "titleParent";
        private Textframe _textframe;

        /// <summary>
        /// The <see cref="Stolon.Textframe"/> managed by the <see cref="UserInterface"/>.
        /// </summary>
        public Textframe Textframe => _textframe;

        /// <summary>
        /// The width of a <see cref="UserInterface"/> line.
        /// </summary>
        public int LineWidth => LINE_WIDTH;

        public UIPath MenuPath { get; set; }
        /// <summary>
        /// Main UIInterface contructor.
        /// </summary>
        public UserInterface() : base(STOLON.Environment)
        {
            string CamelCase(string s)
            {
                string x = s.Replace("_", "");
                if (x.Length == 0) return "null";
                x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
                    m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
                return char.ToLower(x[0]) + x.Substring(1);
            }
            STOLON.Debug.Log(">[s]contructing stolon ui");

            _textframe = new Textframe(this);

            STOLON.Debug.Log(">loading audio");
            foreach (string filePath in Directory.GetFiles("audio", "*.wav", SearchOption.AllDirectories))
            {
                string fileName = CamelCase(Path.GetFileNameWithoutExtension(filePath).Replace(" ", string.Empty));
                STOLON.Audio.Library.Add(fileName, new CachedAudio(filePath, fileName));
                STOLON.Debug.Log("loaded audio with id: " + fileName);
            }
            STOLON.Debug.Success();

            _AllUIElements = new Dictionary<string, UIElement>();
            _drawData = new List<UIElementDrawData>();
            _updateData = new Dictionary<string, UIElementUpdateData>();

            STOLON.Debug.Success();
        }
        public void Initialize()
        {
            STOLON.Debug.Log(">[s]initializing ui..");
            // top
            AddElement(new UIElement(TITLE_PARENT_ID, UIElement.TOP_ID, string.Empty, UIElementType.Listen));

            // board l
            //AddElement(new UIElement("exitGame", boardLeftParentId, "Exit Game", UIElementType.Listen));

            //AddElement(new UIElement("screenRegion2", boardLeftParentId, string.Empty, UIElementType.Ignore));
            //AddElement(new UIElement("screenRegion", boardLeftParentId, "Screen & Camera", UIElementType.Ignore));
            //AddElement(new UIElement("toggleFullscreen", boardLeftParentId, "Go Fullscreen", UIElementType.Listen));
            //AddElement(new UIElement("centerCamera", boardLeftParentId, "Center Camera", UIElementType.Listen));

            //AddElement(new UIElement("boardRegion2", boardLeftParentId, string.Empty, UIElementType.Ignore));
            //AddElement(new UIElement("boardRegion", boardLeftParentId, "Board", UIElementType.Ignore));
            //AddElement(new UIElement("undoMove", boardLeftParentId, "Undo", UIElementType.Listen));
            //AddElement(new UIElement("restartBoard", boardLeftParentId, "Restart", UIElementType.Listen));
            //AddElement(new UIElement("boardSearch", boardLeftParentId, "Search", UIElementType.Listen));
            //AddElement(new UIElement("skipMove", boardLeftParentId, "End Move", UIElementType.Listen));

            // board
            //AddElement(new UIElement("currentPlayer", boardRightParentId, null, UIElementType.Ignore));

            // main menu
            AddElement(new UIElement("startStory", TITLE_PARENT_ID, "Story", UIElementType.Listen, clickSoundId: "exit3"));
            AddElement(new UIElement("startCom", TITLE_PARENT_ID, "COM", UIElementType.Listen, clickSoundId: "coin4"));
            AddElement(new UIElement("startXp", TITLE_PARENT_ID, "2P", UIElementType.Listen, clickSoundId: "coin4"));
            AddElement(new UIElement("options", TITLE_PARENT_ID, "Options", UIElementType.Listen));
            AddElement(new UIElement("specialThanks", TITLE_PARENT_ID, "Special Thanks", UIElementType.Listen));
            AddElement(new UIElement("quit", TITLE_PARENT_ID, "Quit", UIElementType.Listen));

            // options
            AddElement(new UIElement("sound", "options", "Sound", UIElementType.Listen));
            AddElement(new UIElement("graphics", "options", "Graphics", UIElementType.Listen, clickSoundId: "exit3"));

            AddElement(new UIElement("volUp", "sound", "Volume UP", UIElementType.Listen));
            AddElement(new UIElement("volDown", "sound", "Volume DOWN", UIElementType.Listen));

            MenuPath = GetSelfPath(TITLE_PARENT_ID);

            STOLON.Debug.Log(">autogenerating _back_ buttons");
            HashSet<string> parentIds = GetParentIDs();
            foreach (string id in parentIds) AddElement(new UIElement("_back_" + id, id, "Back", UIElementType.Listen));
            STOLON.Debug.Success();
            STOLON.Debug.Success();
        }
        /// <summary>
        /// Clears both the updatedata and drawdata collections, making them ready to be repopulated by the methods in the <see cref="UIOrdering"/> class.<br/>
        /// <i>Does populate the updatedata collection with unhovered <see cref="UIElementDrawData"/> objects.</i>
        /// </summary>
        private void ResetElementData()
        {
            _updateData.Clear();
            foreach (UIElement uiElement in _AllUIElements.Values)
                _updateData.Add(uiElement.Id, new UIElementUpdateData(false, uiElement.Id));
            _drawData.Clear();
        }
        public HashSet<string> GetTopIDs() => UIElements.Values.Where(e => e.IsTop).Select(e => e.Id).ToHashSet();
        public HashSet<string> GetParentIDs()
        {
            var topIds = GetTopIDs();
            return UIElements.Values.WhereSelect(e => (e.ChildOf, !e.IsTop && !topIds.Contains(e.ChildOf))).ToHashSet();
        }
        public override void Update(int elapsedMiliseconds)
        {
            ResetElementData();

            _textframe.Update(elapsedMiliseconds);
        }
        public void PostUpdate(int elapsedMiliseconds)
        {
            foreach (string item in UIElements.Keys)
                if (UIElements[item].Type == UIElementType.Listen)
                {
                    if (_updateData[item].IsClicked)
                        STOLON.Audio.Play(_updateData[item].ClickSound);
                    if (_updateData.TryGetValue("_back_" + item, out UIElementUpdateData updateData2))
                        if (updateData2.IsClicked) MenuPath = UIElement.GetParentPath(item);
                }
        }
        //public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            string id = STOLON.Environment.GameStateManager.Current.GetID();
            foreach (UIElementDrawData elementDrawData in _drawData)
            {
                spriteBatch.DrawString(STOLON.Fonts[elementDrawData.FontName], elementDrawData.Text, elementDrawData.Position, Color.White, 0f, Vector2.Zero, STOLON.Fonts[elementDrawData.FontName].Scale, SpriteEffects.None, 1f);
                if (elementDrawData.DrawRectangle)
                {
                    spriteBatch.DrawRectangle(elementDrawData.Rectangle, Color.White, 1f);
                }
            }
            _textframe.Draw(spriteBatch, elapsedMiliseconds);
            base.Draw(spriteBatch, elapsedMiliseconds);
        }
        /// <summary>
        /// Add an element to the <see cref="UserInterface"/>.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to add.</param>
        public void AddElement(UIElement element)
        {
            _AllUIElements.Add(element.Id, element);
            STOLON.Debug.Log("ui-element with id " + element.Id + " added.");
            //updateData.Add(element.Id, default);
        }
        /// <summary>
        /// Remove an <see cref="UIElement"/> from the <see cref="UserInterface"/>.
        /// </summary>
        /// <param name="elementID">The <see cref="UIElement.Id"/> of the <see cref="UIElement"/> to remove.</param>
        public void RemoveElement(string elementID)
        {
            _AllUIElements.Remove(elementID);
            STOLON.Debug.Log("ui-element with id " + elementID + " removed.");
        }

        public static UserInterface UI => STOLON.Environment.UI;
    }

    public struct UIPath : IEnumerable<string>
    {
        public string TopID => segments[0];
        public string ParentID => segments[^1];
        public string UIElementID => segments.Last();
        public int Lenght => segments.Count;
        public ReadOnlyCollection<string> Segments => segments.AsReadOnly();
        private readonly List<string> segments;
        public UIPath(IEnumerable<string> segments)
        {
            this.segments = new List<string>(segments);
        }
        public string this[int index] => segments[index];
        public IEnumerator<string> GetEnumerator() => segments.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => "{" + segments.ToJoinedString(">") + "}";
        public override int GetHashCode() => segments.ToJoinedString(string.Empty).GetHashCode();
        public override bool Equals([NotNullWhen(true)] object? obj) => obj.GetHashCode() == GetHashCode();
    }

}
