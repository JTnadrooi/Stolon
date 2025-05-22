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
        public void Update(int elapsedMilliseconds);
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds);
        public string DRPStatus { get; }
    }

    public static class GameStateExtensions
    {
        public static string GetID(this IGameState state) => GameStateHelpers.GetID(state.GetType());
    }
    public static class GameStateHelpers
    {
        public static string GetID<T>() where T : IGameState, new() => GetID(typeof(T));
        public static string GetID(Type type) => type.FullName ?? throw new Exception();
    }
    public class GameStateManager
    {
        private IGameState? _currentState;
        private readonly Dictionary<string, IGameState> _stateMemory;

        public IGameState Current => _currentState ?? throw new Exception();

        public GameStateManager()
        {
            _stateMemory = new Dictionary<string, IGameState>();
        }

        public void ChangeState<T>(bool @override = false) where T : IGameState, new()
        {
            if (@override) _currentState = _stateMemory[GameStateHelpers.GetID<T>()] = new T();
            else _currentState = _stateMemory[GameStateHelpers.GetID<T>()] = _stateMemory.GetValueOrDefault(GameStateHelpers.GetID<T>()) ?? new T();
        }

        public void Update(int elapsedMilliseconds)
        {
            _currentState.Update(elapsedMilliseconds);
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            _currentState.Draw(spriteBatch, elapsedMiliseconds);
        }
    }
}