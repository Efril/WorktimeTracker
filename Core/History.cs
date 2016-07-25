using Core.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class History: WorktimeHistoryBase
    {
        #region -> Interface <-
        
        public MonthWorktimeHistory[] WorktimeHistory
        {
            get;
            private set;
        }
        public ReadOnlyDictionary<string, MonthWorktimeHistory[]> ByProjectsHistory
        {
            get;
            private set;
        }

        #endregion

        public History(MonthWorktimeHistory[] WorktimeHistory, ReadOnlyDictionary<string, MonthWorktimeHistory[]> ByProjectsHistory, ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime)
            :base(ByProjectTotalWorktime)
        {
            Contract.Requires(WorktimeHistory != null);
            Contract.Requires(ByProjectsHistory != null);
            this.WorktimeHistory = WorktimeHistory;
            this.ByProjectsHistory = ByProjectsHistory;
        }
    }
}
