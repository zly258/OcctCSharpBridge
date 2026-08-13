# OcctAssemblyDocument

- **Assembly:** `OcctNet.dll`
- **Namespace:** `OcctNet`

First-class snapshot of a STEP/XDE assembly document. Geometry objects remain owned by the importing , while hierarchy, occurrence transforms and XDE styles are preserved independently of legacy application tags.

## Constructors

None.

## Properties

### `Assemblies`

```csharp
public IEnumerable<OcctAssemblyNode> Assemblies { get; }
```

### `Instances`

```csharp
public IEnumerable<OcctAssemblyNode> Instances { get; }
```

### `Nodes`

```csharp
public IReadOnlyList<OcctAssemblyNode> Nodes { get; }
```

### `Parts`

```csharp
public IEnumerable<OcctAssemblyNode> Parts { get; }
```

### `PrimaryShape`

```csharp
public OcctShape PrimaryShape { get; }
```

### `Roots`

```csharp
public IReadOnlyList<OcctAssemblyNode> Roots { get; }
```

### `SourcePath`

```csharp
public string SourcePath { get; }
```

## Events

None.

## Methods

None.

## Fields / Enum Values

None.

