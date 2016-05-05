using HunieBot.BlackJack.Enums;

namespace HunieBot.BlackJack.Objects
{

    /// <summary>
    ///     Represents a playing card.
    /// </summary>
    public struct Card
    {

        /// <summary>
        ///     Gets the <see cref="Suit"/> of this card.
        /// </summary>
        public Suit Suit { get; }

        /// <summary>
        ///     Gets the <see cref="Face"/> of this card.
        /// </summary>
        public Face Face { get; }

        /// <summary>
        ///     Gets the represented value of this card.
        /// </summary>
        /// <remarks>
        ///     If <see cref="Face"/> is <see cref="Face.Ace"/> then <see cref="Value"/> returns 11. It will not ever return 1.
        /// </remarks>
        public int Value
        {
            get
            {
                switch (Face)
                {
                    case Face.Ace:
                        return 11;
                    case Face.King:
                    case Face.Queen:
                    case Face.Jack:
                        return 10;
                    default:
                        return (int)Face;
                }
            }
        }

        /// <summary>
        ///     Creates a new <see cref="Card"/>
        /// </summary>
        /// <param name="suit"><see cref="Enums.Suit"/></param>
        /// <param name="face"><see cref="Enums.Suit"/></param>
        public Card(Suit suit, Face face)
        {
            Suit = suit;
            Face = face;
        }

        /// <summary>
        ///     Returns a string representation of this Card.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Face} of {Suit}";

    }

}
