#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
using Microsoft.Owin.Hosting;
using System.Configuration;

namespace HunieBot2.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var b = new HunieBot.Host.HunieHost())
            {
                var baseAddress = "http://localhost:1337";
                using (WebApp.Start<Startup>(baseAddress))
                {
                    System.Console.WriteLine("WebApi started at: {0}", baseAddress);
                    b.Configuration.DiscordToken = ConfigurationManager.AppSettings["DiscordToken"];
                    b.Start().Wait();
                    System.Console.ReadLine();
                    b.Stop();
                }
            }
        }
    }
}
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed