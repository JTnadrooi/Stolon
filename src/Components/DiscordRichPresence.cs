using System;
using DiscordRPC;
using DiscordRPC.Logging;

namespace DiscordRPC {
	public class DiscordRichPresence
	{
		public static DiscordRpcClient client;
		private RichPresence presence; // Store the current RichPresence

		public void InitializeRPC()
		{
			client = new DiscordRpcClient("1291994415207944255");

			client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

			client.OnReady += (sender, e) =>
			{
				Console.WriteLine("--RPC-- Received Ready from user {0}", e.User.Username);
			};

			client.OnPresenceUpdate += (sender, e) =>
			{
				Console.WriteLine("--RPC-- Received Update! {0}", e.Presence);
			};

			client.Initialize();

			// Initialize the RichPresence and store it
			presence = new RichPresence()
			{
				Details = "Stolon",
				State = "",
				Assets = new Assets()
				{
					LargeImageKey = "stolonicon",
					LargeImageText = "Stolon",
					SmallImageKey = ""
				}
			};

			// Set the initial presence
			client.SetPresence(presence);
		}

		// Method to update the State
		public void UpdateState(string newState)
		{
			if (presence != null)
			{
				presence.State = newState;
				client.SetPresence(presence); // Update the presence on Discord
			}
		}

		// Method to update the Details
		public void UpdateDetails(string newDetails)
		{
			if (presence != null)
			{
				presence.Details = newDetails;
				client.SetPresence(presence); // Update the presence on Discord
			}
		}

		public void DisposeRPC()
		{
			client.Dispose();
		}
	}
}
