# OcctModelCurveDifferential

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Constructors

### `OcctModelCurveDifferential`

```csharp
public OcctModelCurveDifferential(double Parameter, OcctPoint3d Point, OcctVector3d FirstDerivative, OcctVector3d SecondDerivative)
```

## Properties

### `FirstDerivative`

```csharp
public OcctVector3d FirstDerivative { get; set; }
```

### `Parameter`

```csharp
public double Parameter { get; set; }
```

### `Point`

```csharp
public OcctPoint3d Point { get; set; }
```

### `SecondDerivative`

```csharp
public OcctVector3d SecondDerivative { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(double Parameter, OcctPoint3d Point, OcctVector3d FirstDerivative, OcctVector3d SecondDerivative)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctModelCurveDifferential other)
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

