# OcctViewportSelectionEventArgs

- **程序集:** `OcctNet.WinForms.dll`
- **命名空间:** `OcctNet`
- **继承:** `EventArgs`

## 声明

```csharp
public sealed class OcctViewportSelectionEventArgs
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctViewportSelectionEventArgs`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctViewportSelectionEventArgs(IOcctObject selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
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

