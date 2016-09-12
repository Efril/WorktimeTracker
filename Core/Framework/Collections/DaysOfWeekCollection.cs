using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework.Collections
{
    public class DaysOfWeekCollection : IEnumerable<DayOfWeek>
    {
        public static readonly DaysOfWeekCollection DaysOfWeek = new DaysOfWeekCollection();

        public IEnumerator<DayOfWeek> GetEnumerator()
        {
            return new DaysOfWeekEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
