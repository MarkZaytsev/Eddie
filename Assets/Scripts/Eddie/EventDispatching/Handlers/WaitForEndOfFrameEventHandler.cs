using System.Collections;
using Eddie.EventDispatching.Events;
using UnityEngine;

namespace Eddie.EventDispatching.Handlers
{
    internal class WaitForEndOfFrameEventHandler : EventHandlerBase, IRoutinedHandler
    {
        public WaitForEndOfFrameEventHandler(IEvent ev) : base(ev)
        {
        }

        public IEnumerator Handle()
        {
            yield return new WaitForEndOfFrame();
        }
    }
}