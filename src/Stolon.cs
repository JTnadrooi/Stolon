using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.Debug;
using AsitLib.XNA;
using MonoGame.Extended;
using static Stolon.StolonGame;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

#nullable enable

namespace Stolon
{
	public partial class StolonGame : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private RenderTarget2D renderTarget;
		private SLEnvironment environment;
		private Point aspectRatio = new Point(16, 9);
		private float AspectRatioFloat => aspectRatio.X / aspectRatio.Y * 1.7776f;
		// private float AspectRatioFloat => MathF.Pow(aspectRatio.X / aspectRatio.Y, 2f);
		private int virtualModifier;
		private int desiredModifier;
		private Color[] palette;
		private Effect replaceColorEffect;
		private AxTextureCollection textures;

		public SLEnvironment Environment => environment;
		public SLScene Scene => environment.Scene;
		public SLUserInterface UserInterface => environment.UserInterface;
		public Rectangle VirtualBounds => new Rectangle(Point.Zero, VirtualDimensions);
		public Point VirtualDimensions => new Point(aspectRatio.X * virtualModifier, aspectRatio.Y * virtualModifier); // (480, 270) (if vM = 30)
		public Point DesiredDimensions => new Point(aspectRatio.X * desiredModifier, aspectRatio.Y * desiredModifier);
		public Point ScreenCenter => new Point(VirtualDimensions.X / 2, VirtualDimensions.Y / 2);

		public AxTextureCollection Textures => textures;
		public Dictionary<string, SpriteFont> Fonts { get; }

		public SpriteBatch SpriteBatch => spriteBatch;
		public GraphicsDeviceManager GraphicsDeviceManager => graphics;
		public AsitDebugStream DebugStream { get; }
		public Color Color1 => palette[0];
		public Color Color2 => palette[1];

		public string VersionID => "0.049c (Open Alpha)";

		Point oldWindowSize;

		internal float screenScale;

#pragma warning disable CS8618
		public StolonGame()
#pragma warning restore CS8618
		{
			Instance = this;
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Fonts = new Dictionary<string, SpriteFont>();

			DebugStream = new AsitDebugStream();

		}

		protected override void Initialize()
		{
			DebugStream.WriteLine("[s]initializing..");

			oldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);

			desiredModifier = 67; // 67
			virtualModifier = 30; // 30

			graphics.PreferredBackBufferWidth = DesiredDimensions.X;
			graphics.PreferredBackBufferHeight = DesiredDimensions.Y;
			Window.AllowUserResizing = true;
			graphics.ApplyChanges();

			renderTarget = new RenderTarget2D(GraphicsDevice, VirtualDimensions.X, VirtualDimensions.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

			Window.ClientSizeChanged += Window_ClientSizeChanged;
			DebugStream.Succes();
			base.Initialize();
		}
		void Window_ClientSizeChanged(object? sender, EventArgs e)
		{
			Window.ClientSizeChanged -= new EventHandler<EventArgs>(Window_ClientSizeChanged);

			if (Window.ClientBounds.Width != oldWindowSize.X)
			{ 
				graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
				graphics.PreferredBackBufferHeight = (int)(Window.ClientBounds.Width / AspectRatioFloat);
			}
			else if (Window.ClientBounds.Height != oldWindowSize.Y)
			{
				graphics.PreferredBackBufferWidth = (int)(Window.ClientBounds.Height * AspectRatioFloat);
				graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
			}

			graphics.ApplyChanges();

			oldWindowSize = new Point(Window.ClientBounds.Width, Window.ClientBounds.Height);

			Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
		}
		public void SetDesiredResolution(Point resolution) => desiredModifier = resolution.X / aspectRatio.X;
		protected override void LoadContent()
		{
			DebugStream.WriteLine("[s]loading content..");

			spriteBatch = new SpriteBatch(GraphicsDevice);
			textures = new AxTextureCollection(Content);
			Fonts.Add("fiont", textures.HardLoad<SpriteFont>("fonts\\fiont"));

			environment = new SLEnvironment();

			replaceColorEffect = Instance.Textures.HardLoad<Effect>("effects\\replaceColor");
			palette = new Color[]
			{
				System.Drawing.ColorTranslator.FromHtml("#f2fbeb").ToColor(),
				System.Drawing.ColorTranslator.FromHtml("#171219").ToColor(),
			};

			DebugStream.Succes();
			base.LoadContent();
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
				else if (Environment.UserInterface.Textframe.DialogueBounds.Contains(SLMouse.VirualPosition)) SLMouse.Domain = SLMouse.MouseDomain.Dialogue;
				else if (SLMouse.VirualPosition.X > (int)UserInterface.Line1X && SLMouse.VirualPosition.X < (int)UserInterface.Line2X) SLMouse.Domain = SLMouse.MouseDomain.Board;
				else SLMouse.Domain = SLMouse.MouseDomain.UserInterfaceLow;

				screenScale = (GraphicsDevice.Viewport.Bounds.Size.ToVector2() / VirtualDimensions.ToVector2()).Y;

				environment.Update(gameTime.ElapsedGameTime.Milliseconds);

				if (SLKeyboard.IsClicked(Keys.F) || UserInterface.UIElementUpdateData["toggleFullscreen"].IsClicked) GoFullscreen();
			}
			base.Update(gameTime);
		}
		public void GoFullscreen()
		{
			if (graphics.IsFullScreen)
			{
				UserInterface.UIElements["toggleFullscreen"].Text = "Go Fullscreen";
				graphics.PreferredBackBufferWidth = DesiredDimensions.X;
				graphics.PreferredBackBufferHeight = DesiredDimensions.Y;
			}
			else
			{
				UserInterface.UIElements["toggleFullscreen"].Text = "Go Windowed";
				graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
				graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
				graphics.ApplyChanges();
			}
			graphics.ToggleFullScreen();
			graphics.ApplyChanges();
		}

		protected override void Draw(GameTime gameTime)
		{
			replaceColorEffect.Parameters["dcolor1"].SetValue(Color.White.ToVector4());
			replaceColorEffect.Parameters["dcolor2"].SetValue(Color.Black.ToVector4());

			replaceColorEffect.Parameters["color1"].SetValue(palette[0].ToVector4());
			replaceColorEffect.Parameters["color2"].SetValue(palette[1].ToVector4());

			GraphicsDevice.SetRenderTarget(renderTarget);
			GraphicsDevice.Clear(Instance.Color2);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

			environment.Draw(spriteBatch, gameTime.ElapsedGameTime.Milliseconds);
			spriteBatch.DrawString(Fonts["fiont"], "ver: " + VersionID, new Vector2(VirtualDimensions.X / 2 - Fonts["fiont"].MeasureString("ver: " + VersionID).X * SLEnvironment.FontScale / 2, 1f), Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1f);
			spriteBatch.DrawRectangle(new Rectangle(Point.Zero, VirtualDimensions), Color.White, 1);

			spriteBatch.End();
			GraphicsDevice.SetRenderTarget(null);
			GraphicsDevice.Clear(Instance.Color2);

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None,
				RasterizerState.CullCounterClockwise, transformMatrix: Matrix.CreateScale(screenScale), effect: replaceColorEffect);
			spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
			spriteBatch.End();

			replaceColorEffect.CurrentTechnique.Passes[0].Apply();

			base.Draw(gameTime);
		}
	}
	public partial class StolonGame
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static StolonGame Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	}
	public class SLEnvironment : AxComponent, IDialogueProvider
	{
		public enum SLGameState
		{
			OpenBoard,
			Loading,
			InMenu,
		}

		private SLScene scene;
		private SLUserInterface userInterface;
		private SLOverlayer overlayer;
		private SLGameState gameState;

		public SLGameState GameState => gameState;
		public SLScene Scene => scene;
		public SLUserInterface UserInterface => userInterface;
		public SLOverlayer Overlayer => overlayer;
		public ReadOnlyDictionary<string, SLEntity> Entities => new ReadOnlyDictionary<string, SLEntity>(entities);
		public const float FontScale = 0.5f;
		public static SpriteFont Font { get; }

		private Dictionary<string, SLEntity> entities;

		public static SLEnvironment Instance => StolonGame.Instance.Environment;
		public string SymbolNotation => "Ev";
		public string Name => "Environment";
		public Point FontDimensions { get; private set; }

		static SLEnvironment()
		{
			Font = StolonGame.Instance.Fonts["fiont"];
		}

		public SLEnvironment() : base(null)
		{
			entities = new Dictionary<string, SLEntity>();
			FontDimensions = (Font.MeasureString("A") * SLEnvironment.FontScale).ToPoint();

			// RegisterCharacter(new GoldsilkEntity());
			// RegisterCharacter(new DeadlineEntity());

			scene = new SLScene(new Player[]
			{
				new Player("player0"),
				new Player("player1"),
				// Entities["goldsilk"].GetPlayer()
			});
			userInterface = new SLUserInterface(scene, entities);
			overlayer = new SLOverlayer();

			gameState = SLGameState.InMenu;

			overlayer.AddOverlay(new TransitionOverlay());
			overlayer.AddOverlay(new LoadOverlay());
		}

		public void ForceGameState(SLGameState newState, bool unexpect = true)
		{
			if (unexpect)
			{
				switch (newState)
				{
					case SLGameState.InMenu:
						userInterface = new SLUserInterface(scene, entities);
						scene = new SLScene(new Player[]
						{
							new Player("player0"),
							new Player("player1"),
						});
						break;
				}
			}
			gameState = newState;
		}

		public override void Update(int elapsedMiliseconds)
		{
			userInterface.Update(elapsedMiliseconds);

			switch (gameState)
			{
				case SLGameState.OpenBoard:
					scene.Update(elapsedMiliseconds);
					break;
				case SLGameState.InMenu:
					break;
				case SLGameState.Loading:
					break;
			}

			overlayer.Update(elapsedMiliseconds);
			base.Update(elapsedMiliseconds);
		}

		public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
		{

			switch (gameState)
			{
				case SLGameState.OpenBoard:
					scene.Draw(spriteBatch, elapsedMiliseconds);
					break;
				case SLGameState.InMenu:
					break;
				case SLGameState.Loading:
					break;
			}

			userInterface.Draw(spriteBatch, elapsedMiliseconds);


			overlayer.Draw(spriteBatch, elapsedMiliseconds);
			base.Draw(spriteBatch, elapsedMiliseconds);
		}

		public void RegisterCharacter(SLEntity character)
		{
			entities.Add(character.Id, character);
		}
		public void DeregisterCharacter(string characterId)
		{
			entities.Remove(characterId);
		}
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
		public static Vector2 VirualPosition => CurrentState.Position.ToVector2() / Instance.screenScale;
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
