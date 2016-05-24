namespace HunieBot2.Console
{
    public class CommitPayload
    {
        public string Sha { get; set; }
        public string Path { get; set; }
        public string Author { get; set; }
        public string Since { get; set; }
        public string Until { get; set; }
    }
}
