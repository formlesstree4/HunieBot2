#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
namespace HunieBot2.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var b = new HunieBot.Host.HunieHost())
            {
                b.Configuration.DiscordToken = "MTczNjUyNDI2Mzk2NjYzODEw.CjP9Fg.vivyPtd1hKn6NZhrP0ZzHs6RZHU";
                b.Start().Wait();
                System.Console.ReadLine();
                b.Stop();
            }
        }
    }
}
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed