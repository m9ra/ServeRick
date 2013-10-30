using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Languages.HAML.Compiling
{
    class HAMLInterval : IEnumerable<int>
    {
        readonly int _from;
        readonly int _to;

        public HAMLInterval(int from, int to)
        {
            _from = from;
            _to = to;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new HAMLIntervalEnumerator(_from, _to);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new HAMLIntervalEnumerator(_from, _to);
        }
    }

    class HAMLIntervalEnumerator:IEnumerator<int>
    {
        readonly int _from;
        readonly int _to;

        public HAMLIntervalEnumerator(int from, int to)
        {
            _from = from;
            _to = to;
        }


        public int Current
        {
            get;
            private set;
        }

        public void Dispose()
        {
            //nothing to dispose
        }

        object System.Collections.IEnumerator.Current
        {
            get { throw new NotImplementedException(); }
        }

        public bool MoveNext()
        {
            if(Current==_to)
                return false;
            ++Current;

            return true;
        }

        public void Reset()
        {
            Current = _from;
        }
    }
}
