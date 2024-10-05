#nullable enable

using DiscordRPC;

namespace Stolon
{
    public static class Program
    {
        public static void Main()
        {
            //run the game
            using StolonGame game = new StolonGame();
            game.Run();
        }
    }
}
