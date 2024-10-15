using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsitLib.XNA;
using static Stolon.StolonGame;
using System;
using MonoGame.Extended.Collisions.Layers;

#nullable enable

namespace Stolon
{
    public class SLScene : AxComponent
    {
        public enum SLScenePreset
        {
            Empty,
        }

        public Board Board => board ?? throw new Exception();
        public static SLScene MainInstance => Instance.Scene;
        public bool HasBoard => board != null;

        private Board? board;

        public SLScene(SLScenePreset preset = SLScenePreset.Empty) : base(Instance.Environment)
        {
            
        }

        public override void Update(int elapsedMiliseconds)
        {
            Board.Update(elapsedMiliseconds);
            base.Update(elapsedMiliseconds);
        }

        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            Board.Draw(spriteBatch, elapsedMiliseconds);
            base.Draw(spriteBatch, elapsedMiliseconds);
        }


        public void SetBoard(Player[] players) => SetBoard(new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection()));
        public void SetBoard(BoardState state)
        {
            if (BoardState.Validate(state)) board = new Board(this, state);
            else throw new Exception();
        }
    }
}
