using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Linq.Expressions;

namespace SharpServer.Compiling
{
    class ExpressionUnit
    {
        public readonly IEnumerable<Expression> Dependencies;

        public readonly Expression Value;

        public readonly bool WriteToOutput;

        public ExpressionUnit(Expression value, params Expression[] dependencies)
        {
            Dependencies = dependencies;
            Value = value;
        }

        public ExpressionUnit(Expression value, params IEnumerable<Expression>[] dependecySets)
        {
            var dependencies = new List<Expression>();
            foreach (var set in dependecySets)
            {
                dependencies.AddRange(set);
            }

            Dependencies = dependencies.ToArray();
            Value = value;
        }

        public ExpressionUnit(Expression value, bool writeToOutput, params Expression[] dependencies)
        {
            Dependencies = dependencies;
            Value = value;
            WriteToOutput = writeToOutput;
        }
    }
}
