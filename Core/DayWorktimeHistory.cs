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
    public class DayWorktimeHistory : WorktimeHistoryBase
    {
        private readonly int _hash;

        #region -> Interface <-

        public int DayOfMonth
        {
            get;
            private set;
        }
        public DayOfWeek DayOfWeek
        {
            get;
            private set;
        }

        public override bool Equals(object obj)
        {
            DayWorktimeHistory other = obj as DayWorktimeHistory;
            return other != null && other.DayOfMonth==this.DayOfMonth && other.DayOfWeek == this.DayOfWeek;
        }
        public override int GetHashCode()
        {
            return _hash;
        }

        #endregion

        #region -> Constructors <-

        public DayWorktimeHistory(DayOfWeek DayOfWeek, int DayOfMonth, ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime, TimeSpan TotalWorktime)
            :base(ByProjectTotalWorktime, TotalWorktime)
        {
            Contract.Requires(ByProjectTotalWorktime != null);
            this.DayOfWeek = DayOfWeek;
            this.DayOfMonth = DayOfMonth;
            _hash = HashCalculator.Calculate(DayOfMonth, (int)DayOfWeek);
        }

        #endregion
    }
}
