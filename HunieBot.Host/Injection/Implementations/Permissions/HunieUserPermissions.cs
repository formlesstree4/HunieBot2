using Dapper;
using HunieBot.Host.Database;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Collections.Concurrent;

namespace HunieBot.Host.Injection.Implementations.Permissions
{
    internal sealed class HunieUserPermissions : IHunieUserPermissions
    {
        private bool _isDisposed = false; // To detect redundant calls
        private readonly ConcurrentDictionary<CompositeKey, UserPermissions> _userPermissions = new ConcurrentDictionary<CompositeKey, UserPermissions>();
        private readonly HunieConnectionManager _handler;

        private struct CompositeKey
        {
            public ulong ServerId { get; }
            public ulong UserId { get; }

            public CompositeKey(ulong s, ulong u)
            {
                ServerId = s;
                UserId = u;
            }

        }



        /// <summary>
        ///     Creates a new instance of the <see cref="HunieUserPermissions"/> class.
        /// </summary>
        /// <param name="handler">A reference to the <see cref="HunieConnectionManager"/> that will generate connections for us</param>
        public HunieUserPermissions(HunieConnectionManager handler)
        {
            _handler = handler;
        }

        /// <summary>
        ///     Gets or sets the <see cref="UserPermissions"/> for a given user and server combination.
        /// </summary>
        /// <param name="serverId">The unique ID of the server</param>
        /// <param name="userId">The unique ID of the user</param>
        /// <returns><see cref="UserPermissions"/></returns>
        public UserPermissions this[ulong serverId, ulong userId]
        {
            get
            {
                return GetUserPermissions(serverId, userId);
            }
            set
            {
                InsertOrUpdatePermission(serverId, userId, value);
            }
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
        // ~HunieUserPermissions() {
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

        private void InsertIntoDatabase(CompositeKey key, UserPermissions permission)
        {
            using (var c = _handler.GetConnection())
            {
                c.Execute("INSERT INTO UserPermissions (ServerId, UserId, Permission) VALUES(@s, @u, @p)", new
                {
                    s = key.ServerId,
                    u = key.UserId,
                    p = (int)permission
                });
            }
        }

        private void UpdateDatabase(CompositeKey key, UserPermissions permission)
        {
            using (var c = _handler.GetConnection())
            {
                c.Execute("UPDATE UserPermissions SET Permission = @p WHERE ServerId = @s AND UserId = @u", new
                {
                    s = key.ServerId,
                    u = key.UserId,
                    p = (int)permission
                });
            }
        }

        private void InsertOrUpdatePermission(ulong serverId, ulong userId, UserPermissions permission)
        {
            // We're going to utilize AddOrUpdate() on the concurrent dictionary to detect what
            // we need to do! See, AddOrUpdate() allows us to provide a set of Func's that we can
            // then do additional things with.

            // If we are adding this permission to the dictionary (meaning we've never seen it before)
            // then we'll do a query into the SQLite database. If we get NULL back for the query, then,
            // we will INSERT the new value into the database. If we get a value back, we disregard it and
            // perform an update.

            // If we are updating the permission in the dictionary, we'll go ahead and issue an UPDATE
            // to the SQLite database. This should allow us to keep in sync (unless someone messes with the db
            // under the hood).


            _userPermissions.AddOrUpdate(new CompositeKey(serverId, userId),
            c =>
            {
                var existing = GetUserPermissionFromDatabase(c);
                if (existing.HasValue) UpdateDatabase(c, permission);
                else InsertIntoDatabase(c, permission);
                return permission;
            },
            (c, old) =>
            {
                UpdateDatabase(c, permission);
                return permission;
            });
        }

        private UserPermissions GetUserPermissions(ulong serverId, ulong userId)
        {
            var key = new CompositeKey(serverId, userId);
            return _userPermissions.GetOrAdd(key, (c) =>  GetUserPermissionFromDatabase(c) ?? UserPermissions.User );
        }
        
        private UserPermissions? GetUserPermissionFromDatabase(CompositeKey key)
        {
            using (var c = _handler.GetConnection())
            {
                var val = c.ExecuteScalar<int?>("SELECT Permission FROM UserPermissions WHERE ServerId = @s AND UserId = @u", new
                {
                    s = key.ServerId,
                    u = key.UserId
                });
                if (val.HasValue) return (UserPermissions)val.Value;
                return null;
            }
        }
    }
}