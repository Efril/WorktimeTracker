using Core.Framework;
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
using System.IO;

namespace Core.Storage
{
    public class TrackDbProvider
    {
        class BulkOperation
        {
            public TrackDbConnection Connection
            {
                get;
                private set;
            }
            public TrackDbTransaction Transaction
            {
                get;
                private set;
            }

            public BulkOperation(TrackDbConnection Connection, TrackDbTransaction Transaction)
            {
                Contract.Requires(Connection != null);
                Contract.Requires(Transaction != null);
                this.Connection = Connection;
                this.Transaction = Transaction;
            }
        }

        #region -> TrackDbProvider singleton <-

        private const string _trackDbFileName = "trackdb.sqlite";
        private static TrackDbProvider _trackDb;
        public static TrackDbProvider Get()
        {
            if(_trackDb==null) _trackDb= new TrackDbProvider(new SqliteDatabaseConnectionString(Path.Combine(ApplicationServices.GetAssemblyFolderPath(), _trackDbFileName)));
            return _trackDb;
        }

        #endregion

        #region -> Nested Fields <-

        private readonly SqliteDatabaseConnectionString _connectionString;
        private const string _projectsTableName = "tblProjects";
        private const string _timeTrackingTableName = "tblTimeTracking";

        private TrackDbConnection _bulkOperationConnection;
        private TrackDbTransaction _bulkOperationTransaction;
        private bool _insideBulkOperation
        {
            get { return _bulkOperationConnection != null && _bulkOperationTransaction != null; }
        }

        #endregion

        #region -> Interface <-

        public MethodCallResult BeginBulkOperation()
        {
            try
            {
                _bulkOperationConnection = new TrackDbConnection(false);
                _bulkOperationConnection.Open();
                return MethodCallResult.Success;
            }
            catch(Exception ex)
            {
                return MethodCallResult.CreateException(ex);
            }
        }
        public MethodCallResult CompleteBulkOperation()
        {

        }

        #region -> Projects management logic <-

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
        public MethodCallResult AddProject(ref DbProject Project)
        {
            try
            {
                using (IDbConnection connection = new SQLiteConnection(_connectionString.ToString()))
                {
                    connection.Open();
                    Project.Id = connection.ExecuteScalar<int>("INSERT INTO `" + _projectsTableName + "` (Name) VALUES (@Name);SELECT last_insert_rowid();",
                        new { Name = Project.Name });
                    return MethodCallResult.Success;
                }
            }
            catch(Exception ex)
            {
                return MethodCallResult.CreateException(ex);
            }
        }
        public MethodCallResult UpdateProject(DbProject Project)
        {
            try
            {
                using (IDbConnection connection = new SQLiteConnection(_connectionString.ToString()))
                {
                    connection.Open();
                    connection.Execute("UPDATE `" + _projectsTableName + "` SET Name=@Name WHERE rowid=@id",
                        new { Name = Project.Name, id = Project.Id });
                    return MethodCallResult.Success;
                }
            }
            catch(Exception ex)
            {
                return MethodCallResult.CreateException(ex);
            }
        }

        #endregion

        public MethodCallResult GetElapsedWorktime(int ProjectId, DateTime Date, out TimeSpan ElapsedWorktime)
        {
            try
            {
                using (IDbConnection connection = new SQLiteConnection(_connectionString.ToString()))
                {
                    connection.Open();
                    ElapsedWorktime = TimeSpan.FromSeconds(connection.Query<int>("SELECT elapsedTimeSeconds FROM `" + _timeTrackingTableName + "` WHERE projectId=@projectId AND dayOfYear=@dayOfYear;",
                        new
                        {
                            projectId = ProjectId,
                            dayOfYear = Date.DayOfYear
                        }).FirstOrDefault());
                    return MethodCallResult.Success;
                }
            }
            catch(Exception ex)
            {
                ElapsedWorktime = TimeSpan.MinValue;
                return MethodCallResult.CreateException(ex);
            }
        }
        public MethodCallResult SetElapsedWorktime(int ProjectId, DateTime Date, TimeSpan ElapsedWorktime)
        {
            try
            {
                using (IDbConnection connection = new SQLiteConnection(_connectionString.ToString()))
                {
                    connection.Open();
                    using(IDbTransaction transaction=connection.BeginTransaction())
                    {
                        int operationResult;
                        //Check projectId/dayOfYear combination record exist in the database
                        if(connection.Query("SELECT elapsedTimeSeconds FROM `" + _timeTrackingTableName + "` WHERE projectId=@projectId AND dayOfYear=@dayOfYear;",
                        new
                        {
                            projectId = ProjectId,
                            dayOfYear = Date.DayOfYear
                        }, transaction).Any())
                        {
                            //Yes, projectId/dayOfYear combination record exist in the database. Update it.
                            operationResult=connection.Execute("UPDATE `"+_timeTrackingTableName+ "` SET elapsedTimeSeconds=@elapsedTimeSeconds WHERE projectId=@projectId AND dayOfYear=@dayOfYear;",
                            new
                            {
                                elapsedTimeSeconds = ElapsedWorktime.TotalSeconds,
                                projectId = ProjectId,
                                dayOfYear = Date.DayOfYear
                            }, transaction);
                        }
                        else
                        {
                            //No, projectId/dayOfYear combination record does not exist in the database. Create it.
                            operationResult= connection.Execute("INSERT INTO `" + _timeTrackingTableName + "` (projectId, dayOfYear, elapsedTimeSeconds) VALUES (@projectId, @dayOfYear, @elapsedTimeSeconds);",
                                new
                                {
                                    projectId = ProjectId,
                                    dayOfYear = Date.DayOfYear,
                                    elapsedTimeSeconds = ElapsedWorktime.TotalSeconds
                                }, transaction);
                        }
                        if(operationResult==1)
                        {
                            transaction.Commit();
                            return MethodCallResult.Success;
                        }
                        else
                        {
                            return MethodCallResult.CreateFail("Unable to update database because of unknown reason.");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
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

        private IDbConnection GetConnection()
        {
            
        }
        private IDbTransaction GetTransaction()
        {

        }
        private MethodCallResult CreateExceptionMethodCallResult(Exception ex)
        {

        }
        public MethodCallResult CreateFailedMethodCallResult(Exception ex)
        {

        }
    }
}
