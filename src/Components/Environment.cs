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

#nullable enable

namespace STOLON
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
    /// The enviroment of the <see cref="global::STOLON"/> game.
    /// </summary>
    public class GameEnvironment : GameComponent, IDialogueProvider
    {
        /// <summary>
        /// The <see cref="UserInterface"/>.
        /// </summary>
        public UserInterface UI => _userInterface;
        /// <summary>
        /// The <see cref="global::STOLON.OverlayEngine"/>.
        /// </summary>
        public OverlayEngine Overlayer => _overlayer;
        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> listing all <see cref="EntityBase"/> objects and their <see cref="EntityBase.Id"/>.
        /// </summary>
        public ReadOnlyDictionary<string, EntityBase> Entities => new ReadOnlyDictionary<string, EntityBase>(_entities);
        public string SymbolNotation => "Ev";
        public string Name => "Environment";

        public TaskHeap TaskHeap { get; }

        private UserInterface _userInterface;
        private OverlayEngine _overlayer;
        private Dictionary<string, EntityBase> _entities;
        private GameStateManager _gameStateManager;

        internal GameEnvironment() : base(null)
        {
            _entities = new Dictionary<string, EntityBase>();
            _userInterface = null!;
            _overlayer = null!;
            _gameStateManager = new GameStateManager();
            STOLON.StateManager = _gameStateManager;
            STOLON.StateManager.ChangeState<MenuGameState>();
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

            _overlayer.AddOverlay(new TransitionOverlay());
            _overlayer.AddOverlay(new LoadOverlay());
            _overlayer.AddOverlay(new TransitionDitherOverlay(STOLON.Instance.GraphicsDevice));

            //StolonGame.Instance.AudioEngine.SetPlayList(new Playlist(
            //    "debug1",
            //    "debug2"
            //));
        }
        public override void Update(int elapsedMiliseconds)
        {
            TaskHeap.Update(elapsedMiliseconds);
            _userInterface.Update(elapsedMiliseconds);

            STOLON.StateManager.Update(elapsedMiliseconds);

            _userInterface.PostUpdate(elapsedMiliseconds);
            STOLON.Audio.Update(elapsedMiliseconds);
            STOLON.Instance.DRP.UpdateDetails(STOLON.StateManager.Current.DRPStatus);

            _overlayer.Update(elapsedMiliseconds);
            base.Update(elapsedMiliseconds);
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            STOLON.StateManager.Draw(spriteBatch, elapsedMiliseconds);
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
        /// The main StolonGame.Instance of the game.
        /// </summary>
        public static GameEnvironment Instance => STOLON.Environment;
    }
}
