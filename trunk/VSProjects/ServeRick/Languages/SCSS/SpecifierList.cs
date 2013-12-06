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

            var newSpecifiers=new List<string>();

            foreach (var prefix in parent._specifiers)
            {
                foreach(var suffix in _specifiers)
                {
                    var specifier = prefix + " " + suffix;
                    newSpecifiers.Add(specifier);
                }
            }

            _specifiers=newSpecifiers;
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
