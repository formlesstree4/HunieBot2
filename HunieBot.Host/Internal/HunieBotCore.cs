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

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.PrivateMessageReceived, UserPermissions.User, "ping")]
        public async Task HandlePrivatePing(IHunieCommand command)
        {
            await command.User.SendMessage("pong (private)");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, "ping")]
        public async Task HandlePublicPing(IHunieCommand command)
        {
            await command.User.SendMessage("pong (public)");
        }
        
    }

}