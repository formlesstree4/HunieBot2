using Discord;
using Newtonsoft.Json;

namespace HunieBot.BlackJack.Objects
{

    /// <summary>
    ///     This is a discord player.
    /// </summary>
    public class Player
    {

        /// <summary>
        ///     Gets or sets the name of the Player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     
        /// </summary>
        public decimal Money { get; set; } = 50;

        /// <summary>
        ///     Gets or sets the bid the player has set.
        /// </summary>
        [JsonIgnore]
        public decimal Bid { get; set; } = 0;

        /// <summary>
        ///     
        /// </summary>
        [JsonIgnore]
        public Hand Hand { get; set; } = new Hand();

        /// <summary>
        ///     Gets the <see cref="Discord.User"/> that allows us to communicate directly.
        /// </summary>
        [JsonIgnore]
        public User User { get; }



        /// <summary>
        ///     Creates a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="user"></param>
        public Player(User user)
        {
            User = user;
            Name = user?.ToString();
        }

        /// <summary>
        ///     Returns a string representation of <see cref="Player"/>
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public override string ToString()
        {
            return Name;
        }

    }

}
