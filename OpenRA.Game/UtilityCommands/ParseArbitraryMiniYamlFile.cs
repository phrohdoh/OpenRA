using System;

namespace OpenRA.UtilityCommands
{
	class MiniYamlParsePlayground : IUtilityCommand
	{
		string inputMiniYamlFilePath;

		string IUtilityCommand.Name { get { return "--parse-miniyaml"; } }
		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			if (args.Length == 2)
			{
				inputMiniYamlFilePath = args[1];
				return true;
			}

			return false;
		}

		void PrintNode(MiniYamlNode node, int indentLevel = 0)
		{
			var indent = new string('␣', indentLevel) + (indentLevel == 0 ? "" : " ");

			Console.WriteLine("{0}location =  {1}".F(indent, node.Location));
			Console.WriteLine("{0}key      = '{1}'".F(indent, node.Key));
			Console.WriteLine("{0}value    = '{1}'".F(indent, node.Value.Value));
			Console.WriteLine("{0}comment  = '{1}'".F(indent, node.Comment));
			Console.WriteLine("");

			foreach (var child in node.Value.Nodes)
				PrintNode(child, indentLevel + 1);
		}

		void IUtilityCommand.Run(Utility util, string[] args)
		{
			var nodes = MiniYaml.FromFile(inputMiniYamlFilePath, discardCommentsAndWhitespace: false);

			foreach (var node in nodes)
				PrintNode(node);
		}
	}
}
