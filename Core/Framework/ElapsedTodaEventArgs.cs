using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    public class ElapsedTodaEventArgs:EventArgs
    {
        public TimeSpan ElapsedToday
        {
            get;
            private set;
        }

        public ElapsedTodaEventArgs(TimeSpan ElapsedToday)
        {
            this.ElapsedToday = ElapsedToday;
        }
    }
}
