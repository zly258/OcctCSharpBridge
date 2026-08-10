# OcctWpfViewport

- **Assembly:** `OcctNet.Wpf.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `UserControl`

## Declaration

```csharp
public sealed class OcctWpfViewport
```

## Description

Reusable WPF host for the OCCT HWND viewport. The native viewer remains isolated in OcctNet.OcctViewportControl, while WPF applications receive dependency properties, DPI synchronization, resize coordination, and WPF-native event routing.

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

### `WinFormsViewport`

Access to the low-level WinForms HWND host for advanced interoperability.

```csharp
public OcctViewportControl WinFormsViewport { get; }
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
public event EventHandler<OcctViewportErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctViewportSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctViewportWorldPointEventArgs> WorldPointChanged;
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

