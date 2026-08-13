# OcctTopologyReferenceResult

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.

## 构造函数

### `OcctTopologyReferenceResult`

Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.

```csharp
public OcctTopologyReferenceResult(OcctTopologyReferenceStatus Status, OcctModelShape? Shape, double Score, int CandidateCount, bool UsedOperationHistory, bool RuntimeIndexMatched)
```

## 属性

### `CandidateCount`

```csharp
public int CandidateCount { get; set; }
```

### `RuntimeIndexMatched`

```csharp
public bool RuntimeIndexMatched { get; set; }
```

### `Score`

```csharp
public double Score { get; set; }
```

### `Shape`

```csharp
public OcctModelShape? Shape { get; set; }
```

### `Status`

```csharp
public OcctTopologyReferenceStatus Status { get; set; }
```

### `UsedOperationHistory`

```csharp
public bool UsedOperationHistory { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(OcctTopologyReferenceStatus Status, OcctModelShape? Shape, double Score, int CandidateCount, bool UsedOperationHistory, bool RuntimeIndexMatched)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctTopologyReferenceResult other)
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

