using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Eddie.EventDispatching.Events;
using FrostLib.Commands;
using JetBrains.Annotations;

namespace Eddie.EventDispatching.Handlers.CommandExecution
{
    [UsedImplicitly]
    public class TaskCommandExecutionHandler<T> : TaskEventHandler where T : ITaskCommand
    {
        public TaskCommandExecutionHandler(IEvent ev) : base(ev)
        {
        }

        public override UniTask Handle(CancellationToken cancellationToken = default) =>
            Activator.CreateInstance<T>().Execute(cancellationToken);
    }
}