using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

using Irony;
using Irony.Parsing;
using Irony.Ast;
using Irony.Parsing.Construction;

using SharpServer.Compiling;

namespace SharpServer.Languages.SCSS
{
    [Language("SCSS", "1.0", "Testing implementation for SCSS")]
    public class Grammar : Parsing.GrammarBase
    {
        public Grammar()
        {
            var file = NT("file");
            var definitions = NT("definitions");
            var definition = NT("definition");

            var block_def = NT("block_def");
            var variable_def = NT("variable_def");
            var style_def = NT("style_def");
            var comment_def = NT("comment_def");

            var specifiers = NT("specifiers");
            var specifier = NT("specifier");
            var relation = NT("relation");
            var specifiers_relation = NT("specifiers_relation");
            var adjacent_relation = NT("adjacent_relation");
            var child_relation = NT("child_relation");
            var pseudo_relation = NT("pseudo_relation");

            var id_specifier = NT("id_specifier");
            var class_specifier = NT("class_specifier");
            var tag_specifier = NT("tag_specifier");

            var raw_value = T_REG("[^}{\\r\\n:;]+", "raw_value");
            var identifier = T_REG("[a-zA-Z][a-zA-Z1-90_()-]*", "identifier");

            var line_comment = T_REG("[^\n]*[\n]?", "line_comment");
            var multiline_comment = T_REG("( [^*] | [*][^/] )*", "multiline_comment");

            this.Root = file;
            file.Rule = definitions;
            
            definitions.Rule = MakeStarRule(definition);

            definition.Rule = variable_def | block_def | style_def | comment_def;

            block_def.Rule = specifiers + "{" + definitions + "}";
            variable_def.Rule = "$" + identifier + ":" + raw_value + ";";
            style_def.Rule = identifier + ":" + raw_value + ";";

            specifiers.Rule = MakePlusRule(specifier | relation, ",");
            specifier.Rule = id_specifier | class_specifier | tag_specifier;
            id_specifier.Rule = "#" + identifier;
            class_specifier.Rule = "." + identifier;
            tag_specifier.Rule = identifier;

            relation.Rule = adjacent_relation | child_relation | pseudo_relation;
            adjacent_relation.Rule = specifier + ">" + specifier;
            child_relation.Rule = specifier + specifier;
            pseudo_relation.Rule = specifier + ":" + identifier;

            comment_def.Rule = ("//" + line_comment) | ("/*" + multiline_comment + "*/");

            MarkPunctuation("$", ",", ":", ";", "{", "}", "#", ".", ">","*/","/*","//", "");
            MarkTransient(definition, specifier, relation);
        }
    }
}
