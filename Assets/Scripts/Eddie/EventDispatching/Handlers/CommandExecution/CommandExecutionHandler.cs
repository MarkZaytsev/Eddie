using System;
using Eddie.EventDispatching.Events;
using FrostLib.Commands;
using JetBrains.Annotations;

namespace Eddie.EventDispatching.Handlers.CommandExecution
{
    [UsedImplicitly]
    public class CommandExecutionHandler<T> : EventHandler where T : ICommand
    {
        public CommandExecutionHandler(IEvent ev) : base(ev)
        {
        }

        public override void Handle() => Activator.CreateInstance<T>().Execute();
    }
}