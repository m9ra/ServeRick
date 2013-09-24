using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony;
using Irony.Parsing;
using Irony.Ast;
using Irony.Parsing.Construction;

using SharpServer.Compiling;

namespace SharpServer.Languages.SCSS
{
    [Language("SCSS", "1.0", "Testing implementation for SCSS")]
    public class Grammar : GrammarBase
    {
        public Grammar()
        {
            var file = NT("file");
            var definitions = NT("definitions");
            var definition = NT("definition");

            var block_def = NT("block_def");
            var variable_def = NT("variable_def");
            var style_def = NT("style_def");

            var specifiers = NT("specifiers");
            var specifier = NT("specifier");
            var relation = NT("relation");
            var specifiers_relation = NT("specifiers_relation");
            var adjacent_relation = NT("adjacent_relation");
            var child_relation = NT("child_relation");

            var id_specifier = NT("id_specifier");
            var class_specifier = NT("class_specifier");
            var tag_specifier = NT("tag_specifier");

            var raw_value = T_REG("raw_value", "[^}{\\r\\n:;]+");
            var identifier = T_ID("identifier");
            identifier.Priority = raw_value.Priority + 1;

            this.Root = file;
            file.Rule = definitions + Eof;

            //  definitions.Rule = definition + ";" + (definitions | Empty);
            definitions.Rule = MakePlusRule(definitions, definition + ";");

            definition.Rule = variable_def | style_def | block_def;

            block_def.Rule = specifiers + T_HIGH("{") + definitions + T_HIGH("}");
            variable_def.Rule = "$" + identifier + ":" + raw_value;
            style_def.Rule = identifier + ":" + raw_value;

            specifiers.Rule = MakePlusRule(specifiers, T(","), specifier | relation);
            specifier.Rule = id_specifier | class_specifier | tag_specifier;
            id_specifier.Rule = "#" + identifier;
            class_specifier.Rule = "." + identifier;
            tag_specifier.Rule = identifier;

            relation.Rule = adjacent_relation | child_relation;
            adjacent_relation.Rule = specifier + ">" + specifier;
            child_relation.Rule = specifier + specifier;

            MarkPunctuation("$", ",", ":", ";", "{", "}", "#", ".", ">");
            MarkTransient(definition, specifier, relation);
        }
    }
}
