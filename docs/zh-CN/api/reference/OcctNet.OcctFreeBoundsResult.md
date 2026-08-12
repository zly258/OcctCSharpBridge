# OcctFreeBoundsResult

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctFreeBoundsResult
```

## 说明

Read-only result of strict OCCT free-boundary analysis. Closed and open wires are owned by the same modeling session as the analyzed shape.

## 构造函数

无

## 属性

### `ClosedWireCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int ClosedWireCount { get; }
```

### `ClosedWires`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> ClosedWires { get; }
```

### `HasFreeBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasFreeBounds { get; }
```

### `HasOpenFreeBounds`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool HasOpenFreeBounds { get; }
```

### `OpenWireCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int OpenWireCount { get; }
```

### `OpenWires`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctModelShape> OpenWires { get; }
```

### `Tolerance`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double Tolerance { get; }
```

### `TotalWireCount`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int TotalWireCount { get; }
```

## 事件

无

## 方法

无

## 字段 / 枚举值

无

