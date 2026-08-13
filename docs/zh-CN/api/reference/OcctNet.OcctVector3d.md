# OcctVector3d

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctVector3d`

```csharp
public OcctVector3d(double x, double y, double z)
```

## 属性

### `IsFinite`

```csharp
public bool IsFinite { get; }
```

### `Length`

```csharp
public double Length { get; }
```

### `LengthSquared`

```csharp
public double LengthSquared { get; }
```

### `UnitX`

```csharp
public OcctVector3d UnitX { get; }
```

### `UnitY`

```csharp
public OcctVector3d UnitY { get; }
```

### `UnitZ`

```csharp
public OcctVector3d UnitZ { get; }
```

### `Zero`

```csharp
public OcctVector3d Zero { get; }
```

## 事件

无。

## 方法

### `Cross`

```csharp
public OcctVector3d Cross(OcctVector3d other)
```

### `Dot`

```csharp
public double Dot(OcctVector3d other)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctVector3d other)
```

### `GetHashCode`

```csharp
public int GetHashCode()
```

### `Normalized`

```csharp
public OcctVector3d Normalized()
```

### `ToString`

```csharp
public string ToString()
```

### `TryNormalize`

```csharp
public bool TryNormalize(OcctVector3d result)
```

## 字段 / 枚举值

- `X` — `double`
- `Y` — `double`
- `Z` — `double`

