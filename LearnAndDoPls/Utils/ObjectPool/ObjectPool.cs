using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CDTU.Utils
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnRecycle();
    }

    /// <summary>
    /// Main-thread pool for Unity objects. The capacity limits retained inactive objects,
    /// not the number of objects currently leased by callers.
    /// </summary>
    public sealed class ObjectPool<T> where T : Object
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly int _maxInactive;
        private readonly bool _collectionChecks;
        private readonly int _ownerThreadId;
        private readonly Queue<T> _inactive = new Queue<T>();
        private readonly HashSet<T> _active = new HashSet<T>();

        public ObjectPool(
            T prefab,
            int initialCapacity = 10,
            int maxInactive = 0,
            Transform parent = null,
            bool collectionChecks = true)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            if (maxInactive < 0)
                throw new ArgumentOutOfRangeException(nameof(maxInactive));

            _prefab = prefab;
            _parent = parent;
            _maxInactive = maxInactive;
            _collectionChecks = collectionChecks;
            _ownerThreadId = Thread.CurrentThread.ManagedThreadId;
            Warmup(initialCapacity);
        }

        public int CountInactive
        {
            get
            {
                EnsureOwnerThread();
                return _inactive.Count;
            }
        }

        public int CountActive
        {
            get
            {
                EnsureOwnerThread();
                return _active.Count;
            }
        }

        public int CountAll
        {
            get
            {
                EnsureOwnerThread();
                return _inactive.Count + _active.Count;
            }
        }
        public int MaxInactive => _maxInactive;

        public void Warmup(int count)
        {
            EnsureOwnerThread();
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var createCount = count;
            if (_maxInactive > 0)
                createCount = Mathf.Min(createCount, _maxInactive - CountInactive);

            for (var index = 0; index < createCount; index++)
                _inactive.Enqueue(CreateInactiveObject());
        }

        public T Get()
        {
            EnsureOwnerThread();
            var instance = TakeInactiveObject();
            if (instance == null)
                instance = CreateInactiveObject();

            if (!_active.Add(instance))
                throw new InvalidOperationException("The pooled object is already active.");

            SetActive(instance, true);
            try
            {
                var poolable = GetPoolable(instance);
                if (poolable != null)
                    poolable.OnSpawn();
            }
            catch
            {
                _active.Remove(instance);
                SetActive(instance, false);
                StoreOrDestroy(instance);
                throw;
            }

            return instance;
        }

        public void Release(T instance)
        {
            EnsureOwnerThread();
            if (ReferenceEquals(instance, null))
                return;

            if (instance == null)
            {
                _active.Remove(instance);
                if (_collectionChecks)
                    throw new InvalidOperationException("The active pooled object was destroyed outside the pool.");
                return;
            }

            if (!_active.Remove(instance))
            {
                if (_collectionChecks)
                    throw new InvalidOperationException("The object does not belong to this pool or was already released.");
                return;
            }

            Exception callbackFailure = null;
            try
            {
                var poolable = GetPoolable(instance);
                if (poolable != null)
                    poolable.OnRecycle();
            }
            catch (Exception exception)
            {
                callbackFailure = exception;
            }
            finally
            {
                SetActive(instance, false);
                StoreOrDestroy(instance);
            }

            if (callbackFailure != null)
                throw new InvalidOperationException("The pooled object's OnRecycle callback failed.", callbackFailure);
        }

        /// <summary>
        /// Destroys all inactive objects. Active leases remain tracked unless destroyActive is true.
        /// </summary>
        public void Clear(bool destroyActive = false)
        {
            EnsureOwnerThread();
            while (_inactive.Count > 0)
                DestroyObject(_inactive.Dequeue());

            if (!destroyActive)
                return;

            foreach (var instance in _active)
                DestroyObject(instance);
            _active.Clear();
        }

        public bool IsActive(T instance)
        {
            EnsureOwnerThread();
            return instance != null && _active.Contains(instance);
        }

        public void GetActiveObjects(List<T> result)
        {
            EnsureOwnerThread();
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            result.Clear();
            result.AddRange(_active);
        }

        private T TakeInactiveObject()
        {
            while (_inactive.Count > 0)
            {
                var instance = _inactive.Dequeue();
                if (instance != null)
                    return instance;
            }

            return null;
        }

        private T CreateInactiveObject()
        {
            var canUseParent = _prefab is GameObject || _prefab is Component;
            var instance = _parent != null && canUseParent
                ? Object.Instantiate(_prefab, _parent)
                : Object.Instantiate(_prefab);
            SetActive(instance, false);
            return instance;
        }

        private void EnsureOwnerThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != _ownerThreadId)
                throw new InvalidOperationException("ObjectPool must be used from the thread that created it.");
        }

        private void StoreOrDestroy(T instance)
        {
            if (_maxInactive > 0 && CountInactive >= _maxInactive)
            {
                DestroyObject(instance);
                return;
            }

            _inactive.Enqueue(instance);
        }

        private static void SetActive(T instance, bool active)
        {
            if (instance is GameObject gameObject)
            {
                gameObject.SetActive(active);
                return;
            }

            if (instance is Component component)
                component.gameObject.SetActive(active);
        }

        private static IPoolable GetPoolable(T instance)
        {
            if (instance is IPoolable poolable)
                return poolable;
            if (instance is GameObject gameObject)
                return gameObject.GetComponent<IPoolable>();
            if (instance is Component component)
                return component.GetComponent<IPoolable>();

            return null;
        }

        private static void DestroyObject(T instance)
        {
            if (instance == null)
                return;

            if (instance is Component component)
                Object.Destroy(component.gameObject);
            else
                Object.Destroy(instance);
        }
    }
}
