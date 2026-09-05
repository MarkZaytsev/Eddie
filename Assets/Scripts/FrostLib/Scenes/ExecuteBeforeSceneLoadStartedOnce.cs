using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrostLib.Scenes
{
    public class ExecuteBeforeSceneLoadStartedOnce : IDisposable
    {
        private static readonly Dictionary<Guid, Action> Subs = new();
        private static bool _triggerWasExecuted;
        private Guid _guid;

        static ExecuteBeforeSceneLoadStartedOnce() => SceneManager.activeSceneChanged += OnSceneChanged;

        private static void OnSceneChanged(Scene from, Scene to)
        {
            // Loading first scene after startup
            if (string.IsNullOrEmpty(from.name))
                return;

            if (_triggerWasExecuted)
            {
                _triggerWasExecuted = false;
                return;
            }

            if (!Subs.Any())
                return;

            Debug.LogError(
                $"It seems like a scene has been loaded, but {nameof(ExecuteBeforeSceneLoadStartedOnce)} was not triggered. "
                + $"It has {Subs.Count} subscribers that just missed the notification."
                + "Make sure you call Trigger() method manually before loading a scene."
                + "Unfortunately Unity doesn't have a callback for that.");
        }

        public ExecuteBeforeSceneLoadStartedOnce()
        {
        }

        public ExecuteBeforeSceneLoadStartedOnce(Action action) => SetAction(action);

        public void SetAction(Action action)
        {
            _guid = Guid.NewGuid();
            Subs.Add(_guid, action);
        }

        public static void Trigger()
        {
            _triggerWasExecuted = true;
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