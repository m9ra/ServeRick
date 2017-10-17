using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Memory
{
    /// <summary>
    /// Server configuration of memory usage
    /// </summary>
    class MemoryConfiguration
    {
        /// <summary>
        /// Size of buffer used for every client
        /// </summary>
        internal readonly int ClientBufferSize;

        /// <summary>
        /// Maximal memory that can be used for client buffers
        /// </summary>
        internal readonly long MaximalClientMemoryUsage;

        internal MemoryConfiguration(int clientBufferSize, long maximalClientMemoryUsage)
        {
            ClientBufferSize = clientBufferSize;
            MaximalClientMemoryUsage = maximalClientMemoryUsage;
        }
    }
}
