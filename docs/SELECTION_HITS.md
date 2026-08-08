# Structured Viewer Selection Hits

`OcctEngine` exposes structured identity for the current AIS selection and detection state without requiring callers to reverse-map raw object IDs or inspect OCCT owners themselves.

```csharp
var selected = engine.GetSelectedHits();
if (engine.TryGetDetectedHit(out var hover))
{
    Console.WriteLine($"{hover.Owner.Id}: {hover.SubshapeType} #{hover.SubshapeIndex}");
}
```

## Data contract

`OcctSelectionHit` contains only data that is actually available and stable in the current implementation:

- `Owner`: the registered `IOcctObject` that owns the selected/detected sensitive entity;
- `SubshapeType`: `Vertex`, `Edge`, `Wire`, `Face`, `Shell`, `Solid`, or `Shape`;
- `SubshapeIndex`: runtime topology index, or `-1` for whole-object selection;
- `IsSubshape`: convenience flag for `SubshapeIndex >= 0`.

The API deliberately does **not** expose a placeholder hit-point property. A hit point should only be added if the native viewer path can provide it with clear and repeatable semantics.

## Batched selected-hit retrieval

`GetSelectedHits()` uses one two-call batch ABI:

```text
occt_selected_hits(handle, null, 0, &count)
→ allocate exact managed buffer
→ occt_selected_hits(handle, buffer, capacity, &count)
```

This replaces the less scalable `count + hit_at(index)` pattern and keeps selected-hit retrieval at two P/Invoke crossings regardless of the number of selected entities.

`TryGetDetectedHit()` uses the single `occt_detected_hit()` call and follows the bridge's normal success/error contract; absence of a detected registered object is reported through `false`, not through a native error sentinel.

## Subshape identity

For BRep subshape selection, `SubshapeIndex` uses the same `TopExp_Explorer` ordering as:

```csharp
engine.GetSubshapeAt(ownerShape, hit.SubshapeType, hit.SubshapeIndex)
```

This makes the hit directly useful during the current modeling interaction for operations such as fillet/chamfer edge selection, shell face removal, measurement, property inspection, or application-level feature commands.

Whole-object selection uses:

```text
SubshapeType  = Shape
SubshapeIndex = -1
```

## Persistence boundary

The index is a **runtime interaction identifier**, not persistent naming. Applications must not serialize `SubshapeIndex` as the only long-term topological reference.

A parametric CAD application should resolve the hit into an application-owned stable reference, for example:

```text
SelectionHit
→ current runtime subshape
→ feature/operation-history semantic reference
→ geometry + adjacency signature fallback
```

Persistent naming, document entities, undo/redo, command state, and semantic feature references remain application-layer responsibilities rather than Bridge responsibilities.

## Native organization

Structured selection state is declared separately from the rubber-band overlay API:

```text
OcctSelectionOverlay.h     2D selection rectangle overlay
OcctSelectionState.h       structured selected/detected identity
OcctSelectionState.cpp     selection state implementation
```

This keeps Viewer selection identity independent from UI overlay rendering.
