using System.Collections.Generic;

namespace HunieBot.WikipediaSearch
{
    internal class WikipediaResponse
    {
        public WikipediaQuery Query { get; set; }
    }

    internal class WikipediaQuery
    {
        public Dictionary<string, WikipediaPage> Pages { get; set; }
    }

    internal class WikipediaPage
    {
        public string Extract { get; set; }
        public string Title { get; set; }
    }
}
