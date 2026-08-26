# ObjectPool（对象池）

`ObjectPool<T>` 用于复用 `GameObject`、`Component` 或其他 Unity `Object`。所有操作必须在 Unity 主线程执行。

## 创建

```csharp
var bulletPool = new ObjectPool<GameObject>(
    bulletPrefab,
    initialCapacity: 20,
    maxInactive: 50,
    parent: transform,
    collectionChecks: true);
```

- `initialCapacity`：构造时预创建的闲置对象数量。
- `maxInactive`：最多保留多少闲置对象；`0` 表示不限制。
- `parent`：实例化对象的父节点。
- `collectionChecks`：重复归还或归还外部对象时是否抛出异常。

容量只限制闲置缓存，不会抢占仍被调用方使用的活跃对象。

## 获取与归还

```csharp
GameObject bullet = bulletPool.Get();
bullet.transform.SetPositionAndRotation(position, rotation);

bulletPool.Release(bullet);
```

实现 `IPoolable` 后可以接收生命周期回调：

```csharp
public sealed class Bullet : MonoBehaviour, IPoolable
{
    public void OnSpawn()
    {
        // 重置本次使用状态
    }

    public void OnRecycle()
    {
        // 解绑或停止本次使用资源
    }
}
```

`OnSpawn` 在对象激活后调用，`OnRecycle` 在对象停用前调用。回调异常不会破坏池内部的活跃/闲置状态。

## 清理

```csharp
pool.Clear();                  // 只销毁闲置对象，活跃对象继续被追踪
pool.Clear(destroyActive: true); // 同时销毁活跃对象
```

不要从后台线程调用对象池，也不要在池外直接销毁仍处于活跃状态的实例。
