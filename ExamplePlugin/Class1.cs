using System;
using System.Linq;
using Terraria_Server;
using Terraria_Server.Commands;
using Terraria_Server.Events;

//So that it doesn't get mixed up with System.Console!
using Terraria_Server.Plugin;


namespace ExamplePlugin
{
	public class ExamplePlugin : Plugin
	{
		//Plugin info must always be provided exactly like this!
		public override string Name { get; set; }
		public override string Version { get; set; }
		public override string Author { get; set; }
		public override string Description { get; set; }

		public override void onLoad()
		{
			Name = "Example Plugin";
			Version = "1.0";
			Author = "bogeymanEST";
			Description = "Example plugin"
			RegisterHook(Hooks.Hook.COMMAND);
			RegisterHook(Hooks.Hook.PLAYER_CHAT);
			RegisterHook(Hooks.Hook.PLAYER_CONNECT);
			RegisterHook(Hooks.Hook.PLAYER_DISCONNECT);
			Console.WriteLine("Example Plugin has been loaded!");
		}

		public override void onCommand(CommandEvent Event)
		{
			int player = ((Player)Event.GetSender()).whoAmi;
			string command = Event.GetMessage();
			string[] Params = command.Split(' ');
			Sender sender = Event.GetSender();
			Console.WriteLine(GetPlayerName(player) + " sent command " + command);
			if (Params[0].Substring(1) == "players"))
			{
				string players = Statics.ServerWorld.getPlayerList().Aggregate("", (current, ply) => current + (ply.name + " "));
				sender.SendMessage("Currently connected players:", 255f, 240f, 20f);
				sender.SendMessage(players, 255f, 240f, 20f);
			}
			if (Params[0] == "/pvpon")
			{
				Event.SetCancelled(true);
				if (!Permissions.HasPermission(GetPlayerName(player), "examplePlugin.setpvp"))
				{
					SendPlayerMessage(player, "You don't have enough permissions!", 255f, 0f, 0f);
					return;
				}
				int target = Params.Length > 1 ? FindPlayerByName(Params[1]) : player;
				TogglePvP(target,  true);
				SendAllMessage("PvP on " + GetPlayerName(target) + " has been turned on!", 0f, 255f, 0f);
				return;
			}
			if (Params[0] == "/pvpoff")
			{
				Event.SetCancelled(true);
				if (!Permissions.HasPermission(GetPlayerName(player), "examplePlugin.setpvp"))
				{
					SendPlayerMessage(player, "You don't have enough permissions!", 255f, 0f, 0f);
					return;
				}
				int target = Params.Length > 1 ? FindPlayerByName(Params[1]) : player;
				TogglePvP(target, false);
				SendAllMessage("PvP on " + GetPlayerName(target) + " has been turned off!", 255f, 0f, 0f);
				return;
			}
			if (Params[0] == "/settime" && Params.Length > 1)
			{
				Event.SetCancelled(true);
				if (!Permissions.HasPermission(GetPlayerName(player), "examplePlugin.settime"))
				{
					SendPlayerMessage(player, "You don't have enough permissions!", 255f, 0f, 0f);
					return;
				}
				SetWorldTime(Convert.ToDouble(Params[1]));
				SendAllMessage("The time has been set to " + Params[1], 0f, 0f, 0f);
				return;
			}
			if (Params[0] == "/bloodmoon")
			{
				Event.SetCancelled(true);
				if (!Permissions.HasPermission(GetPlayerName(player), "examplePlugin.bloodmoon"))
				{
					SendPlayerMessage(player, "You don't have enough permissions!", 255f, 0f, 0f);
					return;
				}
				ToggleBloodMoon(!Statics.ServerWorld.isBloodMoon());
				SendAllMessage("Blood Moon is now " + (Statics.ServerWorld.isBloodMoon() ? "on" : "off"), 0f, 255f, 0f);
				return;
			}
			if (Params[0] == "/npc" && Params.Length > 1)
			{
				Event.SetCancelled(true);
				if (!Permissions.HasPermission(GetPlayerName(player), "examplePlugin.npc"))
				{
					SendPlayerMessage(player, "You don't have enough permissions!", 255f, 0f, 0f);
					return;
				}
				NPC.NewNPC(Convert.ToInt32(Statics.ServerWorld.getPlayerList()[player].position.X) + 3,
				           Convert.ToInt32(Statics.ServerWorld.getPlayerList()[player].position.Y) + 2,
				           Statics.ServerWorld,
				           Convert.ToInt32(Params[1]));
				SendPlayerMessage(player, "Spawned NPC type " + Params[1], 0f, 255f, 255f);
				return;
			}
		}


		public override void onPlayerSendChat(ChatEvent Event)
		{
			var player = ((Player)Event.GetSender()).whoAmi;
			var message = Event.GetMessage();
			Console.WriteLine("<" + GetPlayerName(player) + "> " + message); //Displays chat in the console.
			try
			{
				SendAllMessage(
					"<" + Permissions.FindPlayer(player).GetPrefix() + GetPlayerName(player) + "> " + message,
					Permissions.FindPlayer(player).GetColor()[0],
					Permissions.FindPlayer(player).GetColor()[1],
					Permissions.FindPlayer(player).GetColor()[2]);
			}
			catch (Exception)
			{
				SendAllMessage("<" + GetPlayerName(player) + "> " + message, 255f, 240f, 20f);
			}
			return;
		}

		public void onUnload()
		{
			Console.WriteLine("Example Plugin unloaded!");
		}

		public override void onPlayerConnect(Event Event)
		{
			SendAllMessage(GetPlayerName(((Player)Event.GetSender()).whoAmi) + " has joined the server.", 255f, 240f, 20f);
		}

		public override void onPlayerDisconnect(Event Event)
		{
			SendAllMessage(GetPlayerName(((Player)Event.GetSender()).whoAmi) + " has left the server.", 255f, 240f, 20f);
		}

	}
}
