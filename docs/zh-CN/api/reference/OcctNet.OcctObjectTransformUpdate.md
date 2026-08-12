# OcctObjectTransformUpdate

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctObjectTransformUpdate
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctObjectTransformUpdate`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctObjectTransformUpdate(IOcctObject Object, OcctTransform3d Transformation)
```

**参数**

- `Object` — `IOcctObject`
- `Transformation` — `OcctTransform3d`

## 属性

### `Object`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IOcctObject Object { get; set; }
```

### `Transformation`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTransform3d Transformation { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out IOcctObject Object, out OcctTransform3d Transformation)
```

**参数**

- `Object` — `out IOcctObject`
- `Transformation` — `out OcctTransform3d`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctObjectTransformUpdate other)
```

**参数**

- `other` — `OcctObjectTransformUpdate`

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

