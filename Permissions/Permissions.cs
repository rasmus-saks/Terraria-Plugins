using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Terraria_Server;
using Terraria_Server.Commands;
using Terraria_Server.Events;
using Terraria_Server.Plugin;


namespace Permissions
{
	public class GroupPermissions
	{
		public string Name;
		public string Prefix;
		public float ColorR;
		public float ColorG;
		public float ColorB;
		public List<string> Nodes = new List<string>();
		public GroupPermissions(string name, string prefix, float colorR, float colorG, float colorB, List<string> nodes)
		{
			Name = name;
			Prefix = prefix;
			ColorR = colorR;
			ColorG = colorG;
			ColorB = colorB;
			Nodes = nodes;
		}
	}
	public class Permissions : Plugin
	{
		public override void Load()
		{
			Name = "Permissions";
			Description = "Plugin for easy permissions";
			Author = "bogeymanEST";
			Version = "1.0";
		    TDSMBuild = 24;

			Console.WriteLine("Initializing permissions...");
			PermissionManager.LoadPermissions();
			Console.WriteLine("Done!");

		    registerHook(Hooks.PLAYER_CHAT);

		}

		public override void Enable()
		{
			
		}

		public override void Disable()
		{
			
		}

        public override void onPlayerChat(MessageEvent Event)
        {
            var player = ((Player) Event.Sender).whoAmi;
            var pname = ((Player) Event.Sender).Name;
            var message = Event.Message;
            Console.WriteLine("<" + Server + "> " + message); //Displays chat in the console.
            try
            {
                SendAllMessage(
                  "<" + PermissionManager.FindPlayer(player).GetPrefix() + pname + "> " + message,
                  PermissionManager.FindPlayer(player).GetColor()[0],
                  PermissionManager.FindPlayer(player).GetColor()[1],
                  PermissionManager.FindPlayer(player).GetColor()[2]);
            }
            catch (Exception)
            {
                SendAllMessage("<" + pname + "> " + message, 255f, 240f, 20f);
            }
            Event.Cancelled = true;
            return;
        }
        public void SendAllMessage(string message, float Red, float Green, float Blue)
        {
            NetMessage.SendData(25, -1, -1, message, 255, Red, Green, Blue);
        }
	}
	public static class PermissionManager
	{
		public static List<GroupPermissions> Groups = new List<GroupPermissions>();
		public static List<PlayerPermissions> Players = new List<PlayerPermissions>();
		public static void SavePermissions()
		{
			try
			{
				/*File.Delete(Application.StartupPath + "\\permissions.xml");
				File.Create(Application.StartupPath + "\\permissions.xml");*/
				using (var writer = new XmlTextWriter(Statics.SavePath + "\\permissions.xml", Encoding.UTF8))
				{
					writer.WriteStartDocument();
					writer.WriteWhitespace("\r\n");
					writer.WriteStartElement("Permissions");
					writer.WriteWhitespace("\r\n\t");
					writer.WriteStartElement("Groups");
					foreach (GroupPermissions group in Groups)
					{
						writer.WriteWhitespace("\r\n\t\t");
						writer.WriteStartElement("Group");
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Name", group.Name);
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Color", group.ColorR + " " + group.ColorG + " " + group.ColorB);
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Prefix", group.Prefix);
						foreach (string nodes in group.Nodes)
						{
							writer.WriteWhitespace("\r\n\t\t\t");
							writer.WriteElementString("Node", nodes);
						}
						writer.WriteWhitespace("\r\n\t\t");
						writer.WriteEndElement();
					}
					writer.WriteWhitespace("\r\n\t");
					writer.WriteEndElement();
					writer.WriteWhitespace("\r\n\t");
					writer.WriteStartElement("Players");
					foreach (PlayerPermissions ply in Players)
					{
						writer.WriteWhitespace("\r\n\t\t");
						writer.WriteStartElement("Player");
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Name", ply.Name);
						writer.WriteWhitespace("\r\n\t\t\t");
						writer.WriteElementString("Group", ply.Group);
						foreach (string node in ply.Nodes)
						{
							writer.WriteWhitespace("\r\n\t\t\t");
							writer.WriteElementString("Node", node);
						}
						writer.WriteWhitespace("\r\n\t\t");
						writer.WriteEndElement();
					}
					writer.WriteWhitespace("\r\n\t");
					writer.WriteEndElement();
					writer.WriteWhitespace("\r\n");
					writer.WriteEndElement();
					writer.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR! " + ex.Message + ex.StackTrace);
				/*Console.Out("Press any key to exit...");
				System.Console.ReadKey(true);
				Environment.Exit(0);*/
			}
		}
		public static void LoadPermissions()
		{
			try
			{
				Players.Clear();
				Groups.Clear();
				if (!File.Exists(Statics.SavePath + "\\permissions.xml")) throw new Exception("Permissions file does not exist!");
				var Reader = new XmlTextReader(Statics.SavePath + "\\permissions.xml");
				string curElement = "";
				bool inGroups = false;
				bool inPlayers = false;
				string name = "";
				string group = "";
				string prefix = "";
				float colorR = 0;
				float colorG = 0;
				float colorB = 0;
				var nodes = new List<string>();
				while (Reader.Read())
				{
					switch (Reader.NodeType)
					{
						case XmlNodeType.Element:
							curElement = Reader.Name;
							if (curElement.ToLower() == "players")
							{
								inPlayers = true;
								inGroups = false;
								name = "";
								group = "";
								prefix = "";
								colorR = 0;
								colorG = 0;
								colorB = 0;
								nodes = new List<string>();
							}
							if (curElement.ToLower() == "groups")
							{
								inGroups = true;
								inPlayers = false;
								name = "";
								group = "";
								prefix = "";
								colorR = 0;
								colorG = 0;
								colorB = 0;
								nodes = new List<string>();
							}
							break;
						case XmlNodeType.Text:
							switch (curElement.ToLower())
							{
								case "name":
									name = Reader.Value;
									break;
								case "color":
									string[] spl = Reader.Value.Split(' ');
									colorR = float.Parse(spl[0]);
									colorG = float.Parse(spl[1]);
									colorB = float.Parse(spl[2]);
									break;
								case "prefix":
									prefix = Reader.Value;
									break;
								case "node":
									nodes.Add(Reader.Value);
									break;
								case "group":
									group = Reader.Value;
									break;

							}
							break;
						case XmlNodeType.EndElement:
							switch (Reader.Name.ToLower())
							{
								case "group":
									if (inPlayers) continue;
									if (String.IsNullOrEmpty(name)) throw new Exception("Group does not have a name!");
									Groups.Add(new GroupPermissions(name, prefix, colorR, colorG, colorB, nodes));
									break;
								case "player":
									if (String.IsNullOrEmpty(name)) throw new Exception("Player does not have a name!");
									//nodes.AddRange(Groups.Find(item => item.Name == group).Nodes);
									//if (!string.IsNullOrEmpty(group)) prefix = Groups.Find(item => item.Name == group).Prefix;
									Players.Add(new PlayerPermissions(name, group, prefix, nodes));
									break;
								case "permissions":
									Reader.Close();
									break;
							}
							break;
					}

				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR! " + ex.Message + ex.StackTrace);
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey(true);
				Environment.Exit(0);
			}
		}
		/*public static void ConCmd_setgroup(string input)
		{
			string[] split = input.Split(' ');
			if (split.Length < 2)
			{
				Console.WriteLine("USAGE: setgroup [player] [group]");
				return;
			}
			PlayerPermissions perm = Players.Find(item => item.Name == split[1]);
			if (perm == null) Players.Add(new PlayerPermissions(split[1], split[2], "", new List<string>()));
			else
			{
				int index = Players.IndexOf(perm);
				perm.Group = split[2];
				Players[index] = perm;
			}
			Console.WriteLine("Reloading permissions...");
			SavePermissions();
			LoadPermissions();
			Console.WriteLine("Permissions reloaded!");
		}
		public static void ConCmd_reloadpermissions(string input)
		{
			Console.WriteLine("Reloading permissions...");
			LoadPermissions();
			Console.WriteLine("Permissions reloaded!");
		}
		public static void ConCmd_savepermissions(string input)
		{
			Console.WriteLine("Saving permissions...");
			SavePermissions();
			Console.WriteLine("Permissions saved!");
		}

		public static void ConCmd_addpermission(string input)
		{
			string[] split = input.Split(' ');
			if (split.Length < 2)
			{
				Console.WriteLine("USAGE: addpermission [player] [node]");
				return;
			}
			PlayerPermissions perm = Players.Find(item => item.Name == split[1]);
			if (perm == null) Players.Add(new PlayerPermissions(split[1], "", "", new List<string>(new[] { split[2] })));
			else
			{
				int index = Players.IndexOf(perm);
				perm.Nodes.Add(split[2]);
				Players[index] = perm;
			}
			Console.WriteLine("Reloading permissions...");
			SavePermissions();
			LoadPermissions();
			Console.WriteLine("Permissions reloaded!");
		}
		public static void ConCmd_haspermission(string input)
		{
			string[] split = input.Split(' ');
			if (split.Length < 2)
			{
				Console.WriteLine("USAGE: haspermission [player] [node]");
				return;
			}
			Console.WriteLine(split[1] + (HasPermission(split[1], split[2]) ? " has" : " doesn't have") + " permissions for " + split[2]);
		}*/
		public static bool HasPermission(ISender sender, string node)
		{
			return sender.Name == "CONSOLE" || Players.Find(item => item.Name == sender.Name).HasPermission(node);
		}

		public static bool HasPermission(string name, string node)
		{
			return Players.Find(item => item.Name == name).HasPermission(node);
		}
		public static bool HasPermission(int player, string node)
		{
			return Players.Find(item => item.Name == Program.server.PlayerList[player].Name).HasPermission(node);
		}
		public static PlayerPermissions FindPlayer(int player)
		{
            return Players.Find(item => item.Name == Program.server.PlayerList[player].Name);
		}
	}
	public class PlayerPermissions
	{
		public string Name;
		public List<string> Nodes = new List<string>();
		public string Group = "";
		private string Prefix = "";

		public PlayerPermissions(string name, string group, string prefix, List<string> nodes)
		{
			Name = name;
			Nodes = nodes;
			Group = group;
			Prefix = prefix;
		}
		public bool HasPermission(string node)
		{
			string[] split = node.Split('.');
			foreach (string[] splitStr in Nodes.Select(str => str.Split('.')))
			{
				for (int i = 0; i < split.Length; i++)
				{
					if (i >= splitStr.Length) continue;
					if (split[i] == splitStr[i])
					{
						if (i == split.Length - 1 && split.Length == splitStr.Length) return true;
						continue;
					}
					if (splitStr[i] == "*") return true;
				}
			}
			if (String.IsNullOrEmpty(Group)) return false;
			foreach (string[] splitStr in PermissionManager.Groups.Find(item => item.Name == Group).Nodes.Select(str => str.Split('.')))
			{
				for (int i = 0; i < split.Length; i++)
				{
					if (split[i] == splitStr[i])
					{
						if (i == split.Length - 1 && split.Length == splitStr.Length) return true;
						continue;
					}
					if (splitStr[i] == "*") return true;
				}
			}
			return false;
		}
		public string GetPrefix()
		{
			if (!String.IsNullOrEmpty(Prefix)) return Prefix;
			return !String.IsNullOrEmpty(Group) ? PermissionManager.Groups.Find(item => item.Name == Group).Prefix : "";
		}
		public float[] GetColor()
		{
			var ret = new float[3];
			if (String.IsNullOrEmpty(Group))
			{
				ret[0] = 255f;
				ret[1] = 240f;
				ret[2] = 20f;
			}
			else
			{
				ret[0] = PermissionManager.Groups.Find(item => item.Name == Group).ColorR;
				ret[1] = PermissionManager.Groups.Find(item => item.Name == Group).ColorG;
				ret[2] = PermissionManager.Groups.Find(item => item.Name == Group).ColorB;
			}
			return ret;
		}
	}
}

