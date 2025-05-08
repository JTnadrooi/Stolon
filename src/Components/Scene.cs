using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using static Stolon.StolonGame;
using System;
using MonoGame.Extended.Collisions.Layers;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Represent a single game scene. 
    /// </summary>
    public class Scene : GameComponent
    {
        public enum SLScenePreset
        {
            Empty,
        }

        public Board Board => board ?? throw new Exception();
        public static Scene MainInstance => Instance.Scene;
        public bool HasBoard => board != null;

        private Board? board;

        private Texture2D? bg;

        public Scene(SLScenePreset preset = SLScenePreset.Empty) : base(Instance.Environment)
        {
            
        }

        public override void Update(int elapsedMiliseconds)
        {
            if (HasBoard) Board.Update(elapsedMiliseconds);
            base.Update(elapsedMiliseconds);
        }

        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            if (HasBoard && Instance.Environment.GameState == StolonEnvironment.SLGameState.OpenBoard) Board.Draw(spriteBatch, elapsedMiliseconds);
            if (bg != null)
            {
                spriteBatch.Draw(bg, new Rectangle(Point.Zero, Instance.VirtualDimensions), Color.White);
            }

            base.Draw(spriteBatch, elapsedMiliseconds);
        }


        public void SetBoard(Player[] players) => SetBoard(new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection()));
        public void SetBoard(BoardState state)
        {
            if (BoardState.Validate(state)) board = new Board(this, state);
            else throw new Exception();
        }
        public void SetImage(Texture2D texture)
        {
            bg = texture;
        }
        public Scene Current => Instance.Environment.Scene;
    }
}
