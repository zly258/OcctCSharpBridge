# OcctSelectionHit

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctSelectionHit
```

## 说明

Structured identity of a selected or detected AIS entity. Subshape indices are runtime topology indices and are not persistent naming.

## 构造函数

### `OcctSelectionHit`

Structured identity of a selected or detected AIS entity. Subshape indices are runtime topology indices and are not persistent naming.

```csharp
public OcctSelectionHit(IOcctObject Owner, OcctShapeType SubshapeType, int SubshapeIndex)
```

**参数**

- `Owner` — `IOcctObject`
- `SubshapeType` — `OcctShapeType`
- `SubshapeIndex` — `int`

## 属性

### `IsSubshape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool IsSubshape { get; }
```

### `Owner`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IOcctObject Owner { get; set; }
```

### `SubshapeIndex`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int SubshapeIndex { get; set; }
```

### `SubshapeType`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShapeType SubshapeType { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out IOcctObject Owner, out OcctShapeType SubshapeType, out int SubshapeIndex)
```

**参数**

- `Owner` — `out IOcctObject`
- `SubshapeType` — `out OcctShapeType`
- `SubshapeIndex` — `out int`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctSelectionHit other)
```

**参数**

- `other` — `OcctSelectionHit`

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

