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
using AsitLib.XNA;
using MonoGame.Extended;
using static Stolon.StolonGame;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Math = System.Math;
using RectangleF = MonoGame.Extended.RectangleF;

#nullable enable
namespace DiscordRPC
{
	public class DiscordRichPresence
	{
		public DiscordRpcClient client;
		private RichPresence presence; // Store the current RichPresence

		public DiscordRichPresence()
		{
			client = new DiscordRpcClient("1291994415207944255");
			//client.Logger = new ConsoleLogger()
			//{
			//	Level = LogLevel.Warning,
			//};
			client.Initialize();
			presence = new RichPresence()
			{
				Details = "Stolon",
				State = string.Empty,
				Assets = new Assets()
				{
					LargeImageKey = "stolonicon",
					LargeImageText = "Stolon",
					SmallImageKey = string.Empty,
				}
			};
			client.SetPresence(presence);
		}

		public void UpdateState(string newState = "")
		{
			presence.State = newState;
			client.SetPresence(presence);
		}

		public void UpdateDetails(string newDetails = "")
		{
			presence.Details = newDetails;
			client.SetPresence(presence);
		}

		public void DisposeRPC() => client.Dispose();
	}
}
