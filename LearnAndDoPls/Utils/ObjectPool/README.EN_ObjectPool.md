# ObjectPool

`ObjectPool<T>` reuses `GameObject`, `Component`, and other Unity `Object` instances. All operations must run on Unity's main thread.

## Create a pool

```csharp
var bulletPool = new ObjectPool<GameObject>(
    bulletPrefab,
    initialCapacity: 20,
    maxInactive: 50,
    parent: transform,
    collectionChecks: true);
```

- `initialCapacity`: inactive instances created during construction.
- `maxInactive`: maximum retained inactive instances; `0` means unlimited.
- `parent`: parent transform used for instantiated objects.
- `collectionChecks`: throw when an object is released twice or belongs to another pool.

The capacity limits only inactive storage. The pool never steals objects that are still leased by callers.

## Get and release

```csharp
GameObject bullet = bulletPool.Get();
bullet.transform.SetPositionAndRotation(position, rotation);
bulletPool.Release(bullet);
```

Implement `IPoolable` for lifecycle callbacks:

```csharp
public sealed class Bullet : MonoBehaviour, IPoolable
{
    public void OnSpawn() { }
    public void OnRecycle() { }
}
```

`OnSpawn` runs after activation and `OnRecycle` runs before deactivation. Callback failures do not corrupt the pool's active or inactive bookkeeping.

## Clear

```csharp
pool.Clear();                    // Destroy inactive objects; keep active leases tracked.
pool.Clear(destroyActive: true); // Destroy both inactive and active objects.
```

Do not call the pool from worker threads or destroy an active pooled instance outside the pool.
