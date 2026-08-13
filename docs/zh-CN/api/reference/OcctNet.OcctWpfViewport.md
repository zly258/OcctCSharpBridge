# OcctWpfViewport

- **程序集:** `OcctNet.Wpf.dll`
- **命名空间:** `OcctNet`

Native WPF host for the OCCT HWND viewport. The WPF adapter owns its child HWND directly through and has no dependency on Windows Forms.

## 构造函数

### `OcctWpfViewport`

```csharp
public OcctWpfViewport()
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
public event EventHandler<OcctWpfErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

```csharp
public event EventHandler<OcctWpfSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

```csharp
public event EventHandler<OcctWpfWorldPointEventArgs> WorldPointChanged;
```

## 方法

### `FocusViewport`

```csharp
public void FocusViewport()
```

### `RaiseSelectionChanged`

```csharp
public void RaiseSelectionChanged()
```

### `RefreshNativeView`

Synchronizes the OCCT render target with the current child HWND size and coalesces presentation into one WPF render-priority callback.

```csharp
public void RefreshNativeView()
```

## 字段 / 枚举值

- `EnableDefaultInteractionProperty` — `DependencyProperty`
- `EnableRectangleSelectionProperty` — `DependencyProperty`
- `RectangleSelectionBehaviorProperty` — `DependencyProperty`
- `RectangleSelectionFillColorProperty` — `DependencyProperty`
- `RectangleSelectionFillTransparencyProperty` — `DependencyProperty`
- `RectangleSelectionLineColorProperty` — `DependencyProperty`
- `RectangleSelectionLineWidthProperty` — `DependencyProperty`
- `RectangleSelectionThresholdProperty` — `DependencyProperty`
- `SynchronizeRenderDpiProperty` — `DependencyProperty`
- `ZoomSensitivityProperty` — `DependencyProperty`

