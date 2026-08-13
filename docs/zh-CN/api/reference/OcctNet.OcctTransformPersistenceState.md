# OcctTransformPersistenceState

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctTransformPersistenceState`

```csharp
public OcctTransformPersistenceState(OcctTransformPersistenceMode Mode, OcctPoint3d Anchor, OcctCornerPosition Position, int OffsetX, int OffsetY)
```

## 属性

### `Anchor`

```csharp
public OcctPoint3d Anchor { get; set; }
```

### `Enabled`

```csharp
public bool Enabled { get; }
```

### `IsScreenAnchored`

```csharp
public bool IsScreenAnchored { get; }
```

### `Mode`

```csharp
public OcctTransformPersistenceMode Mode { get; set; }
```

### `OffsetX`

```csharp
public int OffsetX { get; set; }
```

### `OffsetY`

```csharp
public int OffsetY { get; set; }
```

### `Position`

```csharp
public OcctCornerPosition Position { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(OcctTransformPersistenceMode Mode, OcctPoint3d Anchor, OcctCornerPosition Position, int OffsetX, int OffsetY)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctTransformPersistenceState other)
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

