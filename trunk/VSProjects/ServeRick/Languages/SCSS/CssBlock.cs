using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Languages.SCSS
{
    class CssBlock
    {
        internal readonly string Head;
        internal readonly SpecifierList Specifiers;

        private readonly List<string> _styleDefinitions = new List<string>();

        public IEnumerable<string> Definitions { get { return _styleDefinitions; } }

        public bool IsEmpty { get { return _styleDefinitions.Count == 0; } }

        public CssBlock(SpecifierList specifiers)
        {
            Specifiers = specifiers;

            var specifiersArray = specifiers.ToArray();
            Head = Specifiers.ToCSS();
        }

        public void AddDefinition(string key, string cssValue)
        {
            _styleDefinitions.Add(key + ": " + cssValue + ";");
        }

        public void AddDefinitions(IEnumerable<string> definitions)
        {
            _styleDefinitions.AddRange(definitions);
        }

        public string ToCSS()
        {
            var result = new StringBuilder();

            result.Append(Head);
            result.AppendLine("{");

            foreach (var def in _styleDefinitions)
            {
                result.AppendLine("\t"+def);
            }

            result.AppendLine("}");
            return result.ToString();
        }
    }
}
