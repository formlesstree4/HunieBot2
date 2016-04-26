using Discord;
using HunieBot.Host.Interfaces;


namespace HunieBot.Host
{

    /// <summary>
    ///     An internal implementation of <see cref="IHunieMessage"/>
    /// </summary>
    internal sealed class HunieMessage : IHunieMessage
    {
        public Channel Channel { get; }

        public Message Message { get; }

        public Server Server { get; }

        public User User { get; }

        public DiscordClient Client { get; }

        public HunieMessage(Channel c, Server s, User u, DiscordClient dc, Message m)
        {
            Channel = c;
            Server = s;
            User = u;
            Client = dc;
            Message = m;
        }

    }
}
