# OcctFreeBoundsResult

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Read-only result of strict OCCT free-boundary analysis. Closed and open wires are owned by the same modeling session as the analyzed shape.

## 构造函数

无。

## 属性

### `ClosedWireCount`

```csharp
public int ClosedWireCount { get; }
```

### `ClosedWires`

```csharp
public IReadOnlyList<OcctModelShape> ClosedWires { get; }
```

### `HasFreeBounds`

```csharp
public bool HasFreeBounds { get; }
```

### `HasOpenFreeBounds`

```csharp
public bool HasOpenFreeBounds { get; }
```

### `OpenWireCount`

```csharp
public int OpenWireCount { get; }
```

### `OpenWires`

```csharp
public IReadOnlyList<OcctModelShape> OpenWires { get; }
```

### `Tolerance`

```csharp
public double Tolerance { get; }
```

### `TotalWireCount`

```csharp
public int TotalWireCount { get; }
```

## 事件

无。

## 方法

无。

## 字段 / 枚举值

无。

