using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server;
using Terraria_Server.Commands;
using Terraria_Server.Misc;
using Terraria_Server.Plugin;
using Permissions;


namespace bAdmin
{
	public class bAdmin : Plugin
	{

		private readonly Color cGeneral = new Color(255, 240, 20);
		private readonly Color cInfo = new Color(255, 0, 255);
		private readonly Color cError = new Color(255, 0, 0);

		public override void Load()
		{
			Name = "bAdmin";
			Version = "0.1";
			Author = "bogeymanEST";
			Description = "Admin system for Terraria";
		    TDSMBuild = 24;
			Console.WriteLine("----------------------");
			Console.WriteLine("bAdmin version " + Version);
			Console.WriteLine("by bogeymanEST");
			Console.WriteLine("----------------------");
			registerHook(Hooks.PLAYER_COMMAND);
		}
		public override void Enable()
		{
			return;
		}
		public override void Disable()
		{
			return;
		}
		public override void onPlayerCommand(PlayerCommandEvent Event)
		{
			string[] split = Event.Message.Split(' ');
			split[0] = split[0].ToLower().Replace("/", "");
			string command = split[0];
			ISender sender = Event.Sender;
			if (command == "kick")
			{
				Event.Cancelled = true;
				if (!PermissionManager.HasPermission(sender, "badmin.kick"))
				{
					sender.sendMessage("You don't have enough permissions!");
					return;
				}
				if (split.Length < 2)
				{
					sender.sendMessage("USAGE: " + (sender is Player ? "/" : "") + "kick [player] (reason)");
					return;
				}
				int to = FindPlayerByName(split[1]);
				if (to == -1)
				{
					sender.sendMessage("Couldn't find the player!");
				}
				string name = Program.server.PlayerList[to].Name;
				string message = split.Length > 2 ? split[2] : "No reason provided";
				sender.sendMessage("You kicked " + name + ". Reason: " + message);
				foreach (Player player in Program.server.PlayerList)
				{
					player.sendMessage(name + " has been kicked. Reason: " + message);
				}
				((Player) sender).Kick("KICKED: " + message);
				Console.WriteLine("[bAdmin] " + sender.Name + " kicked " + name + " Reason: " + message);
				return;
			}
			/*if (command == "ban")
			{
				Event.Cancelled(true);
				if (!PermissionManager.HasPermission(sender, "badmin.ban"))
				{
					sender.SendMessage("You don't have enough permissions!", cError);
					return;
				}
				if (split.Length < 2)
				{
					sender.SendMessage("USAGE: " + (sender is Player ? "/" : "") + "ban [player] (reason)", cError);
					return;
				}
				int to = FindPlayerByName(split[1]);
				if (to == -1)
				{
					sender.SendMessage("Couldn't find the player!", cError);
				}
				string message = split.Length > 2 ? split[2] : "No reason provided";
				sender.SendMessage("You banned " + GetPlayerName(to) + ". Reason: " + message, cInfo);
				SendAllMessage(GetPlayerName(to) + " has been banned. Reason: " + message, cInfo);
				((Player)sender).Kick("BANNED: " + message);
				Console.WriteLine("[bAdmin] " + sender.GetName() + " banned " + GetPlayerName(to) + " Reason: " + message);
				return;
			}*/

			
		}
		/// <summary>
		/// Finds a connected player from a part of their name
		/// </summary>
		/// <param name="name">A part of a player's name</param>
		/// <returns>The player ID or -1 if no players were found</returns>
		public static int FindPlayerByName(string name)
		{
			try
			{
				for (int i = 0; i < Program.server.PlayerList.Length; i++)
				{
					if (Program.server.PlayerList[i].Name.ToLower().Contains(name.ToLower())) return i;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR! " + ex.Message + ex.StackTrace);
			}
			return -1;
		}
	}
			
}
