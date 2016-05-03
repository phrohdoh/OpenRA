using System;
using System.Xml;
using System.Text;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public class YamlToXmlCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--yaml-to-xml"; } }

		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			var filename = args[1];
			var yaml = MiniYaml.FromFile(filename);

			var node0 = yaml[0];
			Console.WriteLine(NodeToXml(node0));
		}

		string NodeToXml(MiniYamlNode node)
		{
			var k = node.Key;
			var v = node.Value.Value;

			var sb = new StringBuilder();
			var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true });

			writer.WriteStartElement("Node");
			writer.WriteElementString("Key", k);
			writer.WriteElementString("Value", v);

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

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length == 2;
		}
	}
}