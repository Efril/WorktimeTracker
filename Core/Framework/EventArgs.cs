using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    public class EventArgs<TValue>:EventArgs
    {
        public TValue Value
        {
            get;
            set;
        }

        public EventArgs(TValue Value)
        {
            this.Value = Value;
        }
        public EventArgs() { }
    }
}
