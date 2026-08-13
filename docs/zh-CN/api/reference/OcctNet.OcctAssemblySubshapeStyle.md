# OcctAssemblySubshapeStyle

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Style assigned to a specific subshape of an XDE part definition. The index is the zero-based position in OCCT's indexed subshape map for the owning node geometry.

## 构造函数

### `OcctAssemblySubshapeStyle`

Style assigned to a specific subshape of an XDE part definition. The index is the zero-based position in OCCT's indexed subshape map for the owning node geometry.

```csharp
public OcctAssemblySubshapeStyle(OcctShapeType ShapeType, int SubshapeIndex, OcctAssemblyStyle Style)
```

## 属性

### `ShapeType`

```csharp
public OcctShapeType ShapeType { get; set; }
```

### `Style`

```csharp
public OcctAssemblyStyle Style { get; set; }
```

### `SubshapeIndex`

```csharp
public int SubshapeIndex { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(OcctShapeType ShapeType, int SubshapeIndex, OcctAssemblyStyle Style)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctAssemblySubshapeStyle other)
```

### `GetHashCode`

```csharp
public int GetHashCode()
```

### `ToString`

```csharp
public string ToString()
```

## 字段 / 枚举值

无。

