using Dapper;
using HunieBot.Host.Database;
using HunieBot.Host.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace HunieBot.Host.Injection.Implementations.Permissions
{

    /// <summary>
    ///     Internal implementation of <see cref="IHunieCommandPermissions"/>.
    /// </summary>
    internal sealed class HunieCommandPermissions : IHunieCommandPermissions
    {
        private bool _isDisposed = false; // To detect redundant calls
        private readonly HunieConnectionManager _handler;
        private readonly ConcurrentDictionary<CompositeKey, bool> _commandPermissions = new ConcurrentDictionary<CompositeKey, bool>();

        private struct CompositeKey
        {
            public ulong ServerId { get; }
            public ulong ChannelId { get; }
            public string Command { get; }

            public CompositeKey(ulong s, ulong c, string cmd)
            {
                ServerId = s;
                ChannelId = c;
                Command = cmd;
            }
        }



        public HunieCommandPermissions(HunieConnectionManager handler)
        {
            _handler = handler;
        }

        public string[] this[ulong server, ulong channel]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool this[ulong server, ulong channel, string[] commands]
        {
            get
            {
                return commands.Any(c => GetCommandPermission(server, channel, c));
            }
            set
            {
                InsertOrUpdatePermission(server, channel, commands, value);
            }
        }

        #region IDisposable Support

        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _commandPermissions.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _isDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~HunieCommandPermissions() {
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



        private void InsertIntoDatabase(CompositeKey key, bool value)
        {
            using (var c = _handler.GetConnection())
            {
                c.Execute("INSERT INTO ChannelPermissions (ServerId, ChannelId, Command, Permission) VALUES (@s, @c, @cmd, @p)",
                new
                {
                    s = key.ServerId,
                    c = key.ChannelId,
                    cmd = key.Command,
                    p = value
                });
            }
        }

        private void UpdateDatabase(CompositeKey key, bool value)
        {
            using (var c = _handler.GetConnection())
            {
                c.Execute("UPDATE ChannelPermissions SET Permission = @p WHERE ServerId = @s AND ChannelId = @c AND Command = @cmd",
                new
                {
                    s = key.ServerId,
                    c = key.ChannelId,
                    cmd = key.Command,
                    p = value
                });
            }
        }

        private void InsertOrUpdatePermission(ulong serverId, ulong channelId, string[] commands, bool value)
        {
            var keys = commands.Select(c => new CompositeKey(serverId, channelId, c));
            foreach (var key in keys)
            {
                _commandPermissions.AddOrUpdate(key,
                c =>
                {
                    var existing = GetCommandPermissionFromDatabase(c);
                    if (existing.HasValue) UpdateDatabase(c, value);
                    else InsertIntoDatabase(c, value);
                    return value;
                },
                (c, old) =>
                {
                    UpdateDatabase(c, value);
                    return value;
                });
            }
        }

        private bool GetCommandPermission(ulong serverId, ulong channelId, string command)
        {
            var key = new CompositeKey(serverId, channelId, command);
            return _commandPermissions.GetOrAdd(key, (c) => GetCommandPermissionFromDatabase(key) ?? false);
        }

        private bool? GetCommandPermissionFromDatabase(CompositeKey key)
        {
            using (var c = _handler.GetConnection())
            {
                return c.ExecuteScalar<bool?>("SELECT Permission FROM ChannelPermissions WHERE ServerId = @s AND ChannelId = @c AND Command = @cmd",
                new
                {
                    s = key.ServerId,
                    c = key.ChannelId,
                    cmd = key.Command
                });
            }
        }

    }
}