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
using System.Threading.Tasks;
using NAudio.Wave.SampleProviders;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// The user interface for the <see cref="StolonEnvironment"/>.
    /// </summary>
    public class UserInterface : AxComponent
    {
        private StolonEnvironment environment;

        private List<UIElementDrawData> drawData;

        #region menuVariables

        private Texture2D menuLogoLines;
        private Texture2D menuLogoDummyTiles;
        private Texture2D menuLogoFilledTiles;
        private Texture2D menuLogoLowResFonted;
        private Texture2D dither8x8;

        private Rectangle menuLogoTileHider;

        private bool drawMenuLogoLines;
        private bool drawMenuLogoDummyTiles;
        private bool drawMenuLogoFilledTiles;
        private bool drawMenuLogoLowResFonted;
        private int menuLogoRowsHidden;
        private Rectangle menuLogoBoundingBox;

        private Vector2 menuLogoDrawPos;
        private int milisecondsSinceStartup;

        private int menuLogoFlashTime;
        private int? menuFlashStart;
        private int? menuFlashEnd;
        private int menuLogoMilisecondsFlashing;
        private int milisecondsSinceMenuRemoveStart;
        private bool menuDone;

        private int menuLine1X;
        private int menuLine2X;
        private int menuLineLenght;
        private int menuLineWidth;

        private int menuRemoveLineY;

        private bool loadingFinished;

        private float menuLogoScaling;

        private List<UIElement> depthPath;

        private Point[] menuDitherTexturePositions;

        private Tweener<float> menuLogoEaseTweener;
        private Tweener<float> menuRemoveTweener;


        private string[] tips;
        private Vector2 tipPos;
        private int tipId;

        private const int menuLogoRowCount = 5;

        #endregion
        #region boardUIvariables

        private int lineX1;
        private int lineX2;
        private float lineOffset;
        private int lineWidth;
        private float uiLeftOffset;
        private float uiRightOffset;


        private Dictionary<string, UIElement> AllUIElements;

        private SpriteFont uifont;

        private Dictionary<string, UIElementUpdateData> updateData;

        private Texture2D mouseClickFillElementTexture;
        private Rectangle mouseClickFillElementBounds;
        private float mouseClickElementBoundsCoefficient;

        #endregion
        #region dialogue variables



        #endregion
        #region public properties

        /// <summary>
        /// The virtual X coordiantes of the first line (from left to right).
        /// </summary>
        public float Line1X => lineX1;
        /// <summary>
        /// The virtual X coordiantes of the second line (from left to right).
        /// </summary>
        public float Line2X => lineX2;

        /// <summary>
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing all the <see cref="UIElementDrawData"/> objects from all the <see cref="UIElement"/> objects refreched AFTER the UI update.
        /// </summary>
        public ReadOnlyDictionary<string, UIElementUpdateData> UIElementUpdateData => new ReadOnlyDictionary<string, UIElementUpdateData>(updateData);
        /// <summary>
        /// A <see cref="ReadOnlyDictionary{TKey, TValue}"/> containing all <see cref="UIElement"/> added via the <see cref="AddElement(UIElement)"/> method.
        /// </summary>
        public ReadOnlyDictionary<string, UIElement> UIElements => new ReadOnlyDictionary<string, UIElement>(AllUIElements);

        #endregion

        public const string titleParentId = "titleParent";
        private Textframe textframe;
        public Action? onLeave;

        /// <summary>
        /// The <see cref="Stolon.Textframe"/> managed by the <see cref="UserInterface"/>.
        /// </summary>
        public Textframe Textframe => textframe;

        /// <summary>
        /// The width of a <see cref="UserInterface"/> line. <i>(Why did I make this public again?)</i>
        /// </summary>
        public int LineWidth => lineWidth;

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

            environment = Instance.Environment;

            lineOffset = 96f;
            lineWidth = 2;

            Instance.DebugStream.WriteLine("[s]loaded audio..");
            foreach (string filePath in Directory.GetFiles("audio", "*.wav", SearchOption.AllDirectories))
            {
                string fileName = camelCase(Path.GetFileNameWithoutExtension(filePath).Replace(" ", string.Empty));
                AudioEngine.AudioLibrary.Add(fileName, new CachedAudio(filePath, fileName));
                Instance.DebugStream.WriteLine("\tloaded audio with id: " + fileName);
            }

            uifont = Instance.Fonts["fiont"];

            menuLogoLines = Instance.Textures.GetReference("textures\\" + menuDataFolder+ "\\lines");
            menuLogoDummyTiles = Instance.Textures.GetReference("textures\\" + menuDataFolder+ "\\dummyTiles");
            menuLogoFilledTiles = Instance.Textures.GetReference("textures\\" + menuDataFolder+ "\\filledTiles");
            menuLogoLowResFonted = Instance.Textures.GetReference("textures\\" + menuDataFolder+ "\\lowResFonted");
            dither8x8 = Instance.Textures.GetReference("textures\\dither8x8");

            AllUIElements = new Dictionary<string, UIElement>();

            drawData = new List<UIElementDrawData>();

            updateData = new Dictionary<string, UIElementUpdateData>();
            mouseClickFillElementBounds = new Rectangle();

            mouseClickFillElementTexture = new AxTexture(AxPalette.Empty, new Texture2D(Instance.GraphicsDevice, 1, 1));
            ((Texture2D)mouseClickFillElementTexture).SetData(new Color[] { Color.White });

            textframe = new Textframe(this);

            drawMenuLogoLines = true;
            drawMenuLogoDummyTiles = true;
            drawMenuLogoFilledTiles = false;

            menuLogoFlashTime = 0;
            menuFlashStart = null;
            menuLogoRowsHidden = 5;
            menuDitherTexturePositions = Array.Empty<Point>();

            depthPath = new List<UIElement>();

            menuLogoScaling = 1f;

            menuLogoEaseTweener = new Tweener<float>(0f, 1f, 2f, Ease.Quad.InOut);
            menuRemoveTweener = new Tweener<float>(0f, 1f, 2f, Ease.Quad.InOut);

            tips = new string[]
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
            };

            tipId = new Random().Next(0, tips.Length);
        }
        public void Initialize()
        {
            StolonGame.Instance.DebugStream.WriteLine("[s]initializing ui..");
            // top
            AddElement(new UIElement(titleParentId, UIElement.topId, string.Empty, UIElementType.Listen));

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
            AddElement(new UIElement("startStory", titleParentId, "Story", UIElementType.Listen, clickSoundId: "exit3"));
            AddElement(new UIElement("startCom", titleParentId, "COM", UIElementType.Listen, clickSoundId: "coin4"));
            AddElement(new UIElement("startXp", titleParentId, "2P", UIElementType.Listen, clickSoundId: "coin4"));
            AddElement(new UIElement("options", titleParentId, "Options", UIElementType.Listen));
            AddElement(new UIElement("specialThanks", titleParentId, "Special Thanks :D", UIElementType.Listen));
            AddElement(new UIElement("quit", titleParentId, "Quit", UIElementType.Listen));

            // options
            AddElement(new UIElement("sound", "options", "Sound", UIElementType.Listen));
            AddElement(new UIElement("graphics", "options", "Graphics", UIElementType.Listen, clickSoundId: "exit3"));

            AddElement(new UIElement("volUp", "sound", "Volume UP", UIElementType.Listen));
            AddElement(new UIElement("volDown", "sound", "Volume DOWN", UIElementType.Listen));

            MenuPath = GetSelfPath(titleParentId);

            StolonGame.Instance.DebugStream.WriteLine("autogenerating _back_ buttons..");
            HashSet<string> parentIds = GetParentIDs();
            foreach (string id in parentIds) AddElement(new UIElement("_back_" + id, id, "Back", UIElementType.Listen));
        }
        /// <summary>
        /// Clears both the updatedata and drawdata collections, making them ready to be repopulated by the methods in the <see cref="UIOrdering"/> class.<br/>
        /// <i>Does populate the updatedata collection with unhovered <see cref="UIElementDrawData"/> objects.</i>
        /// </summary>
        private void ResetElementData()
        {
            updateData.Clear();
            foreach (UIElement uiElement in AllUIElements.Values)
                updateData.Add(uiElement.Id, new UIElementUpdateData(false, uiElement.Id));
            drawData.Clear();
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

            textframe.Update(elapsedMiliseconds);
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
                    if (updateData[item].IsClicked)
                    {
                        AudioEngine.Audio.Play(updateData[item].ClickSound);
                    }
                    if (updateData.TryGetValue("_back_" + item, out UIElementUpdateData updateData2))
                    {
                        if (updateData2.IsClicked)
                        {
                            MenuPath = UIElement.GetParentPath(item);
                            Console.WriteLine(GetSelfPath(item));
                        }
                    }
                }
            }
            
            base.Update(elapsedMiliseconds);
        }
        private void UpdateMenuUI(int elapsedMiliseconds)
        {

            int rowHeight = (int)(menuLogoLines.Height / (float)menuLogoRowCount);
            float menuRemoveTweenerOffset = 200f * menuRemoveTweener.Value;
            int lineFromMid = (int)(170f + menuRemoveTweenerOffset);
            bool menuFlashEnded = milisecondsSinceStartup > menuFlashEnd;
            int uiElementOffsetY = (int)(330f + menuRemoveTweenerOffset);
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
            menuLogoTileHider = new Rectangle(menuLogoDrawPos.ToPoint(), new Point((int)(menuLogoLines.Width * menuLogoScaling), (int)(rowHeight * menuLogoRowsHidden)));
            milisecondsSinceStartup += elapsedMiliseconds;
            menuLogoDrawPos = Vector2.Round(Centering.MiddleX(menuLogoLines, logoYoffset, Instance.VirtualDimensions.X, Vector2.One) + new Vector2(0, 8f * menuLogoEaseTweener.Value * (1 - menuRemoveTweener.Value)))
                + new Vector2(0, ((Centering.MiddleY(menuLogoLines, 1, Instance.VirtualDimensions.Y, Vector2.One).Y - logoYoffset * 1.5f) * menuRemoveTweener.Value));
            menuDitherTexturePositions = new Point[(int)Math.Ceiling(Instance.VirtualDimensions.Y / (float)dither8x8.Height) * 2];

            menuFlashStart = 1200;
            menuFlashEnd = menuFlashStart + 400;

            menuLine1X = (int)(Instance.VirtualDimensions.X / 2f) - lineFromMid;
            menuLine2X = (int)(Instance.VirtualDimensions.X / 2f) + lineFromMid;

            menuLineLenght = drawMenuLogoFilledTiles ? StolonGame.Instance.VirtualDimensions.Y : 0;
            menuLineWidth = 1 + (menuFlashEnded ? 1 : 0);

            if (milisecondsSinceStartup > 300) menuLogoRowsHidden = 4;
            if (milisecondsSinceStartup > 600) menuLogoRowsHidden = 3;
            if (milisecondsSinceStartup > 800) menuLogoRowsHidden = 2;
            if (milisecondsSinceStartup > 1000) menuLogoRowsHidden = 1;
            if (milisecondsSinceStartup > menuFlashStart) menuLogoRowsHidden = 0;

            if (menuLogoFlashTime > 0)
            {
                menuLogoMilisecondsFlashing += elapsedMiliseconds;
                if (menuLogoMilisecondsFlashing > menuLogoFlashTime)
                {
                    drawMenuLogoFilledTiles = !drawMenuLogoFilledTiles;
                    menuLogoMilisecondsFlashing = 0;
                }
            }

            if (!menuFlashStart.HasValue) return; // code below only relevant when the dummy board show animation ended.

            if (milisecondsSinceStartup > menuFlashStart.Value) menuLogoFlashTime = 120;
            if (milisecondsSinceStartup > menuFlashStart.Value + 200) menuLogoFlashTime = 100;
            if (milisecondsSinceStartup > menuFlashStart.Value + 300) menuLogoFlashTime = 75;
            if (milisecondsSinceStartup > menuFlashStart.Value + 350) menuLogoFlashTime = 60;
            if (milisecondsSinceStartup < menuFlashEnd) return; // code below only relevant when the full animation ended.

            #endregion
            #region inMenu
            drawMenuLogoLowResFonted = true;
            drawMenuLogoDummyTiles = true;
            drawMenuLogoFilledTiles = true;
            drawMenuLogoLines = true;

            menuLogoFlashTime = 0; // ensures disabled flashing.
            menuLogoEaseTweener.Update(elapsedMiliseconds / 1000f); // update the tweener.
            if (menuLogoEaseTweener.Value == 1 || menuLogoEaseTweener.Value == 0) // reverse and restart if finished.
            {
                menuLogoEaseTweener.Reverse();
                menuLogoEaseTweener.Start();
                Instance.DebugStream.WriteLine("\treversing icon tweener.");
            }
            menuLogoBoundingBox =
                new Rectangle(menuLogoDrawPos.ToPoint() + new Point(-menuLogoBoundingBoxClearing), menuLogoLines.Bounds.Size + new Point(menuLogoBoundingBoxClearing * 2));

            for (int i = 0; i < menuDitherTexturePositions.Length; i++) // dithering positions.
                menuDitherTexturePositions[i] = new Point(
                        (i >= menuDitherTexturePositions.Length / 2f) ? menuLine2X : menuLine1X - dither8x8.Width,
                        (i % (int)(menuDitherTexturePositions.Length / 2f)) * dither8x8.Height);

            UIOrdering.Order(AllUIElements.Values.ToArray(), MenuPath, drawData, updateData, new Vector2(0, uiElementOffsetY), OrderProviders.Menu);

            if (updateData["startXp"].IsClicked)
            {
                Scene.MainInstance.SetBoard(new Player[]
                        {
                            new Player("player0"),
                            new Player("player1"),
                        });

                Leave();
            }
            if (updateData["options"].IsClicked)
            {
                MenuPath = UIElement.GetSelfPath("options");
            }
            if (updateData["sound"].IsClicked)
            {
                MenuPath = UIElement.GetSelfPath("sound");
            }
            if (updateData["volUp"].IsClicked)
            {
                AudioEngine.Audio.MasterVolume += 0.1001f;
                Instance.DebugStream.WriteLine("\tnew volume: " + AudioEngine.Audio.MasterVolume);
            }
            if (updateData["volDown"].IsClicked)
            {
                AudioEngine.Audio.MasterVolume -= 0.1001f;
                Instance.DebugStream.WriteLine("\tnew volume: " + AudioEngine.Audio.MasterVolume);
            }
            if (updateData["startStory"].IsClicked)
            {
                //Leave(() =>
                //{
                //    Instance.Environment.Overlayer.Activate("transitionDither");
                //    textframe.Queue(new DialogueInfo(Instance.Environment.Entities["sto"], "Welcome.", 1000));
                //    textframe.Queue(new DialogueInfo(Instance.Environment.Entities["sto"], "Expecting something..?", 5000));
                //    textframe.Queue(new DialogueInfo(Instance.Environment.Entities["sto"], "Hold on....", 1000));

                //    Scene.MainInstance.SetImage(Instance.Textures.GetReference("textures\\landscape"));
                //});
                textframe.Queue(new DialogueInfo(Instance.Environment, "Not yet implemented."));
            }
            if (updateData["startCom"].IsClicked)
            {
                Scene.MainInstance.SetBoard(new Player[]
                        {
                            new Player("player0"),
                            Instance.Environment.Entities["goldsilk"].GetPlayer()
                        });
                Leave();
            }
            if (updateData["specialThanks"].IsClicked)
            {
                textframe.Queue(new DialogueInfo(Instance.Environment, "Please read the github README."));
            }
            if (updateData["quit"].IsClicked)
            {
                Instance.SLExit();
            }

            if (!menuDone) return;
            #endregion

            menuRemoveTweener.Update(elapsedMiliseconds / 1000f);
            TaskHeap.Heap.SafePush("menuLogoDisapear", new DynamicTask(() => // fire and forget game logic ftw
            {
                loadingFinished = true;
                onLeave?.Invoke();
                onLeave = null;
                if (Scene.MainInstance.HasBoard)
                    Instance.Environment.GameState = StolonEnvironment.SLGameState.OpenBoard;
                else Instance.Environment.GameState = StolonEnvironment.SLGameState.OpenScene;
            }), 2000, false);
            milisecondsSinceMenuRemoveStart += elapsedMiliseconds;

            tipPos = Centering.MiddleX((int)(tips[tipId].Length * Instance.Environment.FontDimensions.X),
             menuLogoDrawPos.Y + menuLogoLines.Height + (menuLogoBoundingBoxClearing * Math.Clamp(menuRemoveTweener.Value * 2f, 0f, 1f)), Instance.VirtualDimensions.X, Vector2.One);

            menuRemoveLineY = (int)(menuRemoveTweener.Value * Instance.VirtualDimensions.Y);

            Centering.OnPixel(ref menuLogoDrawPos);
        }
        private void UpdateBoardUI(int elapsedMiliseconds)
        {
            float zoomIntensity = Instance.Environment.Scene.Board.ZoomIntensity;
            float lineZoomOffset = zoomIntensity * 30f * (zoomIntensity < 0 ? 0.5f : 1f); // 30 being the max zoom in pixels, the last bit is smoothening the inverted zoom.

            lineZoomOffset = Math.Max(0, lineZoomOffset);

            bool mouseIsOnUI = SLMouse.Domain == SLMouse.MouseDomain.UserInterfaceLow;

            uiLeftOffset = -lineZoomOffset;
            uiRightOffset = lineZoomOffset;

            lineX1 = (int)(lineOffset + uiLeftOffset);
            lineX2 = (int)(Instance.VirtualDimensions.X - lineOffset + uiRightOffset);
        }
        /// <summary>
        /// Get random splash text.
        /// </summary>
        /// <returns>A random splash text.</returns>
        public string GetRandomSplashText() => tips[new Random().Next(0, tips.Length)];
        /// <summary>
        /// Get a random splash text and get the <paramref name="i"/> as index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetRandomSplashText(out int i) => tips[i = new Random().Next(0, tips.Length)];
        /// <summary>
        /// Leave the main menu.
        /// </summary>
        public void Leave(Action? onLeave = null)
        {
            menuDone = true;
            this.onLeave = onLeave;
        }
        //public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            switch (StolonEnvironment.Instance.GameState)
            {
                case StolonEnvironment.SLGameState.OpenBoard:
                    spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle(Point.Zero, new Point((int)lineX1, 500)), Color.Black);
                    spriteBatch.DrawLine(lineX1, -10f, lineX1, 500f, Color.White, lineWidth);
                    spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle((int)lineX2, 0, Instance.VirtualDimensions.X - (int)lineX2, 500), Color.Black);
                    spriteBatch.DrawLine(lineX2, -10f, lineX2, 500f, Color.White, lineWidth);

                    if (mouseClickElementBoundsCoefficient > 0.015f) spriteBatch.Draw(mouseClickFillElementTexture, mouseClickFillElementBounds, Color.White);
                    else mouseClickElementBoundsCoefficient = 0f; // I really shouldent be altering this in the Draw() method..

                    break;
                case StolonEnvironment.SLGameState.InMenu:
                    spriteBatch.DrawLine(menuLine1X, -10f, menuLine1X, menuLineLenght, Color.White, menuLineWidth);
                    spriteBatch.DrawLine(menuLine2X, -10f, menuLine2X, menuLineLenght, Color.White, menuLineWidth);
                    if (menuDone) spriteBatch.DrawString(uifont, tips[tipId], tipPos, Color.White, 0f, Vector2.Zero, StolonEnvironment.FontScale, SpriteEffects.None, 1f);

                    if (drawMenuLogoLowResFonted)
                    {
                        for (int i = 0; i < menuDitherTexturePositions.Length; i++)
                            spriteBatch.Draw(dither8x8, menuDitherTexturePositions[i].ToVector2(), null, Color.White, 0f, Vector2.Zero, 1f,
                                (i >= menuDitherTexturePositions.Length / 2f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

                        spriteBatch.Draw(Instance.Textures.Pixel, menuLogoBoundingBox, Color.Black);
                        spriteBatch.DrawRectangle(menuLogoBoundingBox, Color.White, lineWidth);
                    }
                    if (drawMenuLogoDummyTiles) spriteBatch.Draw(menuLogoDummyTiles, menuLogoDrawPos, Color.White);
                    if (drawMenuLogoFilledTiles) spriteBatch.Draw(menuLogoFilledTiles, menuLogoDrawPos, Color.White);
                    if (drawMenuLogoLowResFonted) spriteBatch.Draw(menuLogoLowResFonted, menuLogoDrawPos, Color.White);

                    spriteBatch.Draw(Instance.Textures.Pixel, menuLogoTileHider, Color.Black);
                    if (drawMenuLogoLines) spriteBatch.Draw(menuLogoLines, menuLogoDrawPos, Color.White);

                    int width = (int)(menuLogoDrawPos.X - 8);
                    spriteBatch.DrawLine(width, -10f, width, menuRemoveLineY, Color.White, lineWidth);
                    spriteBatch.DrawLine(Instance.VirtualDimensions.X - width, -10f, Instance.VirtualDimensions.X - width, menuRemoveLineY, Color.White, lineWidth);
                    break;
                case StolonEnvironment.SLGameState.Loading:
                    break;
            }
            foreach (UIElementDrawData elementDrawData in drawData)
            {
                spriteBatch.DrawString(uifont, elementDrawData.Text, elementDrawData.Position, Color.White, 0f, Vector2.Zero, StolonEnvironment.FontScale, SpriteEffects.None, 1f);
                if (elementDrawData.DrawRectangle)
                {
                    spriteBatch.DrawRectangle(elementDrawData.Rectangle, Color.White, 1f);
                }
            }
            textframe.Draw(spriteBatch, elapsedMiliseconds);
            base.Draw(spriteBatch, elapsedMiliseconds);
        }
        /// <summary>
        /// Add an element to the <see cref="UserInterface"/>.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to add.</param>
        public void AddElement(UIElement element)
        {
            AllUIElements.Add(element.Id, element);
            Instance.DebugStream.WriteLine("\tui-element with id " + element.Id + " added.");
            //updateData.Add(element.Id, default);
            //Instance.DebugStream.WriteLine("\t\tstale-ui-element with id " + element.Id + " added.");
        }
        /// <summary>
        /// Remove an <see cref="UIElement"/> from the <see cref="UserInterface"/>.
        /// </summary>
        /// <param name="elementID">The <see cref="UIElement.Id"/> of the <see cref="UIElement"/> to remove.</param>
        public void RemoveElement(string elementID)
        {
            AllUIElements.Remove(elementID);
            Instance.DebugStream.WriteLine("\tui-element with id " + elementID + " removed.");
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
