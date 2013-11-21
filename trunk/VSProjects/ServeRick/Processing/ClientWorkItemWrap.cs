using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Processing
{
    class ClientWorkItemWrap:ClientWorkItem
    {
        readonly WorkItem _wrapped;

        internal ClientWorkItemWrap(WorkItem wrapped)
        {
            _wrapped = wrapped;
        }

        protected override WorkProcessor getPlannedProcessor()
        {
            return _wrapped.PlannedProcessor;
        }

        internal override void Run()
        {
            try
            {
                _wrapped.Run();
            }
            finally
            {
                Complete();
            }
        }
    }
}
