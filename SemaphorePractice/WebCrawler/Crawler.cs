using System.Collections.Concurrent;

namespace SemaphorePractice.WebCrawler
{
    internal class Crawler
    {
        private Semaphore semaphore;

        public delegate Task<Page> DownloadDelegate(string url);
        public delegate Task<IEnumerable<string>> GetUrlsDelegate(Page page);

        public DownloadDelegate Download { get; set; }
        public GetUrlsDelegate GetUrls { get; set; }

        public Crawler()
        {
            semaphore = new Semaphore(10, 10);
        }

        public async Task<IDictionary<string, Page>> Crawl(IEnumerable<string> urls, CancellationToken token)
        {
            var pages = new ConcurrentDictionary<string, Page>();

            await CrawlRecursive(urls, pages, token);

            return pages;
        }

        private async Task CrawlRecursive(IEnumerable<string> urls, ConcurrentDictionary<string, Page> existingPages, CancellationToken token)
        {
            var tasks = new List<Task>();

            foreach (var url in urls)
            {
                var task = CrawlUrl(url, existingPages, token);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task CrawlUrl(string url, ConcurrentDictionary<string, Page> existingPages, CancellationToken token)
        {
            semaphore.WaitOne();

            var page = await Download(url);
            existingPages.TryAdd(url, page);

            var subUrls = await GetUrls(page);

            semaphore.Release();

            var newUrls = subUrls.Where(u => !existingPages.ContainsKey(u));
            await CrawlRecursive(newUrls, existingPages, token);
        }
    }
}
