using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO;

namespace HunieBot.Host.Permissions
{
    internal sealed class HunieUserPermissions : IHunieUserPermissions
    {
        private ConcurrentDictionary<ulong, UserPermissions> _permissions = new ConcurrentDictionary<ulong, UserPermissions>();


        public UserPermissions this[ulong id]
        {
            get
            {
                return _permissions.GetOrAdd(id, UserPermissions.User);
            }

            set
            {
                _permissions.AddOrUpdate(id, value, (identifier, old) => value);
            }
        }

        public void Load(string file)
        {
            if (!File.Exists(file)) Save(file);
            _permissions = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, UserPermissions>>(File.ReadAllText(file));
        }

        public void Save(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(_permissions));
        }
    }
}