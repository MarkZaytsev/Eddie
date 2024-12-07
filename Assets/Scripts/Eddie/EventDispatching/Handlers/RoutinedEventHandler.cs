using System.Collections;
using Eddie.EventDispatching.Events;

namespace Eddie.EventDispatching.Handlers
{
    public abstract class RoutinedEventHandler : EventHandlerBase, IRoutinedHandler
    {
        public abstract IEnumerator Handle();

        protected RoutinedEventHandler(IEvent ev) : base(ev)
        {
        }
    }
}