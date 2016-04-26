using Discord;
using HunieBot.Host.Interfaces;

namespace HunieBot.Host
{

    /// <summary>
    ///     An internal implementation of <see cref="IHunieEvent"/>
    /// </summary>
    internal sealed class HunieEvent : IHunieEvent
    {
        public Channel Channel { get; }

        public Server Server { get; }

        public User User { get; }

        public DiscordClient Client { get; }

        public HunieEvent(Channel c, Server s, User u, DiscordClient dc)
        {
            Channel = c;
            Server = s;
            User = u;
            Client = dc;
        }

    }
}
