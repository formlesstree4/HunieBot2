﻿using Discord;
using HunieBot.Host.Interfaces;
using HunieBot.Host.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HunieBot.Host
{

    /// <summary>
    ///     An internal implementation of <see cref="IHunieCommand"/>
    /// </summary>
    public sealed class HunieCommand : IHunieCommand
    {
        private const char ParamsIndicator = '-';


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
            var clnParams = m.Text.Substring(m.Text.IndexOf(' ') + 1);
            var rawParams = m.RawText.Substring(m.RawText.IndexOf(' ') + 1);
            var clnParamsParsed = clnParams.ParseParameters();
            var rawParamsParsed = rawParams.ParseParameters();

            Command = cleanedRegText.First();
            ParametersArray = clnParams.ParseParameters().ToArray();
            RawParametersArray = rawParams.ParseParameters().ToArray();
            Parameters = new Parameters(ConvertArrayToParameters(rawParamsParsed.ToArray()));

        }

        public HunieCommand(IHunieMessage message) : this(message.Channel, message.Server, message.User, message.Client, message.Message) { }




        /// <summary>
        ///     This is where the magic happens. So, here's how we're going to do this.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private Dictionary<string, object> ConvertArrayToParameters(string[] array)
        {
            var dict = new Dictionary<string, object>();
            var currentParameter = "";
            var currentParameterValues = new List<object>();
            foreach (var item in array)
            {
                if(item[0] == ParamsIndicator)
                {
                    if (!string.IsNullOrWhiteSpace(currentParameter))
                    {
                        dict[currentParameter] = string.Join(" ", currentParameterValues);
                        currentParameterValues.Clear();
                    }
                    currentParameter = item.Substring(1);
                    continue;
                }
                else
                {
                    currentParameterValues.Add(item);
                }
            }
            dict[currentParameter] = string.Join(" ", currentParameterValues);
            return dict;
        }




    }


    public sealed class Parameters : IReadOnlyDictionary<string, object>
    {
        private readonly IDictionary<string, object> _parameters;

        public Parameters(IDictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public object this[string key]
        {
            get
            {
                return _parameters.ContainsKey(key) ? _parameters[key] : "";
            }
        }

        public object this[params string[] keys]
        {
            get
            {
                foreach (var key in keys)
                {
                    if (_parameters.ContainsKey(key)) return _parameters[key];
                }
                return "";
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

        public IEnumerable<object> Values
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

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        public bool TryGetValue(string key, out object value)
        {
            return _parameters.TryGetValue(key, out value);
        }

        public Option<object> GetIfExists(string key)
        {
            object v;
            if (TryGetValue(key, out v)) return v;
            return Option.Empty;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }
    }
    
}