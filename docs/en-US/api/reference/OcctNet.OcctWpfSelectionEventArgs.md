# OcctWpfSelectionEventArgs

- **Assembly:** `OcctNet.Wpf.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `EventArgs`

## Declaration

```csharp
public sealed class OcctWpfSelectionEventArgs
```

## Description

Selection state reported by the native WPF viewport host.

## Constructors

### `OcctWpfSelectionEventArgs`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctWpfSelectionEventArgs(IOcctObject selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
```

**Parameters**

- `selectedObject` — `IOcctObject`
- `selectedObjects` — `IReadOnlyList<IOcctObject>`

## Properties

### `SelectedObject`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IOcctObject SelectedObject { get; }
```

### `SelectedObjects`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public IReadOnlyList<IOcctObject> SelectedObjects { get; }
```

## Events

None

## Methods

None

## Fields / Enum Values

None

