namespace Eddie.Logging
{
    public interface ILogger
    {
        void Log(string msg);
        void LogError(string msg);
    }
}