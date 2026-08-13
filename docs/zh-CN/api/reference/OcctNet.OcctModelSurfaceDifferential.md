# OcctModelSurfaceDifferential

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctModelSurfaceDifferential`

```csharp
public OcctModelSurfaceDifferential(double U, double V, OcctPoint3d Point, OcctVector3d Normal, OcctVector3d UDerivative, OcctVector3d VDerivative, OcctVector3d USecondDerivative, OcctVector3d VSecondDerivative, OcctVector3d UvDerivative, bool HasNormal)
```

## 属性

### `HasNormal`

```csharp
public bool HasNormal { get; set; }
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

### `UDerivative`

```csharp
public OcctVector3d UDerivative { get; set; }
```

### `USecondDerivative`

```csharp
public OcctVector3d USecondDerivative { get; set; }
```

### `UvDerivative`

```csharp
public OcctVector3d UvDerivative { get; set; }
```

### `V`

```csharp
public double V { get; set; }
```

### `VDerivative`

```csharp
public OcctVector3d VDerivative { get; set; }
```

### `VSecondDerivative`

```csharp
public OcctVector3d VSecondDerivative { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(double U, double V, OcctPoint3d Point, OcctVector3d Normal, OcctVector3d UDerivative, OcctVector3d VDerivative, OcctVector3d USecondDerivative, OcctVector3d VSecondDerivative, OcctVector3d UvDerivative, bool HasNormal)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctModelSurfaceDifferential other)
```

### `GetHashCode`

```csharp
public int GetHashCode()
```

### `ToString`

```csharp
public string ToString()
```

## 字段 / 枚举值

无。

