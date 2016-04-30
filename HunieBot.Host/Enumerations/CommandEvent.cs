using System;

namespace HunieBot.Host.Enumerations
{

    /// <summary>
    ///     Defines the various events a command may be fired.
    /// </summary>
    [Flags]
    public enum CommandEvent
    {

        /// <summary>
        ///     A user has sent a message to a channel.
        /// </summary>
        MessageReceived = 1,

        /// <summary>
        ///     A user has joined the server.
        /// </summary>
        UserJoined = 2,

        /// <summary>
        ///     A user has left the server.
        /// </summary>
        UserDeparted = 4,

        /// <summary>
        ///     A user has been banned from the server.
        /// </summary>
        UserBanned = 8,

        /// <summary>
        ///     A user has had their ban lifted from the server.
        /// </summary>
        UserUnbanned = 16,

        /// <summary>
        ///     This bot has joined a server.
        /// </summary>
        JoinedServer = 32,

        /// <summary>
        ///     This bot has left a server.
        /// </summary>
        DepartedServer = 64,

        /// <summary>
        ///     A channel on the server has been created.
        /// </summary>
        ChannelCreated = 128,

        /// <summary>
        ///     A channel on the server has been updated.
        /// </summary>
        ChannelUpdated = 256,

        /// <summary>
        ///     A channel on the server has been deleted.
        /// </summary>
        ChannelDeleted = 512,

        /// <summary>
        ///     A command has been received.
        /// </summary>
        CommandReceived = 1024,

        /// <summary>
        ///     A private message has been received.
        /// </summary>
        PrivateMessageReceived = 2048,

        /// <summary>
        ///     A private or public message has been received.
        /// </summary>
        AnyMessageReceived = PrivateMessageReceived | MessageReceived

    }

}