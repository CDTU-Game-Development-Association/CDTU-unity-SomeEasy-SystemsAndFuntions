using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDTU.Utils
{
    /// <summary>
    /// Main-thread event hub. Listener mutations made during Publish affect the next publish.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, HashSet<Delegate>> Listeners =
            new Dictionary<Type, HashSet<Delegate>>();

        public static void Subscribe<TEvent>(Action<TEvent> listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            var eventType = typeof(TEvent);
            if (!Listeners.TryGetValue(eventType, out var eventListeners))
            {
                eventListeners = new HashSet<Delegate>();
                Listeners.Add(eventType, eventListeners);
            }

            eventListeners.Add(listener);
        }

        public static bool Unsubscribe<TEvent>(Action<TEvent> listener)
        {
            if (listener == null)
                return false;

            var eventType = typeof(TEvent);
            if (!Listeners.TryGetValue(eventType, out var eventListeners))
                return false;

            var removed = eventListeners.Remove(listener);
            if (eventListeners.Count == 0)
                Listeners.Remove(eventType);

            return removed;
        }

        public static void Publish<TEvent>(TEvent message)
        {
            if (!Listeners.TryGetValue(typeof(TEvent), out var eventListeners))
                return;

            var snapshot = new Delegate[eventListeners.Count];
            eventListeners.CopyTo(snapshot);
            List<Exception> failures = null;

            foreach (var listener in snapshot)
            {
                try
                {
                    ((Action<TEvent>)listener).Invoke(message);
                }
                catch (Exception exception)
                {
                    if (failures == null)
                        failures = new List<Exception>();
                    failures.Add(exception);
                }
            }

            if (failures != null)
                throw new AggregateException("One or more event listeners failed.", failures);
        }

        public static void Clear<TEvent>()
        {
            Listeners.Remove(typeof(TEvent));
        }

        public static void ClearAll()
        {
            Listeners.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetBeforeSceneLoad()
        {
            Listeners.Clear();
        }
    }
}
