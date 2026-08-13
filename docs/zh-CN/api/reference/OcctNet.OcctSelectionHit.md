# OcctSelectionHit

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Structured identity of a selected or detected AIS entity. Subshape indices are runtime topology indices and are not persistent naming.

## 构造函数

### `OcctSelectionHit`

Structured identity of a selected or detected AIS entity. Subshape indices are runtime topology indices and are not persistent naming.

```csharp
public OcctSelectionHit(IOcctObject Owner, OcctShapeType SubshapeType, int SubshapeIndex)
```

## 属性

### `IsSubshape`

```csharp
public bool IsSubshape { get; }
```

### `Owner`

```csharp
public IOcctObject Owner { get; set; }
```

### `SubshapeIndex`

```csharp
public int SubshapeIndex { get; set; }
```

### `SubshapeType`

```csharp
public OcctShapeType SubshapeType { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(IOcctObject Owner, OcctShapeType SubshapeType, int SubshapeIndex)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctSelectionHit other)
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

