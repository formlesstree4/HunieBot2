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
                                        "Usage: .choice [choice1; choice2; choice3; ...]";

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true,
            commands: "choose")]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            // no choices or we got a help option
            if (command.ParametersArray.Length < 2
                || (command.ParametersArray.Length == 2 && command.ParametersArray.FirstOrDefault() == "?"))
            {
                await command.Channel.SendMessage($"{command.User.NicknameMention}\n{HelpText}");
                return;
            }

            // this is a little ugly, but i dont know a better way to do it
            var rawParamString = string.Join(" ", command.ParametersArray);
            var choices = rawParamString.Split(';');

            var rand = new Random();
            var choice = rand.Next(0, choices.Length - 1);

            await command.Channel.SendMessage($"{command.User.NicknameMention}\n" +
                $"Between {string.Join("; ", choices)}...\n" +
                $"I choose `{choices[choice]}`!");
        }
    }
}
