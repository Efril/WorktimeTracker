using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage.DbTypes
{
    [DapperType]
    [Serializable]
    public class DbProject
    {
        [Column(Name = "rowid")]
        public int? Id
        {
            get;
            set;
        }
        [Column(Name ="Name")]
        public string Name
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }

        public DbProject(int? Id, string Name)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            this.Id = Id;
            this.Name = Name;
        }
    }
}
