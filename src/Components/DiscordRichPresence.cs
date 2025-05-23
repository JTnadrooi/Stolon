using System;
using DiscordRPC;
using DiscordRPC.Logging;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;

using MonoGame.Extended;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Math = System.Math;
using RectangleF = MonoGame.Extended.RectangleF;

#nullable enable
namespace STOLON
{
    public class DiscordRichPresence
    {
        public DiscordRpcClient _client;
        private RichPresence _presence;

        public DiscordRichPresence()
        {
            _client = new DiscordRpcClient("1291994415207944255");
            //client.Logger = new ConsoleLogger()
            //{
            //	Level = LogLevel.Warning,
            //};
            _client.Initialize();
            _presence = new RichPresence()
            {
                Details = "STOLON",
                State = string.Empty,
                Assets = new Assets()
                {
                    LargeImageKey = "stolonicon",
                    LargeImageText = "STOLON",
                    SmallImageKey = string.Empty,
                }
            };
            _client.SetPresence(_presence);
        }

        public void UpdateState(string newState)
        {
            _presence.State = newState;
            _client.SetPresence(_presence);
        }

        public void UpdateDetails(string newDetails)
        {
            _presence.Details = newDetails;
            _client.SetPresence(_presence);
        }

        public void DisposeRPC() => _client.Dispose();
    }
}
