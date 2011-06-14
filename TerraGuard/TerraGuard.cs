using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Permissions;
using Terraria_Server;
using Terraria_Server.Plugin;
using System.IO;
using System.Xml;


namespace TerraGuard
{
	public class TerraGuard : Plugin
	{
		public override string Name { get; set; }
		public override string Version { get; set; }
		public override string Author { get; set; }
		public override string Description { get; set; }

		public static List<Region> Regions = new List<Region>();
		public TerraEdit.TerraEdit tePlug;
		/*
		private readonly Color cGeneral = new Color(255, 240, 20);
		private readonly Color cError = new Color(255, 0, 0);
		*/
		public TerraEdit.TerraEdit HookIntoTerraEdit()
		{
			bool found = false;
			TerraEdit.TerraEdit ret = null;
			Console.WriteLine("Hooking into TerraEdit...");
			foreach (Plugin plug in Program.server.getPluginManager().pluginList)
			{
				if (plug.Name == "TerraEdit")
				{
					Console.WriteLine("Hooked into TerraEdit!");
					ret = (TerraEdit.TerraEdit)plug;
					found = true;
				}
			}
			if (!found) Console.WriteLine("Could not find TerraEdit!");
			return ret;
		}

		public override void Load()
		{
			Name = "TerraGuard";
			Version = "0.3";
			Author = "bogeymanEST";
			Description = "Protection system for Terraria";
			registerHook(Hooks.PLAYER_COMMAND);
			registerHook(Hooks.TILE_BREAK);
			registerHook(Hooks.PLAYER_CHEST);
			Console.WriteLine("Loading TerraGuard version " + Version);
			tePlug = HookIntoTerraEdit();
			LoadRegions();
		}

		public void LoadRegions()
		{
			Console.WriteLine("Loading regions...");
			if (!File.Exists(Statics.SavePath + "\\regions.xml"))
			{
				Console.WriteLine("Regions file does not exist! Creating...");
				try
				{
					File.Create(Statics.SavePath + "\\regions.xml");
					Console.WriteLine("Created!");
				}
				catch(Exception ex)
				{
					Console.WriteLine("ERROR! " + ex.Message + ex.StackTrace);
				}
			}
			var Reader = new XmlTextReader(Statics.SavePath + "\\regions.xml");
			string name = "";
			string members = "";
			var p1 = new Vector2();
			var p2 = new Vector2();
			string curElement = null;
			while (Reader.Read())
			{
				switch (Reader.NodeType)
				{
					case XmlNodeType.Element:
						curElement = Reader.Name;
						break;
					case XmlNodeType.Text:
						if (curElement != null)
						{
							switch (curElement.ToLower())
							{
								case "name":
									name = Reader.Value;
									break;
								case "members":
									members = Reader.Value;
									break;
								case "pos1":
									p1 = new Vector2(Convert.ToSingle(Reader.Value.Split(' ')[0]),Convert.ToSingle(Reader.Value.Split(' ')[1]));
									break;
								case "pos2":
									p2 = new Vector2(Convert.ToSingle(Reader.Value.Split(' ')[0]),Convert.ToSingle(Reader.Value.Split(' ')[1]));
									break;

							}
						}
						break;
					case XmlNodeType.EndElement:
						switch (Reader.Name.ToLower())
						{
							case "region":
								Regions.Add(new Region(name, members, p1, p2));
								break;
						}
						break;
				}
					
			}
			Console.WriteLine("Loaded " + Regions.Count + " region(s)!");
			return;
		}
		public void SaveRegions()
		{
			Console.WriteLine("Saving regions...");
			try
			{
				/*File.Delete(Application.StartupPath + "\\permissions.xml");
				File.Create(Application.StartupPath + "\\permissions.xml");*/
				using (var writer = new XmlTextWriter(Statics.SavePath + "\\regions.xml", Encoding.UTF8))
				{
					writer.WriteStartDocument();
					writer.WriteWhitespace("\r\n");
					writer.WriteStartElement("WorldGuard");
					writer.WriteWhitespace("\r\n\t");
					writer.WriteStartElement("Regions");
					foreach (Region region in Regions)
					{
						writer.WriteWhitespace("\r\n\t\t");
						writer.WriteStartElement("Region");
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Name", region.Name);
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Members", region.Members);
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Pos1", (int)region.Pos1.X + " " + (int)region.Pos1.Y);
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Pos2", (int)region.Pos2.X + " " + (int)region.Pos2.Y);
						writer.WriteWhitespace("\r\n\t\t");
						writer.WriteEndElement();
					}
					writer.WriteWhitespace("\r\n\t");
					writer.WriteEndElement();
					writer.WriteWhitespace("\r\n");
					writer.WriteEndElement();
					writer.Close();
				}
				Console.WriteLine("Regions saved!");
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERRROR! " + ex.Message + ex.StackTrace);
				return;
			}
		}

		public override void onPlayerCommand(PlayerCommandEvent Event)
		{
			int player = ((Player)Event.getSender()).whoAmi;
			string command = Event.getMessage();
			string[] split = command.Split(' ');
			split[0] = split[0].ToLower();
			var sender = Event.getSender();
			if(split.Length > 1) split[1] = split[1].ToLower();
			if (split[0] == "/region")
			{
				if(split[1] == "define")
				{
					Event.setCancelled(true);
					if (!PermissionManager.HasPermission(player, "terraguard.region.define"))
					{
						sender.sendMessage("You don't have enough permissions!");
						return;
					}
					if (split.Length < 4)
					{
						sender.sendMessage("USAGE: /region define [name] [members]");
						return;
					}
					if (RegionExists(split[2]))
					{
						sender.sendMessage("A region with this name already exists!");
						return;
					}
					if (!SelectionMade(player))
					{
						sender.sendMessage("You haven't made a selection!");
						return;
					}
					var members = new string[split.Length - 3];
					Array.Copy(split, 3, members, 0, split.Length - 3);
					var memb = String.Join(" ", members);
					Regions.Add(new Region(split[2], memb, GetPos1(player), GetPos2(player)));
					sender.sendMessage("Created region " + split[2]);
					sender.sendMessage("Members: " + memb);
					SaveRegions();
					return;
				}
			}
		}

		public override void onTileBreak(Terraria_Server.Events.TileBreakEvent Event)
		{
			int player = ((Player)Event.getSender()).whoAmi;
			var region = IsPointInRegion(Event.getPos());
			if (region != null && !IsPlayerRegionMember(player, region))
			{
				Event.getSender().sendMessage("This area is protected!");
				Event.setCancelled(true);
				return;
			}
			return;

		}
		public override void onPlayerOpenChest(Terraria_Server.Events.ChestOpenEvent Event)
		{
			int player = ((Player)Event.getSender()).whoAmi;
			var region = IsPointInRegion(new Vector2(Main.chest[Event.getChestID()].x, Main.chest[Event.getChestID()].y));
			if (region != null && !IsPlayerRegionMember(player, region))
			{
				Event.getSender().sendMessage("This area is protected!");
				Event.setCancelled(true);
				return;
			}
		}
		public bool SelectionMade(int player)
		{
			return tePlug.SelectionMade(player);
			//return ((Pos1.ContainsKey(player) && Pos2.ContainsKey(player)) && Pos1[player] != null) && Pos2[player] != null;
		}
		public Vector2 GetPos1(int player)
		{
			return tePlug.Pos1[player];
		}
		public Vector2 GetPos2(int player)
		{
			return tePlug.Pos2[player];
		}
		/// <summary>
		/// Checks whether a region exists
		/// </summary>
		/// <param name="name">Name of the region</param>
		/// <returns>True when it exists, false if it does not</returns>
		public static bool RegionExists(string name)
		{
			return Regions.Find(item => item.Name == name) != null;
		}
		/// <summary>
		/// Gets a list of regions the position is in
		/// </summary>
		/// <param name="pos">The position to check</param>
		/// <returns>A list of regions it is in or null if it is in no regions</returns>
		public List<Region> IsPointInRegion(Vector2 pos)
		{
			var ret = new List<Region>();
			foreach(Region region in Regions)
			{
				var smallX = region.Pos1.X > region.Pos2.X ? (int) region.Pos2.X : (int) region.Pos1.X;
				var largeX = region.Pos1.X > region.Pos2.X ? (int) region.Pos1.X : (int) region.Pos2.X;
				var smallY = region.Pos1.Y > region.Pos2.Y ? (int) region.Pos2.Y : (int) region.Pos1.Y;
				var largeY = region.Pos1.Y > region.Pos2.Y ? (int) region.Pos1.Y : (int) region.Pos2.Y;
				if(smallX <= pos.X && largeX >= pos.X && smallY <= pos.Y && largeY >= pos.Y) ret.Add(region);
			}
			return ret.Count == 0 ? null : ret;
		}
		/// <summary>
		/// Checks whether a player is a member of any region in a list of regions
		/// </summary>
		/// <param name="player">Player ID</param>
		/// <param name="regions">List of regions to check</param>
		/// <returns>Whether the player is a member of any of the regions</returns>
		public bool IsPlayerRegionMember(int player, List<Region> regions)
		{
			return IsPlayerRegionMember(Program.server.getPlayerList()[player].getName(), regions);
		}
		/// <summary>
		/// Checks whether a player is a member of any region in a list of regions
		/// </summary>
		/// <param name="player">Player name</param>
		/// <param name="regions">List of regions to check</param>
		/// <returns>Whether the player is a member of any of the regions</returns>
		public bool IsPlayerRegionMember(string player, List<Region> regions)
		{
			foreach (Region region in regions)
			{
				if (region.Members.Split(' ').Any(member => member == player)) return true;
			}
			return false;
		}
		/// <summary>
		/// Checks whether a player is a member of a region.
		/// </summary>
		/// <param name="player">Player name</param>
		/// <param name="region">The region</param>
		/// <returns>Whether the player is a member of the region</returns>
		public bool IsPlayerRegionMember(string player, Region region)
		{
			string[] members = region.Members.Split(' ');
			return members.Any(member => member == player);
		}
		/// <summary>
		/// Checks whether a player is a member of a region.
		/// </summary>
		/// <param name="player">Player ID</param>
		/// <param name="region">The region</param>
		/// <returns>Whether the player is a member of the region</returns>
		public bool IsPlayerRegionMember(int player, Region region)
		{
			return IsPlayerRegionMember(Program.server.getPlayerList()[player].getName(), region);
		}

		public override void Disable()
		{
		}

		public override void Enable()
		{
		}
	}
	public class Region
	{
		public string Name;
		public Vector2 Pos1;
		public Vector2 Pos2;
		public string Members;
		public Region(string name, string members, Vector2 pos1, Vector2 pos2)
		{
			Name = name;
			Members = members;
			Pos1 = pos1;
			Pos2 = pos2;
		}
	}
}
