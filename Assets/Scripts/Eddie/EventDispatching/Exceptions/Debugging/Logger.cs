using System;
using Eddie.EventDispatching.Dispatching;
using Eddie.Logging;

namespace Eddie.EventDispatching.Exceptions.Debugging
{
    //HANDLER_SEQUENCE_CANCELLATION_DEBUG
    public class Logger : IDisposable
    {
        private readonly IEventDispatcher _dispatcher;

        public Logger(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.OnCaughtExceptionSignal.Connect(LogCaughtException);
            _dispatcher.OnCancellationExceptionSignal.Connect(LogCancellationException);
        }

        private static void LogCancellationException(CancellationExceptionInfo exceptionInfo)
        {
            var handlers = string.Join(" -> ", exceptionInfo.Group.Handlers);
            var message =
                $"Handling sequence was canceled. \nEvent: {exceptionInfo.EventType}. \nGroup: {handlers}. {exceptionInfo.Exception}";

            SendLog(message);
        }

        private static void LogCaughtException(Exception e, ExceptionType type)
        {
            var message = type switch
            {
                ExceptionType.Operation => $"Non-sequential handler canceled. \nException: {e}",
                ExceptionType.Sequence =>
                    "Sequence cancellation invoked in non-sequential handling context. Ignored.",
                ExceptionType.OperationAsync => $"Sequential handler canceled. \nException: {e}",
                ExceptionType.SequenceAsync =>
                    $"Sequential handler invoked sequence cancellation. \nException: {e}",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            SendLog(message);
        }

        private static void SendLog(string message) => Log.Debug(message, Mode.SequenceHandlers);

        public void Dispose()
        {
            _dispatcher.OnCaughtExceptionSignal.Disconnect(LogCaughtException);
            _dispatcher.OnCancellationExceptionSignal.Disconnect(LogCancellationException);
        }
    }
}