using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace ServeRick.Compiling.Instructions
{
    class EmittedItem
    {
        internal readonly bool WriteToOutput;

        internal readonly Expression Expression;

        internal EmittedItem(Expression expression, bool writeToOutput)
        {
            Expression = expression;
            WriteToOutput = writeToOutput;
        }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }
}
