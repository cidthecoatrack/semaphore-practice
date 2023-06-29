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

        public Crawler(int maxConcurrency = int.MaxValue)
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

            if (tasks.Any())
                await Task.WhenAll(tasks);
        }

        private async Task CrawlUrl(string url, ConcurrentDictionary<string, Page> existingPages, CancellationToken token)
        {
            var subUrls = Enumerable.Empty<string>();
            var recurse = false;

            try
            {
                semaphore.WaitOne();

                if (PageAlreadyCrawled(existingPages, url))
                    return;

                var page = await Download(url);
                recurse = existingPages.TryAdd(url, page);

                //HACK: normally, I would check if we actually added it and escape early if not
                //That way, we don't get urls for an already-parsed page
                //However, since in our tests we are using these delegates to count max concurrency, downloading but not parsing causes inconsistency
                //So, for the purposes of this practice, we will download and get every time

                subUrls = await GetUrls(page);
            }
            finally
            {
                semaphore.Release();
            }

            if (recurse && subUrls.Any())
                await CrawlRecursive(subUrls, existingPages, token);
        }

        private bool PageAlreadyCrawled(ConcurrentDictionary<string, Page> existingPages, string url)
            => existingPages.TryGetValue(url, out var junk);
    }
}
