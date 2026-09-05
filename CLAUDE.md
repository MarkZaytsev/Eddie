# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Eddie — "Event Dispatching Dependency Injections framEwork". A Unity 6 (6000.2.6f2) project containing the framework itself plus a small example. Version lives in `Assets/Scripts/Eddie/README.txt`.

Only `Assets/`, `Packages/`, `ProjectSettings/` are tracked. Root `*.csproj`/`*.sln` are Unity-generated and gitignored — never edit them; assembly layout is controlled by `.asmdef` files.

## Commands

The `unity` CLI is installed (`~/.local/bin/unity`); editor 6000.2.6f2 is at `/opt/Unity/Hub/Editor/6000.2.6f2`.

```
unity open .                 # open the project in the matching editor
unity build . --help         # batch-mode build
unity test .                 # EditMode/PlayMode tests
unity status                 # live state of connected editors
unity command ...            # execute commands in a running editor
```

There are currently **no test assemblies and no `com.unity.test-framework` entry** in `Packages/manifest.json`; `unity test` has nothing to run until both are added. Prefer the `unity-cli` skill for editor automation over hand-editing `.unity`/`.asset` YAML.

## Assembly layout

Every module has its own `.asmdef` with `autoReferenced: false` and `overrideReferences: true`. Adding a file to a module gives it no new access — a new dependency must be added as a GUID entry in the consuming `.asmdef`.

| Assembly | GUID |
| --- | --- |
| Eddie.EventDispatching | ab4dd1d36a2621f42b00b84ed9aa0c83 |
| Eddie.Logging | 6adb49be4871b094fa764c59eae51f05 |
| Eddie.Example | 422f6039de209484885b9335e5e89396 |
| FrostLib.Commands | cfa0a0afcad415343a47c701fe78ba3f |
| FrostLib.Containers | c8e94838933b2e845919dc48531ae674 |
| FrostLib.Coroutines | 929169fc5a2b154438986e818a83c805 |
| FrostLib.Extensions | 4727d604099bc7e4888bcc88df2910f3 |
| FrostLib.Scenes | e69014c04d1c5784fb145731be3e2792 |
| FrostLib.Services | b62fc2f2a104a4d4782f62625512b2f7 |
| FrostLib.Signals | 6b44886240d8f5b4ba43ff3255d46cac |
| FrostLib.Tasks | 5b60568e79820b44d9468050705bb601 |
| UniTask (package) | f51ebe6a0ceec4240a699833d6309b23 |

`Assets/Scripts/FrostLib` is the engine-agnostic support layer (services, signals, coroutines, cancellation, commands). `Assets/Scripts/Eddie` is the framework built on top of it.

## Dispatch pipeline

`IEvent` → `EventDispatcher.Raise` → matching `Group`s → per-handler instantiation → injection → `Handle`.

- **`Events/EventType.cs` is a central enum registry.** Every event kind must have a value there; the dispatcher preallocates one handler list per enum value at construction. This is the main coupling point when adding features.
- Handlers are created per raise via `Activator.CreateInstance(handlerType, ev)`, so a handler's constructor must take one parameter assignable from the event (`IEvent`, or the concrete event type as in `UrlOpenRequestHandler`).
- Immediately after construction, `ServicesInjector<EventHandlerBase>` fills **properties** (not fields) marked `[Inject]` / `[OptionalInject]`, resolving from the `ServiceLocator.Instance` singleton. Resolution walks the property type's base chain, then its interfaces, matched by optional string tag. Missing `[Inject]` logs an error and still assigns null.
- Three handler shapes, chosen by interface: `IHandler` (sync), `IRoutinedHandler` (coroutine, run through `RoutineRunner`), `ITaskHandler` (UniTask, receives a `CancellationToken`). Base classes: `EventHandler`, `RoutinedEventHandler`, `TaskEventHandler`.

## Binding

Fluent, terminated by `.To(EventType)`:

```csharp
_handlers.Bind()
    .Handler<SomeHandler>()      // also: .Command<T>() .RoutinedCommand<T>() .TaskCommand<T>()
    .Event<SomeOtherEvent>()     // raise a follow-up event
    .WaitForEndOfFrame()
    .AsSequenceAsync()           // await handlers in order instead of firing them together
    .AsSceneSwitchPersistent()   // Group.CancelOnSceneSwitch = false
    .Once()                      // unbind after first execution
    .To(EventType.Something);
```

A `Group` is the unit of binding and unbinding. Non-sequential groups fire all handlers without ordering; sequential groups await each in turn and abort the rest on cancellation. `Propagate()` clones a group to build a variant off the same chain (note: `Clone` does not copy `UnbindAfterFirstExecution`).

Bind through `HandlersBlock` (a `DisposableGroup`) rather than the dispatcher directly — disposing the block unbinds everything it created. Same pattern for services via `ServiceGroup`. See `Eddie/Example/ExampleBootstrapper.cs` for the canonical wiring: create `RoutineRunner`, `CancellationTokenFactory`, `EventDispatcher`, provide them, bind, dispose both groups in `OnDestroy`.

## Scene-switch cancellation (gotcha)

Async work is cancelled by a token from `CancellationTokenFactory`, which hangs off `FrostLib.Scenes.ExecuteBeforeSceneLoadStartedOnce`. Unity has no pre-scene-load callback, so **`ExecuteBeforeSceneLoadStartedOnce.Trigger()` must be called manually before loading a scene**; otherwise subscribers miss the notification and the class logs an error on `activeSceneChanged`.

`ICancellationTokenFactory.GetBeforeSceneLoadStartedToken()` returns an `OnTaskFinished` action — call it when the work completes to unhook, or the subscription lives until the next scene switch. The three `Execute*Once` classes deliberately share a single static `activeSceneChanged` subscription plus a dictionary, because per-callback unsubscription from Unity's event was a measured hotspot.

## Logging

`Eddie.Logging.Log` routes by `Mode`; each mode is a real logger only when its define symbol is set, otherwise a no-op `Mockup` resolved once in the static constructor. Defines: `HANDLERS_DEBUG` (dispatch tracing, `Dispatching/Logger.cs`) and `HANDLER_SEQUENCE_CANCELLATION_DEBUG` (cancellation tracing, `Exceptions/Debugging/Logger.cs`). Neither is set in `ProjectSettings.asset` — add them to `scriptingDefineSymbols` to see the output. Both `Logger` classes are constructed with the dispatcher and must be disposed.

## Notes

- `SampleScene.unity` contains only a Main Camera; `ExampleBootstrapper` is not attached to anything. The example compiles but does not run without wiring it into a scene.
- `Injections/ServiceTag.cs` holds tag constants carried over from a game project (dice/turn/score naming); they are unrelated to the framework itself.
