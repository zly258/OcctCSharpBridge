# OcctViewportControl

- **程序集:** `OcctNet.WinForms.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctViewportControl`

```csharp
public OcctViewportControl()
```

## 属性

### `EnableDefaultInteraction`

```csharp
public bool EnableDefaultInteraction { get; set; }
```

### `EnableRectangleSelection`

```csharp
public bool EnableRectangleSelection { get; set; }
```

### `Engine`

```csharp
public OcctEngine Engine { get; }
```

### `RectangleSelectionBehavior`

```csharp
public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; }
```

### `RectangleSelectionFillColor`

```csharp
public Color RectangleSelectionFillColor { get; set; }
```

### `RectangleSelectionFillTransparency`

```csharp
public double RectangleSelectionFillTransparency { get; set; }
```

### `RectangleSelectionLineColor`

```csharp
public Color RectangleSelectionLineColor { get; set; }
```

### `RectangleSelectionLineWidth`

```csharp
public double RectangleSelectionLineWidth { get; set; }
```

### `RectangleSelectionThreshold`

```csharp
public int RectangleSelectionThreshold { get; set; }
```

### `ZoomSensitivity`

```csharp
public double ZoomSensitivity { get; set; }
```

## 事件

### `EngineInitialized`

```csharp
public event EventHandler EngineInitialized;
```

### `ErrorOccurred`

```csharp
public event EventHandler<OcctViewportErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

```csharp
public event EventHandler<OcctViewportSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

```csharp
public event EventHandler<OcctViewportWorldPointEventArgs> WorldPointChanged;
```

## 方法

### `RaiseSelectionChanged`

```csharp
public void RaiseSelectionChanged()
```

## 字段 / 枚举值

无。

