namespace HunieBot.Host.Interfaces
{

    /// <summary>
    ///     An internal interface that <see cref="HunieHost"/> uses to determine whether or not a command may be invoked on a given server and channel.
    /// </summary>
    internal interface IHunieCommandPermissions
    {

        /// <summary>
        ///     Gets a string array of commands that are able to be invoked for a server and channel.
        /// </summary>
        /// <param name="server">The unique server ID</param>
        /// <param name="channel">The unique channel ID</param>
        /// <returns>A collection of commands that are able to be executed</returns>
        string[] GetServerAndChannelCommands(ulong server, ulong channel);

        /// <summary>
        ///     Sets the listening status of a command for a given server and channel.
        /// </summary>
        /// <param name="host">The <see cref="Attributes.HunieBotAttribute.Name"/> that hosts the command</param>
        /// <param name="command">The command</param>
        /// <param name="server">The unique server ID</param>
        /// <param name="channel">The unique channel ID</param>
        /// <param name="canListen">Status flag indicating whether or not the command will receive messages on that list</param>
        void SetCommandListenerStatus(string command, ulong server, ulong channel, bool canListen);

        /// <summary>
        ///     Gets the listening status of a command for a given server and channel.
        /// </summary>
        /// <param name="host">The <see cref="Attributes.HunieBotAttribute.Name"/> that hosts the command</param>
        /// <param name="command">The command</param>
        /// <param name="server">The unique server ID</param>
        /// <param name="channel">The unique channel ID</param>
        /// <returns>The listening status</returns>
        bool GetCommandListenerStatus(string command, ulong server, ulong channel);

        /// <summary>
        ///     Overload used primarily for commands that have aliases.
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="server"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        bool GetCommandListenerStatus(string[] commands, ulong server, ulong channel);

        /// <summary>
        ///     Saves the current permissions to file.
        /// </summary>
        /// <param name="file">The file to save the permissions to</param>
        void Save(string file);

        /// <summary>
        ///     Loads the current permissions from file.
        /// </summary>
        /// <param name="file">The file to load the permissions from</param>
        void Load(string file);

    }
}