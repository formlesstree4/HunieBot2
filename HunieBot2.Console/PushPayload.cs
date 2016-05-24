namespace HunieBot2.Console
{
    public class PushPayload
    {
        public string Ref { get; set; }
        public string Head { get; set; }
        public string Before { get; set; }
        public int Size { get; set; }
        public int Distince_Size { get; set; }
        public CommitPayload[] Commits { get; set; }
    }
}
