# OcctShapeMeshFaceRange

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Describes the contiguous node and triangle ranges contributed by one source face to a combined shape mesh.

## 构造函数

无。

## 属性

### `Face`

```csharp
public OcctModelShape Face { get; }
```

### `NodeCount`

```csharp
public int NodeCount { get; }
```

### `NodeEndExclusive`

```csharp
public int NodeEndExclusive { get; }
```

### `NodeStart`

```csharp
public int NodeStart { get; }
```

### `TriangleCount`

```csharp
public int TriangleCount { get; }
```

### `TriangleEndExclusive`

```csharp
public int TriangleEndExclusive { get; }
```

### `TriangleStart`

```csharp
public int TriangleStart { get; }
```

## 事件

无。

## 方法

### `ContainsNode`

```csharp
public bool ContainsNode(int nodeIndex)
```

### `ContainsTriangle`

```csharp
public bool ContainsTriangle(int triangleIndex)
```

## 字段 / 枚举值

无。

