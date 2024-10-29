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
using static Stolon.SLUserInterface;
using AsitLib;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// The user interface for the <see cref="SLEnvironment"/>.
    /// </summary>
    public class SLUserInterface : AxComponent
    {

        public enum BoardSide
        {
            Left,
            Right,
        }
        private SLEnvironment environment;

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

        private FlashHandler menuLogoDisapearFlashHandler;

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
        public const string boardLeftParentId = "boardLeftParent";
        public const string boardRightParentId = "boardRightParent";
        private SLTextframe textframe;

        /// <summary>
        /// The <see cref="SLTextframe"/> managed by the <see cref="SLUserInterface"/>.
        /// </summary>
        public SLTextframe Textframe => textframe;

        /// <summary>
        /// The width of a <see cref="SLUserInterface"/> line. <i>(Why did I make this public again?)</i>
        /// </summary>
        public int LineWidth => lineWidth;

        public UIPath MenuPath { get; set; }
        /// <summary>
        /// Main UIInterface contructor.
        /// </summary>
        internal SLUserInterface() : base(Instance.Environment)
        {
            environment = Instance.Environment;

            lineOffset = 96f;
            lineWidth = 2;

            uifont = Instance.Fonts["fiont"];

            menuLogoLines = Instance.Textures.GetReference("textures\\menuLogo\\lines");
            menuLogoDummyTiles = Instance.Textures.GetReference("textures\\menuLogo\\dummyTiles");
            menuLogoFilledTiles = Instance.Textures.GetReference("textures\\menuLogo\\filledTiles");
            menuLogoLowResFonted = Instance.Textures.GetReference("textures\\menuLogo\\lowResFonted");
            dither8x8 = Instance.Textures.GetReference("textures\\dither8x8");

            AllUIElements = new Dictionary<string, UIElement>();

            drawData = new List<UIElementDrawData>();

            updateData = new Dictionary<string, UIElementUpdateData>();
            mouseClickFillElementBounds = new Rectangle();

            mouseClickFillElementTexture = new AxTexture(AxPalette.Empty, new Texture2D(Instance.GraphicsDevice, 1, 1));
            ((Texture2D)mouseClickFillElementTexture).SetData(new Color[] { Color.White });

            textframe = new SLTextframe(this);


            drawMenuLogoLines = true;
            drawMenuLogoDummyTiles = true;
            drawMenuLogoFilledTiles = false;

            menuLogoFlashTime = 0;
            menuFlashStart = null;
            menuLogoRowsHidden = 5;
            menuDitherTexturePositions = Array.Empty<Point>();

            depthPath = new List<UIElement>();
            menuLogoDisapearFlashHandler = new FlashHandler(1000, 1500);
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
                "Inity-Alizing..",
                "Super colliding..",
                "Learning garden chairs how to fly..",
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
                "Lanu X4",
                "Welcome.",
                "Welcome!",
                "Galore.",
                "NOT solved.",
                "NOT CLUELESS!",
                "Tiory?",
                "27 Compile errors.",
                "Simply Rendering,",
                "Behold, The \"Sky Train\"!",
                "dot hat :drool:",
                "Cherry-pilled!"
            };

            tipId = new Random().Next(0, tips.Length);


            
        }
        public void Initialize()
        {
            StolonGame.Instance.DebugStream.WriteLine("\t[s]initializing ui..");
            // top
            AddElement(new UIElement(boardLeftParentId, UIElement.topId, string.Empty, UIElementType.Listen));
            AddElement(new UIElement(boardRightParentId, UIElement.topId, string.Empty, UIElementType.Listen));
            AddElement(new UIElement(titleParentId, UIElement.topId, string.Empty, UIElementType.Listen));

            // board l
            AddElement(new UIElement("exitGame", boardLeftParentId, "Exit Game", UIElementType.Listen));

            AddElement(new UIElement("screenRegion2", boardLeftParentId, string.Empty, UIElementType.Ignore));
            AddElement(new UIElement("screenRegion", boardLeftParentId, "Screen & Camera", UIElementType.Ignore));
            AddElement(new UIElement("toggleFullscreen", boardLeftParentId, "Go Fullscreen", UIElementType.Listen));
            AddElement(new UIElement("centerCamera", boardLeftParentId, "Center Camera", UIElementType.Listen));

            AddElement(new UIElement("boardRegion2", boardLeftParentId, string.Empty, UIElementType.Ignore));
            AddElement(new UIElement("boardRegion", boardLeftParentId, "Board", UIElementType.Ignore));
            AddElement(new UIElement("undoMove", boardLeftParentId, "Undo", UIElementType.Listen));
            AddElement(new UIElement("restartBoard", boardLeftParentId, "Restart", UIElementType.Listen));
            AddElement(new UIElement("boardSearch", boardLeftParentId, "Search", UIElementType.Listen));
            AddElement(new UIElement("skipMove", boardLeftParentId, "End Move", UIElementType.Listen));

            // board r
            AddElement(new UIElement("currentPlayer", boardRightParentId, null, UIElementType.Ignore));

            // main menu
            AddElement(new UIElement("startStory", titleParentId, "Story", UIElementType.Listen));
            AddElement(new UIElement("startCom", titleParentId, "COM", UIElementType.Listen));
            AddElement(new UIElement("startXp", titleParentId, "2P", UIElementType.Listen));
            AddElement(new UIElement("options", titleParentId, "Options", UIElementType.Listen));
            AddElement(new UIElement("specialThanks", titleParentId, "Special Thanks :D", UIElementType.Listen));
            AddElement(new UIElement("quit", titleParentId, "Quit", UIElementType.Listen));

            // options
            AddElement(new UIElement("sound", "options", "Sound", UIElementType.Listen));
            AddElement(new UIElement("graphics", "options", "Graphics", UIElementType.Listen));

            AddElement(new UIElement("volUp", "sound", "Volume UP", UIElementType.Listen));
            AddElement(new UIElement("volDown", "sound", "Volume DOWN", UIElementType.Listen));

            MenuPath = GetSelfPath(titleParentId);
            HashSet<string> parentIds = GetParentIDs();
            foreach (string id in parentIds)
            {
                AddElement(new UIElement("_back_" + id, id, "Back", UIElementType.Listen));
            }
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
        //public HashSet<string> GetDockIDs()
        //{
        //    var topIds = GetTopIDs();
        //    return UIElements.Values.Where(e => topIds.Contains(e.ChildOf)).Select(e => e.Id).ToHashSet();
        //}
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
                case SLEnvironment.SLGameState.OpenBoard:
                    UpdateBoardUI(elapsedMiliseconds);
                    break;
                case SLEnvironment.SLGameState.InMenu:
                    UpdateMenuUI(elapsedMiliseconds);
                    break;
                case SLEnvironment.SLGameState.Loading:
                    break;
            }
            base.Update(elapsedMiliseconds);
        }
        private void UpdateMenuUI(int elapsedMiliseconds)
        {
            int rowHeight = (int)(menuLogoLines.Height / (float)menuLogoRowCount);
            float menuRemoveTweenerOffset = 200f * menuRemoveTweener.Value;
            int lineFromMid = (int)(110f + menuRemoveTweenerOffset);
            bool menuFlashEnded = milisecondsSinceStartup > menuFlashEnd;
            int uiElementOffsetY = (int)(130f + menuRemoveTweenerOffset);
            int logoYoffset = 30;
            int menuLogoBoundingBoxClearing = 8;

            // if (milisecondsSinceStartup < 10000) // to skip start button click and animation
            //{
            //    milisecondsSinceStartup = 10001;
            //    menuDone = true;
            //    menuRemoveTweener.Update(10);
            //    startFrame = true;
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

            menuLineLenght = drawMenuLogoFilledTiles ? 400 : 0;
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
                SLScene.MainInstance.SetBoard(new Player[]
                        {
                            new Player("player0"),
                            new Player("player1"),
                        });

                Leave();
            }
            if (updateData["options"].IsClicked)
            {
                Console.WriteLine(UIElement.GetSelfPath("options"));
                Console.WriteLine(UIElement.GetParentPath("options"));
                MenuPath = UIElement.GetSelfPath("options");
                //Console.WriteLine(GetParentIDs().ToJoinedString(", "));
                //textframe.Queue(new DialogueInfo(Instance.Environment, "Not yet implemented."));
            }
            if (updateData["sound"].IsClicked)
            {
                MenuPath = UIElement.GetSelfPath("sound");
            }
            if (updateData["startStory"].IsClicked)
            {
                textframe.Queue(new DialogueInfo(Instance.Environment, "Not yet implemented."));
            }
            if (updateData["startCom"].IsClicked)
            {
                SLScene.MainInstance.SetBoard(new Player[]
                        {
                            new Player("player0"),
                            Instance.Environment.Entities["goldsilk"].GetPlayer()
                        });
                Leave();
            }
            if (updateData["specialThanks"].IsClicked)
            {
                textframe.Queue(new DialogueInfo(Instance.Environment, "Please read the README :D"));
            }
            if (updateData["quit"].IsClicked)
            {
                Instance.SLExit();
            }
            foreach (string item in UIElements.Keys)
            {
                if (updateData.TryGetValue("_back_" + item, out UIElementUpdateData updateData2))
                {
                    if (updateData2.IsClicked)
                    {
                        MenuPath = UIElement.GetParentPath(item);
                        Console.WriteLine(GetSelfPath(item));
                    }
                }
            }

            if (!menuDone) return;
            #endregion

            menuRemoveTweener.Update(elapsedMiliseconds / 1000f);
            menuLogoDisapearFlashHandler.Update(elapsedMiliseconds);
            milisecondsSinceMenuRemoveStart += elapsedMiliseconds;

            tipPos = Centering.MiddleX((int)(tips[tipId].Length * Instance.Environment.FontDimensions.X),
             menuLogoDrawPos.Y + menuLogoLines.Height + (menuLogoBoundingBoxClearing * Math.Clamp(menuRemoveTweener.Value * 2f, 0f, 1f)), Instance.VirtualDimensions.X, Vector2.One);

            menuRemoveLineY = (int)(menuRemoveTweener.Value * Instance.VirtualDimensions.Y);

            if (menuLogoDisapearFlashHandler.HasEnded && !menuRemoveTweener.Running && !loadingFinished)
            {
                //SLEnvironment.Instance.ForceGameState(SLEnvironment.SLGameState.OpenBoard);
                if (Board.MainInstance == null) throw new Exception();


                loadingFinished = true;
                //Instance.Scene = new SLScene();
                Instance.Environment.GameState = SLEnvironment.SLGameState.OpenBoard;
            }

            Centering.OnPixel(ref menuLogoDrawPos);
        }
        private void UpdateBoardUI(int elapsedMiliseconds)
        {
            float zoomIntensity = Instance.Environment.Scene.Board.ZoomIntensity;
            float lineZoomOffset = zoomIntensity * 30f * (zoomIntensity < 0 ? 0.5f : 1f); // 30 being the max zoom in pixels, the last bit is smoothening the inverted zoom.

            bool mouseIsOnUI = SLMouse.Domain == SLMouse.MouseDomain.UserInterfaceLow;

            uiLeftOffset = -lineZoomOffset;
            uiRightOffset = lineZoomOffset;

            lineX1 = (int)(lineOffset + uiLeftOffset);
            lineX2 = (int)(Instance.VirtualDimensions.X - lineOffset + uiRightOffset);

            UIOrdering.Order(AllUIElements.Values.ToArray(), boardLeftParentId, drawData, updateData, new Vector2(uiLeftOffset, 0) + new Vector2(5), OrderProviders.BoardSide);
            UIOrdering.Order(AllUIElements.Values.ToArray(), boardRightParentId, drawData, updateData, new Vector2(lineX2, 0) + new Vector2(5), OrderProviders.BoardSide);
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
        public string GetRandomSplashText(out int i)
        {
            int tipId = new Random().Next(0, tips.Length);
            i = tipId;
            return tips[tipId];
        }
        /// <summary>
        /// Leave the main menu.
        /// </summary>
        public void Leave()
        {
            menuDone = true;
        }
        //public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            switch (SLEnvironment.Instance.GameState)
            {
                case SLEnvironment.SLGameState.OpenBoard:
                    spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle(Point.Zero, new Point((int)lineX1, 500)), Color.Black);
                    spriteBatch.DrawLine(lineX1, -10f, lineX1, 500f, Color.White, lineWidth);
                    spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle((int)lineX2, 0, Instance.VirtualDimensions.X - (int)lineX2, 500), Color.Black);
                    spriteBatch.DrawLine(lineX2, -10f, lineX2, 500f, Color.White, lineWidth);

                    if (mouseClickElementBoundsCoefficient > 0.015f) spriteBatch.Draw(mouseClickFillElementTexture, mouseClickFillElementBounds, Color.White);
                    else mouseClickElementBoundsCoefficient = 0f; // I really shouldent be altering this in the Draw() method..

                    break;
                case SLEnvironment.SLGameState.InMenu:
                    spriteBatch.DrawLine(menuLine1X, -10f, menuLine1X, menuLineLenght, Color.White, menuLineWidth);
                    spriteBatch.DrawLine(menuLine2X, -10f, menuLine2X, menuLineLenght, Color.White, menuLineWidth);
                    if (menuDone) spriteBatch.DrawString(uifont, tips[tipId], tipPos, Color.White, 0f, Vector2.Zero, SLEnvironment.FontScale, SpriteEffects.None, 1f);

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
                    if (menuLogoDisapearFlashHandler.Flash)
                    {
                        //spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle(Point.Zero, new Point(width, Instance.VirtualDimensions.Y)), Color.White);
                        //spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle(new Point(Instance.VirtualDimensions.X - width, 0), new Point(width, Instance.VirtualDimensions.Y)), Color.White);
                        //spriteBatch.DrawLine(width, -10f, width, menuRemoveLineY, Color.White, 8);
                        //spriteBatch.DrawLine(Instance.VirtualDimensions.X - width, -10f, Instance.VirtualDimensions.X - width, menuRemoveLineY, Color.White, 8);
                        //spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle(Point.Zero, new Point(width, menuRemoveLineY)), Color.White);
                        //spriteBatch.Draw(Instance.Textures.Pixel, new Rectangle(new Point(Instance.VirtualDimensions.X - width, 0), new Point(width, menuRemoveLineY)), Color.White);
                    }
                    spriteBatch.DrawLine(width, -10f, width, menuRemoveLineY, Color.White, lineWidth);
                    spriteBatch.DrawLine(Instance.VirtualDimensions.X - width, -10f, Instance.VirtualDimensions.X - width, menuRemoveLineY, Color.White, lineWidth);
                    break;
                case SLEnvironment.SLGameState.Loading:
                    break;
            }
            foreach (UIElementDrawData elementDrawData in drawData)
            {
                spriteBatch.DrawString(uifont, elementDrawData.Text, elementDrawData.Position, Color.White, 0f, Vector2.Zero, SLEnvironment.FontScale, SpriteEffects.None, 1f);
                if (elementDrawData.DrawRectangle)
                {
                    spriteBatch.DrawRectangle(elementDrawData.Rectangle, Color.White, 1f);
                }
            }
            textframe.Draw(spriteBatch, elapsedMiliseconds);
            base.Draw(spriteBatch, elapsedMiliseconds);
        }
        /// <summary>
        /// Add an element to the <see cref="SLUserInterface"/>.
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> to add.</param>
        public void AddElement(UIElement element)
        {
            AllUIElements.Add(element.Id, element);
            Instance.DebugStream.WriteLine("\tui-element with id " + element.Id + " added.");
            updateData.Add(element.Id, default);
            Instance.DebugStream.WriteLine("\tghost-ui-element with id " + element.Id + " added.");
        }
        /// <summary>
        /// Remove an <see cref="UIElement"/> from the <see cref="SLUserInterface"/>.
        /// </summary>
        /// <param name="elementID">The <see cref="UIElement.Id"/> of the <see cref="UIElement"/> to remove.</param>
        public void RemoveElement(string elementID)
        {
            AllUIElements.Remove(elementID);
            Instance.DebugStream.WriteLine("\tui-element with id " + elementID + " removed.");
        }

        
    }

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
    /// The <see cref="IOrderProvider"/> that orders both sides of the board ui.
    /// </summary>
    public class BoardSideProvider : IOrderProvider
    {
        public (UIElementDrawData drawData, bool isHovered) GetElementDrawData(UIElement element, Vector2 UIOrgin, int index)
        {
            Vector2 elementPos = UIOrgin + new Vector2(0, index * (Instance.Environment.FontDimensions.Y + UIElement.LongDefaultRectangleClearance));
            Centering.OnPixel(ref elementPos);

            Rectangle elementRectangle = element.GetBounds(elementPos.ToPoint(), Instance.Environment.FontDimensions);
            string elementText = element.Text;
            bool elementIsHovered = elementRectangle.Contains(SLMouse.VirualPosition);
            bool drawRectangle = element.Type == UIElementType.Listen;
            return (new UIElementDrawData(element.Id, elementText + ((elementIsHovered && drawRectangle) ? " <" : string.Empty), element.Type, elementPos + Vector2.Zero, elementRectangle, drawRectangle), elementIsHovered);
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
            BoardSide = new BoardSideProvider();
        }
        /// <summary>
        /// The <see cref="IOrderProvider"/> that orders the main menu.
        /// </summary>
        public static IOrderProvider Menu { get; }
        /// <summary>
        /// The <see cref="IOrderProvider"/> that orders both sides of the board ui.
        /// </summary>
        public static IOrderProvider BoardSide { get; }
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
    /// <summary>
    /// Reprecents a button or textplane in the UI. Add new elements to the <see cref="SLUserInterface"/> using the <see cref="SLUserInterface.AddElement(UIElement)"/> method.
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

        public UIElement(string id, string childOf, string? text = null, UIElementType type = UIElementType.Listen, string? order = null, params object?[] drawArgs)
        {
            Text = text ?? id;
            Type = type;
            Id = id;
            Order = order;
            ChildOf = childOf;
            DrawArguments = drawArgs;
        }
        /// <summary>
        /// Get the bounds of a <see cref="UIElement"/>, this also is its hitbox.
        /// </summary>
        /// <param name="elementPos">The position of the <see cref="UIElement"/>.</param>
        /// <param name="fontDimensions">The dimentions of the used font. <i>See: <see cref="SLEnvironment.FontDimensions"/>.</i></param>
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
        /// <param name="fontDimensions">The dimentions of the used font. <i>See: <see cref="SLEnvironment.FontDimensions"/>.</i></param>
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
            return "{"  + $"Id={Id}, Type={Type}, Text={Text}, Order={Order}, ChildOf={ChildOf}" + "}";
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
        /// <summary>
        /// Create a new <see cref="UIElementDrawData"/> object.
        /// </summary>
        /// <param name="sourceId">The source <see cref="UIElement.Id"/>.</param>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <param name="rectangle"></param>
        /// <param name="drawRectangle"></param>
        public UIElementDrawData(string sourceId, string text, UIElementType type, Vector2 position, RectangleF rectangle, bool drawRectangle)
        {
            Position = position;
            Type = type;
            Text = text;
            Rectangle = rectangle;
            DrawRectangle = drawRectangle;
            Id = sourceId;
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
