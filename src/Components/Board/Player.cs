using System;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// A base for players.
    /// </summary>
    public sealed class Player : ICloneable
    {
        /// <summary>
        /// The name of the player. <strong>Does not need to be unique.</strong>
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The <see cref="SLComputer"/> responsible for this player's actions.
        /// </summary>
        public SLComputer? Computer { get; }
        /// <summary>
        /// A value indicating if this <see cref="Player"/> is a computer or not.
        /// </summary>
        public bool IsComputer => Computer != null;
        /// <summary>
        /// Create a new <see cref="Player"/> with set properties.
        /// </summary>
        /// <param name="name">The name of the player. <strong>Does not need to be unique.</strong></param>
        /// <param name="computer">The <see cref="SLComputer"/> responsible for this player's actions.</param>
        public Player(string name, SLComputer? computer = null)
        {
            Name = name;
            Computer = computer;
        }
        public override bool Equals(object? obj) => (Player?)obj != null && ((Player)obj).Name == Name;
        public override string? ToString() => Name;
        public override int GetHashCode() => Name.GetHashCode();
        public object Clone() => new Player(Name, Computer);
        /// <summary>
        /// The default configuration for a 2p senario.
        /// </summary>
        public static Player[] TwoPlayers => new Player[]
                        {
                            new Player("player0"),
                            new Player("player1"),
                        };
        /// <summary>
        /// The default configuration for a player vs com senario.
        /// </summary>
        public static Player[] PlayersCom => new Player[]
                        {
                            new Player("player0"),
                            SLEnvironment.Instance.Entities["goldsilk"].GetPlayer()
                        };
    }
}
