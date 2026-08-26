# CDTU Unity Utils

[English](README.EN_Utils.md) | 中文

一组职责单一、没有业务依赖的 Unity 运行时工具。将整个 `Utils` 目录复制到项目的 `Assets` 下即可使用。

最低支持 Unity 2022.2。

## 模块

- `BFSUtil<T>`：BFS（广度优先搜索）遍历、层级、最短路径和条件查找。
- `EventBus`：主线程事件总线，发布期间增删监听器安全，异常会在全部监听器执行后聚合抛出。
- `CDLogger`：普通日志只在 Editor（编辑器）和 Development Build（开发构建）中保留，警告和错误始终保留。
- `MouseHelper2D/3D`：只负责屏幕坐标换算，不绑定旧输入系统或 Input System（新输入系统）。
- `ObjectPool<T>`：Unity 主线程对象池，支持预热、闲置容量限制和生命周期回调。
- `ReadOnlyAttribute`：Inspector（属性面板）只读字段。
- `Singleton<T>`：场景单例；`SingletonDD<T>` 是跨场景保留的薄封装。
- `CanvasGroupTween`：不依赖 DOTween（补间动画库）的协程渐变。

统一命名空间：

```csharp
using CDTU.Utils;
```

## 快速示例

### BFS

```csharp
var search = new BFSUtil<int>();
List<int> path = search.FindShortestPath(
    start,
    end,
    node => graph[node]);
```

`getNeighbors` 返回 `null` 时按空集合处理；节点比较可通过可选的 `IEqualityComparer<T>` 自定义。

### 事件

```csharp
EventBus.Subscribe<PlayerDied>(OnPlayerDied);
EventBus.Publish(new PlayerDied());
EventBus.Unsubscribe<PlayerDied>(OnPlayerDied);
```

EventBus 是 Unity 主线程工具，不承诺跨线程并发发布。

### 屏幕坐标

```csharp
Vector2 screenPosition = GetPointerPositionFromYourInputSystem();

if (MouseHelper2D.TryGetWorldPosition(screenPosition, out var worldPosition))
{
    transform.position = worldPosition;
}
```

输入系统由调用方决定，工具层只接收屏幕坐标，因此不会强制依赖某个输入包。

### 渐变

```csharp
StartCoroutine(CanvasGroupTween.FadeIn(canvasGroup, 0.2f));
StartCoroutine(CanvasGroupTween.FadeOut(canvasGroup, 0.2f));
```

### 单例

```csharp
public sealed class GameSession : Singleton<GameSession>
{
    protected override bool PersistAcrossScenes => true;

    protected override void OnSingletonInitialized()
    {
        // 初始化一次
    }
}
```

不需要自定义持久化策略时，也可以直接继承 `SingletonDD<T>`。

### 对象池

参见 [对象池文档](ObjectPool/README.CN_ObjectPool.md)。

## 设计约束

- Unity 对象只能在主线程创建、激活和销毁，因此对象池明确限定主线程使用。
- 鼠标工具不保存全局摄像机或输入设备；摄像机为空时才回退到 `Camera.main`。
- EventBus 适合进程内通知，不承担持久化、跨线程或网络消息职责。
- 所有工具均不依赖项目中的游戏业务类型。
