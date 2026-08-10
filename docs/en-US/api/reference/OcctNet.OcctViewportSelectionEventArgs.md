# OcctViewportSelectionEventArgs

- **Assembly:** `OcctNet.WinForms.dll`
- **Namespace:** `OcctNet`
- **Inheritance:** `EventArgs`

## Declaration

```csharp
public sealed class OcctViewportSelectionEventArgs
```

## Description

Public API type. See its declaration, member signatures, and conceptual documentation for ownership, lifetime, and behavioral constraints.

## Constructors

### `OcctViewportSelectionEventArgs`

Public API member. Exact parameters, return type, and available XML documentation are listed below.

```csharp
public OcctViewportSelectionEventArgs(IOcctObject selectedObject, IReadOnlyList<IOcctObject> selectedObjects)
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

