# OcctModelCurveCurvature

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctModelCurveCurvature`

```csharp
public OcctModelCurveCurvature(double Parameter, OcctPoint3d Point, OcctVector3d Tangent, OcctVector3d Normal, OcctPoint3d CenterOfCurvature, double Curvature, bool HasTangent, bool HasNormal, bool HasCenterOfCurvature)
```

## 属性

### `CenterOfCurvature`

```csharp
public OcctPoint3d CenterOfCurvature { get; set; }
```

### `Curvature`

```csharp
public double Curvature { get; set; }
```

### `HasCenterOfCurvature`

```csharp
public bool HasCenterOfCurvature { get; set; }
```

### `HasNormal`

```csharp
public bool HasNormal { get; set; }
```

### `HasTangent`

```csharp
public bool HasTangent { get; set; }
```

### `Normal`

```csharp
public OcctVector3d Normal { get; set; }
```

### `Parameter`

```csharp
public double Parameter { get; set; }
```

### `Point`

```csharp
public OcctPoint3d Point { get; set; }
```

### `RadiusOfCurvature`

```csharp
public double RadiusOfCurvature { get; }
```

### `Tangent`

```csharp
public OcctVector3d Tangent { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(double Parameter, OcctPoint3d Point, OcctVector3d Tangent, OcctVector3d Normal, OcctPoint3d CenterOfCurvature, double Curvature, bool HasTangent, bool HasNormal, bool HasCenterOfCurvature)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctModelCurveCurvature other)
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

