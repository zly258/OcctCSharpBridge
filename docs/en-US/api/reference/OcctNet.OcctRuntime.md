# OcctRuntime

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public static class OcctRuntime
```

## Description

Configures the OCCT runtime before the native bridge is loaded.

## Constructors

None

## Properties

### `ConfiguredNativeDirectory`

Gets the directory containing the configured native bridge.

```csharp
public string ConfiguredNativeDirectory { get; set; }
```

### `ConfiguredRoot`

Gets the OCCT root selected during runtime configuration.

```csharp
public string ConfiguredRoot { get; set; }
```

## Events

None

## Methods

### `Configure`

Configures the runtime using the portable package layout, OCCT_ROOT, or CASROOT.

```csharp
public static void Configure()
```

**Returns:** `void`

### `Configure`

Configures the runtime using the portable package layout, OCCT_ROOT, or CASROOT.

```csharp
public static void Configure(string occtRoot, string nativeBridgeDirectory = null)
```

**Parameters**

- `occtRoot` — `string`
- `nativeBridgeDirectory` — `string` = null

**Returns:** `void`

### `GetDiagnosticInfo`

Returns a structured, side-effect-free snapshot of runtime paths and loaded modules. The snapshot does not configure the runtime or force a native library load.

```csharp
public static OcctRuntimeDiagnosticInfo GetDiagnosticInfo()
```

**Returns:** `OcctRuntimeDiagnosticInfo`

### `GetDiagnosticReport`

Returns a human-readable runtime report suitable for logs and deployment diagnostics. Reading the report does not configure the runtime or load the native bridge.

```csharp
public static string GetDiagnosticReport()
```

**Returns:** `string`

## Fields / Enum Values

None

