using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System;

namespace HunieBot.Host.Injection.Implementations.Permissions
{
    internal sealed class ReadOnlyHunieUserPermissions : IHunieUserPermissions
    {
        private readonly IHunieUserPermissions _backingSource;
        
        public ReadOnlyHunieUserPermissions(IHunieUserPermissions _basePermissions)
        {
            _backingSource = _basePermissions;
        }

        public UserPermissions this[ulong serverId, ulong userId]
        {
            get
            {
                return _backingSource[serverId, userId];
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public void Dispose()
        {
            _backingSource.Dispose();
        }
    }
}
