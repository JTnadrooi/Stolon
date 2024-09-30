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

#nullable enable

namespace Stolon
{
    public class SLUserInterface : AxComponent
    {
        private Board board;
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


        private Dictionary<string, UIElement> uiElements;

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
        public ReadOnlyDictionary<string, UIElement> UIElements => new ReadOnlyDictionary<string, UIElement>(uiElements);

        #endregion


        private SLTextframe textframe;
        public SLTextframe Textframe => textframe;
        public int LineWidth => lineWidth;

        internal SLUserInterface(SLScene scene, Dictionary<string, SLEntity> sLEntities) : base(Instance.Environment)
        {
            environment = Instance.Environment;
            board = scene.Board;

            lineOffset = 96f;
            lineWidth = 2;

            uifont = Instance.Fonts["fiont"];


            menuLogoLines = Instance.Textures.GetReference("textures\\menuLogo\\lines");
            menuLogoDummyTiles = Instance.Textures.GetReference("textures\\menuLogo\\dummyTiles");
            menuLogoFilledTiles = Instance.Textures.GetReference("textures\\menuLogo\\filledTiles");
            menuLogoLowResFonted = Instance.Textures.GetReference("textures\\menuLogo\\lowResFonted");
            dither8x8 = Instance.Textures.GetReference("textures\\dither8x8");

            uiElements = new Dictionary<string, UIElement>();

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
                "All colors, Her.",
                "Thanks for playing! :D",
                "\"Call that a Natural Deadline.\"",
                "Nue not included!",
                "Assembling the Pharos..",
                "Fishing update when?",
                "Hitting the Deadline!",
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
                "You are week, I am month."
            };

            tipId = new Random().Next(0, tips.Length);


            StolonGame.Instance.DebugStream.WriteLine("\t[s]initializing ui..");
            AddElement(new UIElement("exitGame", UIDock.Left, "Exit Game", UIElementType.Clickable));

            AddElement(new UIElement("screenRegion2", UIDock.Left, string.Empty, UIElementType.Text));
            AddElement(new UIElement("screenRegion", UIDock.Left, "Screen & Camera", UIElementType.Text));
            AddElement(new UIElement("toggleFullscreen", UIDock.Left, "Go Fullscreen", UIElementType.Clickable));
            AddElement(new UIElement("centerCamera", UIDock.Left, "Center Camera", UIElementType.Clickable));

            AddElement(new UIElement("boardRegion2", UIDock.Left, string.Empty, UIElementType.Text));
            AddElement(new UIElement("boardRegion", UIDock.Left, "Board", UIElementType.Text));
            AddElement(new UIElement("undoMove", UIDock.Left, "Undo", UIElementType.Clickable));
            AddElement(new UIElement("restartBoard", UIDock.Left, "Restart", UIElementType.Clickable));
            AddElement(new UIElement("boardSearch", UIDock.Left, "Search", UIElementType.Clickable));
            AddElement(new UIElement("skipMove", UIDock.Left, "End Move", UIElementType.Clickable));

            AddElement(new UIElement("currentPlayer", UIDock.Right, null, UIElementType.Text));

            AddElement(new UIElement("startStory", UIDock.MainMenu, "Story", UIElementType.Clickable));
            AddElement(new UIElement("startCom", UIDock.MainMenu, "COM", UIElementType.Clickable));
            AddElement(new UIElement("startXp", UIDock.MainMenu, "2P", UIElementType.Clickable));
            AddElement(new UIElement("options", UIDock.MainMenu, "Options", UIElementType.Clickable));
            AddElement(new UIElement("specialThanks", UIDock.MainMenu, "Special Thanks :D", UIElementType.Clickable));
            AddElement(new UIElement("quit", UIDock.MainMenu, "Quit", UIElementType.Clickable));
        }
        private void ResetElementData()
        {
            updateData.Clear();
            foreach (UIElement uiElement in uiElements.Values)
                updateData.Add(uiElement.Id, new UIElementUpdateData(false, uiElement.Id));
            drawData.Clear();
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

        internal void GameForced()
        {
            milisecondsSinceStartup = 16;
            menuDone = false;
            menuRemoveTweener.Reset();
            menuRemoveTweener.Start();
        }

        public void UpdateMenuUI(int elapsedMiliseconds)
        {
            int rowHeight = (int)(menuLogoLines.Height / (float)menuLogoRowCount);
            float menuRemoveTweenerOffset = 200f * menuRemoveTweener.Value;
            int lineFromMid = (int)(110f + menuRemoveTweenerOffset);
            bool menuFlashEnded = milisecondsSinceStartup > menuFlashEnd;
            int uiElementOffsetY = (int)(130f + menuRemoveTweenerOffset);
            int logoYoffset = 30;
            bool startFrame = false;
            int menuLogoBoundingBoxClearing = 8;

            // if (milisecondsSinceStartup < 10000) // to skip start button click and animation
            // {
            //     milisecondsSinceStartup = 10001;
            //     menuDone = true;
            //     menuRemoveTweener.Update(10);
            //     startFrame = true;
            // }

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

            Ordering.Order(uiElements.Values.ToArray(),
                drawData, updateData,
                UIDock.MainMenu, new Vector2(0, uiElementOffsetY), 2, true);


            if (updateData["startXp"].IsClicked)
            {
                startFrame = !menuDone;
                menuDone = true;
            }
            if (updateData["options"].IsClicked)
            {
                textframe.Queue(new DialogueInfo(Instance.Environment, "Not yet implemented."));
            }
            if (updateData["startStory"].IsClicked)
            {
                textframe.Queue(new DialogueInfo(Instance.Environment, "Not yet implemented."));
            }
            if (updateData["startCom"].IsClicked)
            {
                textframe.Queue(new DialogueInfo(Instance.Environment, "Not yet implemented."));
            }
            if (updateData["specialThanks"].IsClicked)
            {
                textframe.Queue(new DialogueInfo(Instance.Environment, "Please read the README :D"));
            }
            if (updateData["quit"].IsClicked)
            {
                Instance.Exit();
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
                loadingFinished = true;
                SLEnvironment.Instance.ForceGameState(SLEnvironment.SLGameState.OpenBoard);
            }

            Centering.OnPixel(ref menuLogoDrawPos);
        }
        public void UpdateBoardUI(int elapsedMiliseconds)
        {
            float zoomIntensity = board.ZoomIntensity;
            float lineZoomOffset = zoomIntensity * 30f * (zoomIntensity < 0 ? 0.5f : 1f); // 30 being the max zoom in pixels, the last bit is smoothening the inverted zoom.

            bool mouseIsOnUI = SLMouse.Domain == SLMouse.MouseDomain.UserInterfaceLow;

            uiLeftOffset = -lineZoomOffset;
            uiRightOffset = lineZoomOffset;

            lineX1 = (int)(lineOffset + uiLeftOffset);
            lineX2 = (int)(Instance.VirtualDimensions.X - lineOffset + uiRightOffset);

            Ordering.Order(uiElements.Values.ToArray(), drawData, updateData, UIDock.Left, new Vector2(uiLeftOffset, 0) + new Vector2(5));
            Ordering.Order(uiElements.Values.ToArray(), drawData, updateData, UIDock.Right, new Vector2(lineX2, 0) + new Vector2(5));
        }
        /// <summary>
        /// Get random splash text.
        /// </summary>
        /// <returns>A random splash text.</returns>
        public string GetRandomSplashText() => tips[new Random().Next(0, tips.Length)];
        public string GetRandomSplashText(ref int i)
        {
            int tipId = new Random().Next(0, tips.Length);
            i = tipId;
            return tips[tipId];
        }
        public string ShowPercentage(string text, float coefficient) => text.Substring(0, (int)(text.Length * coefficient));
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
        public void AddElement(UIElement element)
        {
            uiElements.Add(element.Id, element);
            Instance.DebugStream.WriteLine("\tui-element with id " + element.Id + " added.");
            updateData.Add(element.Id, default);
            Instance.DebugStream.WriteLine("\tghost-ui-element with id " + element.Id + " added.");
        }
        public void RemoveElement(string elementID)
        {
            uiElements.Remove(elementID);
            Instance.DebugStream.WriteLine("\tui-element with id " + elementID + " removed.");
        }

        public static class Ordering
        {
            public static void Order(UIElement[] uIElements,
                ICollection<UIElementDrawData> drawDump, IDictionary<string, UIElementUpdateData> updateDump, // dumps.
                UIDock dockstyle, Vector2 uiOrgin, // args.
                int lineClearance = UIElement.DefaultRectangleClearance * 2 + 1, bool isMouseRelevant = true, bool exclusive = true,
                Func<UIElement, Point, Rectangle>? rectangleProvider = null,
                Func<UIElement, UIElementUpdateData, (string, Point)>? textProvider = null)
            {
                rectangleProvider ??= (element, pos) => element.GetBounds(pos, Instance.Environment.FontDimensions);
                textProvider ??= DefaultTextProvider;

                int index = 0;

                for (int i = 0; i < uIElements.Length; i++)
                {
                    UIElement uiElement = uIElements[i];

                    if (exclusive && uiElement.Dock != dockstyle) continue;

                    switch (dockstyle)
                    {
                        case UIDock.MainMenu:
                            {
                                Vector2 elementPos = Centering.MiddleX(
                                    Instance.Environment.FontDimensions.X * uiElement.Text.Length,
                                    index * (Instance.Environment.FontDimensions.Y + lineClearance) * 1.5f + uiOrgin.Y,
                                    Instance.VirtualDimensions.X, Vector2.One);
                                Rectangle elementBounds = new Rectangle(elementPos.ToPoint(), new Point(Instance.Environment.FontDimensions.X * uiElement.Text.Length, Instance.Environment.FontDimensions.Y));
                                bool elementIsHovered = elementBounds.Contains(SLMouse.VirualPosition) && isMouseRelevant;
                                string elementText = uiElement.Text;


                                updateDump[uiElement.Id] = new UIElementUpdateData(elementIsHovered, uiElement.Id);
                                var textProviderReturned = textProvider.Invoke(uiElement, updateDump[uiElement.Id]);
                                drawDump.Add(new UIElementDrawData(uiElement.Id, textProviderReturned.Item1, uiElement.Type, elementPos + textProviderReturned.Item2.ToVector2(), Rectangle.Empty, false));
                            }
                            break;
                        case UIDock.Right:
                        case UIDock.Left:
                            {
                                Vector2 elementPos = uiOrgin + new Vector2(0, index * (Instance.Environment.FontDimensions.Y + lineClearance));
                                Rectangle elementRectangle = rectangleProvider(uiElement, elementPos.ToPoint());
                                string elementText = uiElement.Text;

                                bool elementIsHovered = elementRectangle.Contains(SLMouse.VirualPosition) && isMouseRelevant;
                                bool elementIsPressed = elementIsHovered && SLMouse.IsPressed(SLMouse.MouseButton.Left);
                                bool drawRectangle = uiElement.Type == UIElementType.Clickable;
                                if (!drawRectangle) // if there is no rectangle, the element is not clickable (or hoverable)
                                {
                                    elementIsPressed = false;
                                    elementIsHovered = false;
                                }


                                updateDump[uiElement.Id] = new UIElementUpdateData(elementIsHovered, uiElement.Id);
                                var textProviderReturned = textProvider.Invoke(uiElement, updateDump[uiElement.Id]);
                                drawDump.Add(new UIElementDrawData(uiElement.Id, textProviderReturned.Item1, uiElement.Type, elementPos + textProviderReturned.Item2.ToVector2(), elementRectangle, drawRectangle));

                                if (updateDump[uiElement.Id].IsClicked) Instance.DebugStream.WriteLine("\tui-element with id " + uiElement.Id + " clicked.");
                            }
                            break;
                    }
                    index++;
                }
            }
            public static (string, Point) DefaultTextProvider(UIElement element, UIElementUpdateData updateData)
            {
                string elementText = element.Text;
                bool elementIsHovered = updateData.IsHovered;
                switch (element.Dock)
                {
                    case UIDock.Right:
                    case UIDock.Left: return (elementText + (elementIsHovered ? " <" : string.Empty), Point.Zero);
                    case UIDock.MainMenu:
                        {
                            string postPre = element.Id switch
                            {
                                "quit" => "x",
                                "specialThanks" => "!",
                                _ => ">",
                            };
                            return (elementIsHovered ? (postPre + " " + elementText + " " + postPre.Replace(">", "<")) : elementText, elementIsHovered ? new Point(-Instance.Environment.FontDimensions.X * 2, 0) : Point.Zero);
                        }
                    default: throw new Exception();
                };
            }
        }
    }
    public class UIElement
    {
        public enum UIDock
        {
            Left,
            Right,
            MainMenu,
        }
        public enum UIElementType
        {
            Clickable,
            Text,
        }

        public UIElementType Type { get; }
        public UIDock Dock { get; }
        public string Text { get; set; }
        public string Id { get; }
        public string? Order { get; }
        public string? SubElementOf { get; }

        public UIElement(string id, UIDock dock, string? text = null, UIElementType type = UIElementType.Text, string? subElementOf = null, string? order = null)
        {
            Text = text ?? id;
            Type = type;
            Id = id;
            SubElementOf = subElementOf;
            Order = order;
            Dock = dock;
        }
        public Rectangle GetBounds(Point elementPos, Point fontDimensions, int clearance = DefaultRectangleClearance, bool supportMultiline = false, string fontId = "", int posOffsetX = 0, int posOffsetY = 0)
            => GetBounds(elementPos, Text, fontDimensions, clearance, supportMultiline, fontId, posOffsetX, posOffsetY);

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

        public const int DefaultRectangleClearance = 2;
    }
    public struct UIElementDrawData
    {
        public Vector2 Position { get; }
        public bool DrawRectangle { get; }
        public RectangleF Rectangle { get; }
        public UIElementType Type { get; }
        public string Text { get; }
        public string Id { get; }
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
    public struct UIElementUpdateData
    {
        public bool IsHovered { get; }
        public bool IsPressed => IsHovered && SLMouse.CurrentState.LeftButton == ButtonState.Pressed;
        public bool IsClicked => IsPressed && SLMouse.PreviousState.LeftButton == ButtonState.Released;
        public string SourcID { get; }
        public UIElementUpdateData(bool isHovered, string sourcID)
        {
            IsHovered = isHovered;
            SourcID = sourcID;
        }
    }
}
