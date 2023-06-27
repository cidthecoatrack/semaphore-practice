namespace SemaphorePractice
{
    public interface ILogger
    {
        void Log(string message);
        void LogValue(string message, int value);
    }
}
