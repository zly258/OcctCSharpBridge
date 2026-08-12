# OcctAssemblyDocument

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 声明

```csharp
public sealed class OcctAssemblyDocument
```

## 说明

First-class snapshot of a STEP/XDE assembly document. Geometry objects remain owned by the importing OcctNet.OcctEngine, while hierarchy, occurrence transforms and XDE styles are preserved independently of legacy application tags.

## 构造函数

无

## 属性

### `Assemblies`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IEnumerable<OcctAssemblyNode> Assemblies { get; }
```

### `Instances`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IEnumerable<OcctAssemblyNode> Instances { get; }
```

### `Nodes`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctAssemblyNode> Nodes { get; }
```

### `Parts`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IEnumerable<OcctAssemblyNode> Parts { get; }
```

### `PrimaryShape`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctShape PrimaryShape { get; }
```

### `Roots`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public IReadOnlyList<OcctAssemblyNode> Roots { get; }
```

### `SourcePath`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public string SourcePath { get; }
```

## 事件

无

## 方法

无

## 字段 / 枚举值

无

