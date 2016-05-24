using Microsoft.AspNet.WebHooks;
using System.Web.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace HunieBot2.Console
{
    public class GitHubReceiver : WebHookHandler
    {
        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            string action = context.Actions.First();
            JObject data = context.GetDataOrDefault<JObject>();
            return Task.FromResult(true);
        }
    }
}