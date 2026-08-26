# CDTU Unity Utils

English | [中文](README.zh-CN_Utils.md)

A small collection of focused Unity runtime utilities without game-specific dependencies. Copy the complete `Utils` directory into your project's `Assets` directory.

Requires Unity 2022.2 or newer.

## Modules

- `BFSUtil<T>`: breadth-first traversal, levels, shortest paths, and filtered searches.
- `EventBus`: main-thread event dispatch with mutation-safe snapshots and aggregated failures.
- `CDLogger`: strips regular logs from release builds while preserving warnings and errors.
- `MouseHelper2D/3D`: converts caller-provided screen positions without choosing an input package.
- `ObjectPool<T>`: main-thread Unity object pool with warmup, inactive capacity, and lifecycle callbacks.
- `ReadOnlyAttribute`: read-only Inspector fields.
- `Singleton<T>`: scene singleton; `SingletonDD<T>` is the persistent convenience variant.
- `CanvasGroupTween`: dependency-free `CanvasGroup` coroutine fades.

All runtime types use:

```csharp
using CDTU.Utils;
```

## Examples

### Graph search

```csharp
var search = new BFSUtil<int>();
List<int> path = search.FindShortestPath(start, end, node => graph[node]);
```

### Events

```csharp
EventBus.Subscribe<PlayerDied>(OnPlayerDied);
EventBus.Publish(new PlayerDied());
EventBus.Unsubscribe<PlayerDied>(OnPlayerDied);
```

The event bus is intentionally main-thread only.

### Pointer conversion

```csharp
Vector2 screenPosition = GetPointerPositionFromYourInputSystem();
if (MouseHelper2D.TryGetWorldPosition(screenPosition, out var worldPosition))
    transform.position = worldPosition;
```

### Fades

```csharp
StartCoroutine(CanvasGroupTween.FadeIn(canvasGroup, 0.2f));
StartCoroutine(CanvasGroupTween.FadeOut(canvasGroup, 0.2f));
```

### Singleton

```csharp
public sealed class GameSession : Singleton<GameSession>
{
    protected override bool PersistAcrossScenes => true;
}
```

See [the object pool guide](ObjectPool/README.EN_ObjectPool.md) for pool usage and lifecycle semantics.

## Boundaries

- Unity objects are created, activated, and destroyed only on the main thread.
- Pointer helpers do not cache a camera or read a global input device.
- EventBus is an in-process notification tool, not a persistence, networking, or concurrency layer.
- Utilities do not depend on game-specific types or optional third-party packages.
