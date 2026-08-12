# OcctAssemblyDocument

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

## Declaration

```csharp
public sealed class OcctAssemblyDocument
```

## Description

First-class snapshot of a STEP/XDE assembly document. Geometry objects remain owned by the importing OcctNet.OcctEngine, while hierarchy, occurrence transforms and XDE styles are preserved independently of legacy application tags.

## Constructors

None

## Properties

### `Assemblies`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IEnumerable<OcctAssemblyNode> Assemblies { get; }
```

### `Instances`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IEnumerable<OcctAssemblyNode> Instances { get; }
```

### `Nodes`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctAssemblyNode> Nodes { get; }
```

### `Parts`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IEnumerable<OcctAssemblyNode> Parts { get; }
```

### `PrimaryShape`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctShape PrimaryShape { get; }
```

### `Roots`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<OcctAssemblyNode> Roots { get; }
```

### `SourcePath`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public string SourcePath { get; }
```

## Events

None

## Methods

None

## Fields / Enum Values

None

