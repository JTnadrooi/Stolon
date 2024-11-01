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
using DiscordRPC;
using DiscordRPC.Events;

#nullable enable

namespace Stolon
{
    public struct GamestateInfo
    {
        public string? TrackId { get; }
        public string InfoId { get; }
        public GamestateInfo(string? trackId, string infoId) 
        { 
            TrackId = trackId;
            InfoId = infoId;
        }
    }
    /// <summary>
    /// The enviroment of the <see cref="Stolon"/> game.
    /// </summary>
    public class StolonEnvironment : AxComponent, IDialogueProvider
    {
        /// <summary>
        /// The current state of the game.
        /// </summary>
        public enum SLGameState
        {
            /// <summary>
            /// The board is open.
            /// </summary>
            OpenBoard,
            OpenScene,
            /// <summary>
            /// The game is loading.
            /// </summary>
            Loading,
            /// <summary>
            /// The menu is open.
            /// </summary>
            InMenu,
        }

        /// <summary>
        /// The current state of the game.
        /// </summary>
        public SLGameState GameState
        {
            get => gameState; 
            set => gameState = value;
        }
        /// <summary>
        /// The current <see cref="Stolon.Scene"/>.
        /// </summary>
        public Scene Scene
        {
            get => scene;
            set => scene = value;
        }
        /// <summary>
        /// The <see cref="UserInterface"/>.
        /// </summary>
        public UserInterface UI => userInterface;
        /// <summary>
        /// The <see cref="Stolon.OverlayEngine"/>.
        /// </summary>
        public OverlayEngine Overlayer => overlayer;
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> listing all <see cref="EntityBase"/> objects and their <see cref="EntityBase.Id"/>.
        /// </summary>
        public ReadOnlyDictionary<string, EntityBase> Entities => new ReadOnlyDictionary<string, EntityBase>(entities);
        /// <summary>
        /// The scaling applied to the <see cref="Font"/>.
        /// </summary>
        public const float FontScale = 0.5f;
        /// <summary>
        /// The main font used for most text.
        /// </summary>
        public static SpriteFont Font { get; }

        /// <summary>
        /// The main instance of the game.
        /// </summary>
        public static StolonEnvironment Instance => StolonGame.Instance.Environment;
        public string SymbolNotation => "Ev";
        public string Name => "Environment";
        /// <summary>
        /// The dimensions of a single capital letter ("A").
        /// </summary>
        public Point FontDimensions { get; private set; }

        private Scene scene;
        private UserInterface userInterface;
        private OverlayEngine overlayer;
        private SLGameState gameState;
        private Dictionary<string, EntityBase> entities;

        static StolonEnvironment()
        {
            Font = StolonGame.Instance.Fonts["fiont"];
        }

        internal StolonEnvironment() : base(null)
        {
            scene = new Scene();
            entities = new Dictionary<string, EntityBase>();
        }
        internal void Initialize()
        {
            FontDimensions = (Font.MeasureString("A") * StolonEnvironment.FontScale).ToPoint();

            RegisterEntity(new GoldsilkEntity());
            // RegisterCharacter(new DeadlineEntity());

            userInterface = new UserInterface();
            userInterface.Initialize();

            overlayer = new OverlayEngine();

            gameState = SLGameState.InMenu;

            overlayer.AddOverlay(new TransitionOverlay());
            overlayer.AddOverlay(new LoadOverlay());
            overlayer.AddOverlay(new TransitionDitherOverlay(StolonGame.Instance.GraphicsDevice));
        }
        public GamestateInfo GetGamestateInfo()
        {
            return gameState switch
            {
                SLGameState.OpenBoard => new GamestateInfo("cityLights", "openBoard"),
                SLGameState.InMenu => new GamestateInfo("menuTheme", "inMenu"),
                _ => new GamestateInfo(null, "unknown"),
            };
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

            StolonGame.Instance.DRP.UpdateDetails(gameState switch
            {
                SLGameState.OpenBoard => "Placing some markers..",
                SLGameState.InMenu => "Admiring the main menu..",
                SLGameState.Loading => "Loading STOLON..",
                _ => "Unset."
            });

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

        /// <summary>
        /// Register a new <see cref="EntityBase"/>.
        /// </summary>
        /// <param name="entity">The entity to register.</param>
        public void RegisterEntity(EntityBase entity)
        {
            entities.Add(entity.Id, entity);
        }
        /// <summary>
        /// Deregister a new <see cref="EntityBase"/>. <strong>Should never be used.</strong>
        /// </summary>
        /// <param name="entity">The entity to deregister.</param>
        public void DeregisterEntity(string characterId)
        {
            entities.Remove(characterId);
        }
    }
}
