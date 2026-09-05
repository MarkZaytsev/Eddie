using System;
using Eddie.EventDispatching.Binding;
using Eddie.EventDispatching.Events;
using Eddie.EventDispatching.Exceptions.Debugging;
using Eddie.EventDispatching.Handlers;
using FrostLib.Signals;

namespace Eddie.EventDispatching.Dispatching
{
    public interface IEventDispatcher
    {
        Signal<EventHandlerBase, EventType> OnHandlerCreatedSignal { get; }
        Signal<EventType> OnRaisingEventSignal { get; }
        Signal<Exception, ExceptionType> OnCaughtExceptionSignal { get; }
        Signal<CancellationExceptionInfo> OnCancellationExceptionSignal { get; }

        Binder<EventHandlerBase> Bind();

        IUnbinder Unbind<T>() where T : EventHandlerBase;
        IUnbinder UnbindSequenceWith<T>() where T : EventHandlerBase;
        IUnbinder Unbind(Group type);

        void Raise(IEvent ev);
    }
}