# OcctModelMeshNode

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctModelMeshNode
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctModelMeshNode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelMeshNode(OcctPoint3d Point, double U, double V, OcctVector3d Normal, bool HasUv, bool HasNormal)
```

**参数**

- `Point` — `OcctPoint3d`
- `U` — `double`
- `V` — `double`
- `Normal` — `OcctVector3d`
- `HasUv` — `bool`
- `HasNormal` — `bool`

## 属性

### `HasNormal`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasNormal { get; set; }
```

### `HasUv`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasUv { get; set; }
```

### `Normal`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Normal { get; set; }
```

### `Point`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d Point { get; set; }
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
public void Deconstruct(out OcctPoint3d Point, out double U, out double V, out OcctVector3d Normal, out bool HasUv, out bool HasNormal)
```

**参数**

- `Point` — `out OcctPoint3d`
- `U` — `out double`
- `V` — `out double`
- `Normal` — `out OcctVector3d`
- `HasUv` — `out bool`
- `HasNormal` — `out bool`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctModelMeshNode other)
```

**参数**

- `other` — `OcctModelMeshNode`

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

