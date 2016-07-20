using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MonthWorktime : IWorktimeHistory
    {
        public int MonthOfTheYear
        {
            get;
            private set;
        }
        public TimeSpan[] WorktimeByWeeks
        {
            get;
            private set;
        }
        public TimeSpan[] WorktimeByDays
        {
            get;
            private set;
        }
        public TimeSpan TotalWorktime
        {
            get;
            private set;
        }
        public ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime
        {
            get;
            private set;
        }

        public override bool Equals(object obj)
        {
            MonthWorktime other = obj as MonthWorktime;
            return other != null && other.MonthOfTheYear == this.MonthOfTheYear;
        }
        public override int GetHashCode()
        {
            return MonthOfTheYear.GetHashCode();
        }
    }
}
