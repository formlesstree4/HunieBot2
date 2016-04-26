using System;

namespace HunieBot.Host.Enumerations
{

    /// <summary>
    ///     Individual rights managements.
    /// </summary>
    [Flags]
    public enum UserPermissions
    {

        /// <summary>
        ///     Anybody has permission.
        /// </summary>
        User = 1,

        /// <summary>
        ///     Only <see cref="HunieHost"/> deemed moderators may execute this command.
        /// </summary>
        Moderator = 1+2,

        /// <summary>
        ///     Only <see cref="HunieHost"/> deemed administrators may execute this command.
        /// </summary>
        Administrator = 1+2+4,

        /// <summary>
        ///     Only <see cref="HunieHost"/> owners may execute this command.
        /// </summary>
        Owner = 1+2+4+8

    }

}