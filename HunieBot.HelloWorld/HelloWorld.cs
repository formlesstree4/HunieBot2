using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Threading.Tasks;

namespace HunieBot.HelloWorld
{
    [HunieBot(nameof(HelloWorldBot))]
    public sealed class HelloWorldBot
    {
        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, commands: new[] { "ping" })]
        public async Task HandleCommand(IHunieCommand command)
        {
            await command.Channel.SendMessage("pong");
        }
    }
}