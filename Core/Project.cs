using Core.Framework;
using Core.Storage;
using Core.Storage.DbTypes;
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
        private TrackDbProvider _trackDb;
        private DbProject _dbProject;

        #region -> Interface <-

        public TimeTracker TimeTracker
        {
            get;
            private set;
        }

        private string _name;
        public string Name
        {
            get { return _dbProject != null ? _dbProject.Name : _name; }
            private set
            {
                if (_dbProject != null) _dbProject.Name = Name;
                _name = value;
            }
        }
        internal void  BindToStorage(DbProject DbProject, TrackDbProvider TrackDb)
        {
            Contract.Requires(DbProject != null);
            Contract.Requires(TrackDb != null);
            _trackDb = TrackDb;
            _dbProject = DbProject;
            this.TimeTracker.BindToStorage(TrackDb);
        }
        
        #endregion

        #region -> Constructors <-

        /// <summary>
        /// This constructor used to create Project object of already existant in database DbProject.
        /// No need to call BindToStorage when Project created in this way.
        /// </summary>
        /// <param name="DbProject"></param>
        /// <param name="TrackDb"></param>
        internal Project(DbProject DbProject, TrackDbProvider TrackDb)
        {
            this.TimeTracker = new TimeTracker();
            BindToStorage(DbProject, TrackDb);
        }
        /// <summary>
        /// This constructor used to create brand new Project.
        /// BindToStorage method should be called before full use of this Project object.
        /// </summary>
        /// <param name="Name"></param>
        internal Project(string Name)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            this.TimeTracker = new TimeTracker();
            this.Name = Name;
        }

        #endregion
    }
}
