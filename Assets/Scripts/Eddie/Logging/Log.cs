using System.Collections.Generic;
using System.Linq;
using Eddie.Logging.Formatters;
using FrostLib.Extensions;

namespace Eddie.Logging
{
    public enum Mode
    {
        Handlers,
        SequenceHandlers
    }

    public static class Log
    {
        private static readonly Mode[] ModesToUse =
        {
#if HANDLERS_DEBUG
            Mode.Handlers,
#endif
#if HANDLER_SEQUENCE_CANCELLATION_DEBUG
            Mode.SequenceHandlers,
#endif
        };

        private static readonly Dictionary<Mode, ILogger> Loggers = new();

        static Log()
        {
            var mockup = (ILogger) new Mockup();
            var frameFormatter = new IfNotEditorFormatter(new FrameFormatter());
            var timeFormatter = new IfNotEditorFormatter(new TimeFormatter());

            foreach (var mode in Enum<Mode>.GetEnumValues())
            {
                var logger = ModesToUse.Contains(mode)
                    ? new FormattedLogger(new ILogFormatter[]
                    {
                        frameFormatter,
                        timeFormatter,
                        new IfDebugBuildFormatter(new TagFormatter($"#{mode}#"))
                    })
                    : mockup;

                Loggers.Add(mode, logger);
            }
        }

        public static void Debug(string msg, Mode mode) => Loggers[mode].Log(msg);

        public static void Error(string msg, Mode mode) => Loggers[mode].LogError(msg);
    }

    internal class Mockup : ILogger
    {
        public void Log(string msg)
        {
        }

        public void LogError(string msg)
        {
        }
    }
}