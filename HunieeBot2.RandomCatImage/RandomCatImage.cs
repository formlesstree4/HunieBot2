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

        private const string ApiKey = "[get your own api key!]";
        private const int GetCatLimit = 5;

        private static Dictionary<string, string> SupportedCatCommands
            => new Dictionary<string, string>
            {
                {"-help", "`-help [-command command]` - gets help on commands and options"},
                {
                    "-get", "`-get` - returns kitties\n" +
                            "options: \n" +
                            "\t*required*\n" +
                            "\t\t[nothing] - gets a random cat\n" +
                            "\t\tOR `-count n` - gets [n] random cats\n" +
                            "\t\tOR `-id s` - gets the cat with the id [s]\n" +
                            "\t_optional_\n" +
                            "\t\t`-category n` - gets a cat in the category [n]\n" +
                            "\t\t`-size [small, med, full]` - gets a small (250x), medium (500x), or full (source) sized image"
                },
                {"-vote", "`-vote [-id #] [-score #]` - vote on a cat image with a score"},
                {"-getvotes", "`-getvotes` - gets all your different votes"},
                {
                    "-favorite",
                    "`-favorite [-'add' | -'remove'] [-id #]` - add/remove an image [id] from your favorites list"
                },
                {"-getfavorites", "`-getfavorites` - gets all your favorite images"},
                {
                    "-report",
                    "`-report [-id #] [-reason \"your reason here\"]` - reports an image [id] with a [reason]. this will preveent this image from showing again"
                },
                {"-categories", "`-categories` - returns a list of all the active categories and their ids"},
                {"-stats", "`-stats` - gets some statistics on this bot's usage of TheCatApi"}
            };

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true,
            commands: "cat")]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            try
            {
                var catCommand = command.Parameters.Keys.FirstOrDefault();

                switch (catCommand)
                {
                    case "":
                        await command.Channel.SendMessage(RandomBasicResponse(command, logger));
                        return;
                    case "help":
                        await command.User.SendMessage(HelpResponse(command, logger));
                        return;
                    case "get":
                        await command.Channel.SendMessage(GetResponse(command, logger));
                        return;
                    case "vote":
                        await command.User.SendMessage(VoteResponse(command, logger));
                        break;
                    case "getvotes":
                        await command.User.SendMessage(GetVotesResponse(command, logger));
                        break;
                    case "favorite":
                        await command.User.SendMessage("not implemented");
                        break;
                    case "getfavorites":
                        await command.User.SendMessage("not implemented");
                        break;
                    case "report":
                        await command.User.SendMessage("not implemented");
                        break;
                    case "categories":
                        await command.Channel.SendMessage("not implemented");
                        break;
                    case "stats":
                        await command.Channel.SendMessage("not implemented");
                        break;
                    default:
                        await command.Channel.SendMessage("invalid command, try `cat -help`");
                        return;
                }
            }
            catch (Exception e)
            {
                logger.Debug($"RandomCatImage: {e.Message}");
                await command.Channel.SendMessage($"RandomCatImage ran into an error: {e.Message}");
            }
        }

        private static string RandomBasicResponse(IHunieCommand command, ILogging logger)
        {
            using (var webClient = new WebClient())
            {
                var xmlSerializer = new XmlSerializer(typeof (GetResponse.response));
                var request = new GetCat
                {
                    api_key = ApiKey,
                    sub_id = command.User.Id.ToString()
                };
                var response = new StringReader(webClient.DownloadString(request.RequestUrl));
                var deserializedResponse =
                    (GetResponse.response) xmlSerializer.Deserialize(response);
                var imageData = deserializedResponse.data.images[0];
                return $"{command.User.NicknameMention}\nid: `{imageData.id}`\nurl: {imageData.url}";
            }
        }

        private static string HelpResponse(IHunieCommand command, ILogging logger)
        {
            // just '?'
            if (command.ParametersArray.Length == 1)
            {
                var messageLines = new List<string>
                {
                    "Pulls a random cat image from thecatapi.com",
                    "Commands with :grey_exclamation: are sent back by PM"
                };
                messageLines.AddRange(from supportedCatCommand in SupportedCatCommands
                    select supportedCatCommand.Value);

                var messageText = string.Join("\n", messageLines);
                return messageText;
            }
            var catOption = command.ParametersArray[1];
            return $"{SupportedCatCommands[catOption]}";
        }

        private static string GetResponse(IHunieCommand command, ILogging logger)
        {
            var request = new GetCat
            {
                api_key = ApiKey,
                sub_id = command.User.Id.ToString()
            };

            // check for optional parameters first
            foreach (
                var parameter in
                    command.Parameters.Where(
                        parameter => parameter.Key != command.Parameters.Keys.FirstOrDefault()))
            {
                switch (parameter.Key)
                {
                    case "category":
                        var category = 0;
                        if (!int.TryParse(parameter.Value, out category))
                            return
                                $"{command.User.NicknameMention} -category needs a valid number! try `.cat -help -get` for more info";


                        request.category = category;
                        break;
                    case "size":
                        if (parameter.Value != "small"
                            || parameter.Value != "med"
                            || parameter.Value != "full")
                        {
                            return $"{command.User.NicknameMention} -size invalid! try 'cat -help -get' for more info";
                        }

                        request.size = parameter.Value;
                        break;
                    default:
                        return
                            $"{command.User.NicknameMention} invalid option `{parameter.Key}`! try 'cat -help -get' for more info";
                }
            }

            var xmlSerializer = new XmlSerializer(typeof (GetResponse.response));
            using (var webClient = new WebClient())
            {
                // now parse one of our three required options and send our message out to the user
                foreach (
                    var parameter in
                        command.Parameters.Where(
                            parameter => parameter.Key != command.Parameters.Keys.FirstOrDefault()))
                {
                    switch (parameter.Key)
                    {
                        case "count":
                        {
                            // parse the parameter to this option
                            var resultsPerPage = 0;
                            if (!int.TryParse(parameter.Value, out resultsPerPage)
                                || resultsPerPage < 1
                                || resultsPerPage > GetCatLimit)
                            {
                                return
                                    $"-count option needs a number between 1 & {GetCatLimit}! try '.cat -help -get' for more info";
                            }
                            request.results_per_page = resultsPerPage;

                            // get xml response and deserialize it to our object
                            var response =
                                new StringReader(webClient.DownloadString(request.RequestUrl));
                            var deserializedResponse =
                                (GetResponse.response) xmlSerializer.Deserialize(response);
                            var images = deserializedResponse.data.images;

                            // build our message using our xml response
                            var messageTextLines = new List<string>
                            {
                                $"{resultsPerPage} cats requested by {command.User.NicknameMention}"
                            };
                            messageTextLines.AddRange(from image in images
                                select $"id: `{image.id}`\nurl: {image.url}");
                            return string.Join("\n", messageTextLines);
                        }
                        case "id":
                        {
                            request.image_id = parameter.Value;
                            var response = new StringReader(webClient.DownloadString(request.RequestUrl));
                            var deserializedResponse =
                                (GetResponse.response) xmlSerializer.Deserialize(response);
                            var imageData = deserializedResponse.data.images[0];
                            return $"{command.User.NicknameMention} {imageData.url}";
                        }
                        default:
                            return $"{command.User.NicknameMention} type `.cat -help -get` for correct usage";
                    }
                }

                // no count or id, just get a random cat with the optional options
                var basicResponse = new StringReader(webClient.DownloadString(new GetCat().RequestUrl));
                var deserializedBasicResponse =
                    (GetResponse.response) xmlSerializer.Deserialize(basicResponse);
                var basicImageData = deserializedBasicResponse.data.images[0];
                return $"{command.User.NicknameMention}\nid: `{basicImageData.id}`\nurl: {basicImageData.url}";
            }
        }

        private static string VoteResponse(IHunieCommand command, ILogging logger)
        {

            var request = new Vote
            {
                api_key = ApiKey,
                sub_id = command.User.Id.ToString()
            };

            // skip the first parameter "-vote"
            foreach (
                var parameter in
                    command.Parameters.Where(parameter => parameter.Key != command.Parameters.Keys.FirstOrDefault())
                )
            {
                switch (parameter.Key)
                {
                    case "id":
                        request.image_id = parameter.Value;
                        break;
                    case "score":
                        var score = 0;
                        if (!int.TryParse(parameter.Value, out score)
                            || score < 1
                            || score > 10)
                            return "invalid score! try `.cat -help -vote`";

                        request.score = score;
                        break;
                    default:
                        return
                            $"{command.User.NicknameMention} invalid option `{parameter.Key}`! try 'cat -help -vote' for more info";
                }
            }

            using (var webClient = new WebClient())
            {
                var xmlSerializer = new XmlSerializer(typeof (VoteResponse.response));
                var response = new StringReader(webClient.DownloadString(request.RequestUrl));
                var deserializedResponse = (VoteResponse.response) xmlSerializer.Deserialize(response);
                var voteData = deserializedResponse.data.votes.vote;
                var messageLines = new List<string>
                {
                    $"```{nameof(RandomCatImage)}: vote```",
                    $"```id: {voteData.image_id}",
                    $"score: {voteData.score}",
                    $"action: {voteData.action}```"
                };
                return string.Join("\n", messageLines);
            }
        }

        private static string GetVotesResponse(IHunieCommand command, ILogging logger)
        {
            var request = new GetVotes
            {
                api_key = ApiKey,
                sub_id = command.User.Id.ToString()
            };

            using (var webClient = new WebClient())
            {
                var xmlSerializer = new XmlSerializer(typeof (GetVotesResponse.response));
                var response = new StringReader(webClient.DownloadString(request.RequestUrl));
                var deserializedResponse = (GetVotesResponse.response) xmlSerializer.Deserialize(response);
                var voteData = deserializedResponse.data.images;

                var messageLines = new List<string>
                {
                    $"```{nameof(RandomCatImage)}: getvotes```",
                    $"```id: score"
                };
                messageLines.AddRange(from vote in voteData
                    orderby vote.score
                    select $"{vote.id}: {vote.score}"
                    );
                messageLines.Add("```");

                return string.Join("\n", messageLines);
            }
        }

        private static string FavouriteResponse(IHunieCommand command, ILogging logger)
        {
            var request = new Favourite
            {
                api_key = ApiKey,
                sub_id = command.User.Id.ToString()
            };

            foreach (
                var parameter in
                    command.Parameters.Where(parameter => parameter.Key != command.Parameters.Keys.FirstOrDefault())
                )
            {
                switch (parameter.Key)
                {
                    case "add":
                        request.action = parameter.Key;
                        break;
                    case "remove":
                        request.action = parameter.Key;
                        break;
                    case "id":
                        request.image_id = parameter.Key;
                        break;
                    default:
                        return $"{command.User.NicknameMention} invalid option `{parameter.Key}`! try 'cat -help -favorite' for more info";
                }
            }

            using (var webClient = new WebClient())
            {
                var response = new StringReader(webClient.DownloadString(request.RequestUrl));
                return response.ToString();
            }
        }
    }
}
