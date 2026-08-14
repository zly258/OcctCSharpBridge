# OcctAvaloniaViewport

- **Assembly:** `OcctNet.Avalonia.dll`
- **Namespace:** `OcctNet`

Cross-platform Avalonia host for the OCCT viewer.

## Constructors

### `OcctAvaloniaViewport`

```csharp
public OcctAvaloniaViewport()
```

## Properties

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

## Events

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

## Methods

### `RaiseSelectionChanged`

```csharp
public void RaiseSelectionChanged()
```

### `RefreshNativeView`

```csharp
public void RefreshNativeView()
```

## Fields / Enum Values

None.
