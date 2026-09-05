namespace Eddie.EventDispatching.Events
{
    public interface IEvent
    {
        EventType Type { get; }
    }
}