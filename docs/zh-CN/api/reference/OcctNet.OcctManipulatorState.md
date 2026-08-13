# OcctManipulatorState

- **程序集:** `OcctNet.dll`
- **命名空间:** `OcctNet`

## 构造函数

### `OcctManipulatorState`

```csharp
public OcctManipulatorState(bool IsAttached, OcctManipulatorMode ActiveMode, int ActiveAxisIndex, bool HasActiveTransformation, bool ModeActivationOnDetection, bool ZoomPersistence, OcctManipulatorSkin Skin, OcctPoint3d Origin, OcctVector3d Normal, OcctVector3d XDirection, double Size)
```

## 属性

### `ActiveAxisIndex`

```csharp
public int ActiveAxisIndex { get; set; }
```

### `ActiveMode`

```csharp
public OcctManipulatorMode ActiveMode { get; set; }
```

### `HasActiveTransformation`

```csharp
public bool HasActiveTransformation { get; set; }
```

### `IsAttached`

```csharp
public bool IsAttached { get; set; }
```

### `ModeActivationOnDetection`

```csharp
public bool ModeActivationOnDetection { get; set; }
```

### `Normal`

```csharp
public OcctVector3d Normal { get; set; }
```

### `Origin`

```csharp
public OcctPoint3d Origin { get; set; }
```

### `Size`

```csharp
public double Size { get; set; }
```

### `Skin`

```csharp
public OcctManipulatorSkin Skin { get; set; }
```

### `XDirection`

```csharp
public OcctVector3d XDirection { get; set; }
```

### `ZoomPersistence`

```csharp
public bool ZoomPersistence { get; set; }
```

## 事件

无。

## 方法

### `Deconstruct`

```csharp
public void Deconstruct(bool IsAttached, OcctManipulatorMode ActiveMode, int ActiveAxisIndex, bool HasActiveTransformation, bool ModeActivationOnDetection, bool ZoomPersistence, OcctManipulatorSkin Skin, OcctPoint3d Origin, OcctVector3d Normal, OcctVector3d XDirection, double Size)
```

### `Equals`

```csharp
public bool Equals(object obj)
```

### `Equals`

```csharp
public bool Equals(OcctManipulatorState other)
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

