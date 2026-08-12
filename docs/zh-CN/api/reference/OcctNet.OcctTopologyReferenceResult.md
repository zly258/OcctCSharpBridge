# OcctTopologyReferenceResult

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `ValueType`

## 声明

```csharp
public struct OcctTopologyReferenceResult
```

## 说明

Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.

## 构造函数

### `OcctTopologyReferenceResult`

Result of resolving a topology reference. Ambiguous results intentionally do not return a shape.

```csharp
public OcctTopologyReferenceResult(OcctTopologyReferenceStatus Status, OcctModelShape? Shape, double Score, int CandidateCount, bool UsedOperationHistory, bool RuntimeIndexMatched)
```

**参数**

- `Status` — `OcctTopologyReferenceStatus`
- `Shape` — `OcctModelShape?`
- `Score` — `double`
- `CandidateCount` — `int`
- `UsedOperationHistory` — `bool`
- `RuntimeIndexMatched` — `bool`

## 属性

### `CandidateCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int CandidateCount { get; set; }
```

### `RuntimeIndexMatched`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool RuntimeIndexMatched { get; set; }
```

### `Score`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Score { get; set; }
```

### `Shape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctModelShape? Shape { get; set; }
```

### `Status`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctTopologyReferenceStatus Status { get; set; }
```

### `UsedOperationHistory`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool UsedOperationHistory { get; set; }
```

## 事件

无

## 方法

### `Deconstruct`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Deconstruct(out OcctTopologyReferenceStatus Status, out OcctModelShape? Shape, out double Score, out int CandidateCount, out bool UsedOperationHistory, out bool RuntimeIndexMatched)
```

**参数**

- `Status` — `out OcctTopologyReferenceStatus`
- `Shape` — `out OcctModelShape?`
- `Score` — `out double`
- `CandidateCount` — `out int`
- `UsedOperationHistory` — `out bool`
- `RuntimeIndexMatched` — `out bool`

**返回值:** `void`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool Equals(OcctTopologyReferenceResult other)
```

**参数**

- `other` — `OcctTopologyReferenceResult`

**返回值:** `bool`

### `Equals`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual bool Equals(object obj)
```

**参数**

- `obj` — `object`

**返回值:** `bool`

### `GetHashCode`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual int GetHashCode()
```

**返回值:** `int`

### `ToString`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public virtual string ToString()
```

**返回值:** `string`

## 字段 / 枚举值

无

