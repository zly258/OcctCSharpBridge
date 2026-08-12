# OcctAssemblySubshapeStyle

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctAssemblySubshapeStyle
```

## 说明

Style assigned to a specific subshape of an XDE part definition. The index is the zero-based position in OCCT's indexed subshape map for the owning node geometry.

## 构造函数

### `OcctAssemblySubshapeStyle`

Style assigned to a specific subshape of an XDE part definition. The index is the zero-based position in OCCT's indexed subshape map for the owning node geometry.

```csharp
public OcctAssemblySubshapeStyle(OcctShapeType ShapeType, int SubshapeIndex, OcctAssemblyStyle Style)
```

**参数**

- `ShapeType` — `OcctShapeType`
- `SubshapeIndex` — `int`
- `Style` — `OcctAssemblyStyle`

## 属性

### `ShapeType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShapeType ShapeType { get; set; }
```

### `Style`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctAssemblyStyle Style { get; set; }
```

### `SubshapeIndex`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int SubshapeIndex { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out OcctShapeType ShapeType, out int SubshapeIndex, out OcctAssemblyStyle Style)
```

**参数**

- `ShapeType` — `out OcctShapeType`
- `SubshapeIndex` — `out int`
- `Style` — `out OcctAssemblyStyle`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctAssemblySubshapeStyle other)
```

**参数**

- `other` — `OcctAssemblySubshapeStyle`

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

