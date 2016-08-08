using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using Newtonsoft.Json;

namespace RandomCatImage
{
    [HunieBot(nameof(RandomCatImage))]
    public class RandomCatImage
    {

        //private const string ApiKey = "[insert api key]";

        private static Dictionary<string, string> SupportedCatCommands
            => new Dictionary<string, string>
            {
                { "?",              "? [command] - gets help on commands and options"},
                { "get",            "get [count] - gets [count] cats" },
                { "vote",           "vote [score] - give an image a score from 1-10" },
                { "getvotes",       "getvotes - gets all your different votes" },
                { "favorite",       "favorite ['add'/'remove'] [id] - add/remove an image [id] from your favorites list" },
                { "getfavorites",   "getfavorites - gets all your favorite images" },
                { "report",         "report [id] [reason] - reports an image [id] with a [reason]. this will preveent this image from showing again" },
                { "categories",     "categories - returns a list of all the active categories and their ids" },
                { "stats",          "stats - gets some statistics on this bot's usage of TheCatApi" }
            };

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true, commands: "cat")]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    if (!command.ParametersArray.Any())
                    {
                        var xmlSerializer = new XmlSerializer(typeof (GetResponse.response));
                        var response = new StringReader(webClient.DownloadString(new GetCat().RequestUrl));
                        var deserializedResponse = (GetResponse.response)xmlSerializer.Deserialize(response);
                        var imageData = deserializedResponse.data.images[0];
                        await command.Channel.SendMessage($"id: {imageData.id}\nurl: {imageData.url}");
                        return;
                    }

                    var catCommand = command.ParametersArray[0];

                    switch (catCommand)
                    {
                        case "?":
                            // just '?'
                            if (command.ParametersArray.Length == 1)
                            {
                                var messageText = string.Join("\n", from supportedCatCommand in SupportedCatCommands
                                                                    select supportedCatCommand.Value);
                                await command.Channel.SendMessage(messageText);
                                return;
                            }

                            var catOption = command.ParametersArray[1];
                            await command.Channel.SendMessage($"{SupportedCatCommands[catOption]}");
                            return;
                        case "get":
                            await command.Channel.SendMessage("not impleemented");
                            break;
                        case "vote":
                            await command.Channel.SendMessage("not impleemented");
                            break;
                        case "getvotes":
                            await command.Channel.SendMessage("not impleemented");
                            break;
                        case "favorite":
                            await command.Channel.SendMessage("not impleemented");
                            break;
                        case "getfavorites":
                            await command.Channel.SendMessage("not impleemented");
                            break;
                        case "report":
                            await command.Channel.SendMessage("not impleemented");
                            break;
                        case "categories":
                            await command.Channel.SendMessage("not impleemented");
                            break;
                        case "stats":
                            await command.Channel.SendMessage("not impleemented");
                            break;
                        default:
                            await command.Channel.SendMessage("invalid option, try `cat ?`");
                            return;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Debug($"RandomCatImage: {e.Message}");
                await command.Channel.SendMessage($"RandomCatImage ran into an error: {e.Message}");
            }
        }
    }
}
