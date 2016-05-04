using System;
using System.IO;
using System.Text;
using System.Xml;

namespace OpenRA.Mods.Common.UtilityCommands
{
	public class XmlToYamlCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--xml-to-yaml"; } }
		bool IUtilityCommand.ValidateArguments(string[] args) { return args.Length == 2; }

		[Desc("PATH/TO/FILE.xml", "Convert an XML document to a YAML document, written to stdout.")]
		void IUtilityCommand.Run(ModData modData, string[] args)
		{
			var filename = args[1];
			var doc = new XmlDocument();

			try
			{
				doc.Load(filename);
			}
			catch (XmlException)
			{
				doc.LoadXml("<root>" + File.ReadAllText(filename) + "</root>");
			}

			var root = doc.SelectSingleNode("root");
			if (root == null)
			{
				Console.WriteLine("Root node's name must be 'root'. Found '{0}' instead.", doc.DocumentElement.Name);
				Environment.Exit(1);
			}

			var topLevelNodes = root.SelectNodes("Node");

			var sb = new StringBuilder();

			foreach (XmlNode child in topLevelNodes)
				sb.Append(RecurseNode(child, 0));

			Console.WriteLine(sb.ToString());
		}

		string RecurseNode(XmlNode node, int depth)
		{
			var sb = new StringBuilder();

			var key = node.SelectSingleNode("Key");
			var value = node.SelectSingleNode("Value");
			var children = node.SelectNodes("Child/Node");

			var str = new string('\t', depth);

			if (!string.IsNullOrWhiteSpace(key.InnerText))
				str += key.InnerText + ":";

			if (!string.IsNullOrWhiteSpace(value.InnerText))
				str += " " + value.InnerText;

			/* TODO: Once comments are save this can be added too.
			var comment = node.SelectSingleNode("Comment");
			if (comment != null && !string.IsNullOrWhiteSpace(comment.InnerText))
				str += "# " + comment.InnerText;
			*/

			if (!string.IsNullOrWhiteSpace(str))
				sb.AppendLine(str);

			foreach (XmlNode c in children)
				sb.Append(RecurseNode(c, depth + 1));

			return sb.ToString();
		}
	}
}