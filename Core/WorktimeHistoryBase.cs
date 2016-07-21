using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public abstract class WorktimeHistoryBase : IWorktimeHistory
    {
        public ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime
        {
            get;
            private set;
        }
        public virtual TimeSpan TotalWorktime
        {
            get;
            private set;
        }

        #region -> Constructors <-

        protected WorktimeHistoryBase() { }
        public WorktimeHistoryBase(ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime, TimeSpan TotalWorktime)
        {
            Contract.Requires(ByProjectTotalWorktime != null);
            this.ByProjectTotalWorktime = ByProjectTotalWorktime;
            this.TotalWorktime = TotalWorktime;
        }

        #endregion
    }
}
