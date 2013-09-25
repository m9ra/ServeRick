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
            var pseudo_relation = NT("pseudo_relation");

            var id_specifier = NT("id_specifier");
            var class_specifier = NT("class_specifier");
            var tag_specifier = NT("tag_specifier");

            //needed because of Irony's weakness...
            var dotted_specifier = NT("dotted_specifier");
            var dotted_tag_specifier = NT("tag_specifier");
            var dotted_identifier = NT("dotted_identifier");

            var raw_value = T_REG("raw_value", "[^}{\\r\\n:;,]+");
            var identifier = T_ID("identifier");            
            identifier.AllChars += "-";

            identifier.Priority = raw_value.Priority + 1;

            this.Root = file;
            file.Rule = definitions + Eof;

            //  definitions.Rule = definition + ";" + (definitions | Empty);
            definitions.Rule = MakeStarRule(definitions, definition);

            definition.Rule = variable_def | block_def | style_def;

            block_def.Rule = specifiers + T_HIGH("{") + definitions + T_HIGH("}") + T(";").Q();
            variable_def.Rule = "$" + identifier + ":" + raw_value + ";";
            style_def.Rule = dotted_identifier + raw_value + ";";

            specifiers.Rule = MakePlusRule(specifiers, T(","), specifier | relation);
            specifier.Rule = id_specifier | class_specifier | tag_specifier;
            id_specifier.Rule = "#" + identifier;
            class_specifier.Rule = "." + identifier;
            tag_specifier.Rule = identifier;

            relation.Rule = adjacent_relation | child_relation | pseudo_relation;
            adjacent_relation.Rule = specifier + ">" + specifier;
            child_relation.Rule = specifier + specifier;
            pseudo_relation.Rule = dotted_specifier + identifier;

            dotted_specifier.Rule = dotted_tag_specifier | ((id_specifier | class_specifier) + ":");
            dotted_tag_specifier.Rule = dotted_identifier;
            dotted_identifier.Rule = identifier + T_HIGH(":");

            MarkPunctuation("$", ",", ":", ";", "{", "}", "#", ".", ">");
            MarkTransient(definition, specifier, relation, dotted_identifier, dotted_specifier);
        }

        public override void OnScannerSelectTerminal(ParsingContext context)
        {
            
            base.OnScannerSelectTerminal(context);
        }

    }
}
