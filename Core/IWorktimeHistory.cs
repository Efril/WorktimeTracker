using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IWorktimeHistory
    {
        TimeSpan TotalWorktime { get; }
        ReadOnlyDictionary<string, TimeSpan> ByProjectTotalWorktime { get; }
    }
}
