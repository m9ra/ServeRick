using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer
{
    internal delegate void ResponseHandler(Response response);

    class Response
    {
        StringBuilder _builder = new StringBuilder();
        public void Write(string data)
        {
            if (_builder.Length > 10000)
                _builder.Clear();
            _builder.Append(data);
        }

        public string GetResult()
        {
            return _builder.ToString();
        }
    }
}
