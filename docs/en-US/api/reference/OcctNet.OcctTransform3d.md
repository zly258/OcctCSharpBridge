# OcctTransform3d

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctTransform3d
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctTransform3d`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTransform3d(double M00, double M01, double M02, double M03, double M10, double M11, double M12, double M13, double M20, double M21, double M22, double M23)
```

**Parameters**

- `M00` — `double`
- `M01` — `double`
- `M02` — `double`
- `M03` — `double`
- `M10` — `double`
- `M11` — `double`
- `M12` — `double`
- `M13` — `double`
- `M20` — `double`
- `M21` — `double`
- `M22` — `double`
- `M23` — `double`

## Properties

### `Identity`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctTransform3d Identity { get; }
```

### `IsFinite`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool IsFinite { get; }
```

### `M00`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M00 { get; set; }
```

### `M01`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M01 { get; set; }
```

### `M02`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M02 { get; set; }
```

### `M03`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M03 { get; set; }
```

### `M10`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M10 { get; set; }
```

### `M11`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M11 { get; set; }
```

### `M12`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M12 { get; set; }
```

### `M13`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M13 { get; set; }
```

### `M20`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M20 { get; set; }
```

### `M21`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M21 { get; set; }
```

### `M22`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M22 { get; set; }
```

### `M23`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double M23 { get; set; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out double M00, out double M01, out double M02, out double M03, out double M10, out double M11, out double M12, out double M13, out double M20, out double M21, out double M22, out double M23)
```

**Parameters**

- `M00` — `out double`
- `M01` — `out double`
- `M02` — `out double`
- `M03` — `out double`
- `M10` — `out double`
- `M11` — `out double`
- `M12` — `out double`
- `M13` — `out double`
- `M20` — `out double`
- `M21` — `out double`
- `M22` — `out double`
- `M23` — `out double`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctTransform3d other)
```

**Parameters**

- `other` — `OcctTransform3d`

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

### `Translation`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public static OcctTransform3d Translation(double x, double y, double z)
```

**Parameters**

- `x` — `double`
- `y` — `double`
- `z` — `double`

**Returns:** `OcctTransform3d`

## Fields / Enum Values

None

