using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Parsing;

namespace SharpServer.Languages.HAML
{
    public class Grammar : GrammarBase
    {
        NonTerminal hash;

        readonly Terminal INDENT;

        readonly Terminal DEDENT;

        readonly Terminal EOL;

        public Grammar()
        {
            INDENT = T_SPEC("INDENT");
            DEDENT = T_SPEC("DEDENT");
            EOL = T_SPEC("EOL");

            hash = NT("hash");

            var statement = generateStatementGrammar();
            generateTemplateGrammar(statement);

            MarkPunctuation("=", "!!!", ".", "#", "%", "render", "=>", ",", ")", "(", "}", "{", "");
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


            var symbol = T_REG(":[a-zA-Z][a-zA-Z01-9_]*", "symbol");
            var shortKey = T_REG("[a-zA-Z][a-zA-Z01-9_]*:", "shortKey");
            var identifier = T_REG("[a-zA-Z][a-zA-Z01-9_]*", "identifier").Exclude("yield");
            var numberLiteral = T_REG("/d+", "number");
            var stringLiteral = T_REG(@""" [^""]* """, "string");

            #endregion

            //statement
            statement.Rule = render | expression;
            render.Rule = "render" + argList;

            //arguments
            argList.Rule = ("(" + args + ")") | args | Empty;
            args.Rule = (expression + "," + args) | expression;

            //call
            call.Rule = callName + argList;
            callName.Rule = identifier;

            //expression           
            yield.Rule = "yield" + (symbol | Empty);
            expression.Rule = yield | keyPair | symbol | value | call | identifier;
            keyPair.Rule = (symbol + "=>" + expression) | (shortKey + expression);
            keyPairs.Rule = MakeStarRule(keyPair, ",");

            //literals
            value.Rule = stringLiteral | numberLiteral;

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

            var identifier = T_REG("[a-zA-Z][a-zA-Z01-9]*", "identifier");
            var rawOutput = T_REG("[^!%#={.][^\\r\\n]*", "rawOutput");

            #endregion

            #region Grammar rules

            //block rules
            block.Rule = containerBlock | contentBlock;
            blocks.Rule = MakeStarRule(block);

            containerBlock.Rule = head + EOL + INDENT + blocks + DEDENT;
            contentBlock.Rule = (head + EOL + Q(INDENT + content + DEDENT)) | (Q(head) + content + EOL);

            //content rules
            content.Rule = code | rawOutput;
            code.Rule = "=" + statement;

            //head rules
            head.Rule = (tag + Q(hash) + Q(attribs)) | attribs;

            //attribute rules            
            attrib.Rule = id | cls;
            attribs.Rule = MakePlusRule(attrib);

            id.Rule = "#" + identifier;
            cls.Rule = "." + identifier;

            //tag rules
            tag.Rule = "%" + identifier;

            //root
            doctype.Rule = "!!!" + (identifier | Empty);
            view.Rule = BOF + (doctype + EOL | Empty) + blocks + EOF;
            this.Root = view;

            MarkTransient(attrib);

            #endregion
        }


        protected override IEnumerable<Token> OutlineTokens(IEnumerable<Token> tokens)
        {
            //let tokens process by standard way
            tokens = base.OutlineTokens(tokens);

            var result = new List<Token>();
            var indentLevel = 0;
            foreach (var token in tokens)
            {
                if (token.IsSpecial)
                {
                    if (token.Name == "EOF")
                    {
                        for (int i = 0; i < indentLevel; ++i)
                            result.Add(Token.Special("DEDENT", token.StartPosition));
                    }

                    result.Add(token);
                }
                else
                {
                    //split data tokens according to lines and set indentation
                    result.AddRange(splitToken(token, ref indentLevel));
                }
            }

            return result;
        }

        private IEnumerable<Token> splitToken(Token token, ref int indentLevel)
        {
            var result = new List<Token>();
            var previousLineStartOffset = -1;
            var currentCharIndex = 0;
            var globalStart = token.StartPosition;
            while (currentCharIndex < token.Length)
            {
                var lineStart = currentCharIndex + globalStart;
                var textStartOffset = findTextStartOffset(token, currentCharIndex);
                var textStart = currentCharIndex + textStartOffset + globalStart;

                Token indentation = null;
                if (previousLineStartOffset >= 0)
                {
                    if (previousLineStartOffset < textStartOffset)
                    {
                        ++indentLevel;
                        indentation = Token.Special("INDENT", textStart);
                    }
                    else if (previousLineStartOffset > textStartOffset)
                    {
                        --indentLevel;
                        indentation = Token.Special("DEDENT", textStart);
                    }
                }
                previousLineStartOffset = textStartOffset;

                var lineLength = findLineEndOffset(token, textStart);
                var lineEnd = textStart + lineLength;

                var bol = Token.Special("BOL", lineStart);
                var line = Token.Text(token.Data.Substring(textStart, lineLength), textStart);
                var eol = Token.Special("EOL", lineEnd);

                if (lineStart < textStart)
                    result.Add(Token.Text(token.Data.Substring(lineStart, textStart - lineStart), lineStart));
                //       result.Add(bol);
                if (indentation != null)
                    result.Add(indentation);


                result.Add(line);
                result.Add(eol);

                currentCharIndex = lineEnd - globalStart;
            }

            return result;
        }

        private int findLineEndOffset(Token token, int start)
        {
            for (var i = start; i < token.Length; ++i)
            {
                if (token.Data[i] == '\n')
                    return i - start + 1;
            }

            return token.Length - start;
        }

        private int findTextStartOffset(Token token, int start)
        {
            for (var i = start; i < token.Length; ++i)
            {
                var currentChar = token.Data[i];

                if (!char.IsWhiteSpace(currentChar))
                    return i - start;
            }

            return token.Length - start;
        }
    }
}
