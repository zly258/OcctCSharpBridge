# OcctFaceAnalysisInfo

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctFaceAnalysisInfo
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctFaceAnalysisInfo`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctFaceAnalysisInfo(OcctModelShape Face, OcctSurfaceType SurfaceType, OcctModelOrientation Orientation, double Area, double Tolerance, OcctUvBounds UvBounds, OcctBounds Bounds, int EdgeCount, int WireCount)
```

**参数**

- `Face` — `OcctModelShape`
- `SurfaceType` — `OcctSurfaceType`
- `Orientation` — `OcctModelOrientation`
- `Area` — `double`
- `Tolerance` — `double`
- `UvBounds` — `OcctUvBounds`
- `Bounds` — `OcctBounds`
- `EdgeCount` — `int`
- `WireCount` — `int`

## 属性

### `Area`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Area { get; set; }
```

### `Bounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctBounds Bounds { get; set; }
```

### `EdgeCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int EdgeCount { get; set; }
```

### `Face`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Face { get; set; }
```

### `IsAnalytic`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsAnalytic { get; }
```

### `IsFreeform`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsFreeform { get; }
```

### `Orientation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelOrientation Orientation { get; set; }
```

### `SurfaceType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctSurfaceType SurfaceType { get; set; }
```

### `Tolerance`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Tolerance { get; set; }
```

### `UvBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctUvBounds UvBounds { get; set; }
```

### `WireCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int WireCount { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out OcctModelShape Face, out OcctSurfaceType SurfaceType, out OcctModelOrientation Orientation, out double Area, out double Tolerance, out OcctUvBounds UvBounds, out OcctBounds Bounds, out int EdgeCount, out int WireCount)
```

**参数**

- `Face` — `out OcctModelShape`
- `SurfaceType` — `out OcctSurfaceType`
- `Orientation` — `out OcctModelOrientation`
- `Area` — `out double`
- `Tolerance` — `out double`
- `UvBounds` — `out OcctUvBounds`
- `Bounds` — `out OcctBounds`
- `EdgeCount` — `out int`
- `WireCount` — `out int`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctFaceAnalysisInfo other)
```

**参数**

- `other` — `OcctFaceAnalysisInfo`

**返回值:** `bool`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual bool Equals(object obj)
```

**参数**

- `obj` — `object`

**返回值:** `bool`

### `GetHashCode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual int GetHashCode()
```

**返回值:** `int`

### `ToString`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual string ToString()
```

**返回值:** `string`

## 字段 / 枚举值

无

