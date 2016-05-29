using Microsoft.AspNet.WebHooks;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace HunieBot2.Console
{
    public class GitHubReceiver : WebHookHandler
    {
        public GitHubReceiver()
        {
            this.Receiver = GitHubWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            JObject data = context.GetDataOrDefault<JObject>();
            var result = JsonConvert.DeserializeObject<GitHubPayload>(data.ToString());
            System.Console.WriteLine(result);
            string action = context.Actions.First();


            return Task.FromResult(true);
        }
    }
}