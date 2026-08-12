# OcctShapeMeshData

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctShapeMeshData
```

## 说明

Combined shape triangulation plus stable ranges that map mesh nodes and triangles back to the source OCCT faces that contributed them.

## 构造函数

无

## 属性

### `FaceCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int FaceCount { get; }
```

### `FaceRanges`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctShapeMeshFaceRange> FaceRanges { get; }
```

### `Mesh`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctMesh Mesh { get; }
```

### `NodeCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int NodeCount { get; }
```

### `TriangleCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int TriangleCount { get; }
```

## 事件

无

## 方法

### `GetFaceForNode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape GetFaceForNode(int nodeIndex)
```

**参数**

- `nodeIndex` — `int`

**返回值:** `OcctModelShape`

### `GetFaceForTriangle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape GetFaceForTriangle(int triangleIndex)
```

**参数**

- `triangleIndex` — `int`

**返回值:** `OcctModelShape`

### `GetFaceRange`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShapeMeshFaceRange GetFaceRange(int faceIndex)
```

**参数**

- `faceIndex` — `int`

**返回值:** `OcctShapeMeshFaceRange`

### `TryGetFaceForNode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryGetFaceForNode(int nodeIndex, out OcctModelShape face)
```

**参数**

- `nodeIndex` — `int`
- `face` — `out OcctModelShape`

**返回值:** `bool`

### `TryGetFaceForTriangle`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool TryGetFaceForTriangle(int triangleIndex, out OcctModelShape face)
```

**参数**

- `triangleIndex` — `int`
- `face` — `out OcctModelShape`

**返回值:** `bool`

## 字段 / 枚举值

无

