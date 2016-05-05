using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HunieBot.BlackJack
{

    /// <summary>
    ///     Handles commands for BlackJack.
    /// </summary>
    [HunieBot("BlackJack")]
    public class BlackJackBot
    {
        private readonly ConcurrentDictionary<string, BlackJack> _gameHandler = new ConcurrentDictionary<string, BlackJack>();


        [HandleCommand(CommandEvent.AnyMessageReceived | CommandEvent.CommandReceived, UserPermissions.User, true, "help")]
        public async Task HandleHelpCommand(IHunieCommand command)
        {
            await command.Channel.SendMessage("ToDo");
        }

        [HandleCommand(CommandEvent.PrivateMessageReceived| CommandEvent.CommandReceived, UserPermissions.User, true, "balance")]
        public async Task HandleBalanceCommand(IHunieCommand command)
        {
            // PM the user their balance.
            var player = Objects.PlayerManager.GetPlayer(command.User);
            await command.User.SendMessage($"Your balance is {player.Money:C}");
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.User, true, "start")]
        public async Task HandleStartBlackJack(IHunieCommand command)
        {
            var channelName = command.Channel.Name;
            if (!_gameHandler.ContainsKey(channelName) ||
                    (_gameHandler.ContainsKey(channelName) && _gameHandler[channelName].IsComplete))
            {
                var bj = new BlackJack(command.Channel);
                _gameHandler.AddOrUpdate(channelName, bj, (s, b) => bj);
                await bj.InitializeBlackJack(command);
            }
            else
            {
                await command.User.SendMessage($"You may not create a game of BlackJack in \"{channelName}\". A game is currently in progress.");
            }
        }

        [HandleCommand(CommandEvent.MessageReceived | CommandEvent.CommandReceived, UserPermissions.User, true,
            "join", "leave", "h", "hit", "s", "pass", "stand", "bid", "bet", "b")]
        public async Task HandleGameCommands(IHunieCommand command)
        {
            BlackJack b;
            if(_gameHandler.TryGetValue(command.Channel.Name, out b)) await b.HandleChannelInput(command);
        }


    }

}