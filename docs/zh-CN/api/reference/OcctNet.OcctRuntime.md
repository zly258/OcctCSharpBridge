# OcctRuntime

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Configures the OCCT runtime before the native bridge is loaded.

## 构造函数

无。

## 属性

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

## 事件

无。

## 方法

### `Configure`

Configures the runtime using the portable package layout, OCCT_ROOT, or CASROOT.

```csharp
public static void Configure()
```

### `Configure`

Configures the runtime using the portable package layout, OCCT_ROOT, or CASROOT.

```csharp
public static void Configure(string occtRoot, string nativeBridgeDirectory)
```

### `GetDiagnosticInfo`

Returns a structured, side-effect-free snapshot of runtime paths and loaded modules. The snapshot does not configure the runtime or force a native library load.

```csharp
public static OcctRuntimeDiagnosticInfo GetDiagnosticInfo()
```

### `GetDiagnosticReport`

Returns a human-readable runtime report suitable for logs and deployment diagnostics. Reading the report does not configure the runtime or load the native bridge.

```csharp
public static string GetDiagnosticReport()
```

## 字段 / 枚举值

无。

