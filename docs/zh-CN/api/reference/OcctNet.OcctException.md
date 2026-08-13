# OcctException

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctException`

```csharp
public OcctException(string message, string operation, string nativeMessage, Exception innerException)
```

### `OcctException`

```csharp
public OcctException(string message)
```

## 属性

### `NativeMessage`

Gets the original message returned by the native bridge when available.

```csharp
public string NativeMessage { get; }
```

### `Operation`

Gets the managed bridge operation that reported the failure when available.

```csharp
public string Operation { get; }
```

## 事件

无。

## 方法

无。

## 字段 / 枚举值

无。

