# OcctFaceAnalysisInfo

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctFaceAnalysisInfo`

```csharp
public OcctFaceAnalysisInfo(OcctModelShape Face, OcctSurfaceType SurfaceType, OcctModelOrientation Orientation, double Area, double Tolerance, OcctUvBounds UvBounds, OcctBounds Bounds, int EdgeCount, int WireCount)
```

## 属性

### `Area`

```csharp
public double Area { get; set; }
```

### `Bounds`

```csharp
public OcctBounds Bounds { get; set; }
```

### `EdgeCount`

```csharp
public int EdgeCount { get; set; }
```

### `Face`

```csharp
public OcctModelShape Face { get; set; }
```

### `IsAnalytic`

```csharp
public bool IsAnalytic { get; }
```

### `IsFreeform`

```csharp
public bool IsFreeform { get; }
```

### `Orientation`

```csharp
public OcctModelOrientation Orientation { get; set; }
```

### `SurfaceType`

```csharp
public OcctSurfaceType SurfaceType { get; set; }
```

### `Tolerance`

```csharp
public double Tolerance { get; set; }
```

### `UvBounds`

```csharp
public OcctUvBounds UvBounds { get; set; }
```

### `WireCount`

```csharp
public int WireCount { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(OcctModelShape Face, OcctSurfaceType SurfaceType, OcctModelOrientation Orientation, double Area, double Tolerance, OcctUvBounds UvBounds, OcctBounds Bounds, int EdgeCount, int WireCount)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctFaceAnalysisInfo other)
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

