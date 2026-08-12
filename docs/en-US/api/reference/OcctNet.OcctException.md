# OcctException

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `Exception`

## Declaration

```csharp
public sealed class OcctException
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctException`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctException(string message, string operation, string nativeMessage = null, Exception innerException = null)
```

**Parameters**

- `message` — `string`
- `operation` — `string`
- `nativeMessage` — `string` = null
- `innerException` — `Exception` = null

### `OcctException`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctException(string message)
```

**Parameters**

- `message` — `string`

## Properties

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

## Events

None

## Methods

None

## Fields / Enum Values

None

