using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace FrostLib.Scenes
{
    //Unsubscribing a lot of callbacks one by one from SceneManager.activeSceneChanged is expensive
    //Essentially it's find and remove from Array. Profiler proved.
    //That is why only one callback and local hashed collection is used
    public class ExecuteAfterSceneSwitchedOnce : IDisposable
    {
        private static readonly Dictionary<Guid, Action> Subs = new();
        private Guid _guid;

        static ExecuteAfterSceneSwitchedOnce() => SceneManager.activeSceneChanged += OnSceneChanged;

        public ExecuteAfterSceneSwitchedOnce()
        {
        }

        public ExecuteAfterSceneSwitchedOnce(Action action) => SetAction(action);

        public void SetAction(Action action)
        {
            _guid = Guid.NewGuid();
            Subs.Add(_guid, action);
        }

        private static void OnSceneChanged(Scene _, Scene __)
        {
            if (!Subs.Any())
                return;

            var tempSubs = Subs.Values.ToArray();
            Subs.Clear();

            foreach (var sub in tempSubs)
                sub.Invoke();
        }

        public void Dispose() => Subs.Remove(_guid);
    }
}