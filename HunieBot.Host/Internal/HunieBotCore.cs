using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Threading.Tasks;

namespace HunieBot.Host.Internal
{

    /// <summary>
    ///     Core implementation of <see cref="HunieBotAttribute"/> that is used to help monitor and aide <see cref="HunieBot.Host"/>.
    /// </summary>
    [HunieBot("HunieBot.Core")]
    internal sealed class HunieBotCore
    {

        [HandleEvent(CommandEvent.CommandReceived | CommandEvent.PrivateMessageReceived, UserPermissions.Owner)]
        public async Task ActiveMembers(IHunieCommand command)
        {
            switch (command.Command.ToLowerInvariant())
            {
                case "ping":
                    await command.User.SendMessage("pong");
                    break;
            }
        }

    }

}