using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Storage
{
    internal class SqliteDatabaseConnectionString
    {
        private static string _connectionStringTemplate = "Data Source={0};Version=3;Journal Mode={1};Synchronous={2}";

        #region -> Interface <-

        public string DatabaseName
        {
            get;
            private set;
        }
        public SqliteSyncronousModes Synchronous
        {
            get;
            private set;
        }
        public SqliteJournalModes JournalMode
        {
            get;
            private set;
        }

        public static string BuildConnectionString(string DatabaseName, SqliteSyncronousModes Synchronous = SqliteSyncronousModes.Full, SqliteJournalModes JournalMode = SqliteJournalModes.Delete)
        {
            return string.Format(_connectionStringTemplate, DatabaseName, JournalMode.ToString(), Synchronous.ToString());
        }

        public override string ToString()
        {
            return BuildConnectionString(DatabaseName, Synchronous, JournalMode);
        }

        #endregion

        public SqliteDatabaseConnectionString(string DatabaseName = null, SqliteSyncronousModes Synchronous = SqliteSyncronousModes.Full, SqliteJournalModes JournalMode = SqliteJournalModes.Delete)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(DatabaseName));
            this.DatabaseName = DatabaseName;
            this.Synchronous = Synchronous;
            this.JournalMode = JournalMode;
        }
    }
}
