using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using Newtonsoft.Json;

namespace HunieBot.CopyPasta
{
    // todo: move to sqlite database to maintain convention of persisting data storage to a sqlite database handled by HunieBot
    [HunieBot(nameof(CopyPasta))]
    public class CopyPasta
    {
        private IEnumerable<Pasta> _pastaData;

        private const char PastaEchoCommandToken = '~';
        private const string SavedPastasFileName = "copypasta.json";

        private readonly string _workingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HunieBot");

        private static readonly string HelpText = $"```{nameof(CopyPasta)}: ?```\n" +
                                                  "Manages copypasta to be echoed\n" +
                                                  "Usage:\n" +
                                                  "\t`.pasta|.meme [pasta_name] [pasta_text]` - Creates a new (or overwrites an existing) copypasta `pasta_name` with `pasta_text`\n" +
                                                  "\t`.pasta|.meme [pasta_name]` - removes copypasta with `pasta_name`\n" +
                                                  "\t`.pasta|.meme -list` - PMs you the list of currently saved copypastas\n" +
                                                  "To make me echo a copypasta, use `~pasta_name`";

        private IEnumerable<Pasta> PastaData
        {
            get
            {
                if (_pastaData != null) return _pastaData;

                var filePath = Path.Combine(_workingDirectory, SavedPastasFileName);

                if (!File.Exists(filePath)) File.Create(filePath);
                var fileText = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(fileText))
                    return new List<Pasta>();
                
                _pastaData = JsonConvert.DeserializeObject<IEnumerable<Pasta>>(fileText);

                return _pastaData;
            }
            set
            {
                _pastaData = value;
                var filePath = Path.Combine(_workingDirectory, SavedPastasFileName);
                var serializedPastas = JsonConvert.SerializeObject(_pastaData);
                File.WriteAllText(filePath, serializedPastas);
            }
        }


        [HandleEvent(CommandEvent.MessageReceived | CommandEvent.AnyMessageReceived)]
        public async Task HandlePastaEcho(IHunieMessage message, ILogging logger)
        {

            try
            {
                if (message.User.IsBot)
                    return;
                if (message.Message.Text.Length <= 1)
                    return;
                if (message.Message.Text[0] != PastaEchoCommandToken)
                    return;

                var pastaName = message.Message.Text.Substring(1);

                var pasta = PastaData.FirstOrDefault(p => p.ServerId == message.Server.Id && p.PastaName == pastaName);
                if (string.IsNullOrWhiteSpace(pasta?.PastaContent))
                { 
                    await message.Channel.SendMessage($"`{pastaName}` does not exist!");
                    return;
                }

                await message.Channel.SendMessage(pasta.PastaContent);
            }
            catch (Exception e)
            {
                logger.Debug($"{nameof(CopyPasta)}: {e.Message}");
                await message.Channel.SendMessage($"```{nameof(CopyPasta)} ran into an error:\n{e.Message}```");
            }
        }

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true,
            commands: new[] { "pasta", "meme" })]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            try
            {
                if (command.Parameters.Keys.FirstOrDefault() == "list")
                {
                    var messageLines = new List<string>
                            {
                                $"```{nameof(CopyPasta)}: -list"
                            };

                    var pastaList = from pasta in PastaData
                        where pasta.ServerId == command.Server.Id
                        select pasta.PastaContent;
                    
                    messageLines.AddRange(pastaList);
                    messageLines.Add("```");

                    await command.User.SendMessage(string.Join("\n", messageLines));
                    return;
                }

                if (command.ParametersArray.Length < 1)
                {
                    await command.Channel.SendMessage(HelpText);
                    return;
                }

                if (command.ParametersArray.Length == 1)
                {
                    if (command.ParametersArray[0] == $".{command.Command}")
                    {
                        await command.Channel.SendMessage(HelpText);
                        return;
                    }

                    var pastaName = command.ParametersArray[0];
                    var pasta = PastaData.FirstOrDefault(p => p.ServerId == command.Server.Id && p.PastaName == pastaName);
                    if (string.IsNullOrWhiteSpace(pasta?.PastaContent))
                    {
                        await command.Channel.SendMessage($"`{pastaName}` does not exist!");
                        return;
                    }

                    var pastaDataList = (List<Pasta>) PastaData;
                    pastaDataList.Remove(pasta);
                    PastaData = pastaDataList;

                    await command.Channel.SendMessage($"`{pastaName}` removed!");
                }
                else
                {
                    if (command.Server == null)
                    {
                        await command.Channel.SendMessage(
                            $"{command.Command.ToUpperInvariant()}s must be added from within a server, not through PM.");
                        return;
                    }


                    var pastaName = command.ParametersArray[0];
                    var pastaContent = command.Message.Text.Replace($".{command.Command} {pastaName}", "");
                    var pasta = new Pasta
                    {
                        ServerId = command.Server.Id,
                        PastaName = pastaName,
                        PastaContent = pastaContent
                    };

                    if (PastaData.Any(p => p.ServerId == pasta.ServerId && p.PastaName == pasta.PastaName))
                    {
                        await
                        command.Channel.SendMessage(
                            $"{command.Command} already exists!");
                        return;
                    }

                    var pastaDataList = (List<Pasta>)PastaData;
                    pastaDataList.Add(pasta);
                    PastaData = pastaDataList;

                    await
                        command.Channel.SendMessage(
                            $"{command.Command} saved! type `{PastaEchoCommandToken}{pastaName}` to echo it!");
                }
            }
            catch (Exception e)
            {
                logger.Debug($"{nameof(CopyPasta)}: {e.Message}");
                await command.Channel.SendMessage($"```{nameof(CopyPasta)} ran into an error:\n{e.Message}```");
            }
        }

        //[HandleEvent(CommandEvent.MessageReceived | CommandEvent.AnyMessageReceived)]
        //public async Task HandleMergeCommand(IHunieMessage message, ILogging logger)
        //{
        //    if (message.Message.Text != "<some randomly generated password>")
        //        return;

        //    var oldFilePath = Path.Combine(_workingDirectory, "copypasta_old.json");

        //    var oldFileText = File.ReadAllText(oldFilePath);
        //    PastaData = ConvertOldPastaToNew(oldFileText);
        //}

        /// <summary>
        /// Helper function to convert an old copypasta.json file 
        /// using a serialized Dictionary(string, string)
        /// to the new json format using the new <see cref="Pasta"/> object
        /// </summary>
        /// <param name="oldPastaJson">the old copypasta.json file in string form</param>
        /// <returns>the json string in the new format using <see cref="Pasta"/></returns>
        private IEnumerable<Pasta> ConvertOldPastaToNew(string oldPastaJson)
        {
            var serverIdList = new List<ulong> {  };

            var oldPastas = JsonConvert.DeserializeObject<Dictionary<string, string>>(oldPastaJson);
            return from serverId in serverIdList
                from oldPasta in oldPastas
                select new Pasta
                {
                    ServerId = serverId,
                    PastaName = oldPasta.Key,
                    PastaContent = oldPasta.Value
                };
        }
    }

    public class Pasta
    {
        public ulong ServerId { get; set; }
        public string PastaName { get; set; }
        public string PastaContent { get; set; }
    }
}
