using HunieBot.Host.Enumerations;

namespace HunieBot.Host.Interfaces
{

    /// <summary>
    ///     Defines individual HunieHost User permissions.
    /// </summary>
    public interface IHunieUserPermissions
    {

        /// <summary>
        ///     Gets or sets the <see cref="UserPermissions"/> for a unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier for a given user.</param>
        /// <returns><see cref="UserPermissions"/></returns>
        UserPermissions this[ulong id] { get; set; }

        /// <summary>
        ///     Saves <see cref="IHunieUserPermissions"/> to disk.
        /// </summary>
        /// <param name="file">The file to save to</param>
        void Save(string file);

        /// <summary>
        ///     Loads <see cref="IHunieUserPermissions"/> from disk.
        /// </summary>
        /// <param name="file">The file to load from</param>
        void Load(string file);

    }

}