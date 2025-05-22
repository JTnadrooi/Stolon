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
using static Stolon.StolonGame;
#nullable enable

namespace Stolon
{
	//public class FontCollection : IContentCollection<GameFont>
 //   {
 //       public FontCollection(ContentManager contentManager)
	//	{

	//	}

 //   }
    public partial class StolonGame : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private RenderTarget2D _renderTarget;

        private RenderTarget2D _bloomRenderTarget;
		private StolonEnvironment _environment;
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

        public StolonEnvironment Environment => _environment;
		public Scene Scene
		{
			get => _environment.Scene;
			set => _environment.Scene = value;
		}
		public AudioEngine AudioEngine { get; private set; }
        public DiscordRichPresence DRP { get; set; }
        public UserInterface UserInterface => _environment.UI;
		public Rectangle VirtualBounds => new Rectangle(Point.Zero, VirtualDimensions);
		public Point VirtualDimensions => new Point(_aspectRatio.X * _virtualModifier, _aspectRatio.Y * _virtualModifier); //  (912, 513) (if vM = 57) - (480, 270) (if vM = 30)
        public Point DesiredDimensions => new Point(_aspectRatio.X * _desiredModifier, _aspectRatio.Y * _desiredModifier);
		public Point ScreenCenter => new Point(VirtualDimensions.X / 2, VirtualDimensions.Y / 2);
        public float ScreenScale { get; private set; }


        public GameTextureCollection Textures => _textures;
		public GameFontCollection Fonts => _fonts;

		public SpriteBatch SpriteBatch => _spriteBatch;
		public GraphicsDeviceManager GraphicsDeviceManager => _graphics;
		public DebugStream DebugStream { get; }
		public Color Color1 => _palette[0];
		public Color Color2 => _palette[1];

		public string VersionID => "0.051 (Open Alpha)";

		#pragma warning disable CS8618
		public StolonGame()
		#pragma warning restore CS8618
		{
			Instance = this;
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "content";
			IsMouseVisible = true;

            DebugStream = new DebugStream(header: "stolon");
            DebugStream.Silent = false;
            AudioEngine = new AudioEngine();
        }

		protected override void Initialize()
		{
			DebugStream.Log(">[s]initializing stolon");
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
			DebugStream.Success();
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
			DebugStream.Log(">[s]loading stolon content");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
			_textures = new GameTextureCollection(Content);
            _fonts = new GameFontCollection(Content);

			_environment = new StolonEnvironment();
			_environment.Initialize();

            _replaceColorEffect = Instance.Textures.HardLoad<Effect>("effects\\replaceColor");
			_palette = new Color[]
			{
				System.Drawing.ColorTranslator.FromHtml("#f2fbeb").ToColor(),
				System.Drawing.ColorTranslator.FromHtml("#171219").ToColor(),
			};

			_bloomFilter = new BloomFilter();
			_bloomFilter.Load(GraphicsDevice, Content, _aspectRatio.X * _desiredModifier, _aspectRatio.Y * _desiredModifier);
			_bloomFilter.BloomPreset = BloomFilter.BloomPresets.One;

            DebugStream.Success();
            base.LoadContent();
		}
		protected override void UnloadContent()
		{
			_bloomFilter.Dispose();
		}
		public void SLExit()
		{
			MediaPlayer.Stop();
			AudioEngine.Audio.Dispose();
			Exit();
		}
		protected override void Update(GameTime gameTime)
		{
			if (IsActive)
			{
				SLMouse.PreviousState = SLMouse.CurrentState;
				SLMouse.CurrentState = Mouse.GetState();

				SLKeyboard.PreviousState = SLKeyboard.CurrentState;
				SLKeyboard.CurrentState = Keyboard.GetState();

				if (!GraphicsDevice.Viewport.Bounds.Contains(SLMouse.CurrentState.Position)) SLMouse.Domain = SLMouse.MouseDomain.OfScreen;
				else if (Environment.UI.Textframe.DialogueBounds.Contains(SLMouse.VirualPosition)) SLMouse.Domain = SLMouse.MouseDomain.Dialogue;
				else if (SLMouse.VirualPosition.X > (int)UserInterface.Line1X && SLMouse.VirualPosition.X < (int)UserInterface.Line2X) SLMouse.Domain = SLMouse.MouseDomain.Board;
				else SLMouse.Domain = SLMouse.MouseDomain.UserInterfaceLow;

				ScreenScale = (GraphicsDevice.Viewport.Bounds.Size.ToVector2() / VirtualDimensions.ToVector2()).Y;

				_environment.Update(gameTime.ElapsedGameTime.Milliseconds);

				if (SLKeyboard.IsClicked(Keys.F)) GoFullscreen();
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
			GraphicsDevice.Clear(Instance.Color2);
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

			_environment.Draw(_spriteBatch, gameTime.ElapsedGameTime.Milliseconds);

            _spriteBatch.Draw(Textures.GetReference("textures\\characters\\silo"), new Vector2(500, 0), Color.White);

            _spriteBatch.DrawString(Fonts["fonts\\smollerMono"], "ver: " + VersionID, new Vector2(VirtualDimensions.X / 2 - Fonts["fonts\\smollerMono"].FastMeasure("ver: " + VersionID).X / 2, 1f), Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1f);
			_spriteBatch.DrawRectangle(new Rectangle(Point.Zero, VirtualDimensions), Color.White, 1);

			_spriteBatch.End();
			//GraphicsDevice.SetRenderTarget(bloomRenderTarget);
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(Instance.Color2);

			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None,
				RasterizerState.CullCounterClockwise, transformMatrix: Matrix.CreateScale(ScreenScale), effect: _replaceColorEffect);
			_spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
			_spriteBatch.End();

			//         Texture2D bloom = _bloomFilter.Draw(bloomRenderTarget, DesiredDimensions.X, DesiredDimensions.Y);
			//         GraphicsDevice.SetRenderTarget(null);
			//         GraphicsDevice.Clear(Instance.Color2);

			////spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
			//spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);

			//spriteBatch.Draw(bloomRenderTarget, Vector2.Zero, Color.White);
			//spriteBatch.Draw(bloom, Vector2.Zero, Color.White);
			//spriteBatch.End();

			_replaceColorEffect.CurrentTechnique.Passes[0].Apply();

			base.Draw(gameTime);
		}
	}
	public partial class StolonGame
	{
		#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static StolonGame Instance { get; private set; }
		public const string MEDIUM_FONT_ID = "fonts\\pixeloidMono";
		public const string SMALL_FONT_ID = "fonts\\smollerMono";
		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

	public static class SLMouse
	{
		public enum MouseButton
		{
			Left,
			Middle,
			Right,
		}
		public enum MouseDomain
		{
			OfScreen,
			Board,
			UserInterfaceLow,
			UserInterfaceHigh,
			Dialogue,
		}
		public static MouseDomain Domain { get; internal set; }
		public static MouseState PreviousState { get; internal set; }
		public static MouseState CurrentState { get; internal set; }
		public static Vector2 VirualPosition => CurrentState.Position.ToVector2() / Instance.ScreenScale;
		public static bool IsPressed(MouseButton button) => IsPressed(CurrentState, button);
		private static bool IsPressed(MouseState state, MouseButton button) => button switch
		{
			MouseButton.Left => state.LeftButton,
			MouseButton.Middle => state.MiddleButton,
			MouseButton.Right => state.RightButton,
			_ => throw new Exception(),
		} == ButtonState.Pressed;

		public static bool IsClicked(MouseButton button) => IsPressed(CurrentState, button) && !IsPressed(PreviousState, button);
	}
	public static class SLKeyboard
	{
		public static KeyboardState CurrentState { get; internal set; }
		public static KeyboardState PreviousState { get; internal set; }
		private static bool IsPressed(KeyboardState state, Keys key) => state.IsKeyDown(key);
		public static bool IsPressed(Keys key) => IsPressed(CurrentState, key);
		public static bool IsClicked(Keys key) => IsPressed(CurrentState, key) && !IsPressed(PreviousState, key);
	}
}
