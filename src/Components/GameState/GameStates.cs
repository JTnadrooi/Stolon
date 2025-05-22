using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsitLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static Stolon.BoardState;
using static Stolon.StolonGame;

using Point = Microsoft.Xna.Framework.Point;
using Microsoft.Xna.Framework.Content;

#nullable enable

namespace Stolon
{
    public class MenuGameState : IGameState
    {
        public string DRPStatus => "MenuState";
        public void Update(int elapsedMiliseconds)
        {
            
        }
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            
        }
    }
    public class BoardGameState : IGameState
    {
        public string DRPStatus => "BoardState";

        private Board? _board;
        public Board Board => _board ?? throw new Exception();

        public void SetBoard(Player[] players) => SetBoard(new BoardState(Tile.GetTiles(new Vector2(8).ToPoint()), players, new BoardState.SearchTargetCollection()));
        public void SetBoard(BoardState state)
        {
            if (BoardState.Validate(state)) _board = new Board(state);
            else throw new Exception();
        }

        public void Update(int elapsedMiliseconds)
        {
            _board?.Update(elapsedMiliseconds);
        }
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            _board?.Draw(spriteBatch, elapsedMiliseconds);
        }
    }
}
