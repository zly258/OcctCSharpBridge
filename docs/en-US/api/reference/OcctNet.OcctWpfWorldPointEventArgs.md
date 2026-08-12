# OcctWpfWorldPointEventArgs

- **Assembly:** `OcctNet.Wpf.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `EventArgs`

## Declaration

```csharp
public sealed class OcctWpfWorldPointEventArgs
```

## Description

World-space point corresponding to a WPF viewport screen position.

## Constructors

### `OcctWpfWorldPointEventArgs`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctWpfWorldPointEventArgs(int screenX, int screenY, OcctPoint3d worldPoint)
```

**Parameters**

- `screenX` — `int`
- `screenY` — `int`
- `worldPoint` — `OcctPoint3d`

## Properties

### `ScreenX`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int ScreenX { get; }
```

### `ScreenY`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int ScreenY { get; }
```

### `WorldPoint`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctPoint3d WorldPoint { get; }
```

## Events

None

## Methods

None

## Fields / Enum Values

None

