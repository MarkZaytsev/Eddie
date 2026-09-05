using System;
using System.Collections;
using Eddie.EventDispatching.Events;
using FrostLib.Commands;
using JetBrains.Annotations;

namespace Eddie.EventDispatching.Handlers.CommandExecution
{
    [UsedImplicitly]
    public class RoutinedCommandExecutionHandler<T> : RoutinedEventHandler where T : IRoutinedCommand
    {
        public RoutinedCommandExecutionHandler(IEvent ev) : base(ev)
        {
        }

        public override IEnumerator Handle()
        {
            yield return Activator.CreateInstance<T>().Execute();
        }
    }
}