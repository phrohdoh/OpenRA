using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public class ExtractTraitDocsNewCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--test"; } }

		ModData md;

		void IUtilityCommand.Run(ModData modData, string [] args)
		{
			md = modData;

			foreach (var type in modData.ObjectCreator.GetTypesImplementing<ITraitInfo>().OrderBy(t => t.Namespace))
			{
				var name = NameWithoutInfo(type);
				if (name.Contains("`"))
				{
					Console.WriteLine("Skipped {0} because it is generic.", name);
					continue;
				}

				var dirInfo = Directory.CreateDirectory(Path.Combine("docs", type.Namespace));
				var filename = Path.Combine(dirInfo.FullName, name + ".html");
				File.WriteAllText(filename, GenerateHTMLForTraitInfo(type, name));
				Console.WriteLine("Wrote " + filename);
			}
		}

		string NameWithoutInfo(Type type, bool fullName = false)
		{
			var name = fullName ? type.FullName : type.Name;
			return name.EndsWith("Info") ? name.Substring(0, name.Length - 4) : name;
		}

		string GenerateHTMLForTraitInfo(Type typeInfo, string title)
		{
			var ret = SurroundWithTag(title, "title");
			ret = SurroundWithTag(ret, "head");

			ret = "<!DOCTYPE html>" + ret;

			ret += SurroundWithTag("", "nav");

			ret += SurroundWithTag(title, "h1");

			var mainDescLines = typeInfo.GetCustomAttributes<DescAttribute>(false).SelectMany(d => d.Lines);
			ret += SurroundWithTag(mainDescLines.JoinWith(" "), "p");
			ret += SurroundWithTag("Inheritance", "h5");

			var list = "";
			foreach (var type in GetTypeHierarchyReversed(typeInfo))
			{
				if (type != typeInfo)
				{
					var name = NameWithoutInfo(type);
					if (!name.Contains("`"))
						list += SurroundWithTag(name, "li");
				}
			}

			ret += SurroundWithTag(list, "ol");

			ret += SurroundWithTag("Implements", "h5");
			list = "";
			foreach (var type in typeInfo.GetInterfaces())
			{
				if (type != typeInfo)
				{
					var name = NameWithoutInfo(type);
					if (!name.Contains("`"))
						list += SurroundWithTag(name, "li");
				}
			}

			ret += SurroundWithTag(list, "ol");

			ret += "Assembly: " + typeInfo.Assembly.GetName().Name;
			ret += SurroundWithTag("Fields", "h3");

			var infos = FieldLoader.GetTypeLoadInfo(typeInfo);
			if (!infos.Any())
				return ret;

			var liveTraitInfo = typeInfo.IsAbstract ? null : md.ObjectCreator.CreateBasic(typeInfo);
			if (liveTraitInfo != null)
			{
				foreach (var info in infos)
				{
					var desc = info.Field.GetCustomAttributes<DescAttribute>(true).SelectMany(d => d.Lines).JoinWith(" ");
					var loadInfo = info.Field.GetCustomAttributes<FieldLoader.SerializeAttribute>(true).FirstOrDefault();
					var defaultValue = loadInfo != null && loadInfo.Required ? SurroundWithTag("required", "em") : FieldSaver.SaveField(liveTraitInfo, info.Field.Name).Value.Value;

					ret += SurroundWithTag(info.Field.Name, "h4");
					ret += SurroundWithTag(desc, "p");
					ret += SurroundWithTag("Default: " + defaultValue, "p");
					ret += "<hr/>";
				}
			}

			return ret;
		}

		string SurroundWithTag(string toSurround, string tag)
		{
			return "<{0}>{1}</{0}>".F(tag, toSurround);
		}

		static IEnumerable<Type> GetTypeHierarchy(Type ofType)
		{
			for (var curr = ofType; curr != null; curr = curr.BaseType)
				yield return curr;
		}

		static IEnumerable<Type> GetTypeHierarchyReversed(Type ofType)
		{
			return GetTypeHierarchy(ofType).Reverse();
		}

		bool IUtilityCommand.ValidateArguments(string [] args) { return true; }
	}
}
