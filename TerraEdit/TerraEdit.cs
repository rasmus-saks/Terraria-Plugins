using System;
using System.Collections.Generic;
using System.Linq;
using Terraria_Server;
using Terraria_Server.Plugin;
using Terraria_Server.Events;
using Permissions;

namespace TerraEdit
{
	public class TerraEdit : Plugin
	{
		public readonly Dictionary<int,Vector2> Pos1 = new Dictionary<int, Vector2>();
		public readonly Dictionary<int, Vector2> Pos2 = new Dictionary<int, Vector2>();
		public readonly Dictionary<int, bool> Waiting1 = new Dictionary<int, bool>();
		public readonly Dictionary<int, bool> Waiting2 = new Dictionary<int, bool>();
		public readonly Dictionary<int, List<Action>> Actions = new Dictionary<int, List<Action>>();

		public override string Name { get; set; }
		public override string Description { get; set; }
		public override string Version { get; set; }
		public override string Author { get; set; }

		//private Vector2[] Pos1 = new Vector2[Statics.PlayerCap];
		//private Vector2[] Pos2 = new Vector2[Statics.PlayerCap];
		//private bool[] Waiting1 = new bool[Statics.PlayerCap];
		//private bool[] Waiting2 = new bool[Statics.PlayerCap];

		private readonly Color cGeneral = new Color(255, 0, 255);
		private readonly Color cError = new Color(255, 0, 0);

		public override void Load()
		{
			Name = "TerraEdit";
			Version = "0.3";
			Author = "bogeymanEST";
			Description = "Terraria world editor";
			registerHook(Hooks.PLAYER_COMMAND);
			registerHook(Hooks.TILE_BREAK);
			Console.WriteLine("Loading TerraEdit version " + Version);
		}

		public override void Enable()
		{
			
		}

		public override void Disable()
		{

		}

		public override void  onPlayerCommand(PlayerCommandEvent Event)
		{
			int player = ((Player) Event.getSender()).whoAmi;
			string command = Event.getMessage();
			string[] split = command.Split(' ');
			split[0] = split[0].ToLower();
			var sender = Event.getSender();
			if (split[0] == "//pos1" || split[0] == "//p1")
			{
				Event.setCancelled(true);
				if (!PermissionManager.HasPermission(player, "terraedit.selection"))
				{
					sender.sendMessage("You don't have enough permissions!");
					return;
				}
				if (!Waiting1.ContainsKey(player)) Waiting1.Add(player, true);
				if (!Waiting2.ContainsKey(player)) Waiting2.Add(player, false);
				Waiting1[player] = true;
				Waiting2[player] = false;
				sender.sendMessage("Waiting for block place/destroy.");
				return;
			}
			if (split[0] == "//pos2" || split[0] == "//p2")
			{
				Event.setCancelled(true);
				if (!PermissionManager.HasPermission(player, "terraedit.selection"))
				{
					sender.sendMessage("You don't have enough permissions!");
					return;
				}
				if (!Waiting1.ContainsKey(player)) Waiting1.Add(player, false);
				if (!Waiting2.ContainsKey(player)) Waiting2.Add(player, true);
				Waiting2[player] = true;
				Waiting1[player] = false;
				sender.sendMessage("Waiting for block place/destroy.");
				return;
			}
			if (split[0] == "//set")
			{
				Event.setCancelled(true);
				if (!PermissionManager.HasPermission(player, "terraedit.set"))
				{
					sender.sendMessage("You don't have enough permissions!");
					return;
				}
				if (split.Length < 2)
				{
					sender.sendMessage("USAGE: //set [block]");
					return;
				}
				if (!SelectionMade(player))
				{
					sender.sendMessage("You haven't made a selection!");
					return;
				}
				int id;
				if (!Int32.TryParse(split[1], out id))
				{
					id = TileNames.FindTileByName(split[1]);
					if (id == -2)
					{
						sender.sendMessage("Invalid block name!");
						return;
					}
				}
				if (id < -1 || id > 79)
				{
					sender.sendMessage("Invalid block ID!");
					return;
				}
				//var tile = new Tile {type = Byte.Parse(split[1])};
				var action = new Action();
				int highX = Pos1[player].X > Pos2[player].X ? (int)Pos1[player].X : (int)Pos2[player].X;
				int highY = Pos1[player].Y > Pos2[player].Y ? (int)Pos1[player].Y : (int)Pos2[player].Y;
				int lowX = Pos1[player].X < Pos2[player].X ? (int)Pos1[player].X : (int)Pos2[player].X;
				int lowY = Pos1[player].Y < Pos2[player].Y ? (int)Pos1[player].Y : (int)Pos2[player].Y;
				for (int i = lowX; i <= highX; i++)
				{
					for (int j = lowY; j <= highY; j++)
					{
						action.SetBlock(i,j,id);
					}
				}
				int c = action.Do();
				if (!Actions.ContainsKey(player)) Actions.Add(player, new List<Action>());
				if (Actions[player].Count > 0) Actions[player].Insert(0, action);
				else Actions[player].Add(action);
				sender.sendMessage(c + " blocks modified.");
				return;
			}
			if (split[0] == "//disc")
			{
				Event.setCancelled(true);
				if (!PermissionManager.HasPermission(player, "terraedit.disc"))
				{
					sender.sendMessage("You don't have enough permissions!");
					return;
				}
				if (split.Length < 3)
				{
					sender.sendMessage("USAGE: //disc [radius] [block]");
					return;
				}
				if (!Pos1.ContainsKey(player) || Pos1[player] == null)
				{
					sender.sendMessage("You must set position 1 as the circle center!");
					return;
				}
				int radius;
				if (!Int32.TryParse(split[1], out radius))
				{
					sender.sendMessage("Invalid radius!");
					return;
				}
				if (radius < 1)
				{
					sender.sendMessage("Radius must be higher than 0!");
					return;
				}
				int id;
				if (!Int32.TryParse(split[2], out id))
				{
					id = TileNames.FindTileByName(split[2]);
					if (id == -2)
					{
						sender.sendMessage("Invalid block name!");
						return;
					}
				}
				if (id < -1 || id > 79)
				{
					sender.sendMessage("Invalid block ID!");
					return;
				}
				var action = new Action();
				int px = Convert.ToInt32(Pos1[player].X);
				int py = Convert.ToInt32(Pos1[player].Y);
				for (int i = px-radius; i <= px+radius; i++)
				{
					for (int j = py-radius; j <= py+radius; j++)
					{
						if ((px-i)*(px-i) + (py-j)*(py-j) < radius*radius)
						{
							action.SetBlock(i,j,id);
						}
					}
				}
				int c = action.Do();
				if (!Actions.ContainsKey(player)) Actions.Add(player, new List<Action>());
				if (Actions[player].Count > 0) Actions[player].Insert(0, action);
				else Actions[player].Add(action);
				sender.sendMessage(c + " blocks modified.");
				return;
			}
			if (split[0] == "//replace")
			{
				Event.setCancelled(true);
				if (!PermissionManager.HasPermission(player, "terraedit.replace"))
				{
					sender.sendMessage("You don't have enough permissions!");
					return;
				}
				if (split.Length < 3)
				{
					sender.sendMessage("USAGE: //replace [from] [to]");
					return;
				}
				if (!SelectionMade(player))
				{
					sender.sendMessage("You haven't made a selection!");
					return;
				}
				int from;
				if (!Int32.TryParse(split[1], out from))
				{
					from = TileNames.FindTileByName(split[1]);
					if (from == -2)
					{
						sender.sendMessage("Invalid block name!");
						return;
					}
				}
				if (from < -1 || from > 79)
				{
					sender.sendMessage("Invalid block ID!");
					return;
				}
				int to;
				if (!Int32.TryParse(split[2], out to))
				{
					to = TileNames.FindTileByName(split[2]);
					if (to == -2)
					{
						sender.sendMessage("Invalid block name!");
						return;
					}
				}
				if (to < -1 || to > 79)
				{
					sender.sendMessage("Invalid block ID!");
					return;
				}
				var action = new Action();
				int highX = Pos1[player].X > Pos2[player].X ? (int)Pos1[player].X : (int)Pos2[player].X;
				int highY = Pos1[player].Y > Pos2[player].Y ? (int)Pos1[player].Y : (int)Pos2[player].Y;
				int lowX = Pos1[player].X < Pos2[player].X ? (int)Pos1[player].X : (int)Pos2[player].X;
				int lowY = Pos1[player].Y < Pos2[player].Y ? (int)Pos1[player].Y : (int)Pos2[player].Y;
				for (int i = lowX; i <= highX; i++)
				{
					for (int j = lowY; j <= highY; j++)
					{
						if(Main.tile[i,j].type == from) action.SetBlock(i, j, to);
					}
				}
				int c = action.Do();
				if (!Actions.ContainsKey(player)) Actions.Add(player, new List<Action>());
				if (Actions[player].Count > 0) Actions[player].Insert(0, action);
				else Actions[player].Add(action);
				sender.sendMessage(c + " blocks modified.");
				return;

			}
			if (split[0] == "//undo")
			{
				Event.setCancelled(true);
				if (!PermissionManager.HasPermission(player, "terraedit.undo"))
				{
					sender.sendMessage("You don't have enough permissions!");
					return;
				}
				if (Actions[player].Count == 0)
				{
					sender.sendMessage("Nothing to undo!");
					return;
				}
				foreach (KeyValuePair<Vector2, ActionBlock> kvp in Actions[player].First().changedBlocks)
				{
					int X = Convert.ToInt32(kvp.Key.X);
					int Y = Convert.ToInt32(kvp.Key.Y);
					//World world = Program.server.getWorld();
					if (kvp.Value.StartType == -1)
					{
						Main.tile[X, Y].active = false;
						NetMessage.SendTileSquare(-1, X, Y, 1);
					}
					else
					{
						/*WorldGen.KillTile(X, Y, world, false, false, true);
						WorldGen.PlaceTile(X, Y, world, kvp.Value.StartType, false, true);*/
						Main.tile[X, Y].active = true;
						Main.tile[X, Y].type = (byte)kvp.Value.StartType;
						NetMessage.SendTileSquare(-1, X, Y, 1);
					}
				}
				Actions[player].Remove(Actions[player].First());
				sender.sendMessage("Last action undone");
				return;
			}
			
			return;
		}

		public bool SelectionMade(int player)
		{
			return ((Pos1.ContainsKey(player) && Pos2.ContainsKey(player)) && Pos1[player] != null) && Pos2[player] != null;
		}

		public override void  onTileBreak(TileBreakEvent Event)
		{
			int player = ((Player)Event.getSender()).whoAmi;
			var sender = Event.getSender();
			if (Waiting1.ContainsKey(player) && Waiting1[player])
			{
				if (!Pos1.ContainsKey(player)) Pos1.Add(player, Event.getPos());
				Pos1[player] = Event.getPos();
				sender.sendMessage("Position 1 set (" + Event.getPos().X + ", " + Event.getPos().Y + ")");
				Waiting1[player] = false;
				Waiting2[player] = false;
				Event.setCancelled(true);
				return;
			}
			if (Waiting1.ContainsKey(player) && Waiting2[player])
			{
				if (!Pos2.ContainsKey(player)) Pos2.Add(player, Event.getPos());
				Pos2[player] = Event.getPos();
				sender.sendMessage("Position 2 set (" + Event.getPos().X + ", " + Event.getPos().Y + ")");
				Waiting1[player] = false;
				Waiting2[player] = false;
				Event.setCancelled(true);
				return;
			}
			return;
		}
		public enum TileType
		{
			Dirt = 0,
			Stone = 1,
			Torch = 4,
			TreeTrunk = 5,
			IronOre = 6,
			CopperOre = 7,
			GoldOre = 8,
			SilverOre = 9,
			WoodenDoor = 11,
			LifeCrystal = 12,
			Table = 14,
			Chair = 15,
			Anvil = 16,
			Furnace = 17,
			Workbench = 18,
			WoodPlatform = 19,
			Chest = 21,
			DemoniteOre = 22,
			EbonstoneBlock = 25,
			DemonAltar = 26,
			Pot = 28,
			Wood = 30,
			PotGiveWeapon = 31,
			MeteoriteOre = 37,
			GrayBrick = 38,
			RedBrick = 39,
			Clay = 40,
			BlueBrick = 41,
			GreenBrick = 43,
			PinkBrick = 44,
			GoldBrick = 45,
			SilverBrick = 46,
			CopperBrick = 47,
			CobWeb = 51,
			Vine = 52,
			Sand = 53,
			Glass = 54,
			Sign = 55,
			Obsidian = 56,
			Ash = 57,
			Hellstone = 58,
			Mud = 59,
			Grass = 62,
			Sapphire = 63,
			Ruby = 64,
			Emerald = 65,
			Topas = 66,
			Amethyst = 67,
			Diamond = 68,
			CorruptMud = 70, //Not sure
			GlowingMushroom = 72,
			ObsidianBrick = 75,
			HellstoneBrick = 76
		}
		public static class TileNames
		{
			public static Dictionary<int, string> Names = new Dictionary<int, string> {
			{-1, "Air"},
			{0, "Dirt"},
			{1, "Stone"},
			{4, "Torch"},
			{5, "TreeTrunk"},
			{6, "IronOre"},
			{7, "CopperOre"},
			{8, "GoldOre"},
			{9, "SilverOre"},
			{11, "WoodenDoor"},
			{12, "LifeCrystal"},
			{14, "Table"},
			{15, "Chair"},
			{16, "Anvil"},
			{17, "Furnace"},
			{18, "Workbench"},
			{19, "WoodPlatform"},
			{21, "Chest"},
			{22, "DemoniteOre"},
			{25, "EbonstoneBlock"},
			{26, "DemonAltar"},
			{28, "Pot"},
			{30, "Wood"},
			{31, "PotGiveWeapon"},
			{37, "MeteoriteOre"},
			{38, "GrayBrick"},
			{39, "RedBrick"},
			{40, "Clay"},
			{41, "BlueBrick"},
			{43, "GreenBrick"},
			{44, "PinkBrick"},
			{45, "GoldBrick"},
			{46, "SilverBrick"},
			{47, "CopperBrick"},
			{51, "CobWeb"},
			{52, "Vine"},
			{53, "Sand"},
			{54, "Glass"},
			{55, "Sign"},
			{56, "Obsidian"},
			{57, "Ash"},
			{58, "Hellstone"},
			{59, "Mud"},
			{62, "Grass"},
			{63, "Sapphire"},
			{64, "Ruby"},
			{65, "Emerald"},
			{66, "Topas"},
			{67, "Amethyst"},
			{68, "Diamond"},
			{70, "CorruptMud"},
			{72, "GlowingMushroom"},
			{75, "ObsidianBrick"},
			{76, "HellstoneBrick"}
		                             };
			/// <summary>
			/// Gets the ID of a tile from its name
			/// </summary>
			/// <param name="name">The name of the tile</param>
			/// <returns>The tile ID or -2 when none found</returns>
			public static int FindTileByName(string name)
			{
				foreach (KeyValuePair<int, string> kvp in Names.Where(kvp => kvp.Value.ToLower() == name.ToLower())) return kvp.Key;
				return -2;
			}
			/// <summary>
			/// Gets the tile name from its ID
			/// </summary>
			/// <param name="id">The tile ID</param>
			/// <returns>The name of the tile</returns>
			public static string GetTileName(int id)
			{
				return Names[id];
			}
		}
	}
	public struct ActionBlock
	{
		public int StartType;
		public int ChangeType;
		public ActionBlock(int startType, int changeType)
		{
			StartType = startType;
			ChangeType = changeType;
		}
	}
	public class Action
	{
		public Dictionary<Vector2,ActionBlock> changedBlocks = new Dictionary<Vector2, ActionBlock>();

		public void SetBlock(int X, int Y, int Type)
		{
			if (!changedBlocks.ContainsKey(new Vector2(X,Y)))
			{
				var vec = new Vector2(X, Y);
				bool act;
				int typ;
				try
				{
					act = Main.tile[X, Y].active;
					typ = Main.tile[X, Y].type;
				}
				catch (Exception)
				{
					act = false;
					typ = 0;
				}
				var abl = new ActionBlock(!act ? -1 : typ, Type);
				changedBlocks.Add(vec, abl);
			}
		}
		public int Do()
		{
			int count = 0;
			foreach (KeyValuePair<Vector2, ActionBlock> kvp in changedBlocks)
			{
				int X = Convert.ToInt32(kvp.Key.X);
				int Y = Convert.ToInt32(kvp.Key.Y);
				int changeType = kvp.Value.ChangeType;
				try
				{
					if (changeType == Main.tile[X, Y].type && Main.tile[X, Y].active) continue;
				}
				catch (Exception)
				{
					continue;
				}
				Main.tile[X, Y].active = false;
				if (changeType != -1)
				{
					Main.tile[X, Y].active = true;
					Main.tile[X, Y].type = (byte)changeType;
				} 
				NetMessage.SendTileSquare(-1, X, Y, 1);
				count++;
			}
			return count;
		}
	}
}
