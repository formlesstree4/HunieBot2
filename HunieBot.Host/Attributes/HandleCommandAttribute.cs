using HunieBot.Host.Enumerations;
using System;
using System.Linq;

namespace HunieBot.Host.Attributes
{

    /// <summary>
    ///     Attribute used to flag methods with commands they should support.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HandleCommandAttribute : Attribute
    {

        /// <summary>
        ///     Gets the <see cref="CommandEvent"/> that this handler is for.
        /// </summary>
        public CommandEvent Events { get; }

        /// <summary>
        ///     Gets the <see cref="UserPermissions"/> that this handler is for.
        /// </summary>
        public UserPermissions Permissions { get; }

        /// <summary>
        ///     Gets a collection of commands that can be handled.
        /// </summary>
        public string[] Commands { get; }



        /// <summary>
        ///     Creates a new instance of the <see cref="HandleCommandAttribute"/>
        /// </summary>
        /// <param name="@event"><see cref="CommandEvent"/>. This is restricted to a subset of entries inside <see cref="CommandEvent"/>. Please see the remarks below.</param>
        /// <param name="commands">A string array of acceptable commands. This list will be handled as case insensitive and must not include the <see cref="HunieConfiguration.CommandCharacter"/> prefix.</param>
        /// <param name="permissions">The <see cref="UserPermissions"/> level of who may invoke this command</param>
        public HandleCommandAttribute(CommandEvent @event, UserPermissions permissions = UserPermissions.User, params string[] commands)
        {
            ValidateParameters(@event, commands);
            Events = @event;
            Permissions = permissions;
            Commands = commands;
        }



        /// <summary>
        ///     Validates incoming parameters.
        /// </summary>
        /// <param name="event"><see cref="CommandEvent"/></param>
        private void ValidateParameters(CommandEvent @event, string[] commands)
        {
            // Basically, the only VALID types of events are
            // going to be: CommandReceived, MessageReceived, and PrivateMessageReceived.
            // However, we MUST have CommandReceived at ALL times.
            if (commands.Length == 0) throw new ArgumentException();
            if ((@event & CommandEvent.CommandReceived) == 0) throw new ArgumentException();
            if ((@event & CommandEvent.ChannelCreated) != 0) throw new ArgumentException();
            if ((@event & CommandEvent.ChannelDeleted) != 0) throw new ArgumentException();
            if ((@event & CommandEvent.ChannelUpdated) != 0) throw new ArgumentException();
            if ((@event & CommandEvent.DepartedServer) != 0) throw new ArgumentException();
            if ((@event & CommandEvent.JoinedServer) != 0) throw new ArgumentException();
            if ((@event & CommandEvent.UserBanned) != 0) throw new ArgumentException();
            if ((@event & CommandEvent.UserDeparted) != 0) throw new ArgumentException();
            if ((@event & CommandEvent.UserJoined) != 0) throw new ArgumentException();
            if ((@event & CommandEvent.UserUnbanned) != 0) throw new ArgumentException();
            if (commands.Any(c => c.Any(char.IsWhiteSpace))) throw new ArgumentException("Commands may not have any whitespace", nameof(commands));
        }

    }
    
}