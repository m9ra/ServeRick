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
            //   E.Write(compileBlock(_root));
            var definitionsNode = getNode(_root, "definitions");
            discoverDefinitions(definitionsNode);

            //TODO write css blocks
        }

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
                        discoverBlock(definitionNode);
                        break;
                    default:
                        throw new NotSupportedException("Unknown definition");
                }
            }
        }

        private void discoverBlock(ParseTreeNode blockNode)
        {
            var specifiersNode = getNode(blockNode, "specifiers");
            var specifiers = getSpecifiers(specifiersNode);


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
    }
}
