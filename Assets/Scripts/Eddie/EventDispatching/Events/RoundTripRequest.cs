using System;

namespace Eddie.EventDispatching.Events
{
    public abstract class RoundTripRequest : IEvent
    {
        public abstract EventType Type { get; }

        private readonly Action _ok;
        private readonly Action<string> _err;

        protected RoundTripRequest(Action ok, Action<string> err = null)
        {
            _ok = ok;
            _err = err;
        }

        public void Ok() => _ok?.Invoke();

        public void Err(string message) => _err?.Invoke(message);
    }

    public abstract class RoundTripRequest<T> : IEvent
    {
        public abstract EventType Type { get; }

        private readonly Action<T> _ok;
        private readonly Action<string> _err;

        protected RoundTripRequest(Action<T> ok, Action<string> err = null)
        {
            _ok = ok;
            _err = err;
        }

        public void Ok(T response) => _ok?.Invoke(response);

        public void Err(string message) => _err?.Invoke(message);
    }
}