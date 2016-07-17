using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Project
    {
        public string Name
        {
            get;
            set;
        }

        public Project(string Name)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            this.Name = Name;
        }
    }
}
