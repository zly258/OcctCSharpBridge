# OcctShapeMeshFaceRange

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `ValueType`

## Declaration

```csharp
public struct OcctShapeMeshFaceRange
```

## Description

Describes the contiguous node and triangle ranges contributed by one source face to a combined shape mesh.

## Constructors

None

## Properties

### `Face`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctModelShape Face { get; }
```

### `NodeCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int NodeCount { get; }
```

### `NodeEndExclusive`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int NodeEndExclusive { get; }
```

### `NodeStart`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int NodeStart { get; }
```

### `TriangleCount`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int TriangleCount { get; }
```

### `TriangleEndExclusive`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int TriangleEndExclusive { get; }
```

### `TriangleStart`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public int TriangleStart { get; }
```

## Events

None

## Methods

### `ContainsNode`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool ContainsNode(int nodeIndex)
```

**Parameters**

- `nodeIndex` — `int`

**Returns:** `bool`

### `ContainsTriangle`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public bool ContainsTriangle(int triangleIndex)
```

**Parameters**

- `triangleIndex` — `int`

**Returns:** `bool`

## Fields / Enum Values

None

