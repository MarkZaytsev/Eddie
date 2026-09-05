using System;
using System.Threading;

namespace FrostLib.Tasks
{
    public interface ICancellationTokenFactory
    {
        // Call OnTaskFinished or the hook will persist until scene is switched.
        // It will not cause errors, but having tons of hooks dispatched,
        // will slow down scene switch and cause memalloc spike.
        // This is optimization mostly for extensively called handlers.
        // Like any burst mode related handlers.
        // With this optimization, CancellationToken will not be canceled
        // after main body of Handler/Task has completed. 
        // Don't use this optimization if you pass the token to sub-tasks
        // that you don't await in main method.
        (CancellationToken Token, Action OnTaskFinished) GetBeforeSceneLoadStartedToken();

        CancellationToken GetAppClosingToken();
    }
}