using System;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
namespace HunieBot2.Console
{
    class Program
    {
        public const string ConfigurationFile = "conf.json";

        static void Main(string[] args) => new Program().Start();

        public void Start()
        {
            using (var hunieHost = new HunieBot.Host.HunieHost())
            {
                hunieHost.Configuration.Load(ConfigurationFile);
                if (hunieHost.Configuration.DiscordToken == string.Empty)
                {
                    System.Console.WriteLine("Do you want to set up huniebot? Y/n");
                    if (System.Console.ReadLine().ToLower() != "y")
                        return;
                    System.Console.WriteLine("Please enter your token below: ");
                    hunieHost.Configuration.DiscordToken = System.Console.ReadLine();
                    System.Console.WriteLine("Please enter your title: ");
                    hunieHost.Configuration.Game = System.Console.ReadLine();
                    System.Console.WriteLine("Please enter your preferred prefix key: ");
                    hunieHost.Configuration.CommandCharacter = Convert.ToChar(System.Console.ReadLine());
                    hunieHost.Configuration.Save(ConfigurationFile);
                }
                hunieHost.Start().Wait();
                System.Console.ReadLine();
                hunieHost.Stop();
            }
        }
    }
}
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed