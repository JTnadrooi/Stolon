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
using static Stolon.StolonEnvironment;
using static Stolon.StolonGame;

using Point = Microsoft.Xna.Framework.Point;
using Microsoft.Xna.Framework.Content;

#nullable enable

namespace Stolon
{
    public interface IGameState
    {
        void Update(int elapsedMiliseconds);
        void Draw(SpriteBatch spriteBatch);
    }

    public class GameStateManager
    {
        private IGameState? _currentState;

        public void ChangeState(IGameState newState)
        {
            _currentState = newState;
        }

        public void Update(int elapsedMiliseconds)
        {
            _currentState?.Update(elapsedMiliseconds);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _currentState?.Draw(spriteBatch);
        }
    }
}
