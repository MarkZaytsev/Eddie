# Eddie

**E**vent **D**ispatching **D**ependency **I**njections fram**E**work for Unity.

Eddie combines event bus with event handling and dipendency injections. 
Gameplay code raises an event; the framework instantiates the bound sequence of handlers, commands or other events. Injects services to handlers and runs them — either
all at once, or as an awaited sequence that can be cancelled mid-way.


Handlers - high level deicion making, "buisness logic". Example is "GameStartedEventHandler" - describes how a game should start if it depends on the some states.
Commands - concrete actions that can be excuted etiher as a reaction to an event or from other parts of the code. Concrete, stupid, doesn't make devisions, only does what is in its name (Single Responsibility Principle). Example is "StartGameCommand" - just do what is required to start the game.
Event - something has happened. "GameStartedEvent" - no follow-up is expected.
Request - something is required to happen. "StartGameRequest" - a follow-up is expected, probabbly with Respond() method callback (not necessary).

Binding is intentional to enum instead of generic type - this simplifies logic analysis since we can just look at one place and see every important event that can happen in the app. And also easily trace the usages. This approach also simplifies dependcies management between assemblies - they don't necessary need to know type of the event they are subscribing (handlers can declare just 'IEvent' in its construcor).

It's a good practice to declare a concrete event/request type in handler's consturcor to make your intensions clear.

Feel free to modify for you own liking.
Eddie is battle-tested in various projects over the years. For example in GoDice companion app.

- Version: 1.0.2
- Unity: 6000.2.6f2
- Dependency: [UniTask](https://github.com/Cysharp/UniTask)

---

## Table of contents

- [Install](#install)
- [Quick start](#quick-start)
- [Events](#events)
- [Handlers](#handlers)
- [Binding](#binding)
- [Dependency injection](#dependency-injection)
- [Sequences and cancellation](#sequences-and-cancellation)
- [Commands](#commands)
- [Round-trip requests](#round-trip-requests)
- [Lifetime](#lifetime)
- [Scene switching](#scene-switching)
- [Logging](#logging)
- [Assemblies](#assemblies)

---

## Install

Add UniTask to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
  }
}
```

Copy `Assets/Scripts/Eddie` and `Assets/Scripts/FrostLib` into your project. Every module ships its
own assembly definition with `autoReferenced: false`, so reference the ones you use explicitly from
your own `.asmdef`.

---

## Quick start

A minimal setup: register the event, write the request and its handler, wire it up.

**1. Register the event type**

```csharp
namespace Eddie.EventDispatching.Events
{
    public enum EventType
    {
        None = 0,
        OpenUrl = 1
    }
}
```

**2. Describe the event**

```csharp
internal class OpenUrlRequest : IEvent
{
    public EventType Type => EventType.OpenUrl;

    public readonly string Url;

    public OpenUrlRequest(string url) => Url = url;
}
```

**3. Handle it**

```csharp
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
```

**4. Bootstrap and bind**

```csharp
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
        _services = new ServiceGroup(Locator);

        var routineRunner = RoutineRunner.Create();
        _services.Provide<IRoutineRunner>(routineRunner);

        var tokenFactory =
            new CancellationTokenFactory(routineRunner.GetCancellationTokenOnDestroy());
        _services.Provide<ICancellationTokenFactory>(tokenFactory);

        var dispatcher = new EventDispatcher(routineRunner, tokenFactory);
        _services.Provide<IEventDispatcher>(dispatcher);
    }

    private void BindHandlers()
    {
        _handlers = new HandlersBlock(Locator.Get<IEventDispatcher>());

        _handlers.Bind()
            .Handler<UrlOpenRequestHandler>()
            .To(EventType.OpenUrl);
    }

    private void Start() =>
        Locator.Get<IEventDispatcher>().Raise(new OpenUrlRequest("google.com"));

    private void OnDestroy()
    {
        _services?.Dispose();
        _handlers?.Dispose();
    }
}
```

A runnable copy of this lives in `Assets/Scripts/Eddie/Example`, where the services are
registered straight on the locator — going through `_services` instead, as above, is what makes
`Dispose` actually unregister them.

---

## Events

An event is any class implementing `IEvent`. Its `Type` selects which bindings fire, so every event
kind needs a value in the `EventType` enum — that enum is the framework's single registry, and the
dispatcher allocates one binding list per value at construction time.

```csharp
public interface IEvent
{
    EventType Type { get; }
}
```

Events carry data as plain readonly fields. Nothing is pooled or reused: raise a fresh instance each
time.

```csharp
Locator.Get<IEventDispatcher>().Raise(new OpenUrlRequest("https://unity.com"));
```

When an event is raised with no bindings, the dispatcher logs a warning in debug builds and returns.

---

## Handlers

Handlers are created per raise, one instance per execution, via
`Activator.CreateInstance(handlerType, ev)`. The constructor takes a single parameter assignable
from the event — either `IEvent` or the concrete event type.

Pick a base class by how the work runs:

**Synchronous — `EventHandler`**

```csharp
internal class PlaySoundHandler : EventHandler
{
    [Inject] private IAudioService Audio { get; set; }

    public PlaySoundHandler(IEvent ev) : base(ev)
    {
    }

    public override void Handle() => Audio.Play(EventAs<PlaySoundRequest>().Clip);
}
```

**Coroutine — `RoutinedEventHandler`**

```csharp
internal class FadeOutHandler : RoutinedEventHandler
{
    [Inject] private IScreenFader Fader { get; set; }

    public FadeOutHandler(IEvent ev) : base(ev)
    {
    }

    public override IEnumerator Handle()
    {
        yield return Fader.FadeOut(duration: 0.3f);
    }
}
```

**Async — `TaskEventHandler`**

```csharp
internal class LoadProfileHandler : TaskEventHandler
{
    [Inject] private IProfileApi Api { get; set; }

    public LoadProfileHandler(IEvent ev) : base(ev)
    {
    }

    public override async UniTask Handle(CancellationToken cancellationToken = default)
    {
        var profile = await Api.Fetch(cancellationToken);
        Debug.Log(profile.Name);
    }
}
```

Inside a handler, read the event with `EventAs<T>()` (returns null on mismatch) or test it with
`EventIs<T>()`. A handler bound to several event types can branch on that.

---

## Binding

`HandlersBlock.Bind()` opens a fluent chain; `.To(eventType)` closes it and registers the group.

```csharp
_handlers.Bind()
    .Handler<ValidateInputHandler>()
    .Handler<ApplyMoveHandler>()
    .To(EventType.MoveRequested);
```

Everything added before `.To(...)` forms one **group**. A group is the unit of registration,
execution and unbinding.

### Chain steps

| Step | Effect |
| --- | --- |
| `.Handler<T>()` | Add a handler; `T : EventHandlerBase` |
| `.Handler(Type)` | Add a handler by runtime type |
| `.Command<T>()` | Add a handler that executes `ICommand` `T` |
| `.RoutinedCommand<T>()` | Same, for `IRoutinedCommand` |
| `.TaskCommand<T>()` | Same, for `ITaskCommand` |
| `.Event<T>()` | Raise event `T` (needs a parameterless constructor) |
| `.WaitForEndOfFrame()` | Yield one frame — only meaningful inside a sequence |

### Modifiers
a%
| Modifier | Effect |
| --- | --- |
| `.AsSequenceAsync()` | Run handlers one after another, awaiting each, instead of firing them together |
| `.AsSceneSwitchPersistent()` | Keep running across a scene switch instead of cancelling |
| `.Once()` | Unbind the group after its first execution |

```csharp
_handlers.Bind()
    .Handler<ShowSplashHandler>()
    .WaitForEndOfFrame()
    .TaskCommand<PreloadAssetsCommand>()
    .Event<GameReadyEvent>()
    .AsSequenceAsync()
    .AsSceneSwitchPersistent()
    .Once()
    .To(EventType.AppStarted);
```

Multiple groups can be bound to the same event type; each is processed independently.

### Reusing a chain with `Propagate()`

`Propagate()` clones the handlers accumulated so far into a new binder, so a shared prefix can be
sent to a second event type. Propagated binders stay owned by the same `HandlersBlock`.

```csharp
var binder = _handlers.Bind().Handler<LogRequestHandler>();
binder.To(EventType.MoveRequested);

binder.Propagate()
    .Handler<UndoMoveHandler>()
    .To(EventType.UndoRequested);
```

Note that `Once()` is not carried over by the clone.

### Unbinding

Disposing the `HandlersBlock` removes everything it bound. For finer control the dispatcher exposes:

```csharp
dispatcher.Unbind<UrlOpenRequestHandler>().From(EventType.OpenUrl);      // group of exactly this one handler
dispatcher.UnbindSequenceWith<ApplyMoveHandler>().From(EventType.Move);  // sequential group containing it
dispatcher.Unbind(group).From(EventType.Move);                           // a specific group
```

---

## Dependency injection

Services are registered in `ServiceLocator` and pulled into handlers automatically at construction.

```csharp
ServiceLocator.Instance.Provide<IProfileApi>(new ProfileApi());
ServiceLocator.Instance.Provide(playButton, ServiceTag.PlayBtn);
```

Injection targets **properties only** — fields are ignored — marked with `[Inject]` or
`[OptionalInject]`:

```csharp
internal class SubmitScoreHandler : TaskEventHandler
{
    [Inject] private IScoreApi Api { get; set; }
    [Inject(ServiceTag.TotalScoreTrackers)] private IReadOnlyList<ITracker> Trackers { get; set; }
    [OptionalInject] private IAnalytics Analytics { get; set; }

    public SubmitScoreHandler(IEvent ev) : base(ev)
    {
    }

    public override UniTask Handle(CancellationToken cancellationToken = default) =>
        Api.Submit(Trackers.Sum(t => t.Value), cancellationToken);
}
```

Resolution walks the property's declared type, then its base types, then its interfaces, each time
matching the optional string tag. A missing `[Inject]` service logs an error and leaves the property
null; a missing `[OptionalInject]` is silently skipped. Tag constants live in
`Injections/ServiceTag.cs`.

`ServiceLocator.Remove<T>()` disposes the service if it implements `IDisposable` (or is a list of
disposables), so registered services do not need manual teardown beyond removal.

---

## Sequences and cancellation

Without `.AsSequenceAsync()` all handlers in a group start together and nothing waits. With it, each
handler is awaited before the next one starts — coroutines included, wrapped into a UniTask
internally.

```csharp
_handlers.Bind()
    .Handler<CheckBalanceHandler>()
    .TaskCommand<ChargeAccountCommand>()
    .Handler<GrantItemHandler>()
    .AsSequenceAsync()
    .To(EventType.PurchaseRequested);
```

A sequence aborts when a handler throws `SequenceCanceledException`. `SequenceCancellationHandlerBase`
turns that into a declarative gate:

```csharp
internal class AbortIfOfflineHandler : SequenceCancellationHandlerBase
{
    [Inject] private INetwork Network { get; set; }

    protected override bool IsCancellationRequired => !Network.IsOnline;
    protected override string Message => "Purchase aborted: no connection.";

    public AbortIfOfflineHandler(IEvent ev) : base(ev)
    {
    }
}
```

Dropped into a sequence, it stops everything after it:

```csharp
_handlers.Bind()
    .Handler<AbortIfOfflineHandler>()
    .TaskCommand<ChargeAccountCommand>()
    .AsSequenceAsync()
    .To(EventType.PurchaseRequested);
```

Cancellations surface as signals on the dispatcher rather than as thrown exceptions at the call
site:

```csharp
dispatcher.OnCancellationExceptionSignal.Connect(info =>
    Debug.Log($"{info.EventType} cancelled: {info.Exception.Message}"));

dispatcher.OnCaughtExceptionSignal.Connect((e, type) => Debug.Log($"{type}: {e}"));
```

Available signals: `OnRaisingEventSignal`, `OnHandlerCreatedSignal`, `OnCaughtExceptionSignal`,
`OnCancellationExceptionSignal`.

---

## Commands

Commands are reusable units of work with no event context. Bind them directly instead of writing a
pass-through handler.

```csharp
internal class SaveProgressCommand : ICommand
{
    public void Execute() => PlayerPrefs.Save();
}

internal class UploadSaveCommand : ITaskCommand
{
    public UniTask Execute(CancellationToken cancellationToken = default) =>
        Backend.Upload(cancellationToken);
}

_handlers.Bind()
    .Command<SaveProgressCommand>()
    .TaskCommand<UploadSaveCommand>()
    .AsSequenceAsync()
    .To(EventType.GamePaused);
```

Commands are constructed with `Activator.CreateInstance<T>()`, so they need a parameterless
constructor and receive no injection. Anything that needs services should use a handler, or resolve
through `ServiceLocator.Instance` as `RaiseEventCommand` does.

---

## Round-trip requests

`RoundTripRequest` carries callbacks for events that need an answer.

```csharp
internal class LoadProfileRequest : RoundTripRequest<Profile>
{
    public override EventType Type => EventType.LoadProfile;

    public LoadProfileRequest(Action<Profile> ok, Action<string> err = null) : base(ok, err)
    {
    }
}
```

The handler completes it:

```csharp
internal class LoadProfileHandler : TaskEventHandler
{
    [Inject] private IProfileApi Api { get; set; }

    public LoadProfileHandler(IEvent ev) : base(ev)
    {
    }

    public override async UniTask Handle(CancellationToken cancellationToken = default)
    {
        var request = EventAs<LoadProfileRequest>();
        try
        {
            request.Ok(await Api.Fetch(cancellationToken));
        }
        catch (ApiException e)
        {
            request.Err(e.Message);
        }
    }
}
```

Caller side:

```csharp
dispatcher.Raise(new LoadProfileRequest(
    profile => Debug.Log(profile.Name),
    error => Debug.LogError(error)));
```

The non-generic `RoundTripRequest` is the same shape with `Ok()` taking no value.

---

## Lifetime

`HandlersBlock` and `ServiceGroup` are `DisposableGroup`s that undo their own registrations.

```csharp
private void OnDestroy()
{
    _services?.Dispose();   // removes (and disposes) every service provided through the group
    _handlers?.Dispose();   // unbinds every group bound through the block, propagated ones included
}
```

`DisposableGroup` is usable directly for ad-hoc cleanup:

```csharp
var group = new DisposableGroup();
group.Add(() => signal.Disconnect(OnSignal));
group.Add(() => texture.Release());
// ...
group.Dispose();
```

Note that `ServiceGroup.Provide` registers with the shared `ServiceLocator.Instance` singleton —
the group scopes teardown, not visibility.

---

## Scene switching

By default a group's async work is cancelled when a scene switch begins; `.AsSceneSwitchPersistent()`
opts out.

Unity has no callback for "a scene is about to load", so the trigger is manual. **Call
`ExecuteBeforeSceneLoadStartedOnce.Trigger()` before loading a scene:**

```csharp
ExecuteBeforeSceneLoadStartedOnce.Trigger();
SceneManager.LoadScene("Game");
```

Skipping it leaves handlers running into the new scene, and the class logs an error naming the
subscribers that missed the notification.

Long-lived async code that gets its own token should release the hook when finished:

```csharp
var (token, onTaskFinished) = _tokenFactory.GetBeforeSceneLoadStartedToken();
try
{
    await DoWork(token);
}
finally
{
    onTaskFinished?.Invoke();
}
```

Without that call the hook survives until the next scene switch — harmless, but thousands of them
slow the switch down. Skip the optimization if the token is passed to sub-tasks that outlive the
method.

---

## Logging

Two debug channels, each compiled out unless its define symbol is set in
**Project Settings → Player → Scripting Define Symbols**:

| Symbol | Output |
| --- | --- |
| `HANDLERS_DEBUG` | Every raised event and every handler created for it |
| `HANDLER_SEQUENCE_CANCELLATION_DEBUG` | Sequence cancellations and caught handler exceptions |

Both loggers attach to the dispatcher and must be constructed and disposed by the owner:

```csharp
Locator.Provide(new Eddie.EventDispatching.Dispatching.Logger(dispatcher));
Locator.Provide(new Eddie.EventDispatching.Exceptions.Debugging.Logger(dispatcher));
```

`ServiceLocator.Remove<T>()` disposes them, so a `ServiceGroup` handles teardown.

---

## Assemblies

| Assembly | Contents |
| --- | --- |
| `Eddie.EventDispatching` | Dispatcher, binding, handlers, injection attributes |
| `Eddie.Logging` | `Log`, formatters, mode gating |
| `Eddie.Example` | Runnable sample |
| `FrostLib.Services` | `ServiceLocator`, `ServiceGroup` |
| `FrostLib.Commands` | `ICommand`, `IRoutinedCommand`, `ITaskCommand` |
| `FrostLib.Coroutines` | `RoutineRunner`, interruptible coroutine wrappers |
| `FrostLib.Tasks` | `CancellationTokenFactory` |
| `FrostLib.Scenes` | `Execute*Once` scene-switch hooks |
| `FrostLib.Signals` | `Signal`, `Signal<T>`, `Signal<T, U>` |
| `FrostLib.Containers` | `DisposableGroup` |
| `FrostLib.Extensions` | Collection and enum helpers |

`FrostLib` is the general-purpose support layer and has no dependency on Eddie; `Eddie` builds on top
of it. Assembly definitions use `autoReferenced: false` and `overrideReferences: true`, so a new
dependency has to be added to the consuming `.asmdef` explicitly.
