# OcctDisplayBatch

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctDisplayBatch
```

## 说明

Defers OCCT viewer updates until the batch is disposed. Batches can be nested.

## 构造函数

无

## 属性

### `FitAllOnDispose`

Fits all displayed objects before the final redraw when this outermost batch ends.

```csharp
public bool FitAllOnDispose { get; set; }
```

## 事件

无

## 方法

### `Dispose`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void Dispose()
```

**返回值:** `void`

## 字段 / 枚举值

无

