# OcctViewportControl

- **程序集:** `OcctNet.WinForms.dll`
- **命名空间:** `OcctNet`
- **继承:** `Control`

## 声明

```csharp
public sealed class OcctViewportControl
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctViewportControl`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctViewportControl()
```

## 属性

### `EnableDefaultInteraction`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool EnableDefaultInteraction { get; set; }
```

### `EnableRectangleSelection`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool EnableRectangleSelection { get; set; }
```

### `Engine`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctEngine Engine { get; }
```

### `RectangleSelectionBehavior`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; }
```

### `RectangleSelectionFillColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public Color RectangleSelectionFillColor { get; set; }
```

### `RectangleSelectionFillTransparency`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RectangleSelectionFillTransparency { get; set; }
```

### `RectangleSelectionLineColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public Color RectangleSelectionLineColor { get; set; }
```

### `RectangleSelectionLineWidth`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RectangleSelectionLineWidth { get; set; }
```

### `RectangleSelectionThreshold`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int RectangleSelectionThreshold { get; set; }
```

## 事件

### `EngineInitialized`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler EngineInitialized;
```

### `ErrorOccurred`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctViewportErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctViewportSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctViewportWorldPointEventArgs> WorldPointChanged;
```

## 方法

### `RaiseSelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void RaiseSelectionChanged()
```

**返回值:** `void`

## 字段 / 枚举值

无

