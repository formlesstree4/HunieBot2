using HunieBot.Host.Enumerations;
using System;

namespace HunieBot.Host.Interfaces
{

    /// <summary>
    ///     Defines individual HunieHost User permissions.
    /// </summary>
    public interface IHunieUserPermissions : IDisposable
    {

        /// <summary>
        ///     Gets or sets the <see cref="UserPermissions"/> for a unique identifier.
        /// </summary>
        /// <param name="serverId">The unique Id of the server</param>
        /// <param name="userId">The unique Id of the user</param>
        /// <returns><see cref="UserPermissions"/></returns>
        UserPermissions this[ulong serverId, ulong userId] { get; set; }

    }

}