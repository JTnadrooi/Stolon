using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Point = Microsoft.Xna.Framework.Point;
using Microsoft.Xna.Framework.Content;
using Betwixt;
using MonoGame.Extended;

#nullable enable

namespace Stolon
{
    public class MenuGameState : IGameState
    {
        public string DRPStatus => "MenuState";

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
        private Action? _onLeave;

        private const int MENU_LOGO_ROW_COUNT = 5;
        private Player[]? _boardPlayers;

        public MenuGameState()
        {
            string menuDataFolder = "menuLogoMid";
            _menuLogoLines = STOLON.Textures.GetReference("textures\\" + menuDataFolder + "\\lines");
            _menuLogoDummyTiles = STOLON.Textures.GetReference("textures\\" + menuDataFolder + "\\dummyTiles");
            _menuLogoFilledTiles = STOLON.Textures.GetReference("textures\\" + menuDataFolder + "\\filledTiles");
            _menuLogoLowResFonted = STOLON.Textures.GetReference("textures\\" + menuDataFolder + "\\lowResFonted");
            _dither32 = STOLON.Textures.GetReference("textures\\dither_32");
            _drawMenuLogoLines = true;
            _drawMenuLogoDummyTiles = true;
            _drawMenuLogoFilledTiles = false;

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
                ":LOVINGSTARE:",
                ":STARE:",
                "Collida past 3.",
                "That translates to \"flour\".",
                "Index is jealous.",
                "the chairs have eyes",
            };

            _tipId = new Random().Next(0, _tips.Length);
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

        public void Update(int elapsedMiliseconds)
        {
            UpdateUI(elapsedMiliseconds);
        }
        private void UpdateUI(int elapsedMiliseconds)
        {
            int rowHeight = (int)(_menuLogoLines.Height / (float)MENU_LOGO_ROW_COUNT);
            float menuRemoveTweenerOffset = 300f * _menuRemoveTweener.Value;
            int lineFromMid = (int)(170f + menuRemoveTweenerOffset);
            bool menuFlashEnded = _milisecondsSinceStartup > _menuFlashEnd;
            int uiElementOffsetY = (int)(230f + menuRemoveTweenerOffset);
            int logoYoffset = 30;
            int menuLogoBoundingBoxClearing = 8;

            //if (_milisecondsSinceStartup < 10000) // to skip start button click and animation
            //{
            //    _milisecondsSinceStartup = 10001;
            //    _menuDone = true;
            //    _menuRemoveTweener.Update(10);

            //    _boardPlayers = new Player[]
            //            {
            //                    new Player("player0"),
            //                    new Player("player1"),
            //            };
            //    Leave();
            //    //startFrame = true;
            //}

            #region inFlash
            _menuLogoTileHider = new Rectangle(_menuLogoDrawPos.ToPoint(), new Point((int)(_menuLogoLines.Width * _menuLogoScaling), (int)(rowHeight * _menuLogoRowsHidden)));
            _milisecondsSinceStartup += elapsedMiliseconds;
            _menuLogoDrawPos = Vector2.Round(Centering.MiddleX(_menuLogoLines, logoYoffset, STOLON.Instance.VirtualDimensions.X, Vector2.One) + new Vector2(0, 8f * _menuLogoEaseTweener.Value * (1 - _menuRemoveTweener.Value)))
                + new Vector2(0, ((Centering.MiddleY(_menuLogoLines, 1, STOLON.Instance.VirtualDimensions.Y, Vector2.One).Y - logoYoffset * 1.5f) * _menuRemoveTweener.Value));
            _menuDitherTexturePositions = new Point[(int)Math.Ceiling(STOLON.Instance.VirtualDimensions.Y / (float)_dither32.Height) * 2];

            _menuFlashStart = 1200;
            _menuFlashEnd = _menuFlashStart + 400;

            _menuLine1X = (int)(STOLON.Instance.VirtualDimensions.X / 2f) - lineFromMid;
            _menuLine2X = (int)(STOLON.Instance.VirtualDimensions.X / 2f) + lineFromMid;

            _menuLineLenght = _drawMenuLogoFilledTiles ? STOLON.Instance.VirtualDimensions.Y : 0;
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
                STOLON.Debug.Log("reversed icon tweener.");
            }
            _menuLogoBoundingBox =
                new Rectangle(_menuLogoDrawPos.ToPoint() + new Point(-menuLogoBoundingBoxClearing), _menuLogoLines.Bounds.Size + new Point(menuLogoBoundingBoxClearing * 2));

            for (int i = 0; i < _menuDitherTexturePositions.Length; i++) // dithering positions.
                _menuDitherTexturePositions[i] = new Point(
                        (i >= _menuDitherTexturePositions.Length / 2f) ? _menuLine2X : _menuLine1X - _dither32.Width,
                        (i % (int)(_menuDitherTexturePositions.Length / 2f)) * _dither32.Height);

            UIOrdering.Order(STOLON.Instance.UserInterface.UIElements.Values.ToArray(), STOLON.Instance.UserInterface.MenuPath, STOLON.Instance.UserInterface.DrawData, STOLON.Instance.UserInterface.UIElementUpdateData, new Vector2(0, uiElementOffsetY), OrderProviders.Menu);

            if (STOLON.Instance.UserInterface.UIElementUpdateData["startXp"].IsClicked)
            {
                _boardPlayers = new Player[]
                        {
                            new Player("player0"),
                            new Player("player1"),
                        };

                Leave();
            }
            if (STOLON.Instance.UserInterface.UIElementUpdateData["options"].IsClicked)
            {
                STOLON.Instance.UserInterface.MenuPath = UIElement.GetSelfPath("options");
            }
            if (STOLON.Instance.UserInterface.UIElementUpdateData["sound"].IsClicked)
            {
                STOLON.Instance.UserInterface.MenuPath = UIElement.GetSelfPath("sound");
            }
            if (STOLON.Instance.UserInterface.UIElementUpdateData["volUp"].IsClicked)
            {
                STOLON.Audio.MasterVolume += 0.1001f;
                STOLON.Debug.Log("new volume: " + STOLON.Audio.MasterVolume);
            }
            if (STOLON.Instance.UserInterface.UIElementUpdateData["volDown"].IsClicked)
            {
                STOLON.Audio.MasterVolume -= 0.1001f;
                STOLON.Debug.Log("new volume: " + STOLON.Audio.MasterVolume);
            }
            if (STOLON.Instance.UserInterface.UIElementUpdateData["startStory"].IsClicked)
            {
                STOLON.Instance.UserInterface.Textframe.Queue(new DialogueInfo(STOLON.Environment, "Not yet implemented."));
            }
            if (STOLON.Instance.UserInterface.UIElementUpdateData["startCom"].IsClicked)
            {
                _boardPlayers = new Player[]
                        {
                            new Player("player0"),
                            STOLON.Environment.Entities["goldsilk"].GetPlayer()
                        };
                Leave();
            }
            if (STOLON.Instance.UserInterface.UIElementUpdateData["specialThanks"].IsClicked)
            {
                STOLON.Instance.UserInterface.Textframe.Queue(new DialogueInfo(STOLON.Environment, "Please read the github README."));
            }
            if (STOLON.Instance.UserInterface.UIElementUpdateData["quit"].IsClicked)
            {
                STOLON.Instance.SLExit();
            }

            if (!_menuDone) return;
            #endregion

            _menuRemoveTweener.Update(elapsedMiliseconds / 1000f);
            TaskHeap.Instance.SafePush("menuLogoDisapear", new DynamicTask(() => // fire and forget game logic ftw
            {
                _loadingFinished = true;
                _onLeave?.Invoke();
                _onLeave = null;
                STOLON.Environment.GameStateManager.ChangeState<BoardGameState>(true);
                ((BoardGameState)STOLON.Environment.GameStateManager.Current).SetBoard(_boardPlayers!);
                _boardPlayers = null;
            }), 2000, false);
            _milisecondsSinceMenuRemoveStart += elapsedMiliseconds;

            _tipPos = Centering.MiddleX((int)(STOLON.Fonts["fonts\\smollerMono"].FastMeasure(_tips[_tipId]).X),
                _menuLogoDrawPos.Y + _menuLogoLines.Height + (menuLogoBoundingBoxClearing * Math.Clamp(_menuRemoveTweener.Value * 2f, 0f, 1f)), STOLON.Instance.VirtualDimensions.X, Vector2.One);

            _menuRemoveLineY = (int)(_menuRemoveTweener.Value * STOLON.Instance.VirtualDimensions.Y);

            Centering.OnPixel(ref _menuLogoDrawPos);
        }
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.DrawLine(_menuLine1X, -10f, _menuLine1X, _menuLineLenght, Color.White, _menuLineWidth);
            spriteBatch.DrawLine(_menuLine2X, -10f, _menuLine2X, _menuLineLenght, Color.White, _menuLineWidth);
            if (_menuDone) spriteBatch.DrawString(STOLON.Fonts["fonts\\smollerMono"], _tips[_tipId], _tipPos, Color.White, 0f, Vector2.Zero, STOLON.Fonts["fonts\\smollerMono"].Scale, SpriteEffects.None, 1f);

            if (_drawMenuLogoLowResFonted)
            {
                for (int i = 0; i < _menuDitherTexturePositions.Length; i++)
                    spriteBatch.Draw(_dither32, _menuDitherTexturePositions[i].ToVector2(), null, Color.White, 0f, Vector2.Zero, 1f,
                        (i >= _menuDitherTexturePositions.Length / 2f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);

                spriteBatch.Draw(STOLON.Textures.Pixel, _menuLogoBoundingBox, Color.Black);
                spriteBatch.DrawRectangle(_menuLogoBoundingBox, Color.White, UserInterface.LINE_WIDTH);
            }
            if (_drawMenuLogoDummyTiles) spriteBatch.Draw(_menuLogoDummyTiles, _menuLogoDrawPos, Color.White);
            if (_drawMenuLogoFilledTiles) spriteBatch.Draw(_menuLogoFilledTiles, _menuLogoDrawPos, Color.White);
            if (_drawMenuLogoLowResFonted) spriteBatch.Draw(_menuLogoLowResFonted, _menuLogoDrawPos, Color.White);

            spriteBatch.Draw(STOLON.Textures.Pixel, _menuLogoTileHider, Color.Black);
            if (_drawMenuLogoLines) spriteBatch.Draw(_menuLogoLines, _menuLogoDrawPos, Color.White);

            int width = (int)(_menuLogoDrawPos.X - 8);
            spriteBatch.DrawLine(width, -10f, width, _menuRemoveLineY, Color.White, UserInterface.LINE_WIDTH);
            spriteBatch.DrawLine(STOLON.Instance.VirtualDimensions.X - width, -10f, STOLON.Instance.VirtualDimensions.X - width, _menuRemoveLineY, Color.White, UserInterface.LINE_WIDTH);
        }
    }
    public class BoardGameState : IGameState
    {
        public string DRPStatus => "BoardState";

        private int _lineX1;
        private int _lineX2;
        private float _lineOffset;
        private float _uiLeftOffset;
        private float _uiRightOffset;

        private Board? _board;
        public Board Board => _board ?? throw new Exception();
        /// <summary>
        /// The virtual X coordiantes of the first line (from left to right).
        /// </summary>
        public float Line1X => _lineX1;
        /// <summary>
        /// The virtual X coordiantes of the second line (from left to right).
        /// </summary>
        public float Line2X => _lineX2;

        public BoardGameState()
        {
            _lineOffset = 192f;
        }

        public void SetBoard(Player[] players) => SetBoard(new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection()));
        public void SetBoard(BoardState state)
        {
            if (BoardState.Validate(state)) _board = new Board(state);
            else throw new Exception();
        }

        public void Update(int elapsedMiliseconds)
        {
            UpdateUI(elapsedMiliseconds);
            _board?.Update(elapsedMiliseconds);
        }
        private void UpdateUI(int elapsedMiliseconds)
        {
            float zoomIntensity = ((BoardGameState)STOLON.Environment.GameStateManager.Current).Board.ZoomIntensity;
            float lineZoomOffset = zoomIntensity * 30f * (zoomIntensity < 0 ? 0.5f : 1f); // 30 being the max zoom in pixels, the last bit is smoothening the inverted zoom.

            lineZoomOffset = Math.Max(0, lineZoomOffset);

            bool mouseIsOnUI = STOLON.Input.Domain == GameInputManager.MouseDomain.UserInterfaceLow;

            _uiLeftOffset = -lineZoomOffset;
            _uiRightOffset = lineZoomOffset;

            _lineX1 = (int)(_lineOffset + _uiLeftOffset);
            _lineX2 = (int)(STOLON.Instance.VirtualDimensions.X - _lineOffset + _uiRightOffset);
        }
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            _board?.Draw(spriteBatch, elapsedMiliseconds);

            spriteBatch.Draw(STOLON.Textures.Pixel, new Rectangle(Point.Zero, new Point((int)_lineX1, 500)), Color.Black);
            spriteBatch.DrawLine(_lineX1, -10f, _lineX1, 500f, Color.White, UserInterface.LINE_WIDTH);
            spriteBatch.Draw(STOLON.Textures.Pixel, new Rectangle((int)_lineX2, 0, STOLON.Instance.VirtualDimensions.X - (int)_lineX2, 500), Color.Black);
            spriteBatch.DrawLine(_lineX2, -10f, _lineX2, 500f, Color.White, UserInterface.LINE_WIDTH);
        }
    }
}
