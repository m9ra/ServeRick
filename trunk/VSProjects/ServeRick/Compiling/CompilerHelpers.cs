using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web;

namespace ServeRick.Compiling
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

        public static Dictionary<TKey, TVal> CreateDictionary<TKey, TVal>(IEnumerable<KeyValuePair<TKey, TVal>> pairs)
        {
            var dict = new Dictionary<TKey, TVal>();
            foreach (var pair in pairs)
            {
                dict.Add(pair.Key, pair.Value);
            }

            return dict;
        }

        public static Dictionary<Key, Value> SetValue<Key, Value>(Dictionary<Key, Value> container, Key key, Value value)
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
                if (pair.Key == null || pair.Value == null)
                    continue;

                var escaped = pair.Value.Replace("\"", "&quot;");
                attributes.AppendFormat("{0}=\"{1}\" ", pair.Key, escaped);
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

        public static void Yield(Response response, string identifier)
        {
            response.Yield(identifier);
        }

        public static string Flash(Response response, string messageId)
        {
            var result = response.Flash(messageId);
            if (result == null)
                result = "";

            return result;
        }

        public static T Param<T>(Response response, string identifier)
        {
            var param = response.GetParam(identifier);
            if (param == null)
                return default(T);

            return (T)param;
        }

        public static bool IsEqual<T1, T2>(T1 obj1, T2 obj2)
        {
            if (obj1 == null)
                return obj2 == null;

            return obj1.Equals(obj2);
        }

        public static string Concat(params object[] parts)
        {
            var result = string.Concat(parts);
            return result;
        }

        public static T EscapeHTML<T>(T html)
        {
            if (typeof(T) == typeof(string))
            {
                var htmlStr = html as string;
                if (htmlStr != null)
                {
                    //escape html
                    var result = (T)(object)HttpUtility.HtmlEncode(htmlStr);
                    return result;
                }
            }

            return html;
        }

        public static bool Not(bool value)
        {
            return !value;
        }
    }
}
