using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Languages.SCSS
{
    class SpecifierList : IEnumerable<string>
    {
        List<string> _specifiers = new List<string>();

        internal void SetParent(SpecifierList parent)
        {
            //TODO resolve placeholder sign

            var prefix = parent.ToCSS();
            for (int i = 0; i < _specifiers.Count; ++i)
            {
                var specifier = prefix+" "+ _specifiers[i];
                _specifiers[i] = specifier;
            }
        }

        internal string ToCSS()
        {
            var css= string.Join(",", _specifiers.ToArray());

            return css;
        }

        internal void Add(string specifier)
        {
            _specifiers.Add(specifier);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _specifiers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _specifiers.GetEnumerator();
        }
    }
}
