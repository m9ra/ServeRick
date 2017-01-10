using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing;

namespace ServeRick.Languages.HAML
{
    public class Grammar : GrammarBase
    {
        /// <summary>
        /// Number of spaces for one tabulator
        /// Is used for indentation
        /// </summary>
        readonly int TabWidth = 4;

        /// <summary>
        /// Special terminal for indentation
        /// </summary>
        readonly Terminal INDENT = T_SPEC(IndentOutliner.Indent);

        /// <summary>
        /// Special terminal for dedentation
        /// </summary>
        readonly Terminal DEDENT = T_SPEC(IndentOutliner.Dedent);

        /// <summary>
        /// Special terminal for end of line
        /// </summary>
        readonly Terminal EOL = T_SPEC(IndentOutliner.EOL);

        /// <summary>
        /// Special terminal for begining of line
        /// </summary>
        readonly Terminal BOL = T_SPEC(IndentOutliner.BOL);

        /// <summary>
        /// Nonterminal for hash expression
        /// </summary>
        readonly NonTerminal hash = NT("hash");

        /// <summary>
        /// NonTerminal for param usage
        /// </summary>
        readonly NonTerminal param = NT("param");

        /// <summary>
        /// NonTerminal for block definition
        /// </summary>
        readonly NonTerminal block = NT("block");

        /// <summary>
        /// NonTerminal for blocks definition
        /// </summary>
        readonly NonTerminal blocks = NT("blocks");

        readonly Terminal codePrefix = T_REG("[-=~]", "codePrefix");

        public Grammar()
        {
            var statement = generateStatementGrammar();
            generateTemplateGrammar(statement);

            MarkPunctuation("", "!!!", ".", "#", "%", "render", "=>",
                ")", "(", "@", "|", "else", "if", "do", ".."
                , "}", "{", ","
                );
        }

        private NonTerminal generateStatementGrammar()
        {
            #region Terminals/Neterminals definitions

            var statement = NT("statement");
            var expression = NT("expression");
            var binaryExpression = NT("binaryExpression");
            var ifStatement = NT("ifStatement");
            var render = NT("render");

            var condition = NT("condition");
            var ifBranch = NT("ifBranch");
            var elseBranch = NT("elseBranch");
            var branch = NT("branch");

            var methodCall = NT("methodCall");
            var call = NT("call");
            var callName = NT("callName");
            var yield = NT("yield");

            var argList = NT("argList");
            var args = NT("args");
            var calledObject = NT("calledObject");

            var keyPair = NT("keyPair");
            var keyPairs = NT("keyPairs");

            var interval = NT("interval");
            var value = NT("value");

            var blockArguments = NT("blockArguments");
            var lambdaBlock = NT("lambdaBlock");

            var symbol = T_REG(":[a-zA-Z][a-zA-Z01-9_-]*", "symbol");
            var shortKey = T_REG("[a-zA-Z][a-zA-Z01-9_-]*:", "shortKey");
            var identifier = T_REG("[a-zA-Z][a-zA-Z01-9_-]*", "identifier")
                .Exclude("yield", "if", "else", "true", "false", "raw");

            var boolLiteral = T_REG("(true|false)", "bool");
            var numberLiteral = T_REG(@"\d+", "number");
            var stringLiteral = T_REG(@""" [^""]* """, "string");
            var binaryExpressionOperator = T_REG("==|[+]|-|[*]|/", "binaryExpressionOperator");


            #endregion

            //statement
            statement.Rule = render | expression | ifStatement;
            render.Rule = "render" + argList;

            //if
            ifStatement.Rule = "if" + condition + ifBranch + Q(elseBranch);
            condition.Rule = expression;
            ifBranch.Rule = branch;
            elseBranch.Rule = BOL + codePrefix + "else" + branch;

            //let last EOL be consumed from parent
            branch.Rule = statement | (EOL + INDENT + blocks + DEDENT);

            //lambda block
            lambdaBlock.Rule = "do" + blockArguments + EOL + INDENT + blocks + DEDENT;
            blockArguments.Rule = "|" + identifier + "|";

            //arguments
            argList.Rule = ("(" + args + ")") | args | Empty;
            args.Rule = MakeStarRule(expression, ",");

            //call
            call.Rule = callName + argList + Q(lambdaBlock);
            callName.Rule = identifier;

            //method call
            methodCall.Rule = calledObject + "." + call;
            calledObject.Rule = expression;

            //expression           
            yield.Rule = "yield" + (symbol | Empty);
            expression.Rule = yield | keyPair | symbol | value | call | identifier | param | interval | binaryExpression | methodCall | "(" + expression + ")";

            binaryExpression.Rule = expression + binaryExpressionOperator + expression;
            keyPair.Rule = (symbol + "=>" + expression) | (shortKey + expression);
            keyPairs.Rule = MakeStarRule(keyPair, ",");

            interval.Rule = expression + ".." + expression;

            //literals
            value.Rule = stringLiteral | numberLiteral | boolLiteral;

            //hash
            hash.Rule = "{" + keyPairs + "}";
            return statement;
        }

        private void generateTemplateGrammar(NonTerminal statement)
        {
            #region Terminals/Neterminals definitions

            var view = NT("view");
            var doctype = NT("doctype");

            var contentBlock = NT("contentBlock");

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

            var paramDeclarations = NT("paramDeclarations");
            var paramDeclaration = NT("paramDeclaration");

            var typeModifier = T_REG("\\+", "typeModifier");
            var type = T_REG(@"\w[\w.]+", "type");
            var identifier = T_REG("[a-zA-Z_][a-zA-Z01-9_-]*", "identifier");
            var rawOutput = T_REG("[^@!%#=.{-][^\\r\\n]*", "rawOutput");

            #endregion

            #region Grammar rules

            //block rules
            block.Rule = contentBlock;
            blocks.Rule = MakeStarRule(block);

            contentBlock.Rule = BOL + (
                (Q(head) + EOL + Q(INDENT + blocks + DEDENT)) |
                (Q(head) + content + Q(EOL))
                );

            //content rules
            content.Rule = code | rawOutput;
            code.Rule = codePrefix + Q("raw") + statement;

            //head rules
            head.Rule = (tag + Q(hash) + Q(attribs)) | Q(hash) + attribs;

            //attribute rules            
            attrib.Rule = id | cls;
            attribs.Rule = MakePlusRule(attrib);

            id.Rule = "#" + identifier;
            cls.Rule = "." + identifier;

            //tag rules
            tag.Rule = "%" + identifier;

            //root
            doctype.Rule = "!!!" + Q(identifier);
            view.Rule = BOF + paramDeclarations + Q(BOL + doctype + EOL) + blocks + EOF;
            this.Root = view;

            //params
            paramDeclarations.Rule = MakeStarRule(paramDeclaration);
            paramDeclaration.Rule = BOL + param + Q(typeModifier) + type + EOL;
            param.Rule = "@" + identifier;

            MarkTransient(attrib);

            #endregion
        }


        protected override IEnumerable<Token> OutlineTokens(IEnumerable<Token> tokens)
        {
            //let tokens process by standard way
            tokens = base.OutlineTokens(tokens);

            var outliner = new IndentOutliner(tokens, TabWidth);
            var result = outliner.Outline();

            var filteredResult = result.Where(t => t.IsSpecial || t.Data.Trim().Length > 0).ToList();

            return filteredResult;
        }
    }
}