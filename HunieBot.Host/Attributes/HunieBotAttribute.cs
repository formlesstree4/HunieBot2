using System;

namespace HunieBot.Host.Attributes
{

    /// <summary>
    ///     The <see cref="HunieBotAttribute"/> is used to decorate a class with metadata, telling <see cref="HunieHost"/> that this class is a bot that can and should receive commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HunieBotAttribute : Attribute
    {

        /// <summary>
        ///     Gets the display name of this <see cref="HunieBotAttribute"/>
        /// </summary>
        /// <remarks>This is mostly used for reporting purposes, such as <see cref="HunieHost"/> echoing out what bots are currently instantiated</remarks>
        public string Name { get; }

        /// <summary>
        ///     Creates a new instance of the <see cref="HunieBotAttribute"/>.
        /// </summary>
        /// <param name="name">The friendly display name of the bot.</param>
        public HunieBotAttribute(string name)
        {
            Name = name;
        }

    }
}
