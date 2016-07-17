﻿using Core.Framework;
using Core.Storage.DbTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.Contracts;

namespace Core.Storage
{
    internal class TrackDbProvider
    {
        private readonly SqliteDatabaseConnectionString _connectionString;
        private const string _projectsTableName = "tblProjects";

        #region -> Interface <-

        public MethodCallResult GetAllProjects(out DbProject[] Projects)
        {
            try
            {
                using (IDbConnection connection = new SQLiteConnection(_connectionString.ToString()))
                {
                    connection.Open();
                    Projects = connection.Query<DbProject>("SELECT rowid, * FROM `" + _projectsTableName + "`").ToArray();
                    return MethodCallResult.Success;
                }
            }
            catch(Exception ex)
            {
                Projects = null;
                return MethodCallResult.CreateException(ex);
            }
        }

        #endregion

        #region -> Constructors <-

        public TrackDbProvider(SqliteDatabaseConnectionString ConnectionString)
        {
            Contract.Requires(ConnectionString != null);
            _connectionString = ConnectionString;
        }
        static TrackDbProvider()
        {
            TypeMapper.Initialize("Core.Storage.DbTypes", Assembly.GetExecutingAssembly());
        }

        #endregion
    }
}
