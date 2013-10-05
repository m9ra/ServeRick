using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing;

using SharpServer.Compiling;
using SharpServer.Languages.HAML.Compiling;

namespace SharpServer.Languages.SCSS
{
    class Compiler : CompilerBase
    {
        private static readonly Parsing.Parser Parser = new Parsing.Parser(new SCSS.Grammar());
        readonly Node _root;
        readonly Emitter E;
        readonly Dictionary<string, CssBlock> _blocks = new Dictionary<string, CssBlock>();
        readonly Dictionary<string, string> _variables = new Dictionary<string, string>();

        private Compiler(Node root, Emitter emitter)
        {
            E = emitter;
            _root = root;
        }

        public static void Compile(string source, Emitter emitter)
        {
            source = source.Trim();
            var root = Parser.Parse(source);

            if(root==null){
                //TODO error output
                emitter.ReportParseError("Parsing failed during to syntax error");
            }

            print(root);
                     
            var compiler = new Compiler(root, emitter);
            compiler.compile();
        }
        
        static void print(Node node, int level = 0)
        {
            Console.WriteLine("".PadLeft(level * 2, ' ') + node);
            foreach (var child in node.ChildNodes)
            {
                print(child, level + 1);
            }
        }

        private void compile()
        {
            var definitionsNode = GetNode(_root, "definitions");
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
        private void discoverDefinitions(Node definitionsNode)
        {
            foreach (var definitionNode in definitionsNode.ChildNodes)
            {

                switch (definitionNode.Name)
                {
                    case "variable_def":
                        defineVariable(definitionNode);
                        break;
                    case "block_def":
                        discoverBlock(definitionNode, null);
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
        private void discoverBlock(Node blockNode, Node parentBlock)
        {
            if (parentBlock != null)
                throw new NotImplementedException();

            var block = getBlock(blockNode);

            _blocks.Add(block.BlockHead, block);
        }

        private CssBlock getBlock(Node blockNode)
        {
            var specifiersNode = GetNode(blockNode, "specifiers");
            var specifiers = getSpecifiers(specifiersNode);

            var block = new CssBlock(specifiers);

            foreach (var definitionNode in GetNode(blockNode, "definitions").ChildNodes)
            {
                switch (definitionNode.Name)
                {
                    case "variable_def":
                        defineVariable(definitionNode);
                        break;
                    case "style_def":
                        addStyle(definitionNode, block);
                        break;
                }
            }

            return block;
        }

        private SpecifierList getSpecifiers(Node specifiersNode)
        {
            var result = new SpecifierList();
            foreach (var specifierNode in specifiersNode.ChildNodes)
            {
                var specifier = getSpecifier(specifierNode);

                result.Add(specifier);
            }

            return result;
        }


        private string getSpecifier(Node specifierNode)
        {
            var childs = specifierNode.ChildNodes;
            var child = childs[0];
            var childText = GetTerminalText(child);

            var specifierName = specifierNode.Name;
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
                    return getSpecifier(childs[0]) + ":" + GetTerminalText(childs[1]);

                default:
                    throw new NotImplementedException("Unimplemented specifier: " + specifierName);
            }
        }

        private void defineVariable(Node variableDefinition)
        {
            var name = GetTerminalText(variableDefinition.ChildNodes[0]);
            var value = GetTerminalText(variableDefinition.ChildNodes[1]);

            _variables[name] = value;
        }

        private void addStyle(Node styleDefinition, CssBlock block)
        {
            var keyNode = styleDefinition.ChildNodes[0];
            var valueNode = styleDefinition.ChildNodes[1];

            var key = GetTerminalText(keyNode);
            var value = GetTerminalText(valueNode);

            foreach (var variable in _variables)
            {
                value = value.Replace("$" + variable.Key, variable.Value);
            }

            block.AddDefinition(key, value);
        }
    }
}
