# OcctWpfViewport

- **Assembly:** `OcctNet.Wpf.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `HwndHost`

## Declaration

```csharp
public sealed class OcctWpfViewport
```

## Description

Native WPF host for the OCCT HWND viewport. The WPF adapter owns its child HWND directly through System.Windows.Interop.HwndHost and has no dependency on Windows Forms.

## Constructors

### `OcctWpfViewport`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctWpfViewport()
```

## Properties

### `EnableDefaultInteraction`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool EnableDefaultInteraction { get; set; }
```

### `EnableRectangleSelection`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool EnableRectangleSelection { get; set; }
```

### `Engine`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctEngine Engine { get; }
```

### `IsEngineInitialized`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsEngineInitialized { get; }
```

### `NativeHandle`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IntPtr NativeHandle { get; }
```

### `RectangleSelectionBehavior`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; }
```

### `RectangleSelectionFillColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public Color RectangleSelectionFillColor { get; set; }
```

### `RectangleSelectionFillTransparency`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double RectangleSelectionFillTransparency { get; set; }
```

### `RectangleSelectionLineColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public Color RectangleSelectionLineColor { get; set; }
```

### `RectangleSelectionLineWidth`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double RectangleSelectionLineWidth { get; set; }
```

### `RectangleSelectionThreshold`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int RectangleSelectionThreshold { get; set; }
```

### `SynchronizeRenderDpi`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool SynchronizeRenderDpi { get; set; }
```

### `ZoomSensitivity`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double ZoomSensitivity { get; set; }
```

## Events

### `EngineInitialized`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler EngineInitialized;
```

### `ErrorOccurred`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctWpfErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctWpfSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctWpfWorldPointEventArgs> WorldPointChanged;
```

## Methods

### `FocusViewport`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void FocusViewport()
```

**Returns:** `void`

### `RaiseSelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void RaiseSelectionChanged()
```

**Returns:** `void`

### `RefreshNativeView`

Synchronizes the OCCT render target with the current child HWND size and coalesces presentation into one WPF render-priority callback.

```csharp
public void RefreshNativeView()
```

**Returns:** `void`

## Fields / Enum Values

- `EnableDefaultInteractionProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `EnableRectangleSelectionProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `RectangleSelectionBehaviorProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `RectangleSelectionFillColorProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `RectangleSelectionFillTransparencyProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `RectangleSelectionLineColorProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `RectangleSelectionLineWidthProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `RectangleSelectionThresholdProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `SynchronizeRenderDpiProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.
- `ZoomSensitivityProperty` — `DependencyProperty` — Public API member. Exact parameters, return type, and available XML documentation are listed below.

