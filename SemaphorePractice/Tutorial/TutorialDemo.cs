namespace SemaphorePractice.Tutorial
{
    internal class TutorialDemo
    {
        private Semaphore semaphore;
        private ILogger logger;

        public delegate void TaskStart();
        public delegate void TaskEnd();

        public TaskStart Start { get; set; }
        public TaskEnd End { get; set; }

        public TutorialDemo(ILogger logger, int capacity)
        {
            this.logger = logger;

            semaphore = new Semaphore(capacity, capacity);
        }

        public async Task Run(int threadCount)
        {
            logger.Log($"Starting run with {threadCount} tasks");

            var tasks = new List<Task>();

            for (int i = 1; i <= threadCount; i++)
            {
                var name = $"Task {i}";
                var task = new Task(() => DoSomeTask(name));

                tasks.Add(task);
                task.Start();
            }

            await Task.WhenAll(tasks);

            logger.Log("Run complete");
        }

        private void DoSomeTask(string name)
        {
            logger.Log($"{name} Wants to Enter into Critical Section for processing");

            try
            {
                var start = DateTime.UtcNow;
                //Blocks the current thread until the current WaitHandle receives a signal.   
                semaphore.WaitOne();

                Start();

                var elapsed = DateTime.UtcNow - start;
                logger.Log($"{name} waited {elapsed.Milliseconds}ms");
                logger.LogValue(name, elapsed.Milliseconds);

                //Decrease the Initial Count Variable by 1
                logger.Log($"Success: {name} is Doing its work");

                Thread.Sleep(100);
                logger.Log($"{name} Exit.");
            }
            finally
            {
                End();

                //Release() method to release semaphore  
                //Increase the Initial Count Variable by 1
                semaphore.Release();
            }
        }
    }
}
