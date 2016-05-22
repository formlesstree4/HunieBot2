using Discord;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, commands: "ping"), Description("A simple ping command")]
        public async Task HandlePing(IHunieCommand command)
        {
            await command.Channel.SendMessage("pong");
        }
        
        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.Administrator, commands: new[] { "set_user_permission", "sup" })]
        public async Task SetUserPermissions(IHunieCommand command)
        {
            if (command.RawParameters.Length != 2) return;
            var userParameterRaw = command.RawParameters[0].Trim();
            var levelParameterRaw = command.RawParameters[1].Trim();
            UserPermissions userPermission;
            var userId = Convert.ToUInt64(userParameterRaw.Substring(2, userParameterRaw.Length - 3));
            var targetUser = command.Server.Users.First(u => u.Id == userId);
            if (!Enum.TryParse(levelParameterRaw, true, out userPermission)) return;
            _userPermissions[command.Server.Id, targetUser.Id] = userPermission;
            await command.Channel.SendMessage($"{targetUser.Mention}'s new permission is {userPermission}");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.Administrator, commands: "get_user_permission")]
        public async Task GetUserPermission(IHunieCommand command)
        {
            if (command.RawParameters.Length != 0 || command.RawParameters.Length != 2) return;
            var userParameter = command.RawParameters[0].Trim();
            var levelParameter = command.RawParameters[1].Trim();
            await command.Channel.SendMessage($"Sorry {command.User.Mention}, {command.Command} is not implemented yet.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, commands: "get_my_permission")]
        public async Task GetMyPermission(IHunieCommand command)
        {
            var userPerms = _userPermissions[command.Server.Id, command.User.Id];
            await command.Channel.SendMessage($"{command.User.Mention} has a permission level of {userPerms}.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, commands: "get_modules")]
        public async Task GetModules(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            await command.Channel.SendMessage($"Currenty loaded modules: \r\n{string.Join(", ", hhmd.Commands.Select(s => s.Name))}");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.MessageReceived, UserPermissions.User, commands: "get_commands")]
        public async Task GetCommands(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            await command.Channel.SendMessage($"Currently loaded commands: \r\n{string.Join(", ", hhmd.Commands.Select(s => string.Join(", ", s.Commands)))}");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.Administrator, false, "set_command_permission", "scp")]
        public async Task HandleSetCommandPermissions(IHunieCommand command, IHunieHostMetaData hhmd)
        {
            var messageBuilder = new StringBuilder();
            var loadedCommands = new System.Collections.Generic.List<string>();
            foreach (var item in hhmd.Commands) loadedCommands.AddRange(item.Commands);
            var rawCommandNames = command.Parameters[0];
            var rawCommandStatus = command.Parameters[1];
            int commandStatusInt;
            bool commandStatusBool;

            if (int.TryParse(rawCommandStatus, out commandStatusInt))
            {
                commandStatusBool = (commandStatusInt == 1);
            }
            else if (!bool.TryParse(rawCommandStatus, out commandStatusBool))
            {
                messageBuilder.AppendLine($"{rawCommandStatus} is invalid.");
                await command.Channel.SendMessage(messageBuilder.ToString());
                return;
            }

            foreach (var rawCommandName in rawCommandNames.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!loadedCommands.Contains(rawCommandName, StringComparer.OrdinalIgnoreCase))
                {
                    messageBuilder.AppendLine($"The command {rawCommandName} is non-existant. I cannot set a permission for a command that does not exist.");
                    continue;
                }
                _commandPermissions[command.Server.Id, command.Channel.Id, new[] { rawCommandName }] = commandStatusBool;
            }
            messageBuilder.AppendLine($"{rawCommandNames}'s status for {command.Channel.Mention} has been set to {commandStatusBool}.");
            await command.Channel.SendMessage(messageBuilder.ToString());
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.Administrator, false, "get_command_permission", "gcp")]
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
            await command.Channel.SendMessage($"{rawCommandName}'s status for {command.Channel.Mention} is {_commandPermissions[command.Server.Id, command.Channel.Id, new[] { rawCommandName }]}.");
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, false, "get_command_description", "gcd")]
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