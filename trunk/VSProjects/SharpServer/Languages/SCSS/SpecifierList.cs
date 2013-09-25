using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Languages.SCSS
{
    class SpecifierList : IEnumerable<string>
    {
        List<string> _specifiers = new List<string>();
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
