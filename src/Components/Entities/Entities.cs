using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stolon
{
    public static class Entities
    {

    }
    /// <summary>
    /// Represent the main component of a <see cref="EntityBase"/>.
    /// </summary>
    public abstract class EntityBase : IDialogueProvider
    {
        /// <summary>
        /// Create a new <see cref="EntityBase"/> with set values.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="symbolNotation"></param>
        public EntityBase(string id, string name, string symbolNotation)
        {
            Id = id;
            Name = name;
            SymbolNotation = symbolNotation;
        }
        /// <summary>
        /// Get the <see cref="Player"/> of this <see cref="EntityBase"/>.
        /// </summary>
        /// <returns>A new <see cref="Player"/> created from this <see cref="EntityBase"/>.</returns>
        public Player GetPlayer()
        {
            return new Player(Name, Computer);
        }
        /// <summary>
        /// Get the <see cref="SLComputer"/> of this <see cref="EntityBase"/>.
        /// </summary>
        public abstract SLComputer Computer { get; }
        /// <summary>
        /// The texture thats gets drawn when this entity is selected in the selectEntityMernu. (Unused for now)
        /// </summary>
        public abstract Texture2D Splash { get; }
        /// <summary>
        /// A short description of this <see cref="EntityBase"/>.
        /// </summary>
        public virtual string? Description { get; }

        public abstract DialogueInfo GetReaction(PrimitiveReactOption reactOption); // unused.
        /// <summary>
        /// The unique ID of this <see cref="EntityBase"/>, no capital letters.
        /// </summary>
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string SymbolNotation { get; protected set; }

        public enum PrimitiveReactOption // uh
        {
            Afk,
            Distressed,
            Calm,
            GameLost,
            GameWon,
        }
    }
    /// <summary>
    /// A class that can interact with a <see cref="Board"/>.
    /// </summary>
    public abstract class SLComputer
    {
        /// <summary>
        /// The source <see cref="EntityBase"/>.
        /// </summary>
        public EntityBase? Source { get; }
        /// <summary>
        /// Create a new <see cref="SLComputer"/> with a set <see cref="Source"/> <see cref="EntityBase"/>.
        /// </summary>
        /// <param name="source">The source <see cref="EntityBase"/>.</param>
        public SLComputer(EntityBase? source)
        {
            Source = source;
        }
        /// <summary>
        /// Do a move best for the <see cref="Source"/> <see cref="EntityBase"/> on the <paramref name="board"/>.
        /// </summary>
        /// <param name="board">The <see cref="Board"/> to do a move on.</param>
        public abstract void DoMove(Board board);

        /// <summary>
        /// Gets the <see cref="Player"/> this <see cref="SLComputer"/> plays for.
        /// </summary>
        /// <param name="state">The current state of the <see cref="Board"/>.</param>
        /// <returns>The <see cref="Player"/> this <see cref="SLComputer"/> plays for.</returns>
        public Player GetPlayer(BoardState state)
        {
            Player[] players = state.Players.ToArray();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].Computer == this)
                {
                    return players[i];
                }
            }
            throw new Exception();
        }
    }
}
