# OcctException

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`
- **继承:** `Exception`

## 声明

```csharp
public sealed class OcctException
```

## 说明

公开 API 类型。具体语义、所有权和生命周期约束以类型声明、成员签名及对应专题文档为准。

## 构造函数

### `OcctException`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctException(string message, string operation, string nativeMessage = null, Exception innerException = null)
```

**参数**

- `message` — `string`
- `operation` — `string`
- `nativeMessage` — `string` = null
- `innerException` — `Exception` = null

### `OcctException`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctException(string message)
```

**参数**

- `message` — `string`

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

无

## 方法

无

## 字段 / 枚举值

无

