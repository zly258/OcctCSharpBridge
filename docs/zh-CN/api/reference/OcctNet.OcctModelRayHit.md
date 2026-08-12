# OcctModelRayHit

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctModelRayHit
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctModelRayHit`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelRayHit(OcctPoint3d Point, OcctModelShape Face, double RayParameter, double U, double V, OcctModelState State)
```

**参数**

- `Point` — `OcctPoint3d`
- `Face` — `OcctModelShape`
- `RayParameter` — `double`
- `U` — `double`
- `V` — `double`
- `State` — `OcctModelState`

## 属性

### `Face`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Face { get; set; }
```

### `Point`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d Point { get; set; }
```

### `RayParameter`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RayParameter { get; set; }
```

### `State`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelState State { get; set; }
```

### `U`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double U { get; set; }
```

### `V`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double V { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out OcctPoint3d Point, out OcctModelShape Face, out double RayParameter, out double U, out double V, out OcctModelState State)
```

**参数**

- `Point` — `out OcctPoint3d`
- `Face` — `out OcctModelShape`
- `RayParameter` — `out double`
- `U` — `out double`
- `V` — `out double`
- `State` — `out OcctModelState`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctModelRayHit other)
```

**参数**

- `other` — `OcctModelRayHit`

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

