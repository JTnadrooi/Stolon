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

        #region menuVariables

        private Texture2D _menuLogoLines;
        private Texture2D _menuLogoDummyTiles;
        private Texture2D _menuLogoFilledTiles;
        private Texture2D _menuLogoLowResFonted;
        private Texture2D _dither32;

        private Rectangle _menuLogoTileHider;

        private bool _drawMenuLogoLines;
        private bool _drawMenuLogoDummyTiles;
        private bool _drawMenuLogoFilledTiles;
        private bool _drawMenuLogoLowResFonted;
        private int _menuLogoRowsHidden;
        private Rectangle _menuLogoBoundingBox;

        private Vector2 _menuLogoDrawPos;
        private int _milisecondsSinceStartup;

        private int _menuLogoFlashTime;
        private int? _menuFlashStart;
        private int? _menuFlashEnd;
        private int _menuLogoMilisecondsFlashing;
        private int _milisecondsSinceMenuRemoveStart;
        private bool _menuDone;

        private int _menuLine1X;
        private int _menuLine2X;
        private int _menuLineLenght;
        private int _menuLineWidth;

        private int _menuRemoveLineY;

        private bool _loadingFinished;

        private float _menuLogoScaling;

        private List<UIElement> _depthPath;

        private Point[] _menuDitherTexturePositions;

        private Tweener<float> _menuLogoEaseTweener;
        private Tweener<float> _menuRemoveTweener;


        private string[] _tips;
        private Vector2 _tipPos;
        private int _tipId;

        private const int MENU_LOGO_ROW_COUNT = 5;

        #endregion
        #region boardUIvariables

        private int _lineX1;
        private int _lineX2;
        private float _lineOffset;
        private int _lineWidth;
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
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing all the <see cref="UIElementDrawData"/> objects from all the <see cref="UIElement"/> objects refreched AFTER the UI update.
        /// </summary>
        public ReadOnlyDictionary<string, UIElementUpdateData> UIElementUpdateData => new ReadOnlyDictionary<string, UIElementUpdateData>(_updateData);
        /// <summary>
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing all <see cref="UIElement"/> added via the <see cref="AddElement(UIElement)"/> method.
        /// </summary>
        public ReadOnlyDictionary<string, UIElement> UIElements => new ReadOnlyDictionary<string, UIElement>(_AllUIElements);

        #endregion

        public const string TITLE_PARENT_ID = "titleParent";
        private Textframe _textframe;
        private Action? _onLeave;

        /// <summary>
        /// The <see cref="Stolon.Textframe"/> managed by the <see cref="UserInterface"/>.
        /// </summary>
        public Textframe Textframe => _textframe;

        /// <summary>
        /// The width of a <see cref="UserInterface"/> line. <i>(Why did I make this public again?)</i>
        /// </summary>
        public int LineWidth => _lineWidth;

        public UIPath MenuPath { get; set; }
        /// <summary>
        /// Main UIInterface contructor.
        /// </summary>
        public UserInterface() : base(Instance.Environment)
        {
            string camelCase(string s) // ill start camelcasing private-private functions
            {
                string x = s.Replace("_", "");
                if (x.Length == 0) return "null";
                x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
                    m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
                return char.ToLower(x[0]) + x.Substring(1);
            }
            string menuDataFolder = "menuLogoMid";
            Instance.DebugStream.Log(">[s]contructing stolon ui");

            _environment = Instance.Environment;

            _lineOffset = 96f;
            _lineWidth = 2;

            Instance.DebugStream.Log(">loading audio");
            foreach (string filePath in Directory.GetFiles("audio", "*.wav", SearchOption.AllDirectories))
            {
                string fileName = camelCase(Path.GetFileNameWithoutExtension(filePath).Replace(" ", string.Empty));
                AudioEngine.AudioLibrary.Add(fileName, new CachedAudio(filePath, fileName));
                Instance.DebugStream.Log("loaded audio with id: " + fileName);
            }
            Instance.DebugStream.Success();

            _menuLogoLines = Instance.Textures.GetReference("textures\\" + menuDataFolder+ "\\lines");
            _menuLogoDummyTiles = Instance.Textures.GetReference("textures\\" + menuDataFolder+ "\\dummyTiles");
            _menuLogoFilledTiles = Instance.Textures.GetReference("textures\\" + menuDataFolder+ "\\filledTiles");
            _menuLogoLowResFonted = Instance.Textures.GetReference("textures\\" + menuDataFolder+ "\\lowResFonted");
            _dither32 = Instance.Textures.GetReference("textures\\dither_32");

            _AllUIElements = new Dictionary<string, UIElement>();

            _drawData = new List<UIElementDrawData>();

            _updateData = new Dictionary<string, UIElementUpdateData>();
            _mouseClickFillElementBounds = new Rectangle();

            _mouseClickFillElementTexture = new GameTexture(TexturePalette.Empty, new Texture2D(Instance.GraphicsDevice, 1, 1));
            ((Texture2D)_mouseClickFillElementTexture).SetData(new Color[] { Color.White });


            _drawMenuLogoLines = true;
            _drawMenuLogoDummyTiles = true;
            _drawMenuLogoFilledTiles = false;
            _textframe = new Textframe(this);

            _menuLogoFlashTime = 0;
            _menuFlashStart = null;
            _menuLogoRowsHidden = 5;
            _menuDitherTexturePositions = Array.Empty<Point>();

            _depthPath = new List<UIElement>();

            _menuLogoScaling = 1f;

            _menuLogoEaseTweener = new Tweener<float>(0f, 1f, 2f, Ease.Quad.InOut);
            _menuRemoveTweener = new Tweener<float>(0f, 1f, 2f, Ease.Quad.InOut);

            _tips = new string[]
            {
                //"A Stolon is a line where both players cannot drop their tiles.", // to long
                "The center rows are most valueable.",
                "CENTER, ROWS, VALUABLE.",
                "The border rows are most respectable.",
                "They are stingers.",
                "Reality is overrated.",
                "Stolons deadline has always been 2025.",
                "If you listen very closely you can hear the main theme.",
                "If you listen very closely you can hear the sound effects.",
                "Listed twice.",
                "KEES NOOOOOOOO",
                "That definitely something Vox would say.",
                "Inity waits patiently..",
                "Super colliding..",
                "Teaching garden chairs how to fly..",
                "\"Is that an ability or a program?\"",
                "Oh dear..",
                "Goldsilk hates the player.",
                "This week.",
                "Good luck.",
                "Good luck!",
                "Good luck!!",
                "This is a fake loading screen.",
                "This is a real loading screen.",
                "For Them, Light.",
                "Can you read this?",
                "CAN YOU READ THIS?",
                "POWER SURGING!",
                "Galore!",
                "The start of the unending.",
                "There,",
                "No shaders?",
                "All colors, Her.",
                "Thanks for playing! :D",
                "\"Call that a Natural Deadline.\"",
                "Nue not included!",
                "Assembling the Pharos..",
                "Fishing update when?",
                "\"What even is a Stolon?\"",
                "The Sun is gone..",
                "Comparing chaos to disorder..",
                "Luck good.",
                "The chance of getting this message is quite low.",
                "Fax as in the machine.",
                "Self proclaimed?.",
                "The Musical",
                "Why is Lanulox here..",
                "Time's Up! Fate sealed.",
                "Seems vacant..",
                "You are week, I am month.",
                "Lanu Lanu Lanu La-",
                "Welcome.",
                "Welcome!",
                "Galore.",
                "NOT solved.",
                "NOT CLUELESS!",
                "Tiory?",
                "27 Compile errors..?",
                "Simply Rendering,",
                "Behold, The \"Sky Train\"!",
                "dot hat :drool:",
                "Cherry-pilled!",
                "The stolons brace themselfs..",
                "Potatofruit?",
                "A reality loved by many, hated by more.",
                "VWS cares not.",
                "Eeeeh maji? Easy modo???",
                "Sto owes someone 5 dollars.",
                "\"Souls are overrated but quite underused.\"",
                "\"I-I don't quite understand..\"",
                "1bit!",
                "haha",
                "elevenhundredthousand.",
                "The comfort of finity.",
                "Pressure discrepancy detected - reversing airflow.",
                "*Delicieuxmiel!*",
                "REV UP THE CLOUDS!",
                "Headpatted with ease.",
                ":LOVINGSTARE:",
                ":STARE:",
                "Collida past 3.",
                "That translates to \"flour\".",
                "Index is jealous.",
            };

            _tipId = new Random().Next(0, _tips.Length);
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
            switch (Instance.Environment.GameState)
            {
                case StolonEnvironment.SLGameState.OpenBoard:
                    UpdateBoardUI(elapsedMiliseconds);
                    break;
                case StolonEnvironment.SLGameState.InMenu:
                    UpdateMenuUI(elapsedMiliseconds);
                    break;
                case StolonEnvironment.SLGameState.Loading:
                    break;
            }

            foreach (string item in UIElements.Keys)
            {
                if (UIElements[item].Type == UIElementType.Listen)
                {
                    if (_updateData[item].IsClicked)
                    {
                        AudioEngine.Audio.Play(_updateData[item].ClickSound);
                    }
                    if (_updateData.TryGetValue("_back_" + item, out UIElementUpdateData updateData2))
                    {
                        if (updateData2.IsClicked)
                        {
                            MenuPath = UIElement.GetParentPath(item);
                            //Console.WriteLine(GetSelfPath(item));
                        }
                    }
                }
            }
            
            base.Update(elapsedMiliseconds);
        }
        private void UpdateMenuUI(int elapsedMiliseconds)
        {

            int rowHeight = (int)(_menuLogoLines.Height / (float)MENU_LOGO_ROW_COUNT);
            float menuRemoveTweenerOffset = 300f * _menuRemoveTweener.Value;
            int lineFromMid = (int)(170f + menuRemoveTweenerOffset);
            bool menuFlashEnded = _milisecondsSinceStartup > _menuFlashEnd;
            int uiElementOffsetY = (int)(230f + menuRemoveTweenerOffset);
            int logoYoffset = 30;
            int menuLogoBoundingBoxClearing = 8;

            // if (milisecondsSinceStartup < 10000) // to skip start button click and animation
            //{
            //    milisecondsSinceStartup = 10001;
            //    menuDone = true;
            //    menuRemoveTweener.Update(10);

            //    Scene.MainInstance.SetBoard(new Player[]
            //            {
            //                new Player("player0"),
            //                new Player("player1"),
            //            });
            //    Leave();
            //    //startFrame = true;
            //}

            #region inFlash
            _menuLogoTileHider = new Rectangle(_menuLogoDrawPos.ToPoint(), new Point((int)(_menuLogoLines.Width * _menuLogoScaling), (int)(rowHeight * _menuLogoRowsHidden)));
            _milisecondsSinceStartup += elapsedMiliseconds;
            _menuLogoDrawPos = Vector2.Round(Centering.MiddleX(_menuLogoLines, logoYoffset, Instance.VirtualDimensions.X, Vector2.One) + new Vector2(0, 8f * _menuLogoEaseTweener.Value * (1 - _menuRemoveTweener.Value)))
                + new Vector2(0, ((Centering.MiddleY(_menuLogoLines, 1, Instance.VirtualDimensions.Y, Vector2.One).Y - logoYoffset * 1.5f) * _menuRemoveTweener.Value));
            _menuDitherTexturePositions = new Point[(int)Math.Ceiling(Instance.VirtualDimensions.Y / (float)_dither32.Height) * 2];

            _menuFlashStart = 1200;
            _menuFlashEnd = _menuFlashStart + 400;

            _menuLine1X = (int)(Instance.VirtualDimensions.X / 2f) - lineFromMid;
            _menuLine2X = (int)(Instance.VirtualDimensions.X / 2f) + lineFromMid;

            _menuLineLenght = _drawMenuLogoFilledTiles ? StolonGame.Instance.VirtualDimensions.Y : 0;
            _menuLineWidth = 2 + (menuFlashEnded ? 2 : 0);

            if (_milisecondsSinceStartup > 300) _menuLogoRowsHidden = 4;
            if (_milisecondsSinceStartup > 600) _menuLogoRowsHidden = 3;
            if (_milisecondsSinceStartup > 800) _menuLogoRowsHidden = 2;
            if (_milisecondsSinceStartup > 1000) _menuLogoRowsHidden = 1;
            if (_milisecondsSinceStartup > _menuFlashStart) _menuLogoRowsHidden = 0;

            if (_menuLogoFlashTime > 0)
            {
                _menuLogoMilisecondsFlashing += elapsedMiliseconds;
                if (_menuLogoMilisecondsFlashing > _menuLogoFlashTime)
                {
                    _drawMenuLogoFilledTiles = !_drawMenuLogoFilledTiles;
                    _menuLogoMilisecondsFlashing = 0;
                }
            }

            if (!_menuFlashStart.HasValue) return; // code below only relevant when the dummy board show animation ended.

            if (_milisecondsSinceStartup > _menuFlashStart.Value) _menuLogoFlashTime = 120;
            if (_milisecondsSinceStartup > _menuFlashStart.Value + 200) _menuLogoFlashTime = 100;
            if (_milisecondsSinceStartup > _menuFlashStart.Value + 300) _menuLogoFlashTime = 75;
            if (_milisecondsSinceStartup > _menuFlashStart.Value + 350) _menuLogoFlashTime = 60;
            if (_milisecondsSinceStartup < _menuFlashEnd) return; // code below only relevant when the full animation ended.

            #endregion
            #region inMenu
            _drawMenuLogoLowResFonted = true;
            _drawMenuLogoDummyTiles = true;
            _drawMenuLogoFilledTiles = true;
            _drawMenuLogoLines = true;

            _menuLogoFlashTime = 0; // ensures disabled flashing.
            _menuLogoEaseTweener.Update(elapsedMiliseconds / 1000f); // update the tweener.
            if (_menuLogoEaseTweener.Value == 1 || _menuLogoEaseTweener.Value == 0) // reverse and restart if finished.
            {
                _menuLogoEaseTweener.Reverse();
                _menuLogoEaseTweener.Start();
                Instance.DebugStream.Log("reversed icon tweener.");
            }
            _menuLogoBoundingBox =
                new Rectangle(_menuLogoDrawPos.ToPoint() + new Point(-menuLogoBoundingBoxClearing), _menuLogoLines.Bounds.Size + new Point(menuLogoBoundingBoxClearing * 2));

            for (int i = 0; i < _menuDitherTexturePositions.Length; i++) // dithering positions.
                _menuDitherTexturePositions[i] = new Point(
                        (i >= _menuDitherTexturePositions.Length / 2f) ? _menuLine2X : _menuLine1X - _dither32.Width,
                        (i % (int)(_menuDitherTexturePositions.Length / 2f)) * _dither32.Height);

            UIOrdering.Order(_AllUIElements.Values.ToArray(), MenuPath, _drawData, _updateData, new Vector2(0, uiElementOffsetY), OrderProviders.Menu);

            if (_updateData["startXp"].IsClicked)
            {
                Scene.MainInstance.SetBoard(new Player[]
                        {
                            new Player("player0"),
                            new Player("player1"),
                        });

                Leave();
            }
            if (_updateData["options"].IsClicked)
            {
                MenuPath = UIElement.GetSelfPath("options");
            }
            if (_updateData["sound"].IsClicked)
            {
                MenuPath = UIElement.GetSelfPath("sound");
            }
            if (_updateData["volUp"].IsClicked)
            {
                AudioEngine.Audio.MasterVolume += 0.1001f;
                Instance.DebugStream.Log("new volume: " + AudioEngine.Audio.MasterVolume);
            }
            if (_updateData["volDown"].IsClicked)
            {
                AudioEngine.Audio.MasterVolume -= 0.1001f;
                Instance.DebugStream.Log("new volume: " + AudioEngine.Audio.MasterVolume);
            }
            if (_updateData["startStory"].IsClicked)
            {
                //Leave(() =>
                //{
                //    Instance.Environment.Overlayer.Activate("transitionDither");
                //    textframe.Queue(new DialogueInfo(Instance.Environment.Entities["sto"], "Welcome.", 1000));
                //    textframe.Queue(new DialogueInfo(Instance.Environment.Entities["sto"], "Expecting something..?", 5000));
                //    textframe.Queue(new DialogueInfo(Instance.Environment.Entities["sto"], "Hold on....", 1000));

                //    Scene.MainInstance.SetImage(Instance.Textures.GetReference("textures\\landscape"));
                //});
                _textframe.Queue(new DialogueInfo(Instance.Environment, "Not yet implemented."));
            }
            if (_updateData["startCom"].IsClicked)
            {
                Scene.MainInstance.SetBoard(new Player[]
                        {
                            new Player("player0"),
                            Instance.Environment.Entities["goldsilk"].GetPlayer()
                        });
                Leave();
            }
            if (_updateData["specialThanks"].IsClicked)
            {
                _textframe.Queue(new DialogueInfo(Instance.Environment, "Please read the github README."));
            }
            if (_updateData["quit"].IsClicked)
            {
                Instance.SLExit();
            }

            if (!_menuDone) return;
            #endregion

            _menuRemoveTweener.Update(elapsedMiliseconds / 1000f);
            TaskHeap.Heap.SafePush("menuLogoDisapear", new DynamicTask(() => // fire and forget game logic ftw
            {
                _loadingFinished = true;
                _onLeave?.Invoke();
                _onLeave = null;
                if (Scene.MainInstance.HasBoard)
                    Instance.Environment.GameState = StolonEnvironment.SLGameState.OpenBoard;
                else Instance.Environment.GameState = StolonEnvironment.SLGameState.OpenScene;
            }), 2000, false);
            _milisecondsSinceMenuRemoveStart += elapsedMiliseconds;

            _tipPos = Centering.MiddleX((int)(Instance.Fonts["fonts\\smollerMono"].FastMeasure(_tips[_tipId]).X),
                _menuLogoDrawPos.Y + _menuLogoLines.Height + (menuLogoBoundingBoxClearing * Math.Clamp(_menuRemoveTweener.Value * 2f, 0f, 1f)), Instance.VirtualDimensions.X, Vector2.One);

            _menuRemoveLineY = (int)(_menuRemoveTweener.Value * Instance.VirtualDimensions.Y);

            Centering.OnPixel(ref _menuLogoDrawPos);
        }
        private void UpdateBoardUI(int elapsedMiliseconds)
        {
            float zoomIntensity = Instance.Environment.Scene.Board.ZoomIntensity;
            float lineZoomOffset = zoomIntensity * 30f * (zoomIntensity < 0 ? 0.5f : 1f); // 30 being the max zoom in pixels, the last bit is smoothening the inverted zoom.

            lineZoomOffset = Math.Max(0, lineZoomOffset);

            bool mouseIsOnUI = SLMouse.Domain == SLMouse.MouseDomain.UserInterfaceLow;

            _uiLeftOffset = -lineZoomOffset;
            _uiRightOffset = lineZoomOffset;

            _lineX1 = (int)(_lineOffset + _uiLeftOffset);
            _lineX2 = (int)(Instance.VirtualDimensions.X - _lineOffset + _uiRightOffset);
        }
        /// <summary>
        /// Get random splash text.
        /// </summary>
        /// <returns>A random splash text.</returns>
        public string GetRandomSplashText() => _tips[new Random().Next(0, _tips.Length)];
        /// <summary>
        /// Get a random splash text and get the <paramref name="i"/> as index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetRandomSplashText(out int i) => _tips[i = new Random().Next(0, _tips.Length)];
        /// <summary>
        /// Leave the main menu.
        /// </summary>
        public void Leave(Action? onLeave = null)
        {
            _menuDone = true;
            this._onLeave = onLeave;
        }
        //public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            switch (StolonEnvironment.Instance.GameState)
            {
                case StolonEnvironment.SLGameState.OpenBoard:
                    spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle(Point.Zero, new Point((int)_lineX1, 500)), Color.Black);
                    spriteBatch.DrawLine(_lineX1, -10f, _lineX1, 500f, Color.White, _lineWidth);
                    spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle((int)_lineX2, 0, Instance.VirtualDimensions.X - (int)_lineX2, 500), Color.Black);
                    spriteBatch.DrawLine(_lineX2, -10f, _lineX2, 500f, Color.White, _lineWidth);

                    if (_mouseClickElementBoundsCoefficient > 0.015f) spriteBatch.Draw(_mouseClickFillElementTexture, _mouseClickFillElementBounds, Color.White);
                    else _mouseClickElementBoundsCoefficient = 0f; // I really shouldent be altering this in the Draw() method..

                    break;
                case StolonEnvironment.SLGameState.InMenu:
                    spriteBatch.DrawLine(_menuLine1X, -10f, _menuLine1X, _menuLineLenght, Color.White, _menuLineWidth);
                    spriteBatch.DrawLine(_menuLine2X, -10f, _menuLine2X, _menuLineLenght, Color.White, _menuLineWidth);
                    if (_menuDone) spriteBatch.DrawString(Instance.Fonts["fonts\\smollerMono"], _tips[_tipId], _tipPos, Color.White, 0f, Vector2.Zero, Instance.Fonts["fonts\\smollerMono"].Scale, SpriteEffects.None, 1f);

                    if (_drawMenuLogoLowResFonted)
                    {
                        for (int i = 0; i < _menuDitherTexturePositions.Length; i++)
                            spriteBatch.Draw(_dither32, _menuDitherTexturePositions[i].ToVector2(), null, Color.White, 0f, Vector2.Zero, 1f,
                                (i >= _menuDitherTexturePositions.Length / 2f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

                        spriteBatch.Draw(Instance.Textures.Pixel, _menuLogoBoundingBox, Color.Black);
                        spriteBatch.DrawRectangle(_menuLogoBoundingBox, Color.White, _lineWidth);
                    }
                    if (_drawMenuLogoDummyTiles) spriteBatch.Draw(_menuLogoDummyTiles, _menuLogoDrawPos, Color.White);
                    if (_drawMenuLogoFilledTiles) spriteBatch.Draw(_menuLogoFilledTiles, _menuLogoDrawPos, Color.White);
                    if (_drawMenuLogoLowResFonted) spriteBatch.Draw(_menuLogoLowResFonted, _menuLogoDrawPos, Color.White);

                    spriteBatch.Draw(Instance.Textures.Pixel, _menuLogoTileHider, Color.Black);
                    if (_drawMenuLogoLines) spriteBatch.Draw(_menuLogoLines, _menuLogoDrawPos, Color.White);

                    int width = (int)(_menuLogoDrawPos.X - 8);
                    spriteBatch.DrawLine(width, -10f, width, _menuRemoveLineY, Color.White, _lineWidth);
                    spriteBatch.DrawLine(Instance.VirtualDimensions.X - width, -10f, Instance.VirtualDimensions.X - width, _menuRemoveLineY, Color.White, _lineWidth);
                    break;
                case StolonEnvironment.SLGameState.Loading:
                    break;
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
