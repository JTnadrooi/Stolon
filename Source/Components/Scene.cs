using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AsitLib.XNA;
using static Stolon.StolonGame;

#nullable enable

namespace Stolon
{
    public class SLScene : AxComponent
    {
        public Board Board => board;

        private Board board;

        public SLScene(Player[] players) : base(Instance.Environment)
        {
            board = new Board(this, new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection()));
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
    }
}
