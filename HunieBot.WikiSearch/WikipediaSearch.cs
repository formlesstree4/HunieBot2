using Discord;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HunieBot.WikipediaSearch
{
    [HunieBot(nameof(WikipediaSearch))]
    public sealed class WikipediaSearch
    {
        private const string WikipediaApiScheme = "https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exlimit=max&explaintext&exintro&titles={0}&redirects=";

        [HandleCommand(CommandEvent.AnyMessageReceived | CommandEvent.CommandReceived, UserPermissions.User, commands: new[] { "wiki" })]
        public async Task HandleCommand(IHunieCommand command)
        {
            if (!command.Parameters.Any())
            {
                await command.Channel.SendMessage("Perform a wiki search by typing `.wiki baseball`");
            }
            else
            {
                var request = WebRequest.Create(string.Format(WikipediaApiScheme, command.Parameters.FirstOrDefault()));
                var response = await request.GetResponseAsync();
                string resultJson;
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        resultJson = await reader.ReadToEndAsync();
                    }
                }

                if (string.IsNullOrEmpty(resultJson))
                {
                    await command.Channel.SendMessage(string.Format("Failed to find anything for `.wiki {0}`", command.Parameters[0]));
                }
                else
                {
                    var wikipediaResponse = JsonConvert.DeserializeObject<WikipediaResponse>(resultJson);
                    if (wikipediaResponse != null && wikipediaResponse.Query != null && wikipediaResponse.Query.Pages.Any())
                    {
                        var hasResults = false;
                        var builder = new StringBuilder();
                        foreach (var page in wikipediaResponse.Query.Pages.Values)
                        {
                            if (string.IsNullOrWhiteSpace(page.Extract))
                            {
                                await command.Channel.SendMessage(string.Format("Failed to find anything for `.wiki {0}`", command.Parameters[0]));
                            }
                            else
                            {
                                builder.AppendLine(page.Extract);
                                hasResults = true;
                            }
                        }

                        if (hasResults)
                        {
                            builder.Insert(0, "```\r\n");
                            if (builder.Length > DiscordConfig.MaxMessageSize)
                            {
                                // Extra 5 characters for end of code block.
                                // Extra 3 characters for elipses.
                                var difference = builder.Length - DiscordConfig.MaxMessageSize + 5 + 3;
                                builder = builder.Remove(builder.Length - difference, difference);
                                builder.Insert(builder.Length, "...");
                            }
                            builder.Insert(builder.Length, "\r\n```");
                            await command.Channel.SendMessage(builder.ToString());
                        }
                    }
                    else
                    {
                        await command.Channel.SendMessage(string.Format("Failed to parse results `.wiki {0}`", command.Parameters[0]));
                    }
                }
            }
        }
    }
}