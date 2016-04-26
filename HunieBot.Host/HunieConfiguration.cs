using Newtonsoft.Json;
using System;
using System.IO;

namespace HunieBot.Host
{

    /// <summary>
    ///     
    /// </summary>
    public class HunieConfiguration
    {

        /// <summary>
        ///     Gets or sets the character that indicates an incoming message should be interpreted as a command.
        /// </summary>
        public char CommandCharacter { get; set; } = '.';

        /// <summary>
        ///     Gets or sets the Discord Token that is used to login.
        /// </summary>
        public string DiscordToken { get; set; } = string.Empty;



        public void Save(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(this));
        }
        public void Load(string file)
        {
            if (!File.Exists(file)) Save(file);
            var hc = JsonConvert.DeserializeObject<HunieConfiguration>(File.ReadAllText(file));
            CommandCharacter = hc.CommandCharacter;
            DiscordToken = hc.DiscordToken;
        }

    }
}
