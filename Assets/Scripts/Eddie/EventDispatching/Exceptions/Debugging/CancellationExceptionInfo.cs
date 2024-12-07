using System;
using Eddie.EventDispatching.Binding;
using Eddie.EventDispatching.Events;

namespace Eddie.EventDispatching.Exceptions.Debugging
{
    public readonly struct CancellationExceptionInfo
    {
        public readonly OperationCanceledException Exception;
        public readonly Group Group;
        public readonly EventType EventType;

        public CancellationExceptionInfo(OperationCanceledException exception, Group group, EventType eventType)
        {
            Exception = exception;
            Group = group;
            EventType = eventType;
        }
    }
}