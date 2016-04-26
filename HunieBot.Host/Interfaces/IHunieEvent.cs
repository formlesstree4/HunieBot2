using Discord;

namespace HunieBot.Host.Interfaces
{

    /// <summary>
    ///     A <see cref="IHunieEvent"/> is some form of event captured by <see cref="HunieHost"/> and created for your HunieBot to understand and consume.
    /// </summary>
    public interface IHunieEvent
    {

        /// <summary>
        ///     Gets the <see cref="Discord.Server"/> that may have generated this <see cref="IHunieEvent"/>.
        /// </summary>
        Server Server { get; }

        /// <summary>
        ///     Gets the <see cref="Discord.Channel"/> that may have generated this <see cref="IHunieEvent"/>.
        /// </summary>
        Channel Channel { get; }

        /// <summary>
        ///     Gets the <see cref="Discord.User"/> that may have generated this <see cref="IHunieEvent"/>.
        /// </summary>
        User User { get; }

        /// <summary>
        ///     Gets the <see cref="DiscordClient"/> that generated this <see cref="IHunieEvent"/>.
        /// </summary>
        DiscordClient Client { get; }

    }

}