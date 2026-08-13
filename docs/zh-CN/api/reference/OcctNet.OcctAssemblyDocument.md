# OcctAssemblyDocument

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

First-class snapshot of a STEP/XDE assembly document. Geometry objects remain owned by the importing , while hierarchy, occurrence transforms and XDE styles are preserved independently of legacy application tags.

## 构造函数

无。

## 属性

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

## 事件

无。

## 方法

无。

## 字段 / 枚举值

无。

