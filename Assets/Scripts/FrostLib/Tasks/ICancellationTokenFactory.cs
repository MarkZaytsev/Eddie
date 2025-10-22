using System;
using System.Threading;

namespace FrostLib.Tasks
{
    public interface ICancellationTokenFactory
    {
        (CancellationToken Token, Action OnTaskFinished) GetBeforeSceneLoadStartedToken();
        CancellationToken GetAppClosingToken();
    }
}