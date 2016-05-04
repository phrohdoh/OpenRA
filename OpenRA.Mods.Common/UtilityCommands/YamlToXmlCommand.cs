using System;
using System.Text;
using System.Xml;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public class YamlToXmlCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--yaml-to-xml"; } }
		bool IUtilityCommand.ValidateArguments(string[] args) { return args.Length == 2; }

		[Desc("PATH/TO/FILE.yaml", "Convert a YAML document to an XML document, written to stdout.")]
		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			var filename = args[1];
			var yaml = MiniYaml.FromFile(filename);

			var sb = new StringBuilder();
			var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true });

			writer.WriteStartElement("root");
			writer.WriteRaw("\n");

			foreach (var topLevelNode in yaml)
				writer.WriteRaw(NodeToXml(topLevelNode) + "\n");

			writer.WriteEndElement();
			writer.Close();

			Console.WriteLine(sb);
		}

		string NodeToXml(MiniYamlNode node)
		{
			var key = node.Key;
			var value = node.Value.Value;

			// TODO: Once comments are save this can be added too.
			//var comment = node.Comment;

			var sb = new StringBuilder();
			var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true });

			writer.WriteStartElement("Node");
			writer.WriteElementString("Key", key);
			writer.WriteElementString("Value", value);

			foreach (var c in node.Value.Nodes)
			{
				writer.WriteStartElement("Child");
				writer.WriteRaw("\n");
				writer.WriteRaw(NodeToXml(c));
				writer.WriteRaw("\n");
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
			writer.Close();

			return sb.ToString();
		}
	}
}