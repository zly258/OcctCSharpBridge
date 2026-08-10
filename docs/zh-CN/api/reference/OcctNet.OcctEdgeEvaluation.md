# OcctEdgeEvaluation

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctEdgeEvaluation
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctEdgeEvaluation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctEdgeEvaluation(OcctPoint3d Point, OcctVector3d Tangent)
```

**参数**

- `Point` — `OcctPoint3d`
- `Tangent` — `OcctVector3d`

## 属性

### `Point`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctPoint3d Point { get; set; }
```

### `Tangent`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctVector3d Tangent { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out OcctPoint3d Point, out OcctVector3d Tangent)
```

**参数**

- `Point` — `out OcctPoint3d`
- `Tangent` — `out OcctVector3d`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctEdgeEvaluation other)
```

**参数**

- `other` — `OcctEdgeEvaluation`

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

