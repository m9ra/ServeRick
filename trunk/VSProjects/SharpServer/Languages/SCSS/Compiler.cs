using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;

using SharpServer.Compiling;
using SharpServer.Languages.HAML.Compiling;

namespace SharpServer.Languages.SCSS
{
    class Compiler : CompilerBase
    {
        readonly ParseTreeNode _root;
        readonly Emitter E;
        readonly Dictionary<string, CssBlock> _blocks = new Dictionary<string, CssBlock>();
        readonly Dictionary<string, string> _variables = new Dictionary<string, string>();

        private Compiler(ParseTreeNode root, Emitter emitter)
        {
            E = emitter;
            _root = root;
        }

        public static void Compile(ParseTreeNode root, Emitter emitter)
        {
            var compiler = new Compiler(root, emitter);
            compiler.compile();
        }

        private void compile()
        {
            var definitionsNode = getNode(_root, "definitions");
            discoverDefinitions(definitionsNode);

            foreach (var block in _blocks.Values)
            {
                var constant = E.Constant(block.ToCSS());
                E.Write(constant);
            }
        }

        /// <summary>
        /// Discover top level definitions
        /// </summary>
        /// <param name="definitionsNode"></param>
        private void discoverDefinitions(ParseTreeNode definitionsNode)
        {
            foreach (var child in definitionsNode.ChildNodes)
            {
                var definitionNode = skipUnnamedChildren(child);

                switch (getName(definitionNode))
                {
                    case "variable_def":
                        defineVariable(definitionNode);
                        break;
                    case "block_def":
                        discoverBlock(definitionNode,null);
                        break;
                    default:
                        throw new NotSupportedException("Unknown definition");
                }
            }
        }

        /// <summary>
        /// Discover top level block
        /// </summary>
        /// <param name="blockNode"></param>
        private void discoverBlock(ParseTreeNode blockNode, ParseTreeNode parentBlock)
        {
            if (parentBlock != null)
                throw new NotImplementedException();

            var block = getBlock(blockNode);

            _blocks.Add(block.BlockHead, block);
        }

        private CssBlock getBlock(ParseTreeNode blockNode)
        {
            var specifiersNode = getNode(blockNode, "specifiers");
            var specifiers = getSpecifiers(specifiersNode);

            var block = new CssBlock(specifiers);

            foreach (var definitionNode in getNode(blockNode, "definitions").ChildNodes)
            {
                var node = skipUnnamedChildren(definitionNode);

                switch (getName(node))
                {
                    case "variable_def":
                        defineVariable(definitionNode);
                        break;
                    case "style_def":
                        addStyle(node, block);
                        break;
                }
            }

            return block;
        }

        private SpecifierList getSpecifiers(ParseTreeNode specifiersNode)
        {
            var result = new SpecifierList();
            foreach (var specifierNode in specifiersNode.ChildNodes)
            {
                var specifier = getSpecifier(skipUnnamedChildren(specifierNode));

                result.Add(specifier);
            }

            return result;
        }


        private string getSpecifier(ParseTreeNode specifierNode)
        {
            specifierNode = skipUnnamedChildren(specifierNode);

            var childs = specifierNode.ChildNodes;
            var child = skipUnnamedChildren(childs[0]);
            var childText = getTerminalText(child);

            var specifierName = getName(specifierNode);
            switch (specifierName)
            {
                case "tag_specifier":
                    return childText;
                case "id_specifier":
                    return "#" + childText;
                case "class_specifier":
                    return "." + childText;
                case "adjacent_relation":
                    return getSpecifier(childs[0]) + " > " + getSpecifier(childs[1]);
                case "child_relation":
                    return getSpecifier(childs[0]) + " " + getSpecifier(childs[1]);
                case "pseudo_relation":
                    return getSpecifier(childs[0]) + ":" + getTerminalText(childs[1]);

                default:
                    throw new NotImplementedException("Unimplemented specifier: " + specifierName);
            }
        }

        private void defineVariable(ParseTreeNode variableDefinition)
        {
            var name = getTerminalText(variableDefinition.ChildNodes[0]);
            var value = getTerminalText(variableDefinition.ChildNodes[1]);

            _variables[name] = value;
        }

        private void addStyle(ParseTreeNode styleDefinition, CssBlock block)
        {
            var keyNode = styleDefinition.ChildNodes[0];
            var valueNode = styleDefinition.ChildNodes[1];

            var key = getTerminalText(keyNode);
            var value = getTerminalText(valueNode);

            foreach (var variable in _variables)
            {
                value = value.Replace("$" + variable.Key, variable.Value);
            }

            block.AddDefinition(key, value);
        }
    }
}
