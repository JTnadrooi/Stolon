#nullable enable

using AsitLib;
using DiscordRPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stolon
{
    public static class Program
    {
        public static void Main()
        {
            using StolonGame game = new StolonGame();
            game.Run();
        }
    }
}
