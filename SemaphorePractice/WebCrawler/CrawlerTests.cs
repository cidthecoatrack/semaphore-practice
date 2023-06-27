namespace SemaphorePractice.WebCrawler
{
    [TestFixture]
    internal class CrawlerTests
    {
        private Crawler crawler;
        private List<Page> mockPages;
        private Dictionary<string, IEnumerable<string>> mockUrls;

        [SetUp]
        public void Setup()
        {
            mockPages = new List<Page>();
            mockUrls = new Dictionary<string, IEnumerable<string>>();

            crawler = new Crawler();
            crawler.Download = DownloadMock;
            crawler.GetUrls = GetMockUrls;
        }

        private Task<Page> DownloadMock(string url)
        {
            var mockPage = mockPages.First(p => p.Url == url);
            return Task.FromResult(mockPage);
        }

        private Task<IEnumerable<string>> GetMockUrls(Page page)
        {
            if (mockUrls.ContainsKey(page.Content))
                return Task.FromResult(mockUrls[page.Content]);

            return Task.FromResult(Enumerable.Empty<string>());
        }

        [Test]
        public async Task Get1Page()
        {
            var urls = new[] { "url1" };
            mockPages.Add(new Page("url1", "content 1"));

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(1).And.ContainKey("url1"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
        }

        [Test]
        public async Task Get2Pages()
        {
            var urls = new[] { "url1", "url2" };
            mockPages.Add(new Page("url1", "content 1"));
            mockPages.Add(new Page("url2", "content 2"));

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(2)
                .And.ContainKey("url1")
                .And.ContainKey("url2"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
            Assert.That(pages["url2"], Is.EqualTo(mockPages[1]));
        }

        [Test]
        public async Task Get1Subpage()
        {
            var urls = new[] { "url1" };
            mockPages.Add(new Page("url1", "content 1"));
            mockPages.Add(new Page("url1.1", "content 1.1"));

            mockUrls["content 1"] = new[] { "url1.1" };

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(2)
                .And.ContainKey("url1")
                .And.ContainKey("url1.1"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
            Assert.That(pages["url1.1"], Is.EqualTo(mockPages[1]));
        }

        [Test]
        public async Task Get2Subpages()
        {
            var urls = new[] { "url1" };
            mockPages.Add(new Page("url1", "content 1"));
            mockPages.Add(new Page("url1.1", "content 1.1"));
            mockPages.Add(new Page("url1.2", "content 1.2"));

            mockUrls["content 1"] = new[] { "url1.1", "url1.2" };

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(3)
                .And.ContainKey("url1")
                .And.ContainKey("url1.1")
                .And.ContainKey("url1.2"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
            Assert.That(pages["url1.1"], Is.EqualTo(mockPages[1]));
            Assert.That(pages["url1.2"], Is.EqualTo(mockPages[2]));
        }

        [Test]
        public async Task Get1Subsubpage()
        {
            var urls = new[] { "url1" };
            mockPages.Add(new Page("url1", "content 1"));
            mockPages.Add(new Page("url1.1", "content 1.1"));
            mockPages.Add(new Page("url1.1.1", "content 1.1.1"));

            mockUrls["content 1"] = new[] { "url1.1" };
            mockUrls["content 1.1"] = new[] { "url1.1.1" };

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(3)
                .And.ContainKey("url1")
                .And.ContainKey("url1.1")
                .And.ContainKey("url1.1.1"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
            Assert.That(pages["url1.1"], Is.EqualTo(mockPages[1]));
            Assert.That(pages["url1.1.1"], Is.EqualTo(mockPages[2]));
        }

        [Test]
        public async Task GetWholePageTree()
        {
            var urls = new[] { "url1", "url2" };
            mockPages.Add(new Page("url1", "content 1"));
            mockPages.Add(new Page("url1.1", "content 1.1"));
            mockPages.Add(new Page("url1.1.1", "content 1.1.1"));
            mockPages.Add(new Page("url1.1.2", "content 1.1.2"));
            mockPages.Add(new Page("url1.2", "content 1.2"));
            mockPages.Add(new Page("url1.2.1", "content 1.2.1"));
            mockPages.Add(new Page("url1.2.2", "content 1.2.2"));
            mockPages.Add(new Page("url2", "content 2"));
            mockPages.Add(new Page("url2.1", "content 2.1"));
            mockPages.Add(new Page("url2.1.1", "content 2.1.1"));
            mockPages.Add(new Page("url2.1.2", "content 2.1.2"));
            mockPages.Add(new Page("url2.2", "content 2.2"));
            mockPages.Add(new Page("url2.2.1", "content 2.2.1"));
            mockPages.Add(new Page("url2.2.2", "content 2.2.2"));

            mockUrls["content 1"] = new[] { "url1.1", "url1.2" };
            mockUrls["content 1.1"] = new[] { "url1.1.1", "url1.1.2" };
            mockUrls["content 1.2"] = new[] { "url1.2.1", "url1.2.2" };
            mockUrls["content 2"] = new[] { "url2.1", "url2.2" };
            mockUrls["content 2.1"] = new[] { "url2.1.1", "url2.1.2" };
            mockUrls["content 2.2"] = new[] { "url2.2.1", "url2.2.2" };

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(14)
                .And.ContainKey("url1")
                .And.ContainKey("url1.1")
                .And.ContainKey("url1.2")
                .And.ContainKey("url1.1.1")
                .And.ContainKey("url1.1.2")
                .And.ContainKey("url1.2.1")
                .And.ContainKey("url1.2.2")
                .And.ContainKey("url2")
                .And.ContainKey("url2.1")
                .And.ContainKey("url2.2")
                .And.ContainKey("url2.1.1")
                .And.ContainKey("url2.1.2")
                .And.ContainKey("url2.2.1")
                .And.ContainKey("url2.2.2"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
            Assert.That(pages["url1.1"], Is.EqualTo(mockPages[1]));
            Assert.That(pages["url1.2"], Is.EqualTo(mockPages[4]));
            Assert.That(pages["url1.1.1"], Is.EqualTo(mockPages[2]));
            Assert.That(pages["url1.1.2"], Is.EqualTo(mockPages[3]));
            Assert.That(pages["url1.2.1"], Is.EqualTo(mockPages[5]));
            Assert.That(pages["url1.2.2"], Is.EqualTo(mockPages[6]));
            Assert.That(pages["url2"], Is.EqualTo(mockPages[7]));
            Assert.That(pages["url2.1"], Is.EqualTo(mockPages[8]));
            Assert.That(pages["url2.2"], Is.EqualTo(mockPages[11]));
            Assert.That(pages["url2.1.1"], Is.EqualTo(mockPages[9]));
            Assert.That(pages["url2.1.2"], Is.EqualTo(mockPages[10]));
            Assert.That(pages["url2.2.1"], Is.EqualTo(mockPages[12]));
            Assert.That(pages["url2.2.2"], Is.EqualTo(mockPages[13]));
        }

        [Test]
        public async Task DoNotFollowDuplicateLink_Self()
        {
            var urls = new[] { "url1" };
            mockPages.Add(new Page("url1", "content 1"));

            mockUrls["content 1"] = new[] { "url1" };

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(1)
                .And.ContainKey("url1"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
        }

        [Test]
        public async Task DoNotFollowDuplicateLink_Child()
        {
            var urls = new[] { "url1" };
            mockPages.Add(new Page("url1", "content 1"));
            mockPages.Add(new Page("url1.1", "content 1.1"));

            mockUrls["content 1"] = new[] { "url1.1" };
            mockUrls["content 1.1"] = new[] { "url1" };

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(2)
                .And.ContainKey("url1")
                .And.ContainKey("url1.1"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
            Assert.That(pages["url1.1"], Is.EqualTo(mockPages[1]));
        }

        [Test]
        public async Task DoNotFollowDuplicateLink_Grandchild()
        {
            var urls = new[] { "url1" };
            mockPages.Add(new Page("url1", "content 1"));
            mockPages.Add(new Page("url1.1", "content 1.1"));
            mockPages.Add(new Page("url1.1.1", "content 1.1.1"));

            mockUrls["content 1"] = new[] { "url1.1" };
            mockUrls["content 1.1"] = new[] { "url1.1.1" };
            mockUrls["content 1.1.1"] = new[] { "url1" };

            var pages = await crawler.Crawl(urls, default);
            Assert.That(pages, Has.Count.EqualTo(3)
                .And.ContainKey("url1")
                .And.ContainKey("url1.1")
                .And.ContainKey("url1.1.1"));
            Assert.That(pages["url1"], Is.EqualTo(mockPages[0]));
            Assert.That(pages["url1.1"], Is.EqualTo(mockPages[1]));
            Assert.That(pages["url1.1.1"], Is.EqualTo(mockPages[2]));
        }

        [TestCase(2, 2)]
        [TestCase(2, 3)]
        [TestCase(2, 10)]
        [TestCase(3, 2)]
        [TestCase(3, 3)]
        [TestCase(3, 10)]
        [TestCase(10, 2)]
        [TestCase(10, 3)]
        [TestCase(10, 10, Ignore = "too many permutations")]
        public async Task CrawlLargeTree_IsPerformant(int breadth, int depth)
        {
            var urls = BuildLargeTree(breadth, depth);

            crawler.GetUrls = async (Page p) =>
            {
                await Task.Delay(10);
                return await GetMockUrls(p);
            };

            var start = DateTime.UtcNow;
            var pages = await crawler.Crawl(urls, default);
            var end = DateTime.UtcNow;

            double expectedCount = 0;
            while (depth > 0)
            {
                expectedCount += Math.Pow(breadth, depth);
                depth--;
            }

            Assert.That(pages, Has.Count.EqualTo(expectedCount));
            Assert.That(end - start, Is.LessThan(TimeSpan.FromMilliseconds(expectedCount * 10)));
        }

        private IEnumerable<string> BuildLargeTree(int breadth, int depth) => BuildLargeTreeRecursive(breadth, depth);

        private IEnumerable<string> BuildLargeTreeRecursive(int breadth, int depth, string prefix = "")
        {
            if (depth == 0)
                return Enumerable.Empty<string>();

            var urls = Enumerable.Range(1, breadth).Select(i => $"url{prefix}{i}").ToArray();
            var allUrls = new List<string>(urls);

            for (var i = 0; i < urls.Length; i++)
            {
                var mockPage = new Page(urls[i], $"content for {urls[i]}");
                mockPages.Add(mockPage);

                var subUrls = BuildLargeTreeRecursive(breadth, depth - 1, $"{prefix}{i}.");
                mockUrls[mockPage.Content] = subUrls;

                allUrls.AddRange(subUrls);
            }

            return urls;
        }
    }
}
