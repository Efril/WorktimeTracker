using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class WeekWorktime:IWorktimeHistory
    {
        public ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime
        {
            get;
            private set;
        }
        public ReadOnlyDictionary<DayOfWeek, TimeSpan> WorktimeByDaysOfWeek
        {
            get;
            private set;
        }
        public DayOfWeek[] WorktimeByDays
        {
            get;
            private set;
        }
        public TimeSpan TotalWorktime
        {
            get;
            private set;
        }
    }
}
