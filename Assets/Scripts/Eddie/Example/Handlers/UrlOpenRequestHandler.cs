using Eddie.EventDispatching.Handlers;
using Eddie.Example.Requests;
using JetBrains.Annotations;
using UnityEngine;

namespace Eddie.Example.Handlers
{
    [UsedImplicitly]
    internal class UrlOpenRequestHandler : EventHandler
    {
        public UrlOpenRequestHandler(OpenUrlRequest ev) : base(ev)
        {
        }

        public override void Handle()
        {
            var ev = EventAs<OpenUrlRequest>();
            Application.OpenURL(ev.Url);
        }
    }
}