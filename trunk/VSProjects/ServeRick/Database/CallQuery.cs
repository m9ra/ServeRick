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
        /// Arguments passed to the call
        /// </summary>
        private readonly Dictionary<string, object> _arguments = new Dictionary<string, object>();

        /// <summary>
        /// Name of represented call
        /// </summary>
        public readonly string CallName;

        /// <summary>
        /// Arguments passed to the call;
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Arguments { get { return _arguments; } }

        internal CallQuery(string callName, IEnumerable<KeyValuePair<string, object>> arguments)
        {
            CallName = callName;
            if (arguments != null)
            {
                foreach (var arg in arguments)
                {
                    _arguments[arg.Key] = arg.Value;
                }
            }
        }

        /// <summary>
        /// Execute without queueing for client
        /// </summary>
        internal CallQueryWorkItem<ActiveRecord> CreateWork(Action action)
        {
            return new CallQueryWorkItem<ActiveRecord>(this, action);
        }
    }
}
