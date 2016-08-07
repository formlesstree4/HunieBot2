#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
namespace HunieBot2.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var honieHost = new HunieBot.Host.HunieHost())
            {
                honieHost.Configuration.DiscordToken = "MjExNjkzNjkyNjQ4NTU0NDk3.CohCqQ.3e_F8DkpTqtHenCpHEhuaCM_TNA";
                honieHost.Configuration.Game = "I can finally have my own game!";
                honieHost.Start().Wait();
                System.Console.ReadLine();
                honieHost.Stop();
            }
        }
    }
}
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed