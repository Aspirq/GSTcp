using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulaLib
{
    public class EventArgs<T> : EventArgs
    {
        private T item;

        public EventArgs(T item)
        {
            this.item = item;
        }

        public T Item
        {
            get { return item; }
        }
    }
}
