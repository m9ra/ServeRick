using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ServeRick.Networking;

namespace ServeRick.Modules.Input
{
    /// <summary>
    /// TODO: needs optimization and encoding correctnes refactoring
    /// </summary>
    public class UrlEncodedInput : InputController
    {
        private readonly StringBuilder _input = new StringBuilder();

        private readonly int _inputLimit;

        public UrlEncodedInput(int inputLimit)
        {
            _inputLimit = inputLimit;
        }

        protected override void acceptData(byte[] data, int dataOffset, int dataLength)
        {
            if (_input.Length > _inputLimit)
            {
                Log.Error("Input limit {0} reached", _inputLimit);
                ContinueDownloading = false;
                return;
            }

            var stringedData = Encoding.UTF8.GetString(data, dataOffset, dataLength);
            _input.Append(stringedData);
        }

        protected override void onDownloadCompleted()
        {
            var inputData = _input.ToString();
            var match = HttpRequestParser.SplitterGET.Match(inputData);

            var variables = new Dictionary<string, string>();
            HttpRequestParser.ParseOutVariables(inputData, variables);

            foreach (var variable in variables)
            {
                SetPOST(variable.Key, variable.Value);
            }
        }
    }
}
