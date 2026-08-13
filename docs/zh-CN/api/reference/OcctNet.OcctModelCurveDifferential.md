# OcctModelCurveDifferential

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctModelCurveDifferential`

```csharp
public OcctModelCurveDifferential(double Parameter, OcctPoint3d Point, OcctVector3d FirstDerivative, OcctVector3d SecondDerivative)
```

## 属性

### `FirstDerivative`

```csharp
public OcctVector3d FirstDerivative { get; set; }
```

### `Parameter`

```csharp
public double Parameter { get; set; }
```

### `Point`

```csharp
public OcctPoint3d Point { get; set; }
```

### `SecondDerivative`

```csharp
public OcctVector3d SecondDerivative { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(double Parameter, OcctPoint3d Point, OcctVector3d FirstDerivative, OcctVector3d SecondDerivative)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctModelCurveDifferential other)
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

