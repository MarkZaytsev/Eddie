using System.Threading;
using Cysharp.Threading.Tasks;

namespace Eddie.EventDispatching.Handlers
{
    public interface ITaskHandler
    {
        UniTask Handle(CancellationToken cancellationToken = default);
    }
}