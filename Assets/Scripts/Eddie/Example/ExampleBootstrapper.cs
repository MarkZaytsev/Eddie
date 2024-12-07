using Cysharp.Threading.Tasks;
using Eddie.EventDispatching.Binding;
using Eddie.EventDispatching.Dispatching;
using Eddie.Example.Handlers;
using Eddie.Example.Requests;
using FrostLib.Coroutines;
using FrostLib.Services;
using FrostLib.Tasks;
using UnityEngine;

namespace Eddie.Example
{
    internal class ExampleBootstrapper : MonoBehaviour
    {
        private ServiceGroup _services;
        private HandlersBlock _handlers;

        private static ServiceLocator Locator => ServiceLocator.Instance;

        private void Awake()
        {
            ProvideServices();
            BindHandlers();
        }

        private void ProvideServices()
        {
            _services = new ServiceGroup(ServiceLocator.Instance);

            var routineRunner = RoutineRunner.Create();
            Locator.Provide<IRoutineRunner>(routineRunner);

            var cancellationTokenFactory =
                new CancellationTokenFactory(routineRunner.GetCancellationTokenOnDestroy());
            Locator.Provide<ICancellationTokenFactory>(cancellationTokenFactory);

            var dispatcher = new EventDispatcher(routineRunner, cancellationTokenFactory);
            Locator.Provide<IEventDispatcher>(dispatcher);
            Locator.Provide(new EventDispatching.Dispatching.Logger(dispatcher));
        }

        private void BindHandlers()
        {
            _handlers = new HandlersBlock(Locator.Get<IEventDispatcher>());

            _handlers.Bind()
                .Handler<UrlOpenRequestHandler>()
                .To(EventDispatching.Events.EventType.OpenUrl);
        }

        private void Start()
        {
            var dispatcher = Locator.Get<IEventDispatcher>();
            dispatcher.Raise(new OpenUrlRequest("google.com"));
        }

        private void OnDestroy()
        {
            _services?.Dispose();
            _handlers?.Dispose();
        }
    }
}