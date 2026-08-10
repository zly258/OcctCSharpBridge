# OcctAvaloniaViewport

- **Assembly:** `OcctNet.Avalonia.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `NativeControlHost`

## Declaration

```csharp
public sealed class OcctAvaloniaViewport
```

## Description

Reusable Avalonia host for the OCCT Windows HWND viewer. This adapter is Windows-only; it does not make the native OCCT bridge cross-platform.

## Constructors

### `OcctAvaloniaViewport`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctAvaloniaViewport()
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

## Events

### `EngineInitialized`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler EngineInitialized;
```

### `ErrorOccurred`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctAvaloniaErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctAvaloniaSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public event EventHandler<OcctAvaloniaWorldPointEventArgs> WorldPointChanged;
```

## Methods

### `RaiseSelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void RaiseSelectionChanged()
```

**Returns:** `void`

### `RefreshNativeView`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void RefreshNativeView()
```

**Returns:** `void`

## Fields / Enum Values

None

