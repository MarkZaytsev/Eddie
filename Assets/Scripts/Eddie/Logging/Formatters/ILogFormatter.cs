namespace Eddie.Logging.Formatters
{
    public interface ILogFormatter
    {
        string Format(string msg);
    }
}