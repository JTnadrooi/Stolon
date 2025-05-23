#nullable enable

using AsitLib;
using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace STOLON
{
    public static class Program
    {
        public static void Main()
        {
            using STOLON game = new STOLON();
            game.Run();
        }
    }
}
