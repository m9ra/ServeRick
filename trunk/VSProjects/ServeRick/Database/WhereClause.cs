using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    public enum WhereOperation
    {
        Equal,
        HasSubstring
    }

    public class WhereItem
    {
        public readonly string Column;

        public readonly WhereOperation Operation;

        public object Operand;

        public WhereItem(string column, WhereOperation operation, object operand)
        {
            Column = column;
            Operation = operation;
            Operand = operand;
        }
    }

    public class WhereClause:IEnumerable<WhereItem>
    {
        private readonly List<WhereItem> _items;

        internal WhereClause()
        {
            _items = new List<WhereItem>();
        }

        private WhereClause(WhereClause cloned) {
            _items = new List<WhereItem>(cloned._items);
        }

        internal WhereClause Clone()
        {
            return new WhereClause(this);
        }

        internal void Add(WhereItem item)
        {
            _items.Add(item);
        }

        public IEnumerator<WhereItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
