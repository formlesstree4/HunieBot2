#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using Discord;
using HunieBot.BlackJack.Objects;
using HunieBot.Host.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace HunieBot.BlackJack
{

    /// <summary>
    ///     This is where the magic happens.
    /// </summary>
    public sealed class BlackJack
    {

        private int _currentPlayerIndex = -1;
        private bool _canBid = false;
        private bool _isStarted = false;
        private readonly Channel _currentChannel;
        private readonly Dealer _dealer = new Dealer();
        private readonly List<Player> _players = new List<Player>();
        private readonly Deck _deck = Deck.Default;

        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly EventWaitHandle _bidWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);


        /// <summary>
        ///     Gets whether or not the current game is complete.
        /// </summary>
        public bool IsComplete { get; private set; }

        /// <summary>
        ///     Gets whether or not the <see cref="BlackJack"/> game will automatically reset when the last round is over.
        /// </summary>
        public bool AutoReset { get; private set; }



        /// <summary>
        /// 
        /// </summary>
        public BlackJack(Channel channel)
        {
            _currentChannel = channel;
        }



        /// <summary>
        ///     Initializes the <see cref="BlackJack"/> round.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public async Task InitializeBlackJack(IHunieCommand e)
        {

            await _currentChannel.SendIsTyping();
            await _currentChannel.SendMessage($"A game of BlackJack has been started in {_currentChannel.Mention}\r\nThose that wish to join the game may type \".join\" for the next 30 seconds.");
            await Task.Run(async () =>
            {
                await HandleJoinCommand(e);
                await Task.Delay(System.TimeSpan.FromSeconds(15));
                await _currentChannel.SendMessage("The game will begin in approximately 15 seconds!");
                await Task.Delay(System.TimeSpan.FromSeconds(15));
                _isStarted = true;
                lock (_players)
                {
                    if (_players.Count == 0)
                    {
                        // Bail.
                        IsComplete = true;
                        _currentChannel.SendMessage("There are no players in this round. The game will conclude. Please type \".start\" to begin a new round!");
                        return;
                    }
                    else
                    {
                        _currentChannel.SendMessage("The game will now begin!");
                    }
                }

                // Dealer gets two cards.
                _dealer.Hand.Add(_deck.Next());
                _dealer.Hand.Add(_deck.Next());

                // Show the dealer hand.
                await GetStartingBid();
                await ShowPlayerHand(_dealer);
                await BasicGameLoop();
                IsComplete = true;
            });
        }

        /// <summary>
        ///     Handles accepting various bids from the players.
        /// </summary>
        /// <returns></returns>
        private async Task GetStartingBid()
        {
            await _currentChannel.SendMessage("Players, you have 30 seconds to place your starting bids now with \".bid <amount>\".");
            _canBid = true;
            await Task.Run(async () =>
            {
                await Task.Delay(System.TimeSpan.FromSeconds(15));
                await _currentChannel.SendMessage("15 seconds remaining to place your bids!");
                await Task.Delay(System.TimeSpan.FromSeconds(15));
                await _currentChannel.SendMessage("The bidding round is now complete.");
            });
            _canBid = false;
            List<Player> players;
            lock (_players)
            {
                players = _players.ToList();
            }
            foreach (var player in players)
            {
                if (player.Bid == 0)
                {
                    await _currentChannel.SendMessage($"{player.Name} did not place a bid and will be removed from this round.");
                    lock (_players)
                    {
                        _players.Remove(player);
                    }
                }
            }
        }

        /// <summary>
        ///     The game loop.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        private async Task BasicGameLoop()
        {
            for (_currentPlayerIndex = 0; _currentPlayerIndex < _players.Count; _currentPlayerIndex++)
            {
                Player p;
                lock (_players)
                {
                    p = _players[_currentPlayerIndex];
                }
                await _currentChannel.SendMessage($"It is {p.User.Mention}'s turn!");
                await ShowPlayerHand(p);
                var cts = new CancellationTokenSource();
                Task.Run(async () =>
                {
                    await Task.Delay(System.TimeSpan.FromSeconds(30));
                    if (cts.IsCancellationRequested) return;
                    await _currentChannel.SendMessage($"{p.User.Mention}! You have 30 seconds to finish your turn!");
                    await Task.Delay(System.TimeSpan.FromSeconds(30));
                    if (cts.IsCancellationRequested) return;
                    await _currentChannel.SendMessage($"{p.User.Mention}'s is automatically standing for taking too long to complete their turn.");
                    _waitHandle.Set();
                }, cts.Token);
                _waitHandle.WaitOne();
                cts.Cancel();
            }
            await DealerTurn();
            await DetermineWinners();
            await _currentChannel.SendMessage("The current round of BlackJack has concluded! Please type \".start\" to begin a new round!");
        }


        /// <summary>
        ///     The command processor for a given channel.
        /// </summary>
        /// <param name="e"><see cref="MessageEventArgs"/></param>
        /// <returns><see cref="Task"/></returns>
        public async Task HandleChannelInput(IHunieCommand e)
        {
            switch(e.Command)
            {
                case "join":
                    await HandleJoinCommand(e);
                    break;
                case "leave":
                    await HandleLeaveCommand(e);
                    break;
                case "h":
                case "hit":
                    await HandleHitCommand(e);
                    break;
                case "s":
                case "pass":
                case "stand":
                    await HandleStandCommand(e);
                    break;
                case "bid":
                case "bet":
                case "b":
                    await HandleBidCommand(e);
                    break;
            }
        }

        /// <summary>
        ///     Handles "!join".
        /// </summary>
        /// <param name="e"><see cref="MessageEventArgs"/></param>
        /// <returns><see cref="Task"/></returns>
        private async Task HandleJoinCommand(IHunieCommand e)
        {
            if (_isStarted)
            {
                await e.User.SendMessage("The game has already started. Please wait until the next round has started!");
            }
            else
            {

                // So, we're going to see if we already have this user in the players list.
                Player player;
                lock (_players)
                {
                    if (_players.Any(p => p.User.Id.Equals(e.User.Id)))
                    {
                        // Nerd. I got you.
                        return;
                    }
                    player = PlayerManager.GetPlayer(e.User);
                    if (player.Money == 0) player.Money = 50;
                    player.Hand.Add(_deck.Next());
                    player.Hand.Add(_deck.Next());
                    _players.Add(player);
                }
                await _currentChannel.SendMessage($"{e.User.Mention} has joined this round! Current Players: {string.Join(", ", _players.Select(p => p.User.Mention))}");
                await e.User.SendMessage($"Thanks for playing a round of BlackJack in {e.Channel.Mention}! Currently, you have {player.Money:C} to place a bid on. I'll let you know when the bidding round commences.");
            }
        }

        /// <summary>
        ///     Handles "!leave".
        /// </summary>
        /// <param name="e"><see cref="MessageEventArgs"/></param>
        /// <returns><see cref="Task"/></returns>
        private async Task HandleLeaveCommand(IHunieCommand e)
        {
            if (_isStarted) return;
            lock (_players)
            {
                _players.RemoveAll(p => p.User.Id.Equals(e.User.Id));
            }
            await _currentChannel.SendMessage($"{e.User.Mention} has left the game! Current Players: {string.Join(", ", _players.Select(p => p.User.Mention))}");
        }

        /// <summary>
        ///     Handles "!hit".
        /// </summary>
        /// <param name="e"><see cref="MessageEventArgs"/></param>
        /// <returns><see cref="Task"/></returns>
        private async Task HandleHitCommand(IHunieCommand e)
        {
            if (_currentPlayerIndex == -1) return;
            Player currentPlayer;
            lock (_players)
            {
                currentPlayer = _players[_currentPlayerIndex];
            }
            // Make sure the person that sent us the "!hit" is the right person.
            if (currentPlayer.User.Id != e.User.Id) return;

            var nextCard = _deck.Next();
            currentPlayer.Hand.Add(nextCard);
            await _currentChannel.SendMessage($"{currentPlayer.User.Mention} hits and receives {nextCard}!");

            // Check for BUST indicator.
            if (currentPlayer.Hand.IsBust)
            {
                await _currentChannel.SendMessage($"{currentPlayer.User.Mention} has busted! Total: {currentPlayer.Hand.Value}");
                _waitHandle.Set();
                return;
            }

            await ShowPlayerHand(currentPlayer);
            if (currentPlayer.Hand.Count() == 5)
            {
                await _currentChannel.SendMessage($"{currentPlayer.User.Mention} has five cards in their hand. Their turn is now over!");
                _waitHandle.Set();
            }
        }

        /// <summary>
                              ///     Handles "!stand" or "!pass".
                              /// </summary>
                              /// <param name="e"><see cref="MessageEventArgs"/></param>
                              /// <returns><see cref="Task"/></returns>
        private async Task HandleStandCommand(IHunieCommand e)
        {
            if (_currentPlayerIndex == -1) return;
            Player currentPlayer;
            lock (_players)
            {
                currentPlayer = _players[_currentPlayerIndex];
            }
            // Make sure the person that sent us the "!stand" is the right person.
            if (currentPlayer.User.Id != e.User.Id) return;
            _waitHandle.Set();
        }

        /// <summary>
        ///     Handles "!bid <![CDATA[<value>]]>"
        /// </summary>
        /// <param name="e"><see cref="MessageEventArgs"/></param>
        /// <returns><see cref="Task"/></returns>
        private async Task HandleBidCommand(IHunieCommand e)
        {
            if (_currentPlayerIndex == -1) return;
            Player currentPlayer;
            lock (_players)
            {
                currentPlayer = _players[_currentPlayerIndex];
            }
            if (currentPlayer.User.Id != e.User.Id) return;
            if (e.Parameters.Length == 0) return;
            var valueRaw = e.Parameters[0].Trim();
            decimal value;
            if (!decimal.TryParse(valueRaw, out value))
            {
                await e.User.SendMessage("Please enter a valid bid value!");
                return;
            }
            // OK, great. Let's logic check whether or not we can do this.
            decimal pMoney;
            lock (currentPlayer)
            {
                pMoney = currentPlayer.Money + currentPlayer.Bid;
            }
            if (value > pMoney)
            {
                await currentPlayer.User.SendMessage($"You may not bid more than you currently have. You have {currentPlayer.Money:C}");
                return;
            }
            lock (currentPlayer)
            {
                currentPlayer.Money += currentPlayer.Bid;
                currentPlayer.Bid = value;
                currentPlayer.Money -= value;
            }
            await _currentChannel.SendMessage($"{currentPlayer.User.Mention} has set their bid to {value:C}!");
        }
        
        /// <summary>
        ///     Shows the player hand to the room and sends a direct message of the full hand.
        /// </summary>
        /// <param name="currentPlayer">The <see cref="Player"/> to print out the card information for</param>
        /// <returns><see cref="Task"/></returns>
        private async Task ShowPlayerHand(Player currentPlayer)
        {
            var visiblePlayerHand = currentPlayer.Hand.Skip(1).ToList();
            if (!visiblePlayerHand.Any(c => c.Face == Enums.Face.Ace))
            {
                await _currentChannel.SendMessage($"{currentPlayer.User.Mention} currently is showing {visiblePlayerHand.Sum(c => c.Value)} ({string.Join(", ", visiblePlayerHand)}).");
            }
            else
            {
                var visibleTotal = visiblePlayerHand.Where(c => c.Face != Enums.Face.Ace).Sum(c => c.Value);
                var visibleAceCount = visiblePlayerHand.Count(c => c.Face == Enums.Face.Ace);
                if ((visibleTotal + visibleAceCount + 10) > 21)
                {
                    await _currentChannel.SendMessage($"{currentPlayer.User.Mention} currently is showing {visibleTotal + visibleAceCount} ({string.Join(", ", visiblePlayerHand)}).");
                }
                else
                {
                    await _currentChannel.SendMessage($"{currentPlayer.User.Mention} currently is showing {visibleTotal + visibleAceCount} or {visibleTotal + visibleAceCount + 10} ({string.Join(", ", visiblePlayerHand)}).");
                }
            }
            if (currentPlayer.User != null)
            {
                await currentPlayer.User.SendMessage($"Your current hand is: {currentPlayer.Hand} ({currentPlayer.Hand.Value})");
            }
        }

        /// <summary>
        ///     Dealer takes his turn now.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        private async Task DealerTurn()
        {

            // Time for dealer to do things.
            // Dealer does simple things. If less than 17, hit.
            // Otherwise, stop.
            await _currentChannel.SendMessage($"Dealer's hand is: {_dealer.Hand} ({_dealer.Hand.Value})");
            while (_dealer.Hand.Value < 17)
            {
                var card = _deck.Next();
                await _currentChannel.SendMessage($"Dealer is taking a hit and has received {card}!");
                _dealer.Hand.Add(card);
                await _currentChannel.SendMessage($"Dealer's hand is: {_dealer.Hand} ({_dealer.Hand.Value})");
                if (_dealer.Hand.IsBust) await _currentChannel.SendMessage("Dealer has bust!");
            }
        }

        /// <summary>
        ///     Prints out who wins!
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        private async Task DetermineWinners()
        {
            List<Player> p;
            lock (_players)
            {
                p = _players.ToList();
            }
            foreach (var player in p)
            {
                if (player.Hand.IsBust)
                {
                    if (_dealer.Hand.IsBust)
                    {
                        await _currentChannel.SendMessage($"{player.Name} and the dealer have both busted. {player.Name} will receive their bid back.");
                        player.Money += player.Bid;
                    }
                    // else
                    // {
                    //     await _currentChannel.SendMessage($"{player.Name} has busted with {player.Hand.Value} and has lost their bet!");
                    // }
                }
                else
                {
                    if ((player.Hand.Value > _dealer.Hand.Value) || player.Hand.IsBlackJack || _dealer.Hand.IsBust)
                    {
                        await _currentChannel.SendMessage($"{player.Name} has beaten the dealer with {player.Hand.Value}!");
                        var money = System.Math.Round((player.Bid * (decimal)2.0) * (decimal)(player.Hand.IsBlackJack ? 1.5 : 1), 2);
                        await player.User.SendMessage($"Congratulations! You have received {money:C} for your win.");
                        player.Money += money;
                    }
                    else if (player.Hand.Value == _dealer.Hand.Value)
                    {
                        await _currentChannel.SendMessage($"{player.Name} has tied with the dealer and will receive their bid back.");
                        player.Money += player.Bid;
                    }
                    else
                    {
                        await _currentChannel.SendMessage($"{player.Name} has lost to the dealer with {player.Hand.Value}.");
                    }
                }
                player.Money = System.Math.Round(player.Money);
                await player.User.SendMessage($"Your balance is currently: {player.Money:C}.");
                player.Bid = 0;
            }
        }
    }

}
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously