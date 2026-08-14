# OcctTopologyHistorySummary

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Aggregate topology lineage for one source shape in a modeling operation.

## Constructors

### `OcctTopologyHistorySummary`

Aggregate topology lineage for one source shape in a modeling operation.

```csharp
public OcctTopologyHistorySummary(int GeneratedCount, int ModifiedCount, bool Removed)
```

## Properties

### `GeneratedCount`

```csharp
public int GeneratedCount { get; set; }
```

### `ModifiedCount`

```csharp
public int ModifiedCount { get; set; }
```

### `Removed`

```csharp
public bool Removed { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(int GeneratedCount, int ModifiedCount, bool Removed)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctTopologyHistorySummary other)
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
