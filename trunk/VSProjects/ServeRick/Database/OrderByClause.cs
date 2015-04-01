using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeRick.Database
{
    public class OrderByItem
    {
        public readonly string Column;

        public readonly bool IsDescendant;

    }

    public class OrderByClause 
    {
        public IEnumerable<OrderByItem> Items { get { return _items; } }

        public readonly bool IsRandom;

        private readonly List<OrderByItem> _items;

        internal OrderByClause()
        {
            _items= new List<OrderByItem>();
        }

        private OrderByClause(IEnumerable<OrderByItem> items, bool isRandom)
        {
            _items = new List<OrderByItem>(items);
            IsRandom = isRandom;
        }

        internal OrderByClause SetRandom(bool isRandom)
        {
            return new OrderByClause(Items, isRandom);
        }
    }
}
