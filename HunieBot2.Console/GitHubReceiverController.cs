using System.Web.Http;

namespace HunieBot2.Console
{
    public class GitHubReceiverController : ApiController
    {
        [HttpPost]
        [Route("api/webhooks/incoming/github")]
        public void Catch(PushPayload payload)
        {
            System.Console.WriteLine(payload.ToString());
        }
    }
}