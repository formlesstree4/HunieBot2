using HunieBot.Host.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace HunieBot.Host.Injection.Implementations.Permissions
{

    /// <summary>
    ///     Internal implementation of <see cref="IHunieCommandPermissions"/>.
    /// </summary>
    internal sealed class HunieCommandPermissions : IHunieCommandPermissions
    {
        private ConcurrentDictionary<InternalHunieCommandKey, bool> _commandFilters = new ConcurrentDictionary<InternalHunieCommandKey, bool>();


        public bool GetCommandListenerStatus(string command, ulong server, ulong channel)
        {
            var key = new InternalHunieCommandKey
            {
                //Host = host,
                Command = command,
                ServerID = server,
                ChannelID = channel
            };
            return _commandFilters.GetOrAdd(key, false);
        }
        public void SetCommandListenerStatus(string command, ulong server, ulong channel, bool canListen)
        {
            var key = new InternalHunieCommandKey
            {
                //Host = host,
                Command = command,
                ServerID = server,
                ChannelID = channel
            };
            _commandFilters.AddOrUpdate(key, canListen, (c, b) => canListen);
        }
        public string[] GetServerAndChannelCommands(ulong server, ulong channel)
        {
            throw new NotImplementedException();
        }

        public void Load(string file)
        {
            if (!File.Exists(file)) Save(file);
            _commandFilters = JsonConvert.DeserializeObject<ConcurrentDictionary<InternalHunieCommandKey, bool>>(File.ReadAllText(file));
        }
        public void Save(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(_commandFilters));
        }


        private sealed class InternalHunieCommandKey
        {
            public string Host { get; set; }
            public string Command { get; set; }
            public ulong ServerID { get; set; }
            public ulong ChannelID { get; set; }
        }

    }

}