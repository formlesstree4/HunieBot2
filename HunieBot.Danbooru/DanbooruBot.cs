using HunieBot.Danbooru.API;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Text;
using System.Threading.Tasks;


namespace HunieBot.Danbooru
{

    [HunieBot("BooruBot")]
    public sealed class DanbooruBot
    {
        private readonly BooruApi _api;


        public DanbooruBot()
        {
            _api = new BooruApi();
        }

        [HandleCommand(CommandEvent.AnyMessageReceived | CommandEvent.CommandReceived, UserPermissions.User, true, "booru")]
        public async Task Search(IHunieCommand command)
        {
            var p = command.Parameters;
            var tags = p["tags", "t"];
            var ccount = p["count", "c"];
            var coffset = p["page", "p"];
            int count, offset;
            if (string.IsNullOrWhiteSpace(tags) &&
                string.IsNullOrWhiteSpace(ccount) &&
                string.IsNullOrWhiteSpace(coffset))
            {
                tags = string.Join(" ", command.ParametersArray);
                ccount = "1";
                coffset = "1";
            }
            if (p.ContainsKey("sfw") && !p.ContainsKey("nsfw")) { tags += " rating:safe"; }
            if (!p.ContainsKey("sfw") && p.ContainsKey("nsfw")) { tags += " rating:explicit"; }
            if (!int.TryParse(ccount, out count)) count = 1;
            if (!int.TryParse(coffset, out offset)) offset = 1;
            if (count > 6) count = 5;
            await command.Channel.SendMessage($"{command.User.Mention}: Searching on Danbooru for tags: \"{tags}\"");
            var results = await _api.Search(count, offset, tags);
            var sb = new StringBuilder();
            if(results.Length == 0)
            {
                sb.Append($"{command.User.Mention}: No results found for tags: \"{tags}\"");
            }
            else
            {
                sb.AppendLine($"{command.User.Mention}: Results for: \"{tags}\"");
                foreach (var result in results) sb.AppendLine(result.GetDownloadUrl);
            }
            await command.Channel.SendMessage(sb.ToString());
        }
    }
}
