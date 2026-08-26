# CDTU SO Helper

English | [中文](README.zh-CN.md)

An Editor-only ScriptableObject quick editor. It adds an `Edit` button beside ScriptableObject reference fields and hosts the selected asset's regular or custom Inspector in a focused window.

## Install

Copy the `SOHelper` folder and its sibling `SOHelper.meta` into your project's `Assets/Editor` directory.

Requires Unity 2021.3 or newer.

## Use

No runtime integration is required:

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

Once `config` references an asset, click `Edit` to open it in the focused window. The window preserves its target and scroll position across script reloads, supports custom Inspectors, and can select and ping the source asset.

The assembly is Editor-only and is excluded from player builds.
