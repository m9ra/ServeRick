using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    /// <summary>
    /// Base class for call query processed on table
    /// </summary>
    public abstract class CallQuery
    {
        internal CallQuery()
        {
            //prevent from subclassing
        }
    }

    /// <summary>
    /// Call query processed on table
    /// </summary>
    /// <typeparam name="ActiveRecord">Type of active record.</typeparam>
    public class CallQuery<ActiveRecord> : CallQuery
        where ActiveRecord : DataRecord
    {
        /// <summary>
        /// Name of represented call
        /// </summary>
        public readonly string CallName;

        /// <summary>
        /// Arguments passed to the call
        /// </summary>
        public IEnumerable<object> Arguments;

        internal CallQuery(string callName, IEnumerable<object> arguments)
        {
            CallName = callName;
            Arguments = arguments.ToArray();
        }

        /// <summary>
        /// Execute without queueing for client
        /// </summary>
        internal CallQueryWorkItem<ActiveRecord> CreateWork()
        {
            return new CallQueryWorkItem<ActiveRecord>(this);
        }
    }
}
