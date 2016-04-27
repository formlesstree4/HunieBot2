using HunieBot.Host.Enumerations;
using System;

namespace HunieBot.Host.Attributes
{

    /// <summary>
    ///     Attribute used to flag methods with commands they should support.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HandleEventAttribute : Attribute
    {

        /// <summary>
        ///     Gets the <see cref="CommandEvent"/> that this handler is for.
        /// </summary>
        public CommandEvent Events { get; }

        /// <summary>
        ///     Gets the <see cref="UserPermissions"/> that the user submitting the command must be at.s
        /// </summary>
        public UserPermissions Permissions { get; }



        /// <summary>
        ///     Creates a new instance of <see cref="HandleEventAttribute"/> tagged with the appropriate <see cref="CommandEvent"/> flags.
        /// </summary>
        /// <param name="event"><see cref="CommandEvent"/></param>
        /// <param name="permissions"><see cref="UserPermissions"/></param>
        public HandleEventAttribute(CommandEvent @event, UserPermissions permissions = UserPermissions.User)
        {
            if((@event & CommandEvent.CommandReceived) != 0) throw new ArgumentException($"{nameof(HandleEventAttribute)} cannot handle {nameof(CommandEvent.CommandReceived)}. Please use {nameof(HandleCommandAttribute)} for handling commands.");
            Events = @event;
            Permissions = permissions;
        }

    }

}