# OcctAssemblyStyle

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

Presentation style resolved by OCCT for one XDE assembly occurrence.

## 构造函数

### `OcctAssemblyStyle`

Presentation style resolved by OCCT for one XDE assembly occurrence.

```csharp
public OcctAssemblyStyle(bool Visible, OcctAssemblyColor? SurfaceColor, OcctAssemblyColor? CurveColor)
```

## 属性

### `CurveColor`

```csharp
public OcctAssemblyColor? CurveColor { get; set; }
```

### `SurfaceColor`

```csharp
public OcctAssemblyColor? SurfaceColor { get; set; }
```

### `Transparency`

```csharp
public double Transparency { get; }
```

### `Visible`

```csharp
public bool Visible { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(bool Visible, OcctAssemblyColor? SurfaceColor, OcctAssemblyColor? CurveColor)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctAssemblyStyle other)
```

### `GetHashCode`

```csharp
public int GetHashCode()
```

### `ToString`

```csharp
public string ToString()
```

## 字段 / 枚举值

无。

