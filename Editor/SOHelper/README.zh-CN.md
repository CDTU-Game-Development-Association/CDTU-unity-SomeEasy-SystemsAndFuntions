# CDTU SO Helper

[English](README.md) | 中文

一个 Editor-only（仅编辑器）的 ScriptableObject（Unity 配置资产）快速编辑工具。它会在所有 ScriptableObject 引用字段右侧添加 `Edit` 按钮，并在独立窗口中复用该资产原有的 Inspector（属性面板）或自定义 Inspector。

## 安装

将 `SOHelper` 文件夹和同级的 `SOHelper.meta` 一起复制到项目的 `Assets/Editor` 目录。

最低支持 Unity 2021.3。

## 使用

不需要修改业务代码：

```csharp
public sealed class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyConfig config;
}

public sealed class EnemyConfig : ScriptableObject
{
    public float health;
    public float speed;
}
```

`config` 字段赋值后，Inspector 右侧会出现 `Edit`。点击后可以：

- 在单独窗口中编辑完整配置；
- 继续使用配置已有的自定义 Inspector；
- 通过 `Select` 选中并定位原资产；
- 在脚本重载后保留当前目标和滚动位置。

## 边界

- 工具只在 Unity Editor 中编译，不进入玩家构建。
- 窗口复用 Unity 自己的序列化、Undo（撤销）和脏标记流程，不额外调用 `SetDirty`。
- 同一时间复用一个快速编辑窗口；打开另一个配置会切换窗口目标。
