using Discord;
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
        private readonly IHunieUserPermissions _userPermissions;
        private readonly IHunieCommandPermissions _commandPermissions;


        public HunieBotCore(IHunieUserPermissions userPermissiosn, IHunieCommandPermissions commandPermissions, DiscordClient client)
        {
            _userPermissions = userPermissiosn;
            _commandPermissions = commandPermissions;

            // make sure all commands always work on all servers and channels
            foreach (var server in client.Servers)
            {
                foreach (var channel in server.TextChannels)
                {
                    _commandPermissions.SetCommandListenerStatus("ping", server.Id, channel.Id, true);
                    _commandPermissions.SetCommandListenerStatus("setperm", server.Id, channel.Id, true);
                    _commandPermissions.SetCommandListenerStatus("getperm", server.Id, channel.Id, true);
                    _commandPermissions.SetCommandListenerStatus("getmyperm", server.Id, channel.Id, true);
                    _commandPermissions.SetCommandListenerStatus("getmodules", server.Id, channel.Id, true);
                    _commandPermissions.SetCommandListenerStatus("getcommands", server.Id, channel.Id, true);
                }
            }

        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, "ping")]
        public async Task HandlePing(IHunieCommand command)
        {
            await command.Channel.SendMessage("pong");
        }
        
        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.Administrator, "setperm")]
        public async Task SetUserPermissions(IHunieCommand command)
        {
            if (command.RawParameters.Length != 0 || command.RawParameters.Length != 2) return;
            var userParameter = command.RawParameters[0].Trim();
            var levelParameter = command.RawParameters[1].Trim();
            await command.Channel.SendMessage($"Sorry {command.User.Mention}, {command.Command} is not implemented yet.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.Administrator, "getperm")]
        public async Task GetUserPermission(IHunieCommand command)
        {
            if (command.RawParameters.Length != 0 || command.RawParameters.Length != 2) return;
            var userParameter = command.RawParameters[0].Trim();
            var levelParameter = command.RawParameters[1].Trim();
            await command.Channel.SendMessage($"Sorry {command.User.Mention}, {command.Command} is not implemented yet.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived | CommandEvent.PrivateMessageReceived, UserPermissions.User, "getmyperm")]
        public async Task GetMyPermission(IHunieCommand command)
        {
            var userPerms = _userPermissions[command.User.Id];
            await command.Channel.SendMessage($"{command.User.Mention} has a permission level of {userPerms}. This permission applies to me, {command.Client.CurrentUser.Mention}, not to {command.Channel.Mention}.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, "getmodules")]
        public async Task GetModules(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            await command.Channel.SendMessage($"Currenty loaded modules: \r\n{string.Join(", ", hhmd.Commands.Select(s => s.Name))}");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, "getcommands")]
        public async Task GetCommands(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            await command.Channel.SendMessage($"Currently loaded commands: \r\n{string.Join(", ", hhmd.Commands.Select(s => string.Join(", ", s.Commands)))}");
        }

    }

}