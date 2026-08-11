# OcctViewportControl

- **Assembly:** `OcctNet.WinForms.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `Control`

## Declaration

```csharp
public sealed class OcctViewportControl
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctViewportControl`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctViewportControl()
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

### `RaiseSelectionChanged`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void RaiseSelectionChanged()
```

**Returns:** `void`

## Fields / Enum Values

None

