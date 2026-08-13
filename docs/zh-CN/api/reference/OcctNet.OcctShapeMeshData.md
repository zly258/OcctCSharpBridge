# OcctShapeMeshData

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Combined shape triangulation plus stable ranges that map mesh nodes and triangles back to the source OCCT faces that contributed them.

## 构造函数

无。

## 属性

### `FaceCount`

```csharp
public int FaceCount { get; }
```

### `FaceRanges`

```csharp
public IReadOnlyList<OcctShapeMeshFaceRange> FaceRanges { get; }
```

### `Mesh`

```csharp
public OcctMesh Mesh { get; }
```

### `NodeCount`

```csharp
public int NodeCount { get; }
```

### `TriangleCount`

```csharp
public int TriangleCount { get; }
```

## 事件

无。

## 方法

### `GetFaceForNode`

```csharp
public OcctModelShape GetFaceForNode(int nodeIndex)
```

### `GetFaceForTriangle`

```csharp
public OcctModelShape GetFaceForTriangle(int triangleIndex)
```

### `GetFaceRange`

```csharp
public OcctShapeMeshFaceRange GetFaceRange(int faceIndex)
```

### `TryGetFaceForNode`

```csharp
public bool TryGetFaceForNode(int nodeIndex, OcctModelShape face)
```

### `TryGetFaceForTriangle`

```csharp
public bool TryGetFaceForTriangle(int triangleIndex, OcctModelShape face)
```

## 字段 / 枚举值

无。

