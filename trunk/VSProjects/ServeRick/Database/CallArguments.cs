using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    public class CallArguments : IEnumerable<KeyValuePair<string, object>>
    {

        private readonly Dictionary<string, object> _storage = new Dictionary<string, object>();


        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        public void Add(string parameter, object value)
        {
            _storage[parameter] = value;
        }
    }
}
