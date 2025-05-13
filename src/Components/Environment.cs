using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.Debug;

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
    public class StolonEnvironment : GameComponent, IDialogueProvider
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
            get => _gameState; 
            set => _gameState = value;
        }
        /// <summary>
        /// The current <see cref="Stolon.Scene"/>.
        /// </summary>
        public Scene Scene
        {
            get => _scene;
            set => _scene = value;
        }
        /// <summary>
        /// The <see cref="UserInterface"/>.
        /// </summary>
        public UserInterface UI => _userInterface;
        /// <summary>
        /// The <see cref="Stolon.OverlayEngine"/>.
        /// </summary>
        public OverlayEngine Overlayer => _overlayer;
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> listing all <see cref="EntityBase"/> objects and their <see cref="EntityBase.Id"/>.
        /// </summary>
        public ReadOnlyDictionary<string, EntityBase> Entities => new ReadOnlyDictionary<string, EntityBase>(_entities);
        public string SymbolNotation => "Ev";
        public string Name => "Environment";

        public TaskHeap TaskHeap { get; }

        private Scene _scene;
        private UserInterface _userInterface;
        private OverlayEngine _overlayer;
        private SLGameState _gameState;
        private Dictionary<string, EntityBase> _entities;

        internal StolonEnvironment() : base(null)
        {
            _scene = new Scene();
            _entities = new Dictionary<string, EntityBase>();
            _userInterface = null!;
            _overlayer = null!;
            TaskHeap = new TaskHeap();
        }
        internal void Initialize()
        {
            RegisterEntity(new GoldsilkEntity());
            RegisterEntity(new StoEntity());
            // RegisterCharacter(new DeadlineEntity());

            _userInterface = new UserInterface();
            _userInterface.Initialize();

            _overlayer = new OverlayEngine();

            _gameState = SLGameState.InMenu;

            _overlayer.AddOverlay(new TransitionOverlay());
            _overlayer.AddOverlay(new LoadOverlay());
            _overlayer.AddOverlay(new TransitionDitherOverlay(StolonGame.Instance.GraphicsDevice));

            //StolonGame.Instance.AudioEngine.SetPlayList(new Playlist(
            //    "debug1",
            //    "debug2"
            //));
        }
        public GamestateInfo GetGamestateInfo()
        {
            return _gameState switch
            {
                SLGameState.OpenBoard => new GamestateInfo("cityLights", "openBoard"),
                SLGameState.InMenu => new GamestateInfo("menuTheme", "inMenu"),
                _ => new GamestateInfo(null, "unknown"),
            };
        }
        public override void Update(int elapsedMiliseconds)
        {
            TaskHeap.Update(elapsedMiliseconds);
            _userInterface.Update(elapsedMiliseconds);
            AudioEngine.Audio.Update(elapsedMiliseconds);

            switch (_gameState)
            {
                case SLGameState.OpenBoard:
                    _scene.Update(elapsedMiliseconds);
                    break;
                case SLGameState.InMenu:
                    break;
                case SLGameState.Loading:
                    break;
            }

            StolonGame.Instance.DRP.UpdateDetails(_gameState switch
            {
                SLGameState.OpenBoard => "Placing some markers..",
                SLGameState.InMenu => "Admiring the main menu..",
                SLGameState.Loading => "Loading STOLON..",
                _ => "Unset."
            });

            _overlayer.Update(elapsedMiliseconds);
            base.Update(elapsedMiliseconds);
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            switch (_gameState)
            {
                case SLGameState.OpenBoard:
                    break;
                case SLGameState.InMenu:
                    break;
                case SLGameState.Loading:
                    break;
            }
            _scene.Draw(spriteBatch, elapsedMiliseconds);
            _userInterface.Draw(spriteBatch, elapsedMiliseconds);

            _overlayer.Draw(spriteBatch, elapsedMiliseconds);
            base.Draw(spriteBatch, elapsedMiliseconds);
        }

        /// <summary>
        /// Register a new <see cref="EntityBase"/>.
        /// </summary>
        /// <param name="entity">The entity to register.</param>
        public void RegisterEntity(EntityBase entity)
        {
            _entities.Add(entity.Id, entity);
        }
        /// <summary>
        /// Deregister a new <see cref="EntityBase"/>. <strong>Should never be used.</strong>
        /// </summary>
        /// <param name="entity">The entity to deregister.</param>
        public void DeregisterEntity(string characterId)
        {
            _entities.Remove(characterId);
        }

        /// <summary>
        /// The main instance of the game.
        /// </summary>
        public static StolonEnvironment Instance => StolonGame.Instance.Environment;
    }
}
