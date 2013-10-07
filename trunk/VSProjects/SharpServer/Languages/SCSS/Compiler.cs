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
    /// <summary>
    /// Compiler for css language
    /// </summary>
    class Compiler : CompilerBase
    {
        /// <summary>
        /// Parser used for parsing input code
        /// </summary>
        private static readonly Parser Parser = new Parser(new SCSS.Grammar());

        /// <summary>
        /// Emitter where compiled code is emitted
        /// </summary>
        readonly Emitter E;

        /// <summary>
        /// Root node of parsed input
        /// </summary>
        readonly Node _root;

        /// <summary>
        /// Discovered css blocks, identified by specifiers
        /// </summary>
        readonly Dictionary<string, CssBlock> _blocks = new Dictionary<string, CssBlock>();

        /// <summary>
        /// Declared variables with their coresponding values
        /// </summary>
        readonly Dictionary<string, string> _variables = new Dictionary<string, string>();

        private Compiler(Node root, Emitter emitter)
        {
            E = emitter;
            _root = root;
        }

        /// <summary>
        /// Compile given source using emitter
        /// </summary>
        /// <param name="source">Source input to be compiled</param>
        /// <param name="emitter">Emitter where compiled code is emitted</param>
        public static void Compile(string source, Emitter emitter)
        {
            source = source.Trim().Replace("\r"," ");
            var root = Parser.Parse(source);

            if (root == null)
            {
                //TODO error output
                emitter.ReportParseError("Parsing failed during to syntax error");
            }

            Print(root);

            var compiler = new Compiler(root, emitter);
            compiler.compile();
        }


        /// <summary>
        /// Discover and emit blocks from parsed tree
        /// </summary>
        private void compile()
        {
            var definitionsNode = GetNode(_root, "definitions");
            discoverDefinitions(definitionsNode);

            foreach (var block in _blocks.Values)
            {
                if (block.IsEmpty)
                {
                    //dont output blocks without any definitions
                    continue;
                }

                var constant = E.Constant(block.ToCSS());
                E.Write(constant);
            }
        }

        /// <summary>
        /// Discover top level definitions
        /// </summary>
        /// <param name="definitionsNode">Top level definitions node</param>
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
                    case "comment_def":
                        //there is nothing to do
                        break;
                    default:
                        throw new NotSupportedException("Unknown definition: " + definitionNode);
                }
            }
        }

        /// <summary>
        /// Discover css block according to its parent
        /// </summary>
        /// <param name="blockNode">Discovered block</param>
        /// <param name="parentBlock">Parent of discovered block (null if there is no parent)</param>
        private void discoverBlock(Node blockNode, CssBlock parentBlock)
        {            
            var block = getBlock(blockNode,parentBlock);

            if (_blocks.ContainsKey(block.Head))
            {
                _blocks[block.Head].AddDefinitions(block.Definitions);
            }
            else
            {
                _blocks.Add(block.Head, block);
            }
        }

        /// <summary>
        /// Get css block representation from given node
        /// </summary>
        /// <param name="blockNode">Node representing css block</param>
        /// <returns>Created css block</returns>
        private CssBlock getBlock(Node blockNode,CssBlock parentBlock)
        {
            var specifiersNode = GetNode(blockNode, "specifiers");
            var specifiers = getSpecifiers(specifiersNode);

            if (parentBlock != null)
            {
                specifiers.SetParent(parentBlock.Specifiers);
            }

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
                    case "block_def":
                        discoverBlock(definitionNode, block);
                        break;
                    case "comment_def":
                        //there is nothing to do with comments
                        break;

                    default:
                        throw new NotSupportedException("Unsupported definition: " + definitionNode);
                }
            }

            return block;
        }

        /// <summary>
        /// Get specifier list representation of given node
        /// </summary>
        /// <param name="specifiersNode">Node representing specifier list</param>
        /// <returns>Specifier list for given node</returns>
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

        /// <summary>
        /// Get specifier representation of given node
        /// </summary>
        /// <param name="specifierNode">Node representing specifier</param>
        /// <returns>String representation of specifier</returns>
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

        /// <summary>
        /// Define variable declared by given definition into global scope
        /// </summary>
        /// <param name="variableDefinition">Definition of variable</param>
        private void defineVariable(Node variableDefinition)
        {
            var name = GetTerminalText(variableDefinition.ChildNodes[0]);
            var value = GetTerminalText(variableDefinition.ChildNodes[1]);

            _variables[name] = value;
        }

        /// <summary>
        /// Add style defined by given styleDefinition into given block
        /// </summary>
        /// <param name="styleDefinition">Definition of added style</param>
        /// <param name="block">Block where definition will be added</param>
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
