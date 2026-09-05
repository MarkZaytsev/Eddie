using System.Collections;

namespace Eddie.EventDispatching.Handlers
{
    public interface IRoutinedHandler
    {
        IEnumerator Handle();
    }
}