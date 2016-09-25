using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public interface IOptionObject { }

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class OptionAttribute : Attribute
	{
		public readonly string LongName;
		public readonly string HelpText;
		public readonly bool Required;

		public OptionAttribute(string longName, string helpText, bool required = false)
		{
			LongName = "--" + longName;
			HelpText = helpText;
			Required = required;
		}
	}

	public static class OptionsUtils
	{
		static int? ToNullableInt(string s)
		{
			int i;
			if (int.TryParse(s, out i))
				return i;

			return null;
		}

		static int ToIntOrThrow(string s)
		{
			int i;
			if (int.TryParse(s, out i))
				return i;

			throw new ArgumentException($"Could not parse '{s}' into an int");
		}

		class Pair
		{
			public PropertyInfo Property;
			public OptionAttribute Option;
		}

		static string GetValueForArgPair(string argForPair)
		{
			var valIndex = argForPair.IndexOf('=') + 1;
			return argForPair.Substring(valIndex, argForPair.Length - valIndex);
		}

		public static bool ParseOptions(IEnumerable<string> args, IOptionObject options)
		{
			var optionPropPairs = options.GetType().GetProperties()
				.Select(prop => new Pair { Property = prop, Option = prop.GetCustomAttribute<OptionAttribute>() })
				.Where(pair => pair.Option != null);

			foreach (var arg in args)
			{
				var pair = optionPropPairs.SingleOrDefault(p =>
				{
					if (p.Property.PropertyType == typeof(bool) && arg == p.Option.LongName)
						return true;

					var namePlusEq = p.Option.LongName + "=";
					return arg.StartsWith(namePlusEq, StringComparison.OrdinalIgnoreCase) && arg.Length > namePlusEq.Length;
				});

				if (pair == null)
					continue;

				/*
				if (pair.Property.PropertyType == typeof(bool) && pair.Option.Required)
					Console.WriteLine($"Boolean options cannot be required. ({pair.Property.Name})");
				*/

				switch (pair.Property.PropertyType.FullName)
				{
					//case "System.Nullable`1[System.Int32]":
					case "System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
					{
						var val = GetValueForArgPair(arg);
						pair.Property.SetValue(options, ToNullableInt(val));
						continue;
					}
					case "System.Int32":
					{
						var val = GetValueForArgPair(arg);
						pair.Property.SetValue(options, ToIntOrThrow(val));
						continue;
					}
					case "System.String":
					{
						var val = GetValueForArgPair(arg);
						pair.Property.SetValue(options, val);
						continue;
					}
					case "System.Boolean":
						pair.Property.SetValue(options, true);
						continue;
					default:
						return false;
				}
			}

			var missingParams = optionPropPairs.Where(p => p.Option.Required && p.Property.GetValue(options, null) == null);
			if (missingParams.Any())
			{
				Console.WriteLine("Missing required argument(s):");

				foreach (var missingParam in missingParams)
					Console.WriteLine($"  {missingParam.Option.LongName} {missingParam.Option.HelpText}");

				return false;
			}

			return true;
		}
	}
}