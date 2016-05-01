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
        private ConcurrentDictionary<string, bool> _commandFilters = new ConcurrentDictionary<string, bool>();


        public bool GetCommandListenerStatus(string command, ulong server, ulong channel)
        {
            if (command.Equals("set_command_permission", StringComparison.OrdinalIgnoreCase)) return true; // hackhack
            bool value;
            if (!_commandFilters.TryGetValue(ComposeKey(command, server, channel), out value)) return false;
            return value;
        }
        public void SetCommandListenerStatus(string command, ulong server, ulong channel, bool canListen)
        {
            _commandFilters.AddOrUpdate(ComposeKey(command, server, channel), canListen, (c, b) => canListen);
        }
        public string[] GetServerAndChannelCommands(ulong server, ulong channel)
        {
            throw new NotImplementedException();
        }

        public void Load(string file)
        {
            if (!File.Exists(file)) Save(file);
            _commandFilters = JsonConvert.DeserializeObject<ConcurrentDictionary<string, bool>>(File.ReadAllText(file));
        }
        public void Save(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(_commandFilters));
        }


        private string ComposeKey(string command, ulong server, ulong channel)
        {
            return $"{command}_{server}_{channel}";
        }

        public bool GetCommandListenerStatus(string[] commands, ulong server, ulong channel)
        {
            foreach (var command in commands)
                if (GetCommandListenerStatus(command, server, channel)) return true;
            return false;
        }
    }

}