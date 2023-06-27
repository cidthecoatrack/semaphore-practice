using Moq;
using System.Collections.Concurrent;

namespace SemaphorePractice.Tutorial
{
    public class TutorialDemoTests
    {
        private TutorialDemo demo;
        private Mock<ILogger> mockLogger;
        private BlockingCollection<string> messages;
        private ConcurrentDictionary<string, int> waits;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILogger>();
            messages = new BlockingCollection<string>();
            waits = new ConcurrentDictionary<string, int>();

            mockLogger.Setup(l => l.Log(It.IsAny<string>())).Callback((string m) => messages.TryAdd(m));
            mockLogger.Setup(l => l.LogValue(It.IsAny<string>(), It.IsAny<int>())).Callback((string m, int v) => waits.TryAdd(m, v));
        }

        [Test]
        public async Task RunFromDemo()
        {
            demo = new TutorialDemo(mockLogger.Object, 3);

            var current = 0;
            var max = 0;
            demo.Start = () =>
            {
                Interlocked.Increment(ref current);

                if (current > max)
                    max = current;
            };
            demo.End = () => Interlocked.Decrement(ref current);

            await demo.Run(10);

            var messagesArray = messages.ToArray();
            Assert.That(messagesArray[0], Is.EqualTo("Starting run with 10 tasks"));
            Assert.That(messagesArray.Last(), Is.EqualTo("Run complete"));

            Assert.That(messagesArray, Has.Length.EqualTo(42));
            Assert.That(waits, Has.Count.EqualTo(10));
            Assert.That(current, Is.Zero);
            Assert.That(max, Is.EqualTo(3));
        }

        [TestCase(1, 10)]
        [TestCase(8, 100)]
        [TestCase(10, 100)]
        [TestCase(10, 1000)]
        public async Task HeavyRun(int capacity, int threadcount)
        {
            demo = new TutorialDemo(mockLogger.Object, capacity);

            var current = 0;
            var max = 0;
            demo.Start = () =>
            {
                Interlocked.Increment(ref current);

                if (current > max)
                    max = current;
            };
            demo.End = () => Interlocked.Decrement(ref current);

            await demo.Run(threadcount);

            var messagesArray = messages.ToArray();
            Assert.That(messagesArray[0], Is.EqualTo($"Starting run with {threadcount} tasks"));
            Assert.That(messagesArray.Last(), Is.EqualTo("Run complete"));

            Assert.That(messagesArray, Has.Length.EqualTo(4 * threadcount + 2));
            Assert.That(waits, Has.Count.EqualTo(threadcount));
            Assert.That(current, Is.Zero);
            Assert.That(max, Is.AtMost(capacity));
        }
    }
}