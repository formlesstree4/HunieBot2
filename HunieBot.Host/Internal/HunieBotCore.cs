using Discord;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System;
using System.ComponentModel;
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


        public HunieBotCore(IHunieUserPermissions userPermissiosn, IHunieCommandPermissions commandPermissions)
        {
            _userPermissions = userPermissiosn;
            _commandPermissions = commandPermissions;
        }

        [HandleEvent(CommandEvent.Connected)]
        public async Task HandleOnConnect(IHunieEvent hEvent, DiscordClient client)
        {
            //// make sure all commands always work on all servers and channels
            //foreach (var server in client.Servers)
            //{
            //    foreach (var channel in server.TextChannels)
            //    {
            //        _commandPermissions.SetCommandListenerStatus("ping", server.Id, channel.Id, true);
            //        _commandPermissions.SetCommandListenerStatus("setperm", server.Id, channel.Id, true);
            //        _commandPermissions.SetCommandListenerStatus("getperm", server.Id, channel.Id, true);
            //        _commandPermissions.SetCommandListenerStatus("getmyperm", server.Id, channel.Id, true);
            //        _commandPermissions.SetCommandListenerStatus("getmodules", server.Id, channel.Id, true);
            //        _commandPermissions.SetCommandListenerStatus("getcommands", server.Id, channel.Id, true);
            //    }
            //}
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, "ping"), Description("A simple ping command")]
        public async Task HandlePing(IHunieCommand command)
        {
            await command.Channel.SendMessage("pong");
        }
        
        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.Administrator, "set_user_permission")]
        public async Task SetUserPermissions(IHunieCommand command)
        {
            if (command.RawParameters.Length != 0 || command.RawParameters.Length != 2) return;
            var userParameter = command.RawParameters[0].Trim();
            var levelParameter = command.RawParameters[1].Trim();
            await command.Channel.SendMessage($"Sorry {command.User.Mention}, {command.Command} is not implemented yet.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.Administrator, "get_user_permission")]
        public async Task GetUserPermission(IHunieCommand command)
        {
            if (command.RawParameters.Length != 0 || command.RawParameters.Length != 2) return;
            var userParameter = command.RawParameters[0].Trim();
            var levelParameter = command.RawParameters[1].Trim();
            await command.Channel.SendMessage($"Sorry {command.User.Mention}, {command.Command} is not implemented yet.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived | CommandEvent.PrivateMessageReceived, UserPermissions.User, "get_my_permission")]
        public async Task GetMyPermission(IHunieCommand command)
        {
            var userPerms = _userPermissions[command.User.Id];
            await command.Channel.SendMessage($"{command.User.Mention} has a permission level of {userPerms}. This permission applies to me, {command.Client.CurrentUser.Mention}, not to {command.Channel.Mention}.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, "get_modules")]
        public async Task GetModules(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            await command.Channel.SendMessage($"Currenty loaded modules: \r\n{string.Join(", ", hhmd.Commands.Select(s => s.Name))}");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, "get_commands")]
        public async Task GetCommands(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            await command.Channel.SendMessage($"Currently loaded commands: \r\n{string.Join(", ", hhmd.Commands.Select(s => string.Join(", ", s.Commands)))}");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.Administrator, "set_command_permission", "scp")]
        public async Task HandleSetCommandPermissions(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            var loadedCommands = new System.Collections.Generic.List<string>();
            foreach (var item in hhmd.Commands) loadedCommands.AddRange(item.Commands);
            var rawCommandName = command.Parameters[0];
            var rawCommandStatus = command.Parameters[1];
            int commandStatusInt;
            bool commandStatusBool;

            if(!loadedCommands.Contains(rawCommandName, StringComparer.OrdinalIgnoreCase))
            {
                await command.Channel.SendMessage($"The command {rawCommandName} is non-existant. I cannot set a permission for a command that does not exist.");
                return;
            }
            if(int.TryParse(rawCommandStatus, out commandStatusInt))
            {
                _commandPermissions.SetCommandListenerStatus(rawCommandName.ToLowerInvariant(), command.Server.Id, command.Channel.Id, commandStatusInt == 1);
                commandStatusBool = (commandStatusInt == 1);
            }
            else if(bool.TryParse(rawCommandStatus, out commandStatusBool))
            {
                _commandPermissions.SetCommandListenerStatus(rawCommandName.ToLowerInvariant(), command.Server.Id, command.Channel.Id, commandStatusBool);
            }
            await command.Channel.SendMessage($"{rawCommandName}'s status for {command.Channel.Mention} has been set to {commandStatusBool}.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.Administrator, "get_command_permission", "gcp")]
        public async Task HandleGetCommandPermissions(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            var loadedCommands = new System.Collections.Generic.List<string>();
            foreach (var item in hhmd.Commands) loadedCommands.AddRange(item.Commands);
            var rawCommandName = command.Parameters[0];
            if (!loadedCommands.Contains(rawCommandName, StringComparer.OrdinalIgnoreCase))
            {
                await command.Channel.SendMessage($"The command {rawCommandName} is non-existant. I cannot set a permission for a command that does not exist.");
                return;
            }
            await command.Channel.SendMessage($"{rawCommandName}'s status for {command.Channel.Mention} is {_commandPermissions.GetCommandListenerStatus(rawCommandName, command.Server.Id, command.Channel.Id)}.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, "get_command_description", "gcd")]
        public async Task HandleGetCommandDescription(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            var cmd = command.Parameters[0];
            foreach (var wrapper in hhmd.Commands)
                foreach (var item in wrapper.CommandMetadata)
                    if(item.Attribute.Commands.Contains(cmd, StringComparer.OrdinalIgnoreCase))
                        await command.Channel.SendMessage($"{cmd} - {item.Description?.Description ?? "No Description Found"}. Alias(es): {string.Join(", ", item.Attribute.Commands.Except(new[] { cmd }, StringComparer.OrdinalIgnoreCase))}");
        }

    }

}