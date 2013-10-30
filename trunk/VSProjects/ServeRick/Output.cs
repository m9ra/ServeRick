using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

namespace ServeRick
{
    static class Output
    {
        public static string AsString(ParseTree tree)
        {
            var output = new StringBuilder();

            foreach (var tok in tree.Tokens)
            {
                output.AppendLine(tok.ToString().Trim());
            }

            if (tree.Root == null)
            {
                foreach (var message in tree.ParserMessages)
                {
                    output.AppendLine(message.ToString());
                    var str = message.Location.ToUiString();
                }
                return output.ToString();
            }

            output.Append(DisplayTree(tree.Root));
            return output.ToString();
        }

        public static string DisplayTree(ParseTreeNode node, int level = 0)
        {
            var output = new StringBuilder();
            //TODO proper resolving
            var isUnnammed = node.ToString().StartsWith("Unnamed");

            if (isUnnammed)
            {
                level -= 1;
            }
            else
            {
                var caption = node.ToString();
                output.AppendFormat("{0}{1}\n", "".PadLeft(level * 2), caption);
            }


            foreach (ParseTreeNode child in node.ChildNodes)
                output.Append(DisplayTree(child, level + 1));

            return output.ToString();
        }
    }
}
