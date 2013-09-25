using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Languages.SCSS
{
    class CssBlock
    {
        internal readonly string BlockHead;
        internal readonly SpecifierList Specifiers;

        private readonly List<string> _styleDefinitions = new List<string>();

        public CssBlock(SpecifierList specifiers)
        {
            Specifiers = specifiers;

            var specifiersArray = specifiers.ToArray();
            BlockHead = string.Join(",", specifiersArray);
        }

        public void AddDefinition(string key, string cssValue)
        {
            _styleDefinitions.Add(key + " :" + cssValue + ";");
        }

        public string ToCSS()
        {
            var result = new StringBuilder();

            result.Append(BlockHead);
            result.Append('{');

            foreach (var def in _styleDefinitions)
            {
                result.Append(def);
            }

            result.Append("};");
            return result.ToString();
        }
    }
}
