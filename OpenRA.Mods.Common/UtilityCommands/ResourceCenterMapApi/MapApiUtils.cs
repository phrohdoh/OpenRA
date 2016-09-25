using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public static class MapApiUtils
	{
		public static string MapByIdApiUrl = "http://resource.openra.net/map/id/";

		public static JsonSerializerSettings SnakeToPascalCaseSettings = new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver
			{
				NamingStrategy = new SnakeCaseNamingStrategy(true, false)
			}
		};

		public static string GetJsonForMapIds(string[] mapIds)
		{
			var joined = mapIds.JoinWith(",");
			var url = string.Concat(MapByIdApiUrl, joined);
			var req = WebRequest.CreateHttp(url);

			using (var resp = req.GetResponse())
			using (var response = resp.GetResponseStream())
			using (var reader = new StreamReader(response))
				return reader.ReadToEnd();
		}

		public static MapApiObject[] GetMapObjectsFromJson(string json)
		{
			return JsonConvert.DeserializeObject<MapApiObject[]>(json, SnakeToPascalCaseSettings);
		}

		public static byte[] GetBytesForOraMapUrl(string url)
		{
			var req = WebRequest.Create(url);
			using (var resp = req.GetResponse())
			using (var response = resp.GetResponseStream())
			using (var ms = new MemoryStream())
			{
				response.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public static string GetPathForModMaps(string modId, string modVersion)
		{
			return Path.Combine(Platform.SupportDir, "maps", modId, modVersion);
		}

		public static string GetValidFilenameFromMap(MapApiObject map, char replacementChar = '_')
		{
			var invalidFilenameChars = Path.GetInvalidFileNameChars();
			return new string(map.Title.Select(ch => invalidFilenameChars.Contains(ch) ? replacementChar : ch).ToArray());
		}

		public static string GetValidFilenameIncludingHash(MapApiObject map)
		{
			var validFilename = GetValidFilenameFromMap(map);
			return string.Concat(validFilename, "_", map.MapHash).Replace(' ', '_');
		}

		public static void WriteMapToAbsolutePath(MapApiObject map, string absoluteFilePath)
		{
			var bytes = GetBytesForOraMapUrl(map.Url);
			var directory = Path.GetDirectoryName(absoluteFilePath);
			Directory.CreateDirectory(directory);
			File.WriteAllBytes(absoluteFilePath, bytes);
		}
	}
}