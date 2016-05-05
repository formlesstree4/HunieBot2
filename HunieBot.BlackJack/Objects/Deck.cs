using HunieBot.BlackJack.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HunieBot.BlackJack.Objects
{

    /// <summary>
    ///     A deck in the literal sense. Under the hood, we have multiple counts of 52 card decks fueling this.
    /// </summary>
    /// <remarks>
    ///     Take a peek at <see cref="FillCardQueue(int)"/> if you really want to know what we do here. This acts like a casino "deck" does.
    /// </remarks>
    public sealed class Deck
    {

        /// <summary>
        ///     Gets a default instance of <see cref="Deck"/>.
        /// </summary>
        public static Deck Default { get; } = new Deck();

        /// <remarks>
        ///     This should always be even.
        /// </remarks>
        private const int NumberOfDecks = 4;
        private readonly IList<Card> _fullDeck = new List<Card>();
        private Queue<Card> _cards = new Queue<Card>();

        /// <summary>
        ///     Creates a new <see cref="Deck"/>.
        /// </summary>
        public Deck()
        {
            // Add all the cards.
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                foreach (Face face in Enum.GetValues(typeof(Face)))
                    _fullDeck.Add(new Card(suit, face));
            _fullDeck = Enumerable.Repeat(_fullDeck, NumberOfDecks).SelectMany(c => c).ToList();
            FillCardQueue();
        }

        /// <summary>
        ///     Returns the next <see cref="Card"/> in the <see cref="Deck"/>.
        /// </summary>
        /// <returns><see cref="Card"/></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Card Next()
        {
            if (_cards.Count == (52 * (NumberOfDecks / 2))) FillCardQueue();
            return _cards.Dequeue();
        }

        /// <summary>
        ///     Adds a lot of cards to the deck.
        /// </summary>
        /// <param name="decks">The number of decks to add.</param>
        private void FillCardQueue()
        {
            var listOfCards = new List<Card>(_fullDeck);
            listOfCards.Shuffle();
            _cards = new Queue<Card>(listOfCards);
        }
    }

}
