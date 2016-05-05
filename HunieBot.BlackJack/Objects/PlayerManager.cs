using Discord;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HunieBot.BlackJack.Objects
{

    /// <summary>
    ///     Handles players and keeps track of player money.
    /// </summary>
    public static class PlayerManager
    {
        private static ConcurrentDictionary<string, Player> _players = new ConcurrentDictionary<string, Player>();
        private static readonly string _saveFolder = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "Discord.BlackJack");
        private static readonly string _saveFile = System.IO.Path.Combine(_saveFolder, "players.json");


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Player GetPlayer(User discordUser)
        {
            var playerKey = discordUser.ToString();
            return _players.AddOrUpdate(playerKey, new Player(discordUser) { Money = 50 }, (key, oldValue) =>
            {
                return new Player(discordUser) { Money = oldValue.Money, Hand = new Hand() };
            });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Save()
        {
            if (!System.IO.Directory.Exists(_saveFolder))
            {
                System.IO.Directory.CreateDirectory(_saveFolder);
            }
            System.IO.File.WriteAllText(_saveFile, JsonConvert.SerializeObject(_players));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Load()
        {
            if (!System.IO.Directory.Exists(_saveFolder))
            {
                System.IO.Directory.CreateDirectory(_saveFolder);
            }
            if (!System.IO.File.Exists(_saveFile)) return;
            var content = System.IO.File.ReadAllText(_saveFile);
            _players = new ConcurrentDictionary<string, Player>();
            var loadedPlayers = JsonConvert.DeserializeObject<Dictionary<string, Player>>(content);
            foreach (var player in loadedPlayers) _players[player.Key] = player.Value;
        }

    }

}
