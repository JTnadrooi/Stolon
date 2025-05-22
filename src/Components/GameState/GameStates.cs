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
        public void Draw(SpriteBatch spriteBatch)
        {
            
        }
    }
    public class BoardGameState : IGameState
    {
        public string DRPStatus => "BoardState";
        public void Update(int elapsedMiliseconds)
        {
            StolonEnvironment.Instance.Scene.Update(elapsedMiliseconds);
        }
        public void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
