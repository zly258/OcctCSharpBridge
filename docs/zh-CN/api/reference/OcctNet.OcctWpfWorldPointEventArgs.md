# OcctWpfWorldPointEventArgs

- **程序集:** `OcctNet.Wpf.dll`
- **命名空间:** `OcctNet`
- **继承:** `EventArgs`

## 声明

```csharp
public sealed class OcctWpfWorldPointEventArgs
```

## 说明

World-space point corresponding to a WPF viewport screen position.

## 构造函数

### `OcctWpfWorldPointEventArgs`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctWpfWorldPointEventArgs(int screenX, int screenY, OcctPoint3d worldPoint)
```

**参数**

- `screenX` — `int`
- `screenY` — `int`
- `worldPoint` — `OcctPoint3d`

## 属性

### `ScreenX`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int ScreenX { get; }
```

### `ScreenY`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int ScreenY { get; }
```

### `WorldPoint`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d WorldPoint { get; }
```

## 事件

无

## 方法

无

## 字段 / 枚举值

无

