using Eddie.EventDispatching.Events;

namespace Eddie.EventDispatching.Binding
{
    public interface IUnbinder
    {
        void From(EventType eventType);
    }
}