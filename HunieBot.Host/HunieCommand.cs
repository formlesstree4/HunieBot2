using Discord;
using Fclp;
using HunieBot.Host.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using HunieBot.Host.Internal;

namespace HunieBot.Host
{

    /// <summary>
    ///     An internal implementation of <see cref="IHunieCommand"/>
    /// </summary>
    public sealed class HunieCommand : IHunieCommand
    {
        public Channel Channel { get; }

        public Message Message { get; }

        public Server Server { get; }

        public User User { get; }

        public DiscordClient Client { get; }

        public string Command { get; }

        public string[] ParametersArray { get; }

        public string[] RawParametersArray { get; }

        public Parameters Parameters { get; }
        
        public HunieCommand(Channel c, Server s, User u, DiscordClient dc, Message m)
        {
            Channel = c;
            Server = s;
            User = u;
            Client = dc;
            Message = m;

            // Here's how we're going to process this:
            // 1) Remove the first character. The first character is going to be the message text.
            // 2) Split on the space.
            // 3) Each item after the first are the parameters.
            var cleanedRegText = m.Text.Trim().Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim());
            var cleanedRawText = m.RawText.Trim().Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim());
            var processor = new FluentCommandLineParser();

            Command = cleanedRegText.First();
            ParametersArray = cleanedRegText.Skip(1).ToArray();
            RawParametersArray = cleanedRawText.Skip(1).ToArray();
            var processed = processor.Parse(string.Join(" ", RawParametersArray).ParseParameters().ToArray());
            Parameters = new Parameters(processed.AdditionalOptionsFound.ToDictionary(k => k.Key, k => k.Value));

        }

        public HunieCommand(IHunieMessage message) : this(message.Channel, message.Server, message.User, message.Client, message.Message) { }

    }


    public sealed class Parameters : IReadOnlyDictionary<string, string>
    {
        private readonly IDictionary<string, string> _parameters;

        public Parameters(IDictionary<string, string> parameters)
        {
            _parameters = parameters;
        }

        public string this[string key]
        {
            get
            {
                return _parameters[key];
            }
        }

        public int Count
        {
            get
            {
                return _parameters.Count;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return _parameters.Keys;
            }
        }

        public IEnumerable<string> Values
        {
            get
            {
                return _parameters.Values;
            }
        }

        public bool ContainsKey(string key)
        {
            return _parameters.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        public bool TryGetValue(string key, out string value)
        {
            return _parameters.TryGetValue(key, out value);
        }

        public Option<string> GetIfExists(string key)
        {
            string v;
            if (TryGetValue(key, out v)) return v;
            return Option.Empty;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }
    }

}
