using Discord;

namespace HunieBot.Host.Interfaces
{

    /// <summary>
    ///     A <see cref="IHunieMessage"/> is a specialized type of <see cref="IHunieEvent"/> that is created specifically when a message is sent through.
    /// </summary>
    public interface IHunieMessage : IHunieEvent
    {

        /// <summary>
        ///     Gets the <see cref="Discord.Message"/> that may have generated this <see cref="IHunieMessage"/>.
        /// </summary>
        Message Message { get; }

    }
}
