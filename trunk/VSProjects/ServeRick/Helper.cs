using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick
{
    public class Helper
    {
        private static readonly object _L_random = new object();

        private static readonly Random _rnd = new Random();

        public static string ToJson(object data)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var json = serializer.Serialize(data);

            return json;
        }

        public static T JsonDecode<T>(string json)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Deserialize<T>(json);
        }

        public static string CreateRandomString(int length, string availableChars = null)
        {
            if (availableChars == null)
                availableChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOQRSTUVWXYZ0123456789";

            var chars = new char[length];
            lock (_L_random)
            {
                for (int i = 0; i < chars.Length; ++i)
                {
                    int randomIndex;

                    randomIndex = _rnd.Next(availableChars.Length);

                    chars[i] = availableChars[randomIndex];
                }
            }
            var result = new string(chars);
            return result;
        }
    }
}
