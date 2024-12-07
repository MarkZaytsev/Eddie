using System;
using Eddie.EventDispatching.Handlers;
using Eddie.Logging;
using UnityEngine;

namespace Eddie.EventDispatching.Dispatching
{
    //HANDLERS_DEBUG
    public class Logger : IDisposable
    {
        private readonly IEventDispatcher _dispatcher;

        public Logger(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.OnRaisingEventSignal.Connect(LogRaising);

            if (!Debug.isDebugBuild)
                return;

            _dispatcher.OnHandlerCreatedSignal.Connect(LogCreation);
        }

        private static void LogRaising(Events.EventType evType) =>
            Log.Debug($"Raising: {PrintEvent(evType)}", Mode.Handlers);

        private static void LogCreation(EventHandlerBase handler, Events.EventType evType)
        {
            var message = "\t=> Handling, but handler doesn't implement IDebugInfoProvider";
            if (handler is IDebugInfoProvider debugable)
                message = $"\t=> Handling {PrintEvent(evType)}: {debugable.DebugInfo}";

            Log.Debug(message, Mode.Handlers);
        }

        private static string PrintEvent(Events.EventType evType) => evType.ToString();

        public void Dispose()
        {
            _dispatcher.OnRaisingEventSignal.Disconnect(LogRaising);

            if (!Debug.isDebugBuild)
                return;

            _dispatcher.OnHandlerCreatedSignal.Disconnect(LogCreation);
        }
    }
}