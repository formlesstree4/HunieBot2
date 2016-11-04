using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;

namespace HunieBot.Choice
{
    [HunieBot(nameof(Choose))]
    public class Choose
    {
        public static string HelpText = $"```{nameof(Choice)}: ?```\n" +
                                        "Give me a list of choices and I'll pick one for you!\n" +
                                        "Usage: .choose|.choice [choice1; choice2; choice3; ...]";

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true,
            commands: new [] { "choose", "choice"})]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            // no choices or we got a help option
            if (command.ParametersArray.Length < 2
                || (command.ParametersArray.Length == 2 && command.ParametersArray.FirstOrDefault() == "?"))
            {
                await command.Channel.SendMessage($"{command.User.NicknameMention}\n{HelpText}");
                return;
            }
            
            // todo: somehow get the actual command token. alternatively, command could have a (raw) string that is the whole message without the command token, the command name, and the trailing space
            var choices = command.Message.Text.Replace($".{command.Command} ", "").Split(';');

            var rand = new Random();
            var choice = rand.Next(choices.Length);

            await command.Channel.SendMessage($"{command.User.NicknameMention}\n" +
                $"Between {string.Join("; ", choices)}...\n" +
                $"I choose `{choices[choice]}`!");
        }
    }
}
