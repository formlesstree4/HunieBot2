using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;

namespace HunieBot.Avatar
{
    [HunieBot(nameof(Avatar))]
    public class Avatar
    {
        public static string HelpText = $"```{nameof(Avatar)}: ?```\n" +
                                        "Give me a @nickname and I'll retrieve their full avatar for you!\n" +
                                        "Usage: .avatar [@nickname]";

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true,
            commands: "avatar")]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            try
            {
                var targetUserId = command.RawParametersArray.FirstOrDefault();
                targetUserId = targetUserId?.Replace("<", "");
                targetUserId = targetUserId?.Replace("@", "");
                targetUserId = targetUserId?.Replace(">", "");
                targetUserId = targetUserId?.Replace("!", "");
                if (string.IsNullOrWhiteSpace(targetUserId)) throw new Exception("invalid user id!");
                var parsedTargetUserId = ulong.Parse(targetUserId);
                var targetUser = command.Server.GetUser(parsedTargetUserId);

                // no nickname or we got a help option
                if (command.ParametersArray.Length < 1
                    || (command.ParametersArray.Length == 1 && command.ParametersArray.FirstOrDefault() == "?"))
                {
                    await command.Channel.SendMessage($"{command.User.Mention}\n{HelpText}");
                    return;
                }

                await command.Channel.SendMessage($"{command.User.Mention}\n" +
                                                  $"{targetUser.Name}'s avatar: {targetUser.AvatarUrl}");
            }
            catch (Exception e)
            {
                await command.Channel.SendMessage($"{nameof(Avatar)}: error!\n{e.Message}");
            }
        }
    }
}
