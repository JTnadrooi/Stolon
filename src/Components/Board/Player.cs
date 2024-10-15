using System;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Main player class, 
    /// </summary>
    public sealed class Player : ICloneable
    {
        public string Name { get; }

        public SLComputer? Computer { get; }

        public bool IsComputer => Computer != null;

        public Player(string name, SLComputer? computer = null)
        {
            Name = name;
            Computer = computer;
        }
        public override bool Equals(object? obj) => (Player?)obj != null && ((Player)obj).Name == Name;
        public override string? ToString() => Name;
        public override int GetHashCode() => Name.GetHashCode();

        public object Clone() => new Player(Name, Computer);

        public static Player[] TwoPlayers => new Player[]
                        {
                            new Player("player0"),
                            new Player("player1"),
                        };
        public static Player[] PlayersCom => new Player[]
                        {
                            new Player("player0"),
                            SLEnvironment.Instance.Entities["goldsilk"].GetPlayer()
                        };
    }
}
