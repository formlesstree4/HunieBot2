﻿#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
using System.Configuration;

namespace HunieBot2.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var b = new HunieBot.Host.HunieHost())
            {
                b.Configuration.DiscordToken = ConfigurationManager.AppSettings["DiscordToken"];
                b.Start().Wait();
                System.Console.ReadLine();
                b.Stop();
            }
        }
    }
}
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed