using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace WpfGui
{
    internal class ReportGenerator
    {
        private readonly ReportWindowProjectItem[] _projectItems;
        private readonly DateTime _dateFrom;
        private readonly DateTime _dateTo;

        public FlowDocument Generate()
        {

        }

        public ReportGenerator(ReportWindowProjectItem[] ProjectItems, DateTime DateFrom, DateTime DateTo)
        {
            Contract.Requires(ProjectItems != null);
            _projectItems = ProjectItems;
            _dateFrom = DateFrom;
            _dateTo = DateTo;
        }
    }
}
