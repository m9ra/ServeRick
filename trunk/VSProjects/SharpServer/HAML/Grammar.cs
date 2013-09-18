using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony;
using Irony.Parsing;
using Irony.Ast;
using Irony.Parsing.Construction;



namespace SharpServer.HAML
{
    [Language("HAML", "1.0", "Testing implementation for HAML")]
    public class Grammar : Irony.Parsing.Grammar
    {
        NonTerminal hash = new NonTerminal("hash");

        public Grammar()
        {
            var statement = generateStatementGrammar();
            generateTemplateGrammar(statement);

            MarkPunctuation("=", "!!!", ".", "#", "%", "render", "=>", ",", ")", "(", "}", "{");
            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF;
        }

        private NonTerminal generateStatementGrammar()
        {
            #region Terminals/Neterminals definitions

            var statement = NT("statement");
            var expression = NT("expression");

            var render = NT("render");
            var call = NT("call");
            var callName = NT("callName");
            var yield = NT("yield");

            var argList = NT("argList");
            var args = NT("args");

            var keyPair = NT("keyPair");
            var keyPairs = NT("keyPairs");

            var value = NT("value");


            var symbol = new RegexBasedTerminal("symbol", ":[a-zA-Z][a-zA-Z01-9_]*");
            var shortKey = new RegexBasedTerminal("shortKey", "[a-zA-Z][a-zA-Z01-9_]*:");
            var identifier = new RegexBasedTerminal("identifier", "[a-zA-Z][a-zA-Z01-9_]*");
            identifier.Priority = TerminalPriority.Low;

            #endregion

            //statement
            statement.Rule = render | expression;
            render.Rule = T_HIGH("render") + argList;

            //arguments
            argList.Rule = ("(" + args + ")") | args | Empty;
            args.Rule = (expression + "," + args) | expression;

            //call
            call.Rule = callName + argList;
            callName.Rule = identifier;

            //expression           
            yield.Rule = "yield" + (symbol | Empty);
            expression.Rule = yield | identifier | keyPair | symbol | value | call;
            keyPair.Rule = (symbol + "=>" + expression) | (shortKey + expression);
            keyPairs.Rule = (keyPair + "," + keyPairs) | keyPair;

            //literals
            value.Rule = new NumberLiteral("number") | new StringLiteral("string", @"""");

            //hash
            hash.Rule = "{" + keyPairs + "}";
            return statement;
        }

        private void generateTemplateGrammar(NonTerminal statement)
        {
            #region Terminals/Neterminals definitions

            var view = NT("view");
            var doctype = NT("doctype");

            var blocks = NT("blocks");
            var block = NT("block");
            var contentBlock = NT("contentBlock");
            var containerBlock = NT("containerBlock");

            var head = NT("head");
            var content = NT("content");
            var code = NT("code");

            var attrib = NT("attribute");
            var attribs = NT("attributes");

            var extAttrib = NT("extAttribute");
            var extAttribs = NT("extAttributes");

            var id = NT("id");
            var cls = NT("class");
            var tag = NT("tag");

            var identifier = new RegexBasedTerminal("identifier", "[a-zA-Z][a-zA-Z01-9]*");
            var rawOutput = new RegexBasedTerminal("rawOutput", "[^\\r\\n]+");
            rawOutput.Priority = TerminalPriority.Low;

            #endregion

            #region Grammar rules

            //block rules
            block.Rule = containerBlock | contentBlock;
            blocks.Rule = MakeStarRule(blocks, block);

            containerBlock.Rule = head + Eos + (Indent + blocks + Dedent);
            contentBlock.Rule = head + ((content + Eos) | Eos);

            //content rules
            content.Rule = code | rawOutput;
            code.Rule = "=" + statement;

            //head rules
            head.Rule = tag + attribs | tag | attribs | Empty;

            //attribute rules            
            attrib.Rule = id | cls;
            attribs.Rule = MakePlusRule(attribs, attrib);

            id.Rule = "#" + identifier;
            cls.Rule = "." + identifier;

            //tag rules
            tag.Rule = "%" + identifier + (Empty | hash);

            //root
            doctype.Rule = "!!!" + (identifier | Empty) + Eos;
            view.Rule = (doctype | Empty) + blocks;
            this.Root = view;

            #endregion
        }

        private NonTerminal NT(string name)
        {
            return new NonTerminal(name);
        }

        private KeyTerm T_HIGH(string terminal)
        {
            var term = ToTerm(terminal);
            term.Priority = TerminalPriority.High;
            return term;
        }

        public override void CreateTokenFilters(LanguageData language, TokenFilterList filters)
        {
            var outlineFilter = new CodeOutlineFilter(language.GrammarData,
                 OutlineOptions.ProduceIndents | OutlineOptions.CheckBraces, ToTerm(@"\")); // "\" is continuation symbol
            filters.Add(outlineFilter);
        }

        public override string ConstructParserErrorMessage(ParsingContext context, StringSet expectedTerms)
        {
            var expected = base.ConstructParserErrorMessage(context, expectedTerms);

            return string.Format("{0}\n but got: {1}", expected, context.CurrentParserInput);
        }

        public override void ReportParseError(ParsingContext context)
        {
            base.ReportParseError(context);
        }

        public override void OnScannerSelectTerminal(ParsingContext context)
        {
            base.OnScannerSelectTerminal(context);
        }
    }
}
