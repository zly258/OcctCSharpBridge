# OcctWpfSelectionEventArgs

- **程序集:** `OcctNet.Wpf.dll`
- **命名空间:** `OcctNet`
- **继承:** `EventArgs`

## 声明

```csharp
public sealed class OcctWpfSelectionEventArgs
```

## 说明

Selection state reported by the native WPF viewport host.

## 构造函数

### `OcctWpfSelectionEventArgs`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctWpfSelectionEventArgs(IOcctObject selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
```

**参数**

- `selectedObject` — `IOcctObject`
- `selectedObjects` — `IReadOnlyList<IOcctObject>`

## 属性

### `SelectedObject`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IOcctObject SelectedObject { get; }
```

### `SelectedObjects`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<IOcctObject> SelectedObjects { get; }
```

## 事件

无

## 方法

无

## 字段 / 枚举值

无

