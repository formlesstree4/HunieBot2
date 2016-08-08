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

        /// <summary>
        /// Gets or sets the game that the bot is in on startup.
        /// </summary>
        public string Game { get; set; }


        public HunieConfiguration()
        {
            Game = $"I'm HunieBot! PM me \"{CommandCharacter}help\" for more details!";
        }

        public void Save(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(this));
        }

        public void Load(string file)
        {
            if (!File.Exists(file))
                Save(file);

            var configFile = File.ReadAllText(file);

            if (configFile == string.Empty)
                return;

            var hc = JsonConvert.DeserializeObject<HunieConfiguration>(configFile);
            CommandCharacter = hc.CommandCharacter;
            DiscordToken = hc.DiscordToken;
            Game = hc.Game;
        }

    }
}
