using System;
using Eddie.EventDispatching.Events;
using Eddie.EventDispatching.Handlers;
using Eddie.EventDispatching.Handlers.CommandExecution;
using FrostLib.Commands;

namespace Eddie.EventDispatching.Binding
{
    public static class BinderExtension
    {
        public static Binder<EventHandlerBase> Command<TCommand>(
            this Binder<EventHandlerBase> binder) where TCommand : ICommand =>
            binder.Handler<CommandExecutionHandler<TCommand>>();

        public static Binder<EventHandlerBase> RoutinedCommand<TCommand>(
            this Binder<EventHandlerBase> binder)
            where TCommand : IRoutinedCommand =>
            binder.Handler<RoutinedCommandExecutionHandler<TCommand>>();

        public static Binder<EventHandlerBase> TaskCommand<TCommand>(
            this Binder<EventHandlerBase> binder) where TCommand : ITaskCommand =>
            binder.Handler<TaskCommandExecutionHandler<TCommand>>();

        public static Binder<EventHandlerBase> Event<T>(this Binder<EventHandlerBase> binder)
            where T : IEvent =>
            binder.Handler<RaiseEventHandler<T>>();

        public static Binder<EventHandlerBase> Handler<T>(this Binder<EventHandlerBase> binder)
            where T : EventHandlerBase
        {
            binder.Handler(typeof(T));
            return binder;
        }

        public static Binder<EventHandlerBase>
            WaitForEndOfFrame(this Binder<EventHandlerBase> binder) =>
            binder.Handler<WaitForEndOfFrameEventHandler>();

        public static Binder<EventHandlerBase> Handler(this Binder<EventHandlerBase> binder, Type type)
        {
            binder.Add(type);
            return binder;
        }
    }
}