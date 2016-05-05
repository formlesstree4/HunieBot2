using System.ComponentModel;

namespace HunieBot.BlackJack.Enums
{

    /// <summary>
    ///     Indicates the suit of the card.
    /// </summary>
    public enum Suit
    {

        /// <summary>
        ///     The suit is Hearts.
        /// </summary>
        [Description("♥")]
        Hearts,

        /// <summary>
        ///     The suit is Diamonds.
        /// </summary>
        [Description("♦")]
        Diamonds,

        /// <summary>
        ///     The suit is Spades.
        /// </summary>
        [Description("♠")]
        Spades,

        /// <summary>
        ///     The suit is Clubs.
        /// </summary>
        [Description("♣")]
        Clubs

    }

}
