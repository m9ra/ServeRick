using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick;
using ServeRick.Networking;
using ServeRick.Modules.Input;

namespace ServeRick.Defaults
{
    public class InputManager : InputManagerBase
    {
        const string UrlEncoded_ContentType = "application/x-www-form-urlencoded";
        const string ContentType_Header = "Content-type";
        const string ContentLength_Header = "Content-length";

        StringBuilder _testInput = new StringBuilder();

        protected override InputController createController(HttpRequest request)
        {
            string contentType;
            request.TryGetHeader(ContentType_Header, out contentType, UrlEncoded_ContentType);

            string contentLengthStr;
            request.TryGetHeader(ContentLength_Header, out contentLengthStr, "0");

            int contentLength;
            int.TryParse(contentLengthStr, out contentLength);
            contentLength = Math.Min(10 * 1024, contentLength);

            var lowerContentType = contentType.ToLower();
            if (contentType.Contains(UrlEncoded_ContentType))
            {
                return new UrlEncodedInput(contentLength);
            }
            else
            {
                Log.Error("Unknown POST content type: " + contentType);
                return null;
            }
        }
    }
}
