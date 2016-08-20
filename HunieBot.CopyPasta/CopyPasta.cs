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
    [HunieBot(nameof(CopyPasta))]
    public class CopyPasta
    {
        private Dictionary<string, string> _copyPastas;

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
                                                    

        private Dictionary<string, string> CopyPastas
        {
            get
            {
                if (_copyPastas != null) return _copyPastas;

                LoadPastas();

                return _copyPastas;
            }
        }

        [HandleEvent(CommandEvent.MessageReceived | CommandEvent.AnyMessageReceived)]
        public async Task HandlePastaEcho(IHunieMessage message, ILogging logger)
        {

            try
            {
                if (message.Message.Text.Length <= 1)
                    return;
                if (message.Message.Text[0] != PastaEchoCommandToken)
                    return;

                var pastaName = message.Message.Text.Substring(1);
                if (!CopyPastas.ContainsKey(pastaName))
                {
                    await message.Channel.SendMessage($"`{pastaName}` does not exist!");
                    return;
                }

                await message.Channel.SendMessage(CopyPastas[pastaName]);
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

                    messageLines.AddRange(CopyPastas.Keys);
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
                    if (!CopyPastas.ContainsKey(pastaName))
                    {
                        await command.Channel.SendMessage($"`{pastaName}` does not exist!");
                        return;
                    }

                    CopyPastas.Remove(pastaName);
                    SavePastas();
                    await command.Channel.SendMessage($"`{pastaName}` removed!");
                }
                else
                {
                    var pastaName = command.ParametersArray[0];
                    var pasta = command.Message.Text.Replace($".{command.Command} {pastaName}", "");

                    if (CopyPastas.ContainsKey(pasta))
                        CopyPastas[pastaName] = pasta;
                    else
                        CopyPastas.Add(pastaName, pasta);

                    SavePastas();
                    await
                        command.Channel.SendMessage(
                            $"{command.Command} saved! type `{PastaEchoCommandToken}{pastaName}` to echo it!");
                }


                //var option = command.Parameters.Keys.FirstOrDefault();

                //switch (option)
                //{
                //    case "make":
                //    {
                //        var pastaName = command.ParametersArray[1];
                //        var pasta = command.Message.Text.Replace($".{command.Command} -make {pastaName}", "");
                //        CopyPastas.Add(pastaName, pasta);
                //        SavePastas();
                //        await
                //            command.Channel.SendMessage(
                //                $"{command.Command} saved! type `.{command.Command} {pastaName}` to echo it!");
                //        return;
                //    }
                //    case "list":
                //    {
                //        var messageLines = new List<string>
                //        {
                //            $"```{nameof(CopyPasta)}: -list"
                //        };

                //        messageLines.AddRange(CopyPastas.Keys);
                //        messageLines.Add("```");

                //        await command.User.SendMessage(string.Join("\n", messageLines));
                //        return;
                //    }
                //    case "remove":
                //    {
                //        var pastaName = command.ParametersArray[1];
                //        if (!CopyPastas.ContainsKey(pastaName))
                //        {
                //            await command.User.SendMessage($"`{pastaName}` does not exist!");
                //            return;
                //        }

                //        CopyPastas.Remove(pastaName);
                //        SavePastas();
                //        await command.Channel.SendMessage($"`{pastaName}` removed!");
                //        return;
                //    }
                //    default:
                //    {
                //        if (CopyPastas.ContainsKey(command.ParametersArray[0]))
                //        {
                //            var pastaName = command.ParametersArray[0];
                //            await command.Channel.SendMessage(CopyPastas[pastaName]);
                //            return;
                //        }

                //        await command.Channel.SendMessage(HelpText);
                //        return;
                //    }
                //}
            }
            catch (Exception e)
            {
                logger.Debug($"{nameof(CopyPasta)}: {e.Message}");
                await command.Channel.SendMessage($"```{nameof(CopyPasta)} ran into an error:\n{e.Message}```");
            }
        }

        private void LoadPastas()
        {
            // todo: change this when we move to a different filestructure for modules, or leave it and keep it in %APPDATA%
            var filePath = Path.Combine(_workingDirectory, SavedPastasFileName);

            if (!File.Exists(filePath)) File.Create(filePath);
            var fileText = File.ReadAllText(filePath);
            _copyPastas = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileText) ??
                          new Dictionary<string, string>();
        }

        private void SavePastas()
        {
            // todo: change this when we move to a different filestructure for modules, or leave it and keep it in %APPDATA%
            var filePath = Path.Combine(_workingDirectory, SavedPastasFileName);

            // probably not necessary since File.WriteAllText() overwrites existing files
            // if (!File.Exists(filePath)) File.Create(filePath);
            var serializedPastas = JsonConvert.SerializeObject(CopyPastas);
            File.WriteAllText(filePath, serializedPastas);
        }
    }
}
