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

        public UserPermissions this[ulong id]
        {
            get
            {
                return _backingSource[id];
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public void Load(string file)
        {
            throw new NotSupportedException();
        }

        public void Save(string file)
        {
            throw new NotSupportedException();
        }
    }
}
