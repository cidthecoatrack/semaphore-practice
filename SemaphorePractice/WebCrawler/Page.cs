namespace SemaphorePractice.WebCrawler
{
    internal class Page
    {
        public string Url { get; set; }
        public string Content { get; set; }

        public Page(string url, string content)
        {
            Url = url;
            Content = content;
        }
    }
}
