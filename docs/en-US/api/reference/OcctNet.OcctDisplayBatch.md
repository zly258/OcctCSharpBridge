# OcctDisplayBatch

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Defers OCCT viewer updates until the batch is disposed. Batches can be nested.

## Constructors

None.

## Properties

### `FitAllOnDispose`

Fits all displayed objects before the final redraw when this outermost batch ends.

```csharp
public bool FitAllOnDispose { get; set; }
```

## Events

None.

## Methods

### `Dispose`

```csharp
public void Dispose()
```

## Fields / Enum Values

None.

