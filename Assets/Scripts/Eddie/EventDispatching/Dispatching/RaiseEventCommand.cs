using Eddie.EventDispatching.Events;
using FrostLib.Commands;
using FrostLib.Services;

namespace Eddie.EventDispatching.Dispatching
{
    public class RaiseEventCommand : ICommand
    {
        private static ServiceLocator Servicer => ServiceLocator.Instance;
        private static IEventDispatcher Dispatcher => Servicer.Get<IEventDispatcher>();

        private readonly IEvent _ev;

        public RaiseEventCommand(IEvent ev) => _ev = ev;

        public void Execute() => Dispatcher.Raise(_ev);
    }
}