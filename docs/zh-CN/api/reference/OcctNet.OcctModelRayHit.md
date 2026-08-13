# OcctModelRayHit

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctModelRayHit`

```csharp
public OcctModelRayHit(OcctPoint3d Point, OcctModelShape Face, double RayParameter, double U, double V, OcctModelState State)
```

## 属性

### `Face`

```csharp
public OcctModelShape Face { get; set; }
```

### `Point`

```csharp
public OcctPoint3d Point { get; set; }
```

### `RayParameter`

```csharp
public double RayParameter { get; set; }
```

### `State`

```csharp
public OcctModelState State { get; set; }
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
public void Deconstruct(OcctPoint3d Point, OcctModelShape Face, double RayParameter, double U, double V, OcctModelState State)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctModelRayHit other)
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

