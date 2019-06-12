using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.UtilityCommands
{
	class NamespacedType
	{
		public string Namespace { get; set; }
		public string Name { get; set; }
	}

	[JsonConverter(typeof(StringEnumConverter))]
	enum TraitPropertyKind
	{
		Single,
		Multi,
		Choice,
		Map, // TODO: if this kind is a map how do we express the generic types in JSON?
	}

	class TraitProperty
	{
		public TraitPropertyKind Kind { get; set; }
		public string TypeName { get; set; }
		public string HumanFriendlyTypeName { get; set; }
		public string Name { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] DocLines { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string DefaultValue { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] ValidValues { get; set; }
	}

	class TraitDetail : NamespacedType
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string[] DocLines { get; set; }

		public string DefiningAssemblyName { get; set; }
		public bool IsConditional { get; set; }
		public NamespacedType[] RequiredTraits { get; set; }
		public TraitProperty[] Properties { get; set; }
	}

	/// <summary>
	/// Generate a JSON array containing trait data for use in the OpenRA IDE project
	///
	/// See https://github.com/Phrohdoh/oraide.
	/// </summary>
	class ExtractTypeDataCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--type-data"; } }

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			args = args.Skip(1).ToArray(); // skip the command name

			var isValid = args.Length == 0;

			if (!isValid)
			{
				var msg = "Command `{0}` does not accept arguments"
					.F((this as IUtilityCommand).Name);

				Console.Error.WriteLine(msg);
			}

			return isValid;
		}

		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			Game.ModData = utility.ModData;

			var traitDetails = new List<TraitDetail>();

			foreach (var ty in Game.ModData.ObjectCreator.GetTypesImplementing<ITraitInfo>())
			{
				if (ty.ContainsGenericParameters || ty.IsAbstract)
					continue;

				var infos = FieldLoader.GetTypeLoadInfo(ty);
				if (!infos.Any())
					continue;

				var instance = Game.ModData.ObjectCreator.CreateBasic(ty);

				var docLines = ty.GetCustomAttributes<DescAttribute>(true)
					.SelectMany(desc => desc.Lines)
					.ToArray();

				if (docLines.Length == 0)
					docLines = null;

				var properties = infos
					.SelectMany(info => ComputeTraitProperties(instance, ty))
					.ToArray();

				var detail = new TraitDetail
				{
					DefiningAssemblyName = ty.Assembly.GetName().Name,
					Namespace = ty.Namespace,
					Name = ComputeTraitName(ty),
					IsConditional = IsTraitConditional(ty),
					RequiredTraits = ComputeRequiredNamespacedTypes(ty),
					DocLines = docLines,
					Properties = properties,
				};

				traitDetails.Add(detail);
			}

			var json = JsonConvert.SerializeObject(traitDetails);
			Console.WriteLine(json);
		}

		static string ComputeTraitName(Type ty)
		{
			return ty.Name.EndsWith("Info")
				? ty.Name.Substring(0, ty.Name.Length - 4)
				: ty.Name;
		}

		static bool IsTraitConditional(Type ty)
		{
			return typeof(OpenRA.Mods.Common.Traits.ConditionalTraitInfo)
				.IsAssignableFrom(ty);
		}

		static IEnumerable<Type> RequiredTraitTypes(Type ty)
		{
			return ty.GetInterfaces()
				.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Requires<>))
				.SelectMany(i => i.GetGenericArguments())
				.Where(i => !i.IsInterface && !ty.IsSubclassOf(i))
				.OrderBy(i => i.Name);
		}

		static NamespacedType[] ComputeRequiredNamespacedTypes(Type ty)
		{
			return RequiredTraitTypes(ty)
				.Select(t => new NamespacedType
				{
					Namespace = t.Namespace,
					Name = ComputeTraitName(t),
				})
				.ToArray();
		}

		static TraitProperty[] ComputeTraitProperties(object instance, Type ty)
		{
			return ty.GetFields()
				.Where(field => !field.FieldType.IsGenericType || field.FieldType.GetGenericTypeDefinition() != typeof(Dictionary<,>)) // TODO
				.Select(field => {
					var loadInfo = field.GetCustomAttributes<FieldLoader.SerializeAttribute>(true).FirstOrDefault();
					var defaultMiniYamlValueStr = loadInfo != null && loadInfo.Required ? null : FieldSaver.SaveField(instance, field.Name).Value.Value;
					var humanFriendlyTypeName = Util.FriendlyTypeName(field.FieldType);
					var typeName = ComputeFieldTypeName(field.FieldType);
					var kind = ComputeTraitPropertyKind(field.FieldType);

					var docLines = field.GetCustomAttributes<DescAttribute>(true)
						.SelectMany(desc => desc.Lines)
						.ToArray();

					if (docLines.Length == 0)
						docLines = null;

					string[] validValues = null;

					// if (ty.IsEnum) // TODO: support enums
					// 	validValues = Enum.GetValues(ty).Select(val => val.ToString()).ToArray();

					return new TraitProperty
					{
						Name = field.Name,
						TypeName = typeName,
						HumanFriendlyTypeName = humanFriendlyTypeName,
						Kind = kind,
						DefaultValue = defaultMiniYamlValueStr,
						ValidValues = validValues,
						DocLines = docLines,
					};
				})
				.ToArray();
		}

		static string ComputeFieldTypeName(Type ty)
		{
			if (ty.IsSubclassOf(typeof(Array)))
				return ty.GetElementType().Name;
			else if (!ty.IsGenericType)
				return ty.Name;
			else if (ty.IsGenericType && ty.GetGenericTypeDefinition().GetInterfaces().Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
				return ComputeFieldTypeName(ty.GenericTypeArguments[0]);
			else if (ty.GetGenericTypeDefinition() == typeof(HashSet<>))
				return ComputeFieldTypeName(ty.GenericTypeArguments[0]);

			throw new NotImplementedException(ty.FullName);
		}

		static TraitPropertyKind ComputeTraitPropertyKind(Type ty)
		{
			if (ty.IsEnum)
				return TraitPropertyKind.Choice;

			if (ty.IsSubclassOf(typeof(Array)))
				return TraitPropertyKind.Multi;

			if (ty.IsGenericType && ty.GetGenericTypeDefinition().GetInterfaces().Any(e => e.IsGenericType && e.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
				return TraitPropertyKind.Multi;

			if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(HashSet<>))
				return TraitPropertyKind.Multi;

			if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(Dictionary<,>))
				return TraitPropertyKind.Map;

			return TraitPropertyKind.Single;
		}
	}
}
