# OcctAvaloniaViewport

- **程序集:** `OcctNet.Avalonia.dll`
- **命名空间:** `OcctNet`

Cross-platform Avalonia host for the OCCT viewer.

## 构造函数

### `OcctAvaloniaViewport`

```csharp
public OcctAvaloniaViewport()
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

### `IsEngineInitialized`

```csharp
public bool IsEngineInitialized { get; }
```

### `NativeHandle`

```csharp
public IntPtr NativeHandle { get; }
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

### `SynchronizeRenderDpi`

```csharp
public bool SynchronizeRenderDpi { get; set; }
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
public event EventHandler<OcctAvaloniaErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

```csharp
public event EventHandler<OcctAvaloniaSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

```csharp
public event EventHandler<OcctAvaloniaWorldPointEventArgs> WorldPointChanged;
```

## 方法

### `RaiseSelectionChanged`

```csharp
public void RaiseSelectionChanged()
```

### `RefreshNativeView`

```csharp
public void RefreshNativeView()
```

## 字段 / 枚举值

无。
