# OcctAssemblyStyle

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctAssemblyStyle
```

## Description

Presentation style resolved by OCCT for one XDE assembly occurrence.

## Constructors

### `OcctAssemblyStyle`

Presentation style resolved by OCCT for one XDE assembly occurrence.

```csharp
public OcctAssemblyStyle(bool Visible, OcctAssemblyColor? SurfaceColor, OcctAssemblyColor? CurveColor)
```

**Parameters**

- `Visible` — `bool`
- `SurfaceColor` — `OcctAssemblyColor?`
- `CurveColor` — `OcctAssemblyColor?`

## Properties

### `CurveColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctAssemblyColor? CurveColor { get; set; }
```

### `SurfaceColor`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctAssemblyColor? SurfaceColor { get; set; }
```

### `Transparency`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Transparency { get; }
```

### `Visible`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Visible { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out bool Visible, out OcctAssemblyColor? SurfaceColor, out OcctAssemblyColor? CurveColor)
```

**Parameters**

- `Visible` — `out bool`
- `SurfaceColor` — `out OcctAssemblyColor?`
- `CurveColor` — `out OcctAssemblyColor?`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctAssemblyStyle other)
```

**Parameters**

- `other` — `OcctAssemblyStyle`

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

