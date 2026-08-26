using UnityEngine;

namespace CDTU.Utils
{
    /// <summary>
    /// Lazy MonoBehaviour singleton intended for Unity's main thread.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (SingletonRuntimeState.IsQuitting)
                    return null;
                if (_instance != null)
                    return _instance;

                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                if (_instance != null)
                {
                    _instance.ApplyPersistence();
                    return _instance;
                }

                var singletonObject = new GameObject(typeof(T).Name);
                return singletonObject.AddComponent<T>();
            }
        }

        public static bool HasInstance => _instance != null;

        protected virtual bool PersistAcrossScenes => false;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;
            ApplyPersistence();
            OnSingletonInitialized();
        }

        protected virtual void OnDestroy()
        {
            if (_instance != this)
                return;

            _instance = null;
            OnSingletonDestroyed();
        }

        protected virtual void OnSingletonInitialized()
        {
        }

        protected virtual void OnSingletonDestroyed()
        {
        }

        private void ApplyPersistence()
        {
            if (!PersistAcrossScenes)
                return;

            if (transform.parent != null)
                transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }

    internal static class SingletonRuntimeState
    {
        public static bool IsQuitting { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            IsQuitting = false;
            Application.quitting -= MarkQuitting;
            Application.quitting += MarkQuitting;
        }

        private static void MarkQuitting()
        {
            IsQuitting = true;
        }
    }
}
