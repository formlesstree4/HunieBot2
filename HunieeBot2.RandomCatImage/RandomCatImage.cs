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
    // todo: get all the error responses from thecatapi somehow (not documented)
    [HunieBot(nameof(RandomCatImage))]
    public class RandomCatImage
    {

        private const string ApiKey = "[get your own api key!]";
        private const int GetCatLimit = 5;

        private static Dictionary<string, string> SupportedCatCommands
            => new Dictionary<string, string>
            {
                {"-help", ":grey_exclamation:`-help [-command command]` - gets help on commands and options"},
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
                {"-vote", ":grey_exclamation:`-vote [id] [score]` - vote on a cat image with a score"},
                {"-getvotes", ":grey_exclamation:`-getvotes` - gets all your different votes"},
                {
                    "-favorite",
                    ":grey_exclamation:`-favorite [add | remove] [id]` - [add] or [remove] an image [id] from your favorites list"
                },
                {"-getfavorites", ":grey_exclamation:`-getfavorites` - gets all your favorite images"},
                //{ unsupported by api currently
                //    "-report",
                //    ":grey_exclamation:`-report [id] [\"reason\"]` - reports an image [id] with a [reason]. this will preveent this image from showing again"
                //},
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
                        await command.User.SendMessage(
                            $"```{nameof(RandomCatImage)}: help```\n" + 
                            HelpResponse(command, logger));
                        return;
                    case "get":
                        await command.Channel.SendMessage(
                            $"```{nameof(RandomCatImage)}: get```\n" + 
                            GetResponse(command, logger));
                        return;
                    case "vote":
                        await command.User.SendMessage(
                            $"```{nameof(RandomCatImage)}: vote```\n" + 
                            VoteResponse(command, logger));
                        return;
                    case "getvotes":
                        await command.User.SendMessage(
                            $"```{nameof(RandomCatImage)}: getvotes```\n" + 
                            GetVotesResponse(command, logger));
                        return;
                    case "favorite":
                        await command.User.SendMessage(
                            $"```{nameof(RandomCatImage)}: favorite```\n" + 
                            FavouriteResponse(command, logger));
                        return;
                    case "getfavorites":
                        await command.User.SendMessage(
                            $"```{nameof(RandomCatImage)}: getfavorites```\n" + 
                            GetFavouritesResponse(command, logger));
                        return;
                    //case "report": unsupported by api currently
                    //    await command.User.SendMessage
                    //        ($"```{nameof(RandomCatImage)}: report```\n" +
                    //        ReportResponse(command, logger));
                    //    break;
                    case "categories":
                        await command.Channel.SendMessage(
                            $"```{nameof(RandomCatImage)}: categories```\n" +
                            CategoriesResponse(command, logger));
                        return;
                    case "stats":
                        await command.Channel.SendMessage(
                            $"```{nameof(RandomCatImage)}: stats```\n" +
                            StatsResponse(command, logger));
                        return;
                    default:
                        await command.Channel.SendMessage(
                            $"```{nameof(RandomCatImage)}: invalid command, try `cat -help```");
                        return;
                }
            }
            catch (Exception e)
            {
                logger.Debug($"{nameof(RandomCatImage)}: {e.Message}");
                await command.Channel.SendMessage($"```{nameof(RandomCatImage)} ran into an error:\n{e.Message}```");
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

                using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                {
                    var deserializedResponse =
                        (GetResponse.response) xmlSerializer.Deserialize(response);
                    var imageData = deserializedResponse.data.images[0];
                    return $"{command.User.NicknameMention}\nid: `{imageData.id}`\nurl: {imageData.url}";
                }
            }
        }

        private static string HelpResponse(IHunieCommand command, ILogging logger)
        {
            // just '?'
            if (command.ParametersArray.Length == 1)
            {
                var messageLines = new List<string>
                {
                    "Get kitties, vote on kitties, favorite kitties from thecatapi.com",
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
                            using (var response =
                                new StringReader(webClient.DownloadString(request.RequestUrl)))
                            {
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
                        }
                        case "id":
                        {
                            request.image_id = parameter.Value;
                            using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                            {
                                var deserializedResponse =
                                    (GetResponse.response) xmlSerializer.Deserialize(response);
                                var imageData = deserializedResponse.data.images[0];
                                return $"{command.User.NicknameMention} {imageData.url}";
                            }
                        }
                        default:
                            return $"{command.User.NicknameMention} type `.cat -help -get` for correct usage";
                    }
                }

                // no count or id, just get a random cat with the optional options
                using (var basicResponse = new StringReader(webClient.DownloadString(new GetCat().RequestUrl)))
                {
                    var deserializedBasicResponse =
                        (GetResponse.response) xmlSerializer.Deserialize(basicResponse);
                    var basicImageData = deserializedBasicResponse.data.images[0];
                    return $"{command.User.NicknameMention}\nid: `{basicImageData.id}`\nurl: {basicImageData.url}";
                }
            }
        }

        private static string VoteResponse(IHunieCommand command, ILogging logger)
        {
            var request = new Vote
            {
                api_key = ApiKey,
                sub_id = command.User.Id.ToString(),
                image_id = command.ParametersArray[1]
            };

            var score = 0;
            if (!int.TryParse(command.ParametersArray[2], out score)
                            || score < 1
                            || score > 10)
                return "invalid score! try `.cat -help -vote`";
            request.score = score;

            using (var webClient = new WebClient())
            {
                var xmlSerializer = new XmlSerializer(typeof (VoteResponse.response));
                using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                {
                    var deserializedResponse = (VoteResponse.response) xmlSerializer.Deserialize(response);
                    var voteData = deserializedResponse.data.votes.vote;
                    var messageLines = new List<string>
                    {
                        $"```id: {voteData.image_id}",
                        $"score: {voteData.score}",
                        $"action: {voteData.action}```"
                    };
                    return string.Join("\n", messageLines);
                }
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
                using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                {
                    var deserializedResponse = (GetVotesResponse.response) xmlSerializer.Deserialize(response);
                    var voteData = deserializedResponse.data.images;

                    var messageLines = new List<string>
                    {
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
        }

        private static string FavouriteResponse(IHunieCommand command, ILogging logger)
        {
            var request = new Favourite
            {
                api_key = ApiKey,
                sub_id = command.User.Id.ToString(),
                action = command.ParametersArray[1],
                image_id = command.ParametersArray[2]
            };

            using (var webClient = new WebClient())
            {

                using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                {
                    var messageText = new List<string>
                    {
                        $"{request.image_id} added to favorites list"
                    };
                    return string.Join("\n", messageText);
                }
            }
        }

        private static string GetFavouritesResponse(IHunieCommand command, ILogging logger)
        {
            var request = new GetFavourites
            {
                api_key = ApiKey,
                sub_id = command.User.Id.ToString()
            };

            using (var webClient = new WebClient())
            {
                var xmlSerializer = new XmlSerializer(typeof(GetFavouritesResponse.response));
                using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                {
                    var deserializedResponse = (GetFavouritesResponse.response) xmlSerializer.Deserialize(response);
                    var images = deserializedResponse.data.images;

                    var messageLines = new List<string>
                    {
                        $"```id: date"
                    };
                    messageLines.AddRange(from image in images
                                          select $"{image.id}: {DateTime.Parse(image.created).ToUniversalTime()}");
                    messageLines.Add("```");


                    return string.Join("\n", messageLines);
                }
            }
        }

        private static string ReportResponse(IHunieCommand command, ILogging logger)
        {
            var request = new Report
            {
                api_key = ApiKey,
                sub_id = command.User.Id.ToString(),
                image_id = command.ParametersArray[1],
                reason = command.ParametersArray[2]
            };

            using (var webClient = new WebClient())
            {
                //var xmlSerializer = new XmlSerializer(typeof(GetFavouritesResponse.response));
                using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                {
                    return webClient.DownloadString(request.RequestUrl);
                }
            }
        }

        private static string CategoriesResponse(IHunieCommand command, ILogging logger)
        {
            var request = new Categories();
            using (var webClient = new WebClient())
            {
                var xmlSerializer = new XmlSerializer(typeof(CategoriesResponse.response));
                using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                {
                    var deserializedResponse = (CategoriesResponse.response) xmlSerializer.Deserialize(response);
                    var categories = deserializedResponse.data.categories;

                    var messageLines = new List<string>
                    {
                        $"{command.User.NicknameMention}",
                        "```id: name"
                    };
                    messageLines.AddRange(from category in categories
                                          select $"{category.id}: {category.name}");
                    messageLines.Add("```");

                    return string.Join("\n", messageLines);
                }
            }
        }

        private static string StatsResponse(IHunieCommand command, ILogging logger)
        {
            var request = new Stats
            {
                api_key = ApiKey
            };

            using (var webClient = new WebClient())
            {
                var xmlSerializer = new XmlSerializer(typeof(StatsResponse.response));
                using (var response = new StringReader(webClient.DownloadString(request.RequestUrl)))
                {
                    var deserializedResponse = (StatsResponse.response)xmlSerializer.Deserialize(response);
                    var stats = deserializedResponse.data.stats.statsoverview;

                    var messageLines = new List<string>
                    {
                        $"{command.User.NicknameMention}",
                        $"total favorites: {stats.total_favourites}",
                        $"total cat requests: {stats.total_get_requests}",
                        $"total votes: {stats.total_votes}",
                    };

                    return string.Join("\n", messageLines);
                }
            }
        }
    }
}
