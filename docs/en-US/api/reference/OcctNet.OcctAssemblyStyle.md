# OcctAssemblyStyle

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

Presentation style resolved by OCCT for one XDE assembly occurrence.

## Constructors

### `OcctAssemblyStyle`

Presentation style resolved by OCCT for one XDE assembly occurrence.

```csharp
public OcctAssemblyStyle(bool Visible, OcctAssemblyColor? SurfaceColor, OcctAssemblyColor? CurveColor)
```

## Properties

### `CurveColor`

```csharp
public OcctAssemblyColor? CurveColor { get; set; }
```

### `SurfaceColor`

```csharp
public OcctAssemblyColor? SurfaceColor { get; set; }
```

### `Transparency`

```csharp
public double Transparency { get; }
```

### `Visible`

```csharp
public bool Visible { get; set; }
```

## Events

None.

## Methods

### `Deconstruct`

```csharp
public void Deconstruct(bool Visible, OcctAssemblyColor? SurfaceColor, OcctAssemblyColor? CurveColor)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctAssemblyStyle other)
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

