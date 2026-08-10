# OcctWpfViewport

- **程序集:** `OcctNet.Wpf.dll`
- **命名空间:** `OcctNet`
- **继承:** `UserControl`

## 声明

```csharp
public sealed class OcctWpfViewport
```

## 说明

Reusable WPF host for the OCCT HWND viewport. The native viewer remains isolated in OcctNet.OcctViewportControl, while WPF applications receive dependency properties, DPI synchronization, resize coordination, and WPF-native event routing.

## 构造函数

### `OcctWpfViewport`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctWpfViewport()
```

## 属性

### `EnableDefaultInteraction`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool EnableDefaultInteraction { get; set; }
```

### `EnableRectangleSelection`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool EnableRectangleSelection { get; set; }
```

### `Engine`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctEngine Engine { get; }
```

### `RectangleSelectionBehavior`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public OcctRectangleSelectionBehavior RectangleSelectionBehavior { get; set; }
```

### `RectangleSelectionFillColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public Color RectangleSelectionFillColor { get; set; }
```

### `RectangleSelectionFillTransparency`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RectangleSelectionFillTransparency { get; set; }
```

### `RectangleSelectionLineColor`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public Color RectangleSelectionLineColor { get; set; }
```

### `RectangleSelectionLineWidth`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public double RectangleSelectionLineWidth { get; set; }
```

### `RectangleSelectionThreshold`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public int RectangleSelectionThreshold { get; set; }
```

### `SynchronizeRenderDpi`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public bool SynchronizeRenderDpi { get; set; }
```

### `WinFormsViewport`

Access to the low-level WinForms HWND host for advanced interoperability.

```csharp
public OcctViewportControl WinFormsViewport { get; }
```

## 事件

### `EngineInitialized`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler EngineInitialized;
```

### `ErrorOccurred`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctViewportErrorEventArgs> ErrorOccurred;
```

### `ObjectSelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctViewportSelectionEventArgs> ObjectSelectionChanged;
```

### `SelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctShape?> SelectionChanged;
```

### `WorldPointChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public event EventHandler<OcctViewportWorldPointEventArgs> WorldPointChanged;
```

## 方法

### `FocusViewport`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void FocusViewport()
```

**返回值:** `void`

### `RaiseSelectionChanged`

公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

```csharp
public void RaiseSelectionChanged()
```

**返回值:** `void`

## 字段 / 枚举值

- `EnableDefaultInteractionProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `EnableRectangleSelectionProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionBehaviorProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionFillColorProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionFillTransparencyProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionLineColorProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionLineWidthProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `RectangleSelectionThresholdProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。
- `SynchronizeRenderDpiProperty` — `DependencyProperty` — 公开 API 成员。精确参数、返回类型和可用 XML Documentation 见本节。

