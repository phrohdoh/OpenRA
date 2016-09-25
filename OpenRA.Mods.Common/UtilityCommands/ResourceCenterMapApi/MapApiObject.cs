using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public class MapApiObject
	{
		public bool LastRevision { get; set; }
		public int Id { get; set; }
		public int Revision { get; set; }
		public string MapHash { get; set; }
		public string Posted { get; set; }
		public string MapType { get; set; }

		[JsonProperty("mapformat")]
		public int MapFormat { get; set; }

		public int Players { get; set; }
		public string Bounds { get; set; }
		public int Viewed { get; set; }
		public string MapGridType { get; set; }
		public string Spawnpoints { get; set; }
		public string Parser { get; set; }
		public string Title { get; set; }
		public bool Downloading { get; set; }
		public string Url { get; set; }
		public string Width { get; set; }
		public string Info { get; set; }
		public string Description { get; set; }
		public int Reports { get; set; }
		public string Tileset { get; set; }
		public string Rules { get; set; }
		public bool AdvancedMap { get; set; }
		public List<string> Categories { get; set; }
		public string GameMod { get; set; }
		public bool RequiresUpgrade { get; set; }
		public bool Lua { get; set; }
		public string Author { get; set; }
		public string Height { get; set; }
		public int Downloaded { get; set; }
		public string PlayersBlock { get; set; }
		public string License { get; set; }
		public double Rating { get; set; }
		public string Minimap { get; set; }
	}
}
