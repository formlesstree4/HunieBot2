using Discord;
using HunieBot.Host.Interfaces;
using System.Linq;

namespace HunieBot.Host
{

    /// <summary>
    ///     An internal implementation of <see cref="IHunieCommand"/>
    /// </summary>
    public sealed class HunieCommand : IHunieCommand
    {
        public Channel Channel { get; }

        public Message Message { get; }

        public Server Server { get; }

        public User User { get; }

        public DiscordClient Client { get; }

        public string Command { get; }

        public string[] Parameters { get; }
        
        public HunieCommand(Channel c, Server s, User u, DiscordClient dc, Message m)
        {
            Channel = c;
            Server = s;
            User = u;
            Client = dc;
            Message = m;

            // Here's how we're going to process this:
            // 1) Remove the first character. The first character is going to be the message text.
            // 2) Split on the space.
            // 3) Each item after the first are the parameters.
            var preprocessedString = m.Text.Remove(0, 1);
            var commandAndArgs = preprocessedString.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            Command = commandAndArgs[0];
            Parameters = commandAndArgs.Skip(1).ToArray();
        }

        public HunieCommand(IHunieMessage message) : this(message.Channel, message.Server, message.User, message.Client, message.Message) { }

    }
}
