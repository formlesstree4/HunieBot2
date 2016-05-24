using Owin;
using System.Web.Http;

namespace HunieBot2.Console
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.InitializeReceiveGitHubWebHooks();
            appBuilder.UseWebApi(config);
        }
    }
}