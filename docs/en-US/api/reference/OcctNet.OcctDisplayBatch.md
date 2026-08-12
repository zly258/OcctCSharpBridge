# OcctDisplayBatch

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctDisplayBatch
```

## Description

Defers OCCT viewer updates until the batch is disposed. Batches can be nested.

## Constructors

None

## Properties

### `FitAllOnDispose`

Fits all displayed objects before the final redraw when this outermost batch ends.

```csharp
public bool FitAllOnDispose { get; set; }
```

## Events

None

## Methods

### `Dispose`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Dispose()
```

**Returns:** `void`

## Fields / Enum Values

None

