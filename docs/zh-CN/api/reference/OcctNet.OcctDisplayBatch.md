# OcctDisplayBatch

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Defers OCCT viewer updates until the batch is disposed. Batches can be nested.

## 构造函数

无。

## 属性

### `FitAllOnDispose`

Fits all displayed objects before the final redraw when this outermost batch ends.

```csharp
public bool FitAllOnDispose { get; set; }
```

## 事件

无。

## 方法

### `Dispose`

```csharp
public void Dispose()
```

## 字段 / 枚举值

无。

