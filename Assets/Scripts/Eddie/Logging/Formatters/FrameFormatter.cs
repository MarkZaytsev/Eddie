using UnityEngine;

namespace Eddie.Logging.Formatters
{
    public class FrameFormatter : ILogFormatter
    {
        public string Format(string msg) => $"[F: {Time.frameCount}] {msg}";
    }
}