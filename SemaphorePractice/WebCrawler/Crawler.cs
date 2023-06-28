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

        public Crawler(int maxConcurrency = 8)
        {
            semaphore = new Semaphore(maxConcurrency, maxConcurrency);
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
            try
            {
                //semaphore.WaitOne();

                if (PageAlreadyCrawled(existingPages, url))
                    return;

                var page = await Download(url);
                var added = existingPages.TryAdd(url, page);
                if (!added)
                    return;

                var subUrls = await GetUrls(page);
                await CrawlRecursive(subUrls, existingPages, token);

                //var newUrls = subUrls.Where(u => !PageAlreadyCrawled(existingPages, u));
                //await CrawlRecursive(newUrls, existingPages, token);
            }
            finally
            {
                //semaphore.Release();
            }
        }

        private bool PageAlreadyCrawled(ConcurrentDictionary<string, Page> existingPages, string url)
            => existingPages.TryGetValue(url, out var junk);
    }
}
