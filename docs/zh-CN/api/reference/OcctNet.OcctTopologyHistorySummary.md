# OcctTopologyHistorySummary

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Aggregate topology lineage for one source shape in a modeling operation.

## 构造函数

### `OcctTopologyHistorySummary`

Aggregate topology lineage for one source shape in a modeling operation.

```csharp
public OcctTopologyHistorySummary(int GeneratedCount, int ModifiedCount, bool Removed)
```

## 属性

### `GeneratedCount`

```csharp
public int GeneratedCount { get; set; }
```

### `ModifiedCount`

```csharp
public int ModifiedCount { get; set; }
```

### `Removed`

```csharp
public bool Removed { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(int GeneratedCount, int ModifiedCount, bool Removed)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctTopologyHistorySummary other)
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
