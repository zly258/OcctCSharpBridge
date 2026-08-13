# OcctModelMeshNode

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Constructors

### `OcctModelMeshNode`

```csharp
public OcctModelMeshNode(OcctPoint3d Point, double U, double V, OcctVector3d Normal, bool HasUv, bool HasNormal)
```

## Properties

### `HasNormal`

```csharp
public bool HasNormal { get; set; }
```

### `HasUv`

```csharp
public bool HasUv { get; set; }
```

### `Normal`

```csharp
public OcctVector3d Normal { get; set; }
```

### `Point`

```csharp
public OcctPoint3d Point { get; set; }
```

### `U`

```csharp
public double U { get; set; }
```

### `V`

```csharp
public double V { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(OcctPoint3d Point, double U, double V, OcctVector3d Normal, bool HasUv, bool HasNormal)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctModelMeshNode other)
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

