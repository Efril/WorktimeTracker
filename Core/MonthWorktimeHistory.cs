using Core.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MonthWorktimeHistory : WorktimeHistoryBase
    {
        private readonly int _hash;

        public int Year
        {
            get;
            private set;
        }
        public int MonthOfTheYear
        {
            get;
            private set;
        }
        public WeekWorktimeHistory[] WorktimeByWeeks
        {
            get;
            private set;
        }
        public DayWorktimeHistory[] WorktimeByDays
        {
            get;
            private set;
        }

        public override bool Equals(object obj)
        {
            MonthWorktimeHistory other = obj as MonthWorktimeHistory;
            return other != null && other.Year==this.Year && other.MonthOfTheYear == this.MonthOfTheYear;
        }
        public override int GetHashCode()
        {
            return _hash;
        }

        public MonthWorktimeHistory(int Year, int MonthOfTheYear, WeekWorktimeHistory[] WorktimeByWeeks, DayWorktimeHistory[] WorktimeByDays, ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime, TimeSpan TotalWorktime)
            :base(ByProjectTotalWorktime, TotalWorktime)
        {
            Contract.Requires(WorktimeByWeeks != null);
            Contract.Requires(WorktimeByWeeks.Length == 4 || WorktimeByWeeks.Length == 5);
            Contract.Requires(WorktimeByDays != null);
            Contract.Requires(WorktimeByDays.Length == 4 || WorktimeByDays.Length == 5);
            this.WorktimeByDays = WorktimeByDays;
            this.WorktimeByWeeks = WorktimeByWeeks;
            this.Year = Year;
            this.MonthOfTheYear = MonthOfTheYear;
            _hash = HashCalculator.Calculate(Year, MonthOfTheYear);
        }
    }
}
