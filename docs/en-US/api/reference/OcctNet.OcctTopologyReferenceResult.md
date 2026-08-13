# OcctTopologyReferenceResult

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.

## Constructors

### `OcctTopologyReferenceResult`

Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.

```csharp
public OcctTopologyReferenceResult(OcctTopologyReferenceStatus Status, OcctModelShape? Shape, double Score, int CandidateCount, bool UsedOperationHistory, bool RuntimeIndexMatched)
```

## Properties

### `CandidateCount`

```csharp
public int CandidateCount { get; set; }
```

### `RuntimeIndexMatched`

```csharp
public bool RuntimeIndexMatched { get; set; }
```

### `Score`

```csharp
public double Score { get; set; }
```

### `Shape`

```csharp
public OcctModelShape? Shape { get; set; }
```

### `Status`

```csharp
public OcctTopologyReferenceStatus Status { get; set; }
```

### `UsedOperationHistory`

```csharp
public bool UsedOperationHistory { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(OcctTopologyReferenceStatus Status, OcctModelShape? Shape, double Score, int CandidateCount, bool UsedOperationHistory, bool RuntimeIndexMatched)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctTopologyReferenceResult other)
```

### `GetHashCode`

```csharp
public int GetHashCode()
```

### `ToString`

```csharp
public string ToString()
```

## Fields / Enum Values

None.

