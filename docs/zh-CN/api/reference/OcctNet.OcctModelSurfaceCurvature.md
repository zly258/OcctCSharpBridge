# OcctModelSurfaceCurvature

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctModelSurfaceCurvature`

```csharp
public OcctModelSurfaceCurvature(double U, double V, OcctPoint3d Point, OcctVector3d Normal, OcctVector3d MaximumDirection, OcctVector3d MinimumDirection, double MaximumCurvature, double MinimumCurvature, double MeanCurvature, double GaussianCurvature, bool IsUmbilic, bool HasNormal, bool HasCurvature)
```

## 属性

### `GaussianCurvature`

```csharp
public double GaussianCurvature { get; set; }
```

### `HasCurvature`

```csharp
public bool HasCurvature { get; set; }
```

### `HasNormal`

```csharp
public bool HasNormal { get; set; }
```

### `IsUmbilic`

```csharp
public bool IsUmbilic { get; set; }
```

### `MaximumCurvature`

```csharp
public double MaximumCurvature { get; set; }
```

### `MaximumDirection`

```csharp
public OcctVector3d MaximumDirection { get; set; }
```

### `MeanCurvature`

```csharp
public double MeanCurvature { get; set; }
```

### `MinimumCurvature`

```csharp
public double MinimumCurvature { get; set; }
```

### `MinimumDirection`

```csharp
public OcctVector3d MinimumDirection { get; set; }
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

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(double U, double V, OcctPoint3d Point, OcctVector3d Normal, OcctVector3d MaximumDirection, OcctVector3d MinimumDirection, double MaximumCurvature, double MinimumCurvature, double MeanCurvature, double GaussianCurvature, bool IsUmbilic, bool HasNormal, bool HasCurvature)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctModelSurfaceCurvature other)
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

