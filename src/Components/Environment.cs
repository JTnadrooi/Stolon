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
    /// <summary>
    /// The enviroment of the <see cref="Stolon"/> game.
    /// </summary>
    public class SLEnvironment : AxComponent, IDialogueProvider
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
        /// The current <see cref="SLScene"/>.
        /// </summary>
        public SLScene Scene
        {
            get => scene;
            set => scene = value;
        }
        /// <summary>
        /// The <see cref="SLUserInterface"/>.
        /// </summary>
        public SLUserInterface UI => userInterface;
        /// <summary>
        /// The <see cref="SLOverlayer"/>.
        /// </summary>
        public SLOverlayer Overlayer => overlayer;
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> listing all <see cref="SLEntity"/> objects and their <see cref="SLEntity.Id"/>.
        /// </summary>
        public ReadOnlyDictionary<string, SLEntity> Entities => new ReadOnlyDictionary<string, SLEntity>(entities);
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
        public static SLEnvironment Instance => StolonGame.Instance.Environment;
        public string SymbolNotation => "Ev";
        public string Name => "Environment";
        /// <summary>
        /// The dimensions of a single capital letter ("A").
        /// </summary>
        public Point FontDimensions { get; private set; }

        private SLScene scene;
        private SLUserInterface userInterface;
        private SLOverlayer overlayer;
        private SLGameState gameState;
        private Dictionary<string, SLEntity> entities;

        static SLEnvironment()
        {
            Font = StolonGame.Instance.Fonts["fiont"];
        }

        internal SLEnvironment() : base(null)
        {
            scene = new SLScene();
            entities = new Dictionary<string, SLEntity>();
        }
        internal void Initialize()
        {
            FontDimensions = (Font.MeasureString("A") * SLEnvironment.FontScale).ToPoint();

            RegisterEntity(new GoldsilkEntity());
            // RegisterCharacter(new DeadlineEntity());

            userInterface = new SLUserInterface();
            userInterface.Initialize();

            overlayer = new SLOverlayer();

            gameState = SLGameState.InMenu;

            overlayer.AddOverlay(new TransitionOverlay());
            overlayer.AddOverlay(new LoadOverlay());
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
                _ => throw new Exception()
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
        /// Register a new <see cref="SLEntity"/>.
        /// </summary>
        /// <param name="entity">The entity to register.</param>
        public void RegisterEntity(SLEntity entity)
        {
            entities.Add(entity.Id, entity);
        }
        /// <summary>
        /// Deregister a new <see cref="SLEntity"/>. <strong>Should never be used.</strong>
        /// </summary>
        /// <param name="entity">The entity to deregister.</param>
        public void DeregisterEntity(string characterId)
        {
            entities.Remove(characterId);
        }
    }
}
