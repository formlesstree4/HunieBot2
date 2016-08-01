using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Threading.Tasks;

namespace HunieBot.RandomCatImage
{
    [HunieBot(nameof(RandomCatImage))]
    public class RandomCatImage
    {
        private const string ApiKey = "MTA2Mjgx";

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true, "cat")]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            await command.Channel.SendMessage("dicks");
        }
    }
}
