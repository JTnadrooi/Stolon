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

        public SLGameState GameState
        {
            get => gameState; 
            set => gameState = value;
        }
        public SLScene Scene
        {
            get => scene;
            set => scene = value;
        }
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
            scene = new SLScene();
            entities = new Dictionary<string, SLEntity>();
            FontDimensions = (Font.MeasureString("A") * SLEnvironment.FontScale).ToPoint();

            RegisterCharacter(new GoldsilkEntity());
            // RegisterCharacter(new DeadlineEntity());

            userInterface = new SLUserInterface(entities);
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

        public void RegisterCharacter(SLEntity character)
        {
            entities.Add(character.Id, character);
        }
        public void DeregisterCharacter(string characterId)
        {
            entities.Remove(characterId);
        }
    }
}
