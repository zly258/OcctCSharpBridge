# OcctFreeBoundsResult

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctFreeBoundsResult
```

## Description

Read-only result of strict OCCT free-boundary analysis. Closed and open wires are owned by the same modeling session as the analyzed shape.

## Constructors

None

## Properties

### `ClosedWireCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int ClosedWireCount { get; }
```

### `ClosedWires`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> ClosedWires { get; }
```

### `HasFreeBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasFreeBounds { get; }
```

### `HasOpenFreeBounds`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool HasOpenFreeBounds { get; }
```

### `OpenWireCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int OpenWireCount { get; }
```

### `OpenWires`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctModelShape> OpenWires { get; }
```

### `Tolerance`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Tolerance { get; }
```

### `TotalWireCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int TotalWireCount { get; }
```

## Events

None

## Methods

None

## Fields / Enum Values

None

