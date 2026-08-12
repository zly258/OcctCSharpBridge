# OcctAvaloniaViewport

- **程序集:** `OcctNet.Avalonia.dll`
- **命名空间:** `OcctNet`
- **继承:** `NativeControlHost`

## 声明

```csharp
public sealed class OcctAvaloniaViewport
```

## 说明

Reusable Avalonia host for the OCCT Windows HWND viewer. This adapter is Windows-only; it does not make the native OCCT bridge cross-platform.

## 构造函数

### `OcctAvaloniaViewport`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctAvaloniaViewport()
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

### `IsEngineInitialized`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsEngineInitialized { get; }
```

### `NativeHandle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IntPtr NativeHandle { get; }
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

### `SynchronizeRenderDpi`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool SynchronizeRenderDpi { get; set; }
```

### `ZoomSensitivity`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double ZoomSensitivity { get; set; }
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
public event EventHandler<OcctAvaloniaErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctAvaloniaSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctAvaloniaWorldPointEventArgs> WorldPointChanged;
```

## 方法

### `RaiseSelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void RaiseSelectionChanged()
```

**返回值:** `void`

### `RefreshNativeView`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void RefreshNativeView()
```

**返回值:** `void`

## 字段 / 枚举值

无

