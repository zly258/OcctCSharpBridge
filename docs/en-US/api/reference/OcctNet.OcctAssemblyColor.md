# OcctAssemblyColor

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctAssemblyColor
```

## Description

RGBA color retained from XDE presentation style.

## Constructors

### `OcctAssemblyColor`

RGBA color retained from XDE presentation style.

```csharp
public OcctAssemblyColor(double R, double G, double B, double A = 1)
```

**Parameters**

- `R` — `double`
- `G` — `double`
- `B` — `double`
- `A` — `double` = 1

## Properties

### `A`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double A { get; set; }
```

### `B`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double B { get; set; }
```

### `G`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double G { get; set; }
```

### `R`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double R { get; set; }
```

### `Transparency`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public double Transparency { get; }
```

## Events

None

## Methods

### `Deconstruct`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public void Deconstruct(out double R, out double G, out double B, out double A)
```

**Parameters**

- `R` — `out double`
- `G` — `out double`
- `B` — `out double`
- `A` — `out double`

**Returns:** `void`

### `Equals`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool Equals(OcctAssemblyColor other)
```

**Parameters**

- `other` — `OcctAssemblyColor`

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

