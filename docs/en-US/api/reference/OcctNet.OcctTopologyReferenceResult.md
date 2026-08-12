# OcctTopologyReferenceResult

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctTopologyReferenceResult
```

## Description

Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.

## Constructors

### `OcctTopologyReferenceResult`

Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.

```csharp
public OcctTopologyReferenceResult(OcctTopologyReferenceStatus Status, OcctModelShape? Shape, double Score, int CandidateCount, bool UsedOperationHistory, bool RuntimeIndexMatched)
```

**Parameters**

- `Status` — `OcctTopologyReferenceStatus`
- `Shape` — `OcctModelShape?`
- `Score` — `double`
- `CandidateCount` — `int`
- `UsedOperationHistory` — `bool`
- `RuntimeIndexMatched` — `bool`

## Properties

### `CandidateCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int CandidateCount { get; set; }
```

### `RuntimeIndexMatched`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool RuntimeIndexMatched { get; set; }
```

### `Score`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Score { get; set; }
```

### `Shape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape? Shape { get; set; }
```

### `Status`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTopologyReferenceStatus Status { get; set; }
```

### `UsedOperationHistory`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool UsedOperationHistory { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out OcctTopologyReferenceStatus Status, out OcctModelShape? Shape, out double Score, out int CandidateCount, out bool UsedOperationHistory, out bool RuntimeIndexMatched)
```

**Parameters**

- `Status` — `out OcctTopologyReferenceStatus`
- `Shape` — `out OcctModelShape?`
- `Score` — `out double`
- `CandidateCount` — `out int`
- `UsedOperationHistory` — `out bool`
- `RuntimeIndexMatched` — `out bool`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctTopologyReferenceResult other)
```

**Parameters**

- `other` — `OcctTopologyReferenceResult`

**Returns:** `bool`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual bool Equals(object obj)
```

**Parameters**

- `obj` — `object`

**Returns:** `bool`

### `GetHashCode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual int GetHashCode()
```

**Returns:** `int`

### `ToString`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public virtual string ToString()
```

**Returns:** `string`

## Fields / Enum Values

None

