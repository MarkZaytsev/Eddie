using System;
using System.Collections;
using FrostLib.Scenes;

namespace FrostLib.Coroutines
{
    internal class StopBeforeSceneLoadStarted
    {
        private readonly InterruptWrapper _wrapper;
        private readonly ExecuteBeforeSceneLoadStartedOnce _executor;

        public StopBeforeSceneLoadStarted(IEnumerator enumerator, Action stopCallback)
        {
            _executor = new ExecuteBeforeSceneLoadStartedOnce(OnSceneChanged);
            _wrapper = new InterruptWrapper(enumerator, stopCallback, _executor.Dispose);
        }

        public IEnumerator Start() => _wrapper.Start();

        private void OnSceneChanged() => _wrapper.Stop();
    }
}