using Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class History: IWorktimeHistory
    {
        #region -> Interface <-

        private MonthWorktimeHistory[] _worktimeHistory;
        public MonthWorktimeHistory[] WorktimeHistory
        {
            get
            {
                return _worktimeHistory;
            }
        }

        private ReadOnlyDictionary<string, MonthWorktimeHistory[]> _byProjectsHistory;
        public ReadOnlyDictionary<string, MonthWorktimeHistory[]> ByProjectsHistory
        {
            get { return _byProjectsHistory; }
        }

        private ReadOnlyDictionary<string, TimeSpan> _byProjectTotalWorktime;
        public ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime
        {
            get { return _byProjectTotalWorktime; }
        }

        public TimeSpan TotalWorktime
        {
            get;
            private set;
        }

        #endregion
    }
}
