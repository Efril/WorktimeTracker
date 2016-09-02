using Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfGui
{
    internal class ReportWindowProjectItem
    {
        private readonly Project _project;

        public bool Selected
        {
            get;
            set;
        }
        public int ProjectId
        {
            get { return _project.ProjectId.Value; }
        }
        public string ProjectName
        {
            get { return _project.Name; }
        }

        public ReportWindowProjectItem(Project Project)
        {
            Contract.Requires(Project != null);
            _project = Project;
        }
    }
}
