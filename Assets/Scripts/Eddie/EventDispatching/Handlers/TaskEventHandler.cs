using System.Threading;
using Cysharp.Threading.Tasks;
using Eddie.EventDispatching.Events;

namespace Eddie.EventDispatching.Handlers
{
    public abstract class TaskEventHandler : EventHandlerBase, ITaskHandler
    {
        protected TaskEventHandler(IEvent ev) : base(ev)
        {
        }

        public abstract UniTask Handle(CancellationToken cancellationToken = default);
    }
}