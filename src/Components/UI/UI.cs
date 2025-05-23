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
using static Stolon.StolonGame;
using static Stolon.UIElement;
using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// The user interface for the <see cref="StolonEnvironment"/>.
    /// </summary>
    public class UserInterface : GameComponent
    {
        private StolonEnvironment _environment;

        private List<UIElementDrawData> _drawData;
        public List<UIElementDrawData> DrawData => _drawData;

        #region boardUIvariables

        private int _lineX1;
        private int _lineX2;
        private float _lineOffset;
        public const int LINE_WIDTH = 2;
        private float _uiLeftOffset;
        private float _uiRightOffset;


        private Dictionary<string, UIElement> _AllUIElements;
        private Dictionary<string, UIElementUpdateData> _updateData;

        private Texture2D _mouseClickFillElementTexture;
        private Rectangle _mouseClickFillElementBounds;
        private float _mouseClickElementBoundsCoefficient;

        #endregion
        #region dialogue variables



        #endregion
        #region public properties

        /// <summary>
        /// The virtual X coordiantes of the first line (from left to right).
        /// </summary>
        public float Line1X => _lineX1;
        /// <summary>
        /// The virtual X coordiantes of the second line (from left to right).
        /// </summary>
        public float Line2X => _lineX2;

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing all the <see cref="UIElementDrawData"/> objects from all the <see cref="UIElement"/> objects refreched AFTER the UI update.
        /// </summary>
        public Dictionary<string, UIElementUpdateData> UIElementUpdateData => _updateData;
        /// <summary>
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing all <see cref="UIElement"/> added via the <see cref="AddElement(UIElement)"/> method.
        /// </summary>
        public ReadOnlyDictionary<string, UIElement> UIElements => new ReadOnlyDictionary<string, UIElement>(_AllUIElements);

        #endregion

        public const string TITLE_PARENT_ID = "titleParent";
        private Textframe _textframe;

        /// <summary>
        /// The <see cref="Stolon.Textframe"/> managed by the <see cref="UserInterface"/>.
        /// </summary>
        public Textframe Textframe => _textframe;

        /// <summary>
        /// The width of a <see cref="UserInterface"/> line. <i>(Why did I make this public again?)</i>
        /// </summary>
        public int LineWidth => LINE_WIDTH;

        public UIPath MenuPath { get; set; }
        /// <summary>
        /// Main UIInterface contructor.
        /// </summary>
        public UserInterface() : base(Instance.Environment)
        {
            string CamelCase(string s)
            {
                string x = s.Replace("_", "");
                if (x.Length == 0) return "null";
                x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
                    m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
                return char.ToLower(x[0]) + x.Substring(1);
            }
            Instance.DebugStream.Log(">[s]contructing stolon ui");

            _environment = Instance.Environment;
            _textframe = new Textframe(this);

            _lineOffset = 192f;

            Instance.DebugStream.Log(">loading audio");
            foreach (string filePath in Directory.GetFiles("audio", "*.wav", SearchOption.AllDirectories))
            {
                string fileName = CamelCase(Path.GetFileNameWithoutExtension(filePath).Replace(" ", string.Empty));
                AudioEngine.AudioLibrary.Add(fileName, new CachedAudio(filePath, fileName));
                Instance.DebugStream.Log("loaded audio with id: " + fileName);
            }
            Instance.DebugStream.Success();


            _AllUIElements = new Dictionary<string, UIElement>();

            _drawData = new List<UIElementDrawData>();

            _updateData = new Dictionary<string, UIElementUpdateData>();
            _mouseClickFillElementBounds = new Rectangle();

            _mouseClickFillElementTexture = new GameTexture(TexturePalette.Empty, new Texture2D(Instance.GraphicsDevice, 1, 1));
            ((Texture2D)_mouseClickFillElementTexture).SetData(new Color[] { Color.White });

            Instance.DebugStream.Success();
        }
        public void Initialize()
        {
            StolonGame.Instance.DebugStream.Log(">[s]initializing ui..");
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

            StolonGame.Instance.DebugStream.Log(">autogenerating _back_ buttons");
            HashSet<string> parentIds = GetParentIDs();
            foreach (string id in parentIds) AddElement(new UIElement("_back_" + id, id, "Back", UIElementType.Listen));
            Instance.DebugStream.Success();
            Instance.DebugStream.Success();
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
                        AudioEngine.Audio.Play(_updateData[item].ClickSound);
                    if (_updateData.TryGetValue("_back_" + item, out UIElementUpdateData updateData2))
                        if (updateData2.IsClicked) MenuPath = UIElement.GetParentPath(item);
                }
        }
        private void UpdateBoardUI(int elapsedMiliseconds)
        {
            float zoomIntensity = ((BoardGameState)Instance.Environment.GameStateManager.Current).Board.ZoomIntensity;
            float lineZoomOffset = zoomIntensity * 30f * (zoomIntensity < 0 ? 0.5f : 1f); // 30 being the max zoom in pixels, the last bit is smoothening the inverted zoom.

            lineZoomOffset = Math.Max(0, lineZoomOffset);

            bool mouseIsOnUI = SLMouse.Domain == SLMouse.MouseDomain.UserInterfaceLow;

            _uiLeftOffset = -lineZoomOffset;
            _uiRightOffset = lineZoomOffset;

            _lineX1 = (int)(_lineOffset + _uiLeftOffset);
            _lineX2 = (int)(Instance.VirtualDimensions.X - _lineOffset + _uiRightOffset);
        }
        //public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            string id = Instance.Environment.GameStateManager.Current.GetID();
            if (id == GameStateHelpers.GetID<BoardGameState>())
            {

                spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle(Point.Zero, new Point((int)_lineX1, 500)), Color.Black);
                spriteBatch.DrawLine(_lineX1, -10f, _lineX1, 500f, Color.White, LINE_WIDTH);
                spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle((int)_lineX2, 0, Instance.VirtualDimensions.X - (int)_lineX2, 500), Color.Black);
                spriteBatch.DrawLine(_lineX2, -10f, _lineX2, 500f, Color.White, LINE_WIDTH);

                if (_mouseClickElementBoundsCoefficient > 0.015f) spriteBatch.Draw(_mouseClickFillElementTexture, _mouseClickFillElementBounds, Color.White);
                else _mouseClickElementBoundsCoefficient = 0f; // I really shouldent be altering this in the Draw() method..

            }
            else if (id == GameStateHelpers.GetID<MenuGameState>())
            {
                
            }
            foreach (UIElementDrawData elementDrawData in _drawData)
            {
                spriteBatch.DrawString(Instance.Fonts[elementDrawData.FontName], elementDrawData.Text, elementDrawData.Position, Color.White, 0f, Vector2.Zero, Instance.Fonts[elementDrawData.FontName].Scale, SpriteEffects.None, 1f);
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
            Instance.DebugStream.Log("ui-element with id " + element.Id + " added.");
            //updateData.Add(element.Id, default);
        }
        /// <summary>
        /// Remove an <see cref="UIElement"/> from the <see cref="UserInterface"/>.
        /// </summary>
        /// <param name="elementID">The <see cref="UIElement.Id"/> of the <see cref="UIElement"/> to remove.</param>
        public void RemoveElement(string elementID)
        {
            _AllUIElements.Remove(elementID);
            Instance.DebugStream.Log("ui-element with id " + elementID + " removed.");
        }

        public static UserInterface UI => Instance.Environment.UI;
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
