using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace ServeRick
{
    public class WebItem
    {
        /// <summary>
        /// Originating file path for item
        /// <remarks>Is null if there is no file path (it may happen for runtime generated web items)</remarks>
        /// </summary>
        public readonly string FilePath;

        /// <summary>
        /// Actual response handler for web item
        /// <remarks>Can be reset according to file system changes</remarks>
        /// </summary>
        internal ResponseHandler Handler;

        /// <summary>
        /// Actual file watcher 
        /// <remarks>Can cause reseting of handler</remarks>
        /// </summary>
        internal FileSystemWatcher Watcher;

        /// <summary>
        /// ETag used in http headers
        /// </summary>
        internal string ETag;

        /// <summary>
        /// Create web item for given path
        /// </summary>
        /// <param name="filePath">Originating file path or null</param>
        internal WebItem(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Create web item based on runtime handler (without any originating file)
        /// </summary>
        /// <param name="handler">Runtime handler</param>
        /// <returns>Created web item</returns>
        public static WebItem Runtime(ResponseHandler handler)
        {
            var item = new WebItem(null);
            item.Handler = handler;

            return item;
        }
    }
}
