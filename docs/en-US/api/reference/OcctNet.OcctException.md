# OcctException

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Constructors

### `OcctException`

```csharp
public OcctException(string message, string operation, string nativeMessage, Exception innerException)
```

### `OcctException`

```csharp
public OcctException(string message)
```

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

None.

## Methods

None.

## Fields / Enum Values

None.

