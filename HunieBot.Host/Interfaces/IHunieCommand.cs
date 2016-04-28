﻿namespace HunieBot.Host.Interfaces
{

    /// <summary>
    ///     A <see cref="IHunieCommand"/> is a specialized type of <see cref="IHunieMessage"/> that contains verbiage that can be processed into a command.
    /// </summary>
    public interface IHunieCommand : IHunieMessage
    {

        /// <summary>
        ///     Gets the name of the command.
        /// </summary>
        string Command { get; }

        /// <summary>
        ///     Gets the parameters of the command that were passed in.
        /// </summary>
        /// <remarks>
        ///     Parsed from <see cref="Discord.Message.Text"/>
        /// </remarks>
        string[] Parameters { get; }

        /// <summary>
        ///     Gets the parameters of the command that were passed in.
        /// </summary>
        /// <remarks>
        ///     Parsed from <see cref="Discord.Message.RawText"/>
        /// </remarks>
        string[] RawParameters { get; }

    }
}
