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
        private bool _dataLoaded = false;

        #region -> Interface <-

        private MonthWorktime[] _worktimeHistory;
        public MonthWorktime[] WorktimeHistory
        {
            get
            {
                return _worktimeHistory;
            }
        }

        private ReadOnlyDictionary<string, MonthWorktime[]> _byProjectsHistory;
        public ReadOnlyDictionary<string, MonthWorktime[]> ByProjectsHistory
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
