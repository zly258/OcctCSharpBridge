# OcctWpfViewport

- **程序集:** `OcctNet.Wpf.dll`
- **命名空间:** `OcctNet`
- **继承:** `HwndHost`

## 声明

```csharp
public sealed class OcctWpfViewport
```

## 说明

Native WPF host for the OCCT HWND viewport. The WPF adapter owns its child HWND directly through System.Windows.Interop.HwndHost and has no dependency on Windows Forms.

## 构造函数

### `OcctWpfViewport`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctWpfViewport()
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
public event EventHandler<OcctWpfErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctWpfSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctWpfWorldPointEventArgs> WorldPointChanged;
```

## 方法

### `FocusViewport`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void FocusViewport()
```

**返回值:** `void`

### `RaiseSelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void RaiseSelectionChanged()
```

**返回值:** `void`

### `RefreshNativeView`

Synchronizes the OCCT render target with the current child HWND size and coalesces presentation into one WPF render-priority callback.

```csharp
public void RefreshNativeView()
```

**返回值:** `void`

## 字段 / 枚举值

- `EnableDefaultInteractionProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `EnableRectangleSelectionProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionBehaviorProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionFillColorProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionFillTransparencyProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionLineColorProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionLineWidthProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionThresholdProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `SynchronizeRenderDpiProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `ZoomSensitivityProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

