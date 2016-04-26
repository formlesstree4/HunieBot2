namespace HunieBot2.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var b = new HunieBot.Host.HunieHost())
            {
                // b.Configuration.DiscordToken = "<token here>";
                b.Start().Wait();
                System.Console.ReadLine();
                b.Stop();
            }
        }
    }
}
