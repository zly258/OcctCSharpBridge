# OcctEdgeIntersection

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

A bounded Edge/Edge common part. Parameters are native curve parameters, not normalized values. For a point intersection, start and end points/parameters are equal.

## Constructors

### `OcctEdgeIntersection`

A bounded Edge/Edge common part. Parameters are native curve parameters, not normalized values. For a point intersection, start and end points/parameters are equal.

```csharp
public OcctEdgeIntersection(OcctIntersectionKind Kind, OcctPoint3d StartPoint, OcctPoint3d EndPoint, double FirstParameterStart, double FirstParameterEnd, double SecondParameterStart, double SecondParameterEnd)
```

## Properties

### `EndPoint`

```csharp
public OcctPoint3d EndPoint { get; set; }
```

### `FirstParameterEnd`

```csharp
public double FirstParameterEnd { get; set; }
```

### `FirstParameterStart`

```csharp
public double FirstParameterStart { get; set; }
```

### `Kind`

```csharp
public OcctIntersectionKind Kind { get; set; }
```

### `SecondParameterEnd`

```csharp
public double SecondParameterEnd { get; set; }
```

### `SecondParameterStart`

```csharp
public double SecondParameterStart { get; set; }
```

### `StartPoint`

```csharp
public OcctPoint3d StartPoint { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(OcctIntersectionKind Kind, OcctPoint3d StartPoint, OcctPoint3d EndPoint, double FirstParameterStart, double FirstParameterEnd, double SecondParameterStart, double SecondParameterEnd)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctEdgeIntersection other)
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

