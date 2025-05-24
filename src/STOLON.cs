using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.Debug;
using MonoGame.Extended;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using DiscordRPC;
using DiscordRPC.Events;
using Microsoft.Xna.Framework.Media;
using System.Drawing;
using Microsoft.Xna.Framework.Content;

#nullable enable

namespace STOLON
{
    public partial class STOLON : Game
    {
        private GraphicsDeviceManager _graphics;
        private GameInputManager _input;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;

        private RenderTarget2D _bloomRenderTarget;
        private GameEnvironment _environment;
        private Point _aspectRatio = new Point(16, 9);
        private float AspectRatioFloat => _aspectRatio.X / _aspectRatio.Y * 1.7776f;
        private int _virtualModifier;
        private int _desiredModifier;
        private Color[] _palette;
        private GameTextureCollection _textures;
        private GameFontCollection _fonts;
        private Point _oldWindowSize;
        private EffectPipeline _post;

        public DiscordRichPresence DRP { get; set; }
        public UserInterface UserInterface => _environment.UI;
        public Rectangle VirtualBounds => new Rectangle(Point.Zero, VirtualDimensions);
        public Point VirtualDimensions => new Point(_aspectRatio.X * _virtualModifier, _aspectRatio.Y * _virtualModifier); //  (912, 513) (if vM = 57) - (480, 270) (if vM = 30)
        public Point DesiredDimensions => new Point(_aspectRatio.X * _desiredModifier, _aspectRatio.Y * _desiredModifier);
        public Point ScreenCenter => new Point(VirtualDimensions.X / 2, VirtualDimensions.Y / 2);
        public float ScreenScale { get; private set; }

        public SpriteBatch SpriteBatch => _spriteBatch;
        public GraphicsDeviceManager GraphicsDeviceManager => _graphics;
        public Color Color1 => _palette[0];
        public Color Color2 => _palette[1];

        public string VersionID => "0.051 (Open Alpha)";

#pragma warning disable CS8618
        public STOLON()
#pragma warning restore CS8618
        {
            Instance = this;
            _graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
            };
            Content.RootDirectory = "content";
            IsMouseVisible = true;

            Debug = new DebugStream(header: "stolon");
            Debug.Silent = false;
            Audio = new AudioEngine();
        }

        protected override void Initialize()
        {
            Debug.Log(">[s]initializing stolon");
            DRP = new DiscordRichPresence();
            DRP.UpdateDetails("Initializing..");

            _oldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);

            _desiredModifier = 57;
            _virtualModifier = 57; //(prev = 30, so = x1.9)

            _graphics.PreferredBackBufferWidth = DesiredDimensions.X;
            _graphics.PreferredBackBufferHeight = DesiredDimensions.Y;
            _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 0;
            _graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();

            _renderTarget = new RenderTarget2D(GraphicsDevice, VirtualDimensions.X, VirtualDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            _bloomRenderTarget = new RenderTarget2D(GraphicsDevice, DesiredDimensions.X, DesiredDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Debug.Success();
            base.Initialize();
        }
        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            RecalculateScreenScaleAndViewport();
        }
        private void RecalculateScreenScaleAndViewport()
        {
            var windowWidth = Window.ClientBounds.Width;
            var windowHeight = Window.ClientBounds.Height;

            float targetAspectRatio = (float)VirtualDimensions.X / VirtualDimensions.Y;

            int newWidth = windowWidth;
            int newHeight = (int)(newWidth / targetAspectRatio);

            if (newHeight > windowHeight)
            {
                newHeight = windowHeight;
                newWidth = (int)(newHeight * targetAspectRatio);
            }

            int offsetX = (windowWidth - newWidth) / 2;
            int offsetY = (windowHeight - newHeight) / 2;

            GraphicsDevice.Viewport = new Viewport(offsetX, offsetY, newWidth, newHeight);

            ScreenScale = newWidth / (float)VirtualDimensions.X;
        }

        protected override void LoadContent()
        {
            Debug.Log(">[s]loading stolon content");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _textures = new GameTextureCollection(Content);
            _fonts = new GameFontCollection(Content);
            _input = new GameInputManager();

            STOLON.Textures = _textures;
            STOLON.Fonts = _fonts;
            STOLON.Input = _input;

            _environment = new GameEnvironment();
            STOLON.Environment = _environment;

            _environment.Initialize();

            _palette = new Color[]
            {
                System.Drawing.ColorTranslator.FromHtml("#f2fbeb").ToColor(),
                System.Drawing.ColorTranslator.FromHtml("#171219").ToColor(),
            };
            _post = new EffectPipeline(GraphicsDevice, _spriteBatch, VirtualDimensions.X, VirtualDimensions.Y);

            _post.AddEffect(new ReplaceColorEffect(Content.Load<Effect>("effects\\ReplaceColor"))
            {
                Target1 = Color.White,
                Target2 = Color.Black,
                Replace1 = _palette[0],
                Replace2 = _palette[1]
            });

            Debug.Success();
            base.LoadContent();
        }
        protected override void UnloadContent()
        {

        }
        public void SLExit()
        {
            MediaPlayer.Stop();
            Audio.Dispose();
            Exit();
        }
        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                STOLON.Input.PreviousMouse = STOLON.Input.CurrentMouse;
                STOLON.Input.CurrentMouse = Mouse.GetState();

                STOLON.Input.PreviousKeyboard = STOLON.Input.CurrentKeyboard;
                STOLON.Input.CurrentKeyboard = Keyboard.GetState();

                if (!GraphicsDevice.Viewport.Bounds.Contains(STOLON.Input.CurrentMouse.Position)) STOLON.Input.Domain = GameInputManager.MouseDomain.OfScreen;
                else if (Environment.UI.Textframe.DialogueBounds.Contains(STOLON.Input.VirtualMousePos)) STOLON.Input.Domain = GameInputManager.MouseDomain.Dialogue;
                else if (GameStateManager.IsCurrent<BoardGameState>() && STOLON.Input.VirtualMousePos.X > (int)GameStateManager.GetCurrent<BoardGameState>().Line1X && STOLON.Input.VirtualMousePos.X < (int)GameStateManager.GetCurrent<BoardGameState>().Line2X) STOLON.Input.Domain = GameInputManager.MouseDomain.Board;
                else STOLON.Input.Domain = GameInputManager.MouseDomain.UserInterfaceLow;

                ScreenScale = (GraphicsDevice.Viewport.Bounds.Size.ToVector2() / VirtualDimensions.ToVector2()).Y;

                _environment.Update(gameTime.ElapsedGameTime.Milliseconds);

                if (STOLON.Input.IsClicked(Keys.F)) GoFullscreen();
            }
            base.Update(gameTime);
        }
        public void GoFullscreen()
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;

            if (_graphics.IsFullScreen)
            {
                var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                _graphics.PreferredBackBufferWidth = display.Width;
                _graphics.PreferredBackBufferHeight = display.Height;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = VirtualDimensions.X; // Or whatever window size you want
                _graphics.PreferredBackBufferHeight = VirtualDimensions.Y;
            }

            _graphics.ApplyChanges();
            RecalculateScreenScaleAndViewport();
        }


        protected override void Draw(GameTime gameTime)
        {
            int elapsedMiliseconds = gameTime.ElapsedGameTime.Milliseconds;
            _post.BeginScene();
            GraphicsDevice.Clear(STOLON.Instance.Color2);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            _environment.Draw(_spriteBatch, gameTime.ElapsedGameTime.Milliseconds);
            _spriteBatch.Draw(STOLON.Textures.GetReference("textures\\characters\\silo"), new Vector2(500, 0), Color.White);
            _spriteBatch.DrawPoint(STOLON.Input.VirtualMousePos, Color1, 10);
            _spriteBatch.DrawString(STOLON.Fonts["fonts\\smollerMono"], "ver: " + VersionID, new Vector2(VirtualDimensions.X / 2 - STOLON.Fonts["fonts\\smollerMono"].FastMeasure("ver: " + VersionID).X / 2, 1f), Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1f);
            _spriteBatch.DrawRectangle(new Rectangle(Point.Zero, VirtualDimensions), Color.White, 1);

            _spriteBatch.End();
            _post.EndScene();

            base.Draw(gameTime);
        }
    }
    public partial class STOLON
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static STOLON Instance { get; private set; }
        public static GameTextureCollection Textures { get; private set; }
        public static GameFontCollection Fonts { get; private set; }
        public static AudioEngine Audio { get; private set; }
        public static DebugStream Debug { get; private set; }
        public static GameEnvironment Environment { get; private set; }
        public static GameInputManager Input { get; private set; }
        public static GameStateManager StateManager { get; internal set; }
        public const string MEDIUM_FONT_ID = "fonts\\pixeloidMono";
        public const string SMALL_FONT_ID = "fonts\\smollerMono";
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
