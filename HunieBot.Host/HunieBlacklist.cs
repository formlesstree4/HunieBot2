using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace HunieBot.Host
{
    /// <summary>
    /// Handles list of disabled modules by module name
    /// </summary>
    class HunieBlacklist
    {
        /// <summary>
        /// List of blacklisted implementations(whereas blacklisted means it is disabled)
        /// </summary>
        public Dictionary<string, bool> Implementations { get; set; }= new Dictionary<string, bool>();

        private const string BlacklistFile = "Blacklist.json";

        public HunieBlacklist()
        {
            if (!File.Exists(BlacklistFile))
                Save();
        }

        public void Load()
        {
            var blacklistFile = File.ReadAllText(BlacklistFile);

            if (blacklistFile == string.Empty)
                return;

            var rawBlacklist = JsonConvert.DeserializeObject<HunieBlacklist>(blacklistFile);
            Implementations = rawBlacklist.Implementations;
        }

        /// <summary>
        /// Adds key to blacklist if unknown
        /// </summary>
        /// <param name="key">Key that should be added</param>
        /// <param name="state">OPTIONAL: Should it be disabled or enabled? Enabled by default</param>
        public void AddKey(string key, bool state = false)
        {
            Load();
            if (Implementations.ContainsKey(key))
                return;

            Implementations.Add(key, state);
            Save();
        }

        private void Save()
        {
            File.WriteAllText(BlacklistFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// Checks if a key is blacklisted
        /// </summary>
        /// <param name="key">Key that should be checked</param>
        /// <returns></returns>
        public bool IsBlacklisted(string key)
        {
            return Implementations[key];
        }
    }
}
