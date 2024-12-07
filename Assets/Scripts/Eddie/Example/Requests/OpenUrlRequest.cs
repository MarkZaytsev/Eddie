using Eddie.EventDispatching.Events;

namespace Eddie.Example.Requests
{
    internal class OpenUrlRequest : IEvent
    {
        public EventType Type => EventType.OpenUrl;

        public readonly string Url;

        public OpenUrlRequest(string url) => Url = url;
    }
}