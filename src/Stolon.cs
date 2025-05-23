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

namespace Stolon
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
		private Effect _replaceColorEffect;
		private GameTextureCollection _textures;
        private GameFontCollection _fonts;
        private BloomFilter _bloomFilter;
        private Point _oldWindowSize;

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
			_graphics = new GraphicsDeviceManager(this);
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
			Window.AllowUserResizing = true;
			_graphics.ApplyChanges();

			_renderTarget = new RenderTarget2D(GraphicsDevice, VirtualDimensions.X, VirtualDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			_bloomRenderTarget = new RenderTarget2D(GraphicsDevice, DesiredDimensions.X, DesiredDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);


			Window.ClientSizeChanged += Window_ClientSizeChanged;
            Debug.Success();
			base.Initialize();
		}
		void Window_ClientSizeChanged(object? sender, EventArgs e)
		{
			Window.ClientSizeChanged -= new EventHandler<EventArgs>(Window_ClientSizeChanged);

			if (Window.ClientBounds.Width != _oldWindowSize.X)
			{
				_graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
				_graphics.PreferredBackBufferHeight = (int)(Window.ClientBounds.Width / AspectRatioFloat);
			}
			else if (Window.ClientBounds.Height != _oldWindowSize.Y)
			{
				_graphics.PreferredBackBufferWidth = (int)(Window.ClientBounds.Height * AspectRatioFloat);
				_graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
			}

			_graphics.ApplyChanges();

			_oldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);

			Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
		}
		public void SetDesiredResolution(Point resolution) => _desiredModifier = resolution.X / _aspectRatio.X;
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

            _replaceColorEffect = STOLON.Textures.HardLoad<Effect>("effects\\replaceColor");
			_palette = new Color[]
			{
				System.Drawing.ColorTranslator.FromHtml("#f2fbeb").ToColor(),
				System.Drawing.ColorTranslator.FromHtml("#171219").ToColor(),
			};

			_bloomFilter = new BloomFilter();
			_bloomFilter.Load(GraphicsDevice, Content, _aspectRatio.X * _desiredModifier, _aspectRatio.Y * _desiredModifier);
			_bloomFilter.BloomPreset = BloomFilter.BloomPresets.One;

            Debug.Success();
            base.LoadContent();
		}
		protected override void UnloadContent()
		{
			_bloomFilter.Dispose();
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
				else if (Environment.UI.Textframe.DialogueBounds.Contains(STOLON.Input.VirtualPosition)) STOLON.Input.Domain = GameInputManager.MouseDomain.Dialogue;
				else if (GameStateManager.IsCurrent<BoardGameState>() && STOLON.Input.VirtualPosition.X > (int)GameStateManager.GetCurrent<BoardGameState>().Line1X && STOLON.Input.VirtualPosition.X < (int)GameStateManager.GetCurrent<BoardGameState>().Line2X) STOLON.Input.Domain = GameInputManager.MouseDomain.Board;
				else STOLON.Input.Domain = GameInputManager.MouseDomain.UserInterfaceLow;

				ScreenScale = (GraphicsDevice.Viewport.Bounds.Size.ToVector2() / VirtualDimensions.ToVector2()).Y;

				_environment.Update(gameTime.ElapsedGameTime.Milliseconds);

				if (STOLON.Input.IsClicked(Keys.F)) GoFullscreen();
			}
			base.Update(gameTime);
		}
		public void GoFullscreen()
		{
			if (_graphics.IsFullScreen)
			{
				_graphics.PreferredBackBufferWidth = DesiredDimensions.X;
				_graphics.PreferredBackBufferHeight = DesiredDimensions.Y;
			}
			else
			{
				_graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
				_graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
				_graphics.ApplyChanges();
			}
			_bloomFilter.UpdateResolution(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
			_graphics.ToggleFullScreen();
			_graphics.ApplyChanges();
		}

		protected override void Draw(GameTime gameTime)
		{
			// replaceColor shader values setting.
			_replaceColorEffect.Parameters["dcolor1"].SetValue(Color.White.ToVector4());
			_replaceColorEffect.Parameters["dcolor2"].SetValue(Color.Black.ToVector4());

			_replaceColorEffect.Parameters["color1"].SetValue(_palette[0].ToVector4());
			_replaceColorEffect.Parameters["color2"].SetValue(_palette[1].ToVector4());

			GraphicsDevice.SetRenderTarget(_renderTarget);
			GraphicsDevice.Clear(STOLON.Instance.Color2);
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

			_environment.Draw(_spriteBatch, gameTime.ElapsedGameTime.Milliseconds);

            _spriteBatch.Draw(STOLON.Textures.GetReference("textures\\characters\\silo"), new Vector2(500, 0), Color.White);

            _spriteBatch.DrawString(STOLON.Fonts["fonts\\smollerMono"], "ver: " + VersionID, new Vector2(VirtualDimensions.X / 2 - STOLON.Fonts["fonts\\smollerMono"].FastMeasure("ver: " + VersionID).X / 2, 1f), Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1f);
			_spriteBatch.DrawRectangle(new Rectangle(Point.Zero, VirtualDimensions), Color.White, 1);

			_spriteBatch.End();
			//GraphicsDevice.SetRenderTarget(bloomRenderTarget);
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(STOLON.Instance.Color2);

			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None,
				RasterizerState.CullCounterClockwise, transformMatrix: Matrix.CreateScale(ScreenScale), effect: _replaceColorEffect);
			_spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
			_spriteBatch.End();

			//         Texture2D bloom = _bloomFilter.Draw(bloomRenderTarget, DesiredDimensions.X, DesiredDimensions.Y);
			//         GraphicsDevice.SetRenderTarget(null);
			//         GraphicsDevice.Clear(StolonGame.Instance.Color2);

			////spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			//spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

			//spriteBatch.Draw(bloomRenderTarget, Vector2.Zero, Color.White);
			//spriteBatch.Draw(bloom, Vector2.Zero, Color.White);
			//spriteBatch.End();

			_replaceColorEffect.CurrentTechnique.Passes[0].Apply();

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
        public const string MEDIUM_FONT_ID = "fonts\\pixeloidMono";
		public const string SMALL_FONT_ID = "fonts\\smollerMono";
		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

	public static class SLMouse
	{
	}
	public static class SLKeyboard
	{
	}
}
