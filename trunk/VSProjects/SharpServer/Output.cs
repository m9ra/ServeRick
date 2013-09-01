using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

namespace SharpServer
{
    static class Output
    {
        public static void DisplayTree(ParseTree tree)
        {
            foreach (var tok in tree.Tokens)
            {
                Console.WriteLine(tok.ToString().Trim());
            }

            if (tree.Root == null)
            {
                foreach (var message in tree.ParserMessages)
                {
                    Console.WriteLine(message);
                    var str = message.Location.ToUiString();
                }
                return;
            }

          DisplayTree(tree.Root);
        }

        public static void DisplayTree(ParseTreeNode node, int level = 0)
        {
            //TODO proper resolving
            var isUnnammed = node.ToString().StartsWith("Unnamed");

            if (isUnnammed)
            {
                level -= 1;
            }
            else
            {
                var caption = node.ToString();
                Console.WriteLine("{0}{1}", "".PadLeft(level * 2), caption);
            }


            foreach (ParseTreeNode child in node.ChildNodes)
                DisplayTree(child, level + 1);
        }
    }
}
