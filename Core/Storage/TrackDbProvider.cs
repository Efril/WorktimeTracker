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
using System.Threading;

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

        private readonly Dictionary<int, BulkOperation> _bulkOperations = new Dictionary<int, BulkOperation>();
        private readonly object _bulkOperationsSyncRoot = new object();

        #endregion

        #region -> Interface <-

        #region -> Bulk operations support <-

        public MethodCallResult BeginBulkOperation()
        {
            lock (_bulkOperationsSyncRoot)
            {
                Contract.Assume(!_bulkOperations.ContainsKey(Thread.CurrentThread.ManagedThreadId), "Bulk operation already started in this thread.");
                try
                {
                    _bulkOperations.Add(Thread.CurrentThread.ManagedThreadId, CreateBulkOperation());
                    return MethodCallResult.Success;
                }
                catch (Exception ex)
                {
                    return MethodCallResult.CreateException(ex);
                }
            }
        }
        public MethodCallResult CompleteBulkOperation()
        {
            lock (_bulkOperationsSyncRoot)
            {
                BulkOperation bulkOperation = null;
                try
                {
                    if (!_bulkOperations.TryGetValue(Thread.CurrentThread.ManagedThreadId, out bulkOperation))
                    {
                        return MethodCallResult.CreateFail("Bulk operation not found.");
                    }
                    if (bulkOperation.Transaction.IsActive) bulkOperation.Transaction.ManualCommit();
                    bulkOperation.Connection.ManualDispose();
                    return MethodCallResult.Success;
                }
                catch (Exception ex)
                {
                    if (bulkOperation != null && bulkOperation.Connection.State != ConnectionState.Closed && bulkOperation.Connection.State != ConnectionState.Broken)
                    {
                        bulkOperation.Connection.ManualDispose();
                    }
                    return MethodCallResult.CreateException(ex);
                }
                finally
                {
                    _bulkOperations.Remove(Thread.CurrentThread.ManagedThreadId);
                }
            }
        }

        private BulkOperation CreateBulkOperation()
        {
            SQLiteConnection nativeConnection = new SQLiteConnection(_connectionString.ToString());
            TrackDbConnection connection = new TrackDbConnection(nativeConnection);
            nativeConnection.Open();
            TrackDbTransaction transaction = new TrackDbTransaction(nativeConnection.BeginTransaction());
            BulkOperation bulkOperation = new BulkOperation(connection, transaction);
            return bulkOperation;
        }
        private IDbConnection GetConnection()
        {
            lock (_bulkOperationsSyncRoot)
            {
                BulkOperation bulkOperation;
                if (_bulkOperations.TryGetValue(Thread.CurrentThread.ManagedThreadId, out bulkOperation))
                {
                    return bulkOperation.Connection;
                }
            }
            SQLiteConnection connection = new SQLiteConnection(_connectionString.ToString());
            connection.Open();
            return connection;
        }
        private IDbTransaction GetTransaction(IDbConnection Connection)
        {
            lock (_bulkOperationsSyncRoot)
            {
                BulkOperation bulkOperation;
                if (_bulkOperations.TryGetValue(Thread.CurrentThread.ManagedThreadId, out bulkOperation) && bulkOperation.Transaction.IsActive)
                {
                    return bulkOperation.Transaction;
                }
            }
            return Connection.BeginTransaction();
        }
        private MethodCallResult CreateExceptionMethodCallResult(Exception ex)
        {
            RollbackBulkTransaction();
            return MethodCallResult.CreateException(ex);
        }
        private MethodCallResult CreateFailedMethodCallResult(string FailReason)
        {
            RollbackBulkTransaction();
            return MethodCallResult.CreateFail(FailReason);
        }
        private void RollbackBulkTransaction()
        {
            lock (_bulkOperationsSyncRoot)
            {
                BulkOperation bulkOperation;
                if (_bulkOperations.TryGetValue(Thread.CurrentThread.ManagedThreadId, out bulkOperation))
                {
                    if (bulkOperation.Transaction.IsActive) bulkOperation.Transaction.Rollback();
                }
            }
        }

        #endregion

        #region -> Projects management logic <-

        public MethodCallResult GetAllProjects(out DbProject[] Projects)
        {
            try
            {
                using (IDbConnection connection = GetConnection())
                {
                    connection.Open();
                    Projects = connection.Query<DbProject>("SELECT rowid, * FROM `" + _projectsTableName + "`").ToArray();
                    return MethodCallResult.Success;
                }
            }
            catch(Exception ex)
            {
                Projects = null;
                return CreateExceptionMethodCallResult(ex);
            }
        }
        public MethodCallResult AddProject(ref DbProject Project)
        {
            try
            {
                using (IDbConnection connection = GetConnection())
                {
                    connection.Open();
                    Project.Id = connection.ExecuteScalar<int>("INSERT INTO `" + _projectsTableName + "` (Name) VALUES (@Name);SELECT last_insert_rowid();",
                        new { Name = Project.Name });
                    return MethodCallResult.Success;
                }
            }
            catch(Exception ex)
            {
                return CreateExceptionMethodCallResult(ex);
            }
        }
        public MethodCallResult UpdateProject(DbProject Project)
        {
            try
            {
                using (IDbConnection connection = GetConnection())
                {
                    connection.Open();
                    connection.Execute("UPDATE `" + _projectsTableName + "` SET Name=@Name WHERE rowid=@id",
                        new { Name = Project.Name, id = Project.Id });
                    return MethodCallResult.Success;
                }
            }
            catch(Exception ex)
            {
                return CreateExceptionMethodCallResult(ex);
            }
        }
        public MethodCallResult DeleteProject(int ProjectId)
        {
            try
            {
                using (IDbConnection connection = GetConnection())
                {
                    connection.Open();
                    using (IDbTransaction transaction = GetTransaction(connection))
                    {
                        connection.Execute("DELETE FROM `" + _projectsTableName + "` WHERE rowid=@projectId", new { projectId = ProjectId }, transaction);
                        connection.Execute("DELETE FROM `" + _timeTrackingTableName + "` WHERE projectId=@projectId", new { projectId = ProjectId }, transaction);
                        transaction.Commit();
                        return MethodCallResult.Success;
                    }
                }
            }
            catch(Exception ex)
            {
                return CreateExceptionMethodCallResult(ex);
            }
        }

        #endregion

        #region -> Elapsed worktime logic <-

        public MethodCallResult GetElapsedWorktime(int ProjectId, DateTime Date, out TimeSpan ElapsedWorktime)
        {
            try
            {
                using (IDbConnection connection = GetConnection())
                {
                    connection.Open();
                    ElapsedWorktime = TimeSpan.FromSeconds(connection.Query<int>("SELECT elapsedTimeSeconds FROM `" + _timeTrackingTableName + "` WHERE projectId=@projectId AND year=@year AND dayOfYear=@dayOfYear;",
                        new
                        {
                            projectId = ProjectId,
                            year = Date.Year,
                            dayOfYear = Date.DayOfYear
                        }).FirstOrDefault());
                    return MethodCallResult.Success;
                }
            }
            catch(Exception ex)
            {
                ElapsedWorktime = TimeSpan.MinValue;
                return CreateExceptionMethodCallResult(ex);
            }
        }
        public MethodCallResult SetElapsedWorktime(int ProjectId, DateTime Date, TimeSpan ElapsedWorktime)
        {
            try
            {
                using (IDbConnection connection = GetConnection())
                {
                    connection.Open();
                    using(IDbTransaction transaction=GetTransaction(connection))
                    {
                        int operationResult;
                        //Check projectId/dayOfYear combination record exist in the database
                        if(connection.Query("SELECT elapsedTimeSeconds FROM `" + _timeTrackingTableName + "` WHERE projectId=@projectId AND year=@year AND dayOfYear=@dayOfYear;",
                        new
                        {
                            projectId = ProjectId,
                            year=Date.Year,
                            dayOfYear = Date.DayOfYear
                        }, transaction).Any())
                        {
                            //Yes, projectId/dayOfYear combination record exist in the database. Update it.
                            operationResult = connection.Execute("UPDATE `" + _timeTrackingTableName + "` SET elapsedTimeSeconds=@elapsedTimeSeconds WHERE projectId=@projectId AND year=@year AND dayOfYear=@dayOfYear;",
                            new
                            {
                                elapsedTimeSeconds = ElapsedWorktime.TotalSeconds,
                                projectId = ProjectId,
                                year = Date.Year,
                                dayOfYear = Date.DayOfYear
                            }, transaction);
                        }
                        else
                        {
                            //No, projectId/dayOfYear combination record does not exist in the database. Create it.
                            operationResult= connection.Execute("INSERT INTO `" + _timeTrackingTableName + "` (projectId, year, dayOfYear, elapsedTimeSeconds) VALUES (@projectId, @year, @dayOfYear, @elapsedTimeSeconds);",
                                new
                                {
                                    projectId = ProjectId,
                                    year=Date.Year,
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
                            return CreateFailedMethodCallResult("Unable to update database because of unknown reason.");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return CreateExceptionMethodCallResult(ex);
            }
        }
        /*public MethodCallResult GetElapsedWorktime(int ProjectId, DateTime From, DateTime Till, out DbTimeTracking[] ElapsedTimeTracking)
        {

        }*/

        #endregion

        #endregion

        #region -> Constructors <-

        private TrackDbProvider(SqliteDatabaseConnectionString ConnectionString)
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
