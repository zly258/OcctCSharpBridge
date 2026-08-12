# OcctShapeMeshFaceRange

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctShapeMeshFaceRange
```

## 说明

Describes the contiguous node and triangle ranges contributed by one source face to a combined shape mesh.

## 构造函数

无

## 属性

### `Face`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape Face { get; }
```

### `NodeCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int NodeCount { get; }
```

### `NodeEndExclusive`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int NodeEndExclusive { get; }
```

### `NodeStart`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int NodeStart { get; }
```

### `TriangleCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int TriangleCount { get; }
```

### `TriangleEndExclusive`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int TriangleEndExclusive { get; }
```

### `TriangleStart`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int TriangleStart { get; }
```

## 事件

无

## 方法

### `ContainsNode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool ContainsNode(int nodeIndex)
```

**参数**

- `nodeIndex` — `int`

**返回值:** `bool`

### `ContainsTriangle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool ContainsTriangle(int triangleIndex)
```

**参数**

- `triangleIndex` — `int`

**返回值:** `bool`

## 字段 / 枚举值

无

