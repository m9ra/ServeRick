using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer.Compiling
{
    /// <summary>
    /// Helpers used by compiler
    /// </summary>
    static class CompilerHelpers
    {
        public static string print(object data)
        {
            return data.ToString();
        }

        public static Dictionary<TKey, TVal> CreateDictionary<TKey,TVal>(IEnumerable<KeyValuePair<TKey, TVal>> pairs)
        {
            var dict = new Dictionary<TKey, TVal>();
            foreach (var pair in pairs)
            {
                dict.Add(pair.Key, pair.Value);
            }

            return dict;
        }

        public static Dictionary<Key,Value> SetValue<Key,Value>(Dictionary<Key,Value> container, Key key, Value value)
        {
            container[key] = value;
            return container;
        }

        public static string AttributesToString(IDictionary<string, string> attributesContainer)
        {
            var attributes = new StringBuilder();
            if (attributesContainer.Count > 0)
            {
                attributes.Append(" ");
            }

            foreach (var pair in attributesContainer)
            {
                attributes.AppendFormat("{0}=\"{1}\" ", pair.Key, pair.Value);
            }


            return attributes.ToString();
        }

        public static Dictionary<TKey, TValue> PairsToContainer<TKey, TValue>(IEnumerable<Tuple<TKey, TValue>> pairs)
        {
            var container = new Dictionary<TKey, TValue>();
            foreach (var pair in pairs)
            {
                container[pair.Item1] = pair.Item2;
            }
            return container;
        }

    }
}
