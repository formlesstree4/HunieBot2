using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace HunieBot.Host.Internal
{

    /// <summary>
    ///     Core implementation of <see cref="HunieBotAttribute"/> that is used to help monitor and aide <see cref="HunieBot.Host"/>.
    /// </summary>
    [HunieBot("HunieBot.Core")]
    internal sealed class HunieBotCore
    {
        private readonly IHunieUserPermissions _permissions;



        public HunieBotCore(IHunieUserPermissions permissions)
        {
            _permissions = permissions;
        }

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


        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.Administrator, "setperm")]
        public async Task SetUserPermissions(IHunieCommand command)
        {

        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.Administrator, "getperm")]
        public async Task GetUserPermission(IHunieCommand command)
        {
            if (command.RawParameters.Length != 0) return;
            var userParameter = command.RawParameters[0].Trim();

        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived | CommandEvent.PrivateMessageReceived, UserPermissions.User, "getmyperm")]
        public async Task GetMyPermission(IHunieCommand command)
        {
            var userPerms = _permissions[command.User.Id];
            await command.User.SendMessage($"Your permission level is {userPerms}");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, "getcommands")]
        public async Task GetCommands(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            // return await command.User.SendMessage(hhmd.Commands.Select(s => s.N)
        }
        
    }

}