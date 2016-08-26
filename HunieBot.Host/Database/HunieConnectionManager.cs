using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace HunieBot.Host.Database
{

    /// <summary>
    ///     Manages the HunieBot database.
    /// </summary>
    public sealed class HunieConnectionManager : IDisposable
    {

        private bool _isDisposed = false; // To detect redundant calls
        private readonly string _connectionString;



        /// <summary>
        ///     Creates a new instance of the <see cref="HunieConnectionManager"/>
        /// </summary>
        /// <param name="dbLocation">The location of the database</param>
        internal HunieConnectionManager(string dbLocation)
        {
            _connectionString = $"Data Source={dbLocation};Version=3";
            if(!File.Exists(dbLocation)) SetupDatabase(dbLocation);
        }



        /// <summary>
        ///     Creates the database file and populates it with the initial tables and indexes.
        /// </summary>
        /// <param name="dbFile">The full path to the database</param>
        private void SetupDatabase(string dbFile)
        {
            SQLiteConnection.CreateFile(dbFile);
            using (var hbConn = GetConnection())
            {
                // Let's create the initial structure of the database.
                hbConn.Execute("CREATE TABLE ChannelPermissions (ServerId INTEGER NOT NULL, ChannelId INTEGER NOT NULL, Command VARCHAR NOT NULL, Permission BIT NOT NULL, PRIMARY KEY (ServerId, ChannelId, Command));");
                hbConn.Execute("CREATE TABLE UserPermissions (ServerId INTEGER NOT NULL, UserId INTEGER NOT NULL, Permission INTEGER NOT NULL, PRIMARY KEY (ServerId, UserId))");
                hbConn.Execute("CREATE TABLE HunieConfiguration (HunieBotName VARCHAR NOT NULL, Name VARCHAR NOT NULL, Value VARCHAR NOT NULL, PRIMARY KEY (HunieBotName))");

                // Now throw on the indexes.
                hbConn.Execute("CREATE UNIQUE INDEX IX_ChannelPermissions_Query ON ChannelPermissions (ServerId, ChannelId, Command)");
                hbConn.Execute("CREATE UNIQUE INDEX IX_UserPermissions_Query ON UserPermissions (ServerId, UserId)");
                hbConn.Execute("CREATE UNIQUE INDEX IX_HunieConfiguration_Query ON HunieConfiguration (HunieBotName, Name)");
            }
        }

        /// <summary>
        ///     Returns a new <see cref="IDbConnection"/> to the SQLite database.
        /// </summary>
        /// <returns><see cref="IDbConnection"/></returns>
        public IDbConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        #region IDisposable Support
        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SqliteHandler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
