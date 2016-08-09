using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;

namespace HunieBot.Magic8Ball
{
    [HunieBot(nameof(Magic8Ball))]
    public class Magic8Ball
    {
        public static string HelpText = $"```{nameof(Magic8Ball)}: ?```\n" +
                                        "Ask the Magic 8-Ball any question, and it may tell your future!\n" +
                                        "Usage: .8ball [question]";

        public static IEnumerable<BallAnswer> PossibleAnswers = new List<BallAnswer>
        {
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "It is certain"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "It is decidedly so"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "Without a doubt"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "Yes, definitely"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "You may rely on it"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "As I see it, yes"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "Most likely"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "Outlook good"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "Yes"},
            new BallAnswer { Alignment = AnswerAlignment.Positive, Answer = "Signs point to yes"},
            new BallAnswer { Alignment = AnswerAlignment.Neutral, Answer = "Reply hazy try again"},
            new BallAnswer { Alignment = AnswerAlignment.Neutral, Answer = "Ask again later"},
            new BallAnswer { Alignment = AnswerAlignment.Neutral, Answer = "Better not tell you now"},
            new BallAnswer { Alignment = AnswerAlignment.Neutral, Answer = "Cannot predict now"},
            new BallAnswer { Alignment = AnswerAlignment.Neutral, Answer = "Concentrate and ask again"},
            new BallAnswer { Alignment = AnswerAlignment.Negative, Answer = "Don't count on it"},
            new BallAnswer { Alignment = AnswerAlignment.Negative, Answer = "My reply is no"},
            new BallAnswer { Alignment = AnswerAlignment.Negative, Answer = "My sources say no"},
            new BallAnswer { Alignment = AnswerAlignment.Negative, Answer = "Outlook not so good"},
            new BallAnswer { Alignment = AnswerAlignment.Negative, Answer = "Very doubtful"},
        };

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true,
            commands: "8ball")]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            // no question asked or we got a help option
            if (command.ParametersArray.Length == 1
                || (command.ParametersArray.Length == 2 && command.ParametersArray.FirstOrDefault() == "?"))
            {
                await command.Channel.SendMessage($"{command.User.NicknameMention}\n{HelpText}");
                return;
            }

            var rand = new Random();
            var answer = PossibleAnswers.ElementAt(rand.Next(0, PossibleAnswers.Count() - 1));
            string emote;
            switch (answer.Alignment)
            {
                case AnswerAlignment.Positive:
                    emote = "^.^";
                    break;
                case AnswerAlignment.Negative:
                    emote = "x.x";
                    break;
                case AnswerAlignment.Neutral:
                    emote = "o.o";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var question = string.Join(" ", command.ParametersArray);

            await command.Channel.SendMessage(
                $"{command.User.NicknameMention}: `{question}`\n" + 
                $"{answer.Answer} {emote}");
        }
    }

    public class BallAnswer
    {
        public string Answer { get; set; }
        public AnswerAlignment Alignment { get; set; }
    }

    public enum AnswerAlignment
    {
        Positive,
        Negative,
        Neutral
    }
}
