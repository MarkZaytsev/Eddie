using Eddie.EventDispatching.Binding;
using Eddie.EventDispatching.Handlers;
using FrostLib.Containers;

namespace Eddie.EventDispatching.Dispatching
{
    public class HandlersBlock : DisposableGroup
    {
        private readonly IEventDispatcher _dispatcher;

        public HandlersBlock(IEventDispatcher dispatcher) => _dispatcher = dispatcher;

        public Binder<EventHandlerBase> Bind()
        {
            var binder = _dispatcher.Bind();
            Subscribe(binder);
            return binder;
        }

        private void Subscribe(Binder<EventHandlerBase> binder)
        {
            binder.OnPropagatedSignal.Connect(Subscribe);

            Add(() =>
            {
                binder.OnPropagatedSignal.Disconnect(Subscribe);
                _dispatcher.Unbind(binder.Group).From(binder.EventType);
            });
        }
    }
}