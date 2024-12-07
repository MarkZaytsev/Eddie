using Eddie.EventDispatching.Events;

namespace Eddie.EventDispatching.Handlers
{
    public abstract class EventHandler : EventHandlerBase, IHandler
    {
        public abstract void Handle();

        protected EventHandler(IEvent ev) : base(ev)
        {
        }
    }
}