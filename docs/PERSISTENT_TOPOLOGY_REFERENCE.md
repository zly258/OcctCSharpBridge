# Persistent Topology Reference Design

## Status

Design contract for a future additive Bridge capability. It does **not** introduce OCAF/XDE, does not change Native ABI 3, and does not claim that OCCT subshape traversal indices are persistent names.

## Problem

`OcctSelectionHit.SubshapeIndex` and `OcctModelingSession.GetSubshapeAt(...)` intentionally expose the current `TopExp_Explorer` order. That is useful for interactive work inside one topology state, but the index can change after Boolean operations, fillets, healing, import/export, or feature regeneration.

A CAD application therefore needs a stronger way to refer back to a Face/Edge/Vertex after topology changes. A raw runtime ShapeId, hash code, or traversal index is not sufficient.

## Boundary

The reusable Bridge should provide **topology facts and resolution evidence**. The application owns document identity, Feature/Entity identity, persistence policy, user confirmation, and fallback behavior.

The Bridge must not embed application DocumentId, FeatureId, Command history, or JSON schema into the topology reference.

## Non-goals

A Bridge topology reference is not:

- a guaranteed immutable subshape ID;
- an OCAF/XDE label;
- a `TopExp_Explorer` index persisted as truth;
- a runtime `OcctObjectId` or `OcctModelShape.Id` persisted across sessions;
- a single geometry hash assumed to be globally unique;
- a promise that an ambiguous topology can always be resolved automatically.

## Proposed reference

A future `OcctTopologyReference` should be a versioned, serializable value that contains only neutral geometry/topology evidence.

Recommended fields:

| Field | Purpose |
|---|---|
| `Version` | Allows the fingerprint/resolver algorithm to evolve |
| `ShapeType` | Vertex / Edge / Face |
| `RuntimeIndexHint` | Fast hint only; never authoritative |
| `GeometryType` | Curve/surface analytic type where available |
| `Measure` | Edge length or Face area |
| `Center` | Geometric/mass center used as a spatial signature |
| `Bounds` | AABB or normalized bounds signature |
| `Tolerance` | Original topology tolerance |
| `Orientation` | Secondary evidence, not identity by itself |
| `GeometryParameters` | Radius, axis, normal, etc. for analytic geometry |
| `AdjacencySignature` | Counts/types of neighboring topology |
| `ParentSignature` | Optional evidence from the root/owning shape |

Floating-point values should be normalized with an explicit tolerance policy before hashing or comparison. Exact binary equality is not a valid matching strategy.

## Resolution pipeline

Resolution should return candidates rather than silently choosing a shape too early.

1. **Operation-history mapping** — when the caller has an `OcctOperationId`, inspect `Generated`, `Modified`, and `IsRemoved` first. This is the strongest local evidence and should precede geometric matching.
2. **Type filter** — reject candidates of a different topology type.
3. **Runtime-index hint** — test the old index as a fast candidate, but verify its signature before accepting it.
4. **Geometry filter** — compare analytic curve/surface type and invariant parameters.
5. **Measure/spatial filter** — compare length/area, center and bounds using configured tolerances.
6. **Adjacency comparison** — compare neighboring Face/Edge/Vertex signatures to distinguish geometrically similar candidates.
7. **Score and ambiguity check** — rank remaining candidates and report ambiguity when the best two candidates are too close.

The resolver must never turn an ambiguous result into a false deterministic identity merely to return one object.

## Proposed resolution result

A future result should distinguish at least:

- `Resolved` — one candidate clearly satisfies the policy;
- `Ambiguous` — multiple plausible candidates remain;
- `Removed` — operation history explicitly reports removal;
- `NotFound` — no candidate satisfies the minimum score;
- `InvalidReference` — unsupported version or malformed reference.

Recommended result evidence:

- resolved/candidate `OcctModelShape`;
- score in a documented range;
- score components or match flags;
- candidate count;
- whether operation history was used;
- whether runtime index matched;
- ambiguity margin.

## Scoring guidance

Do not hard-code one universal CAD policy into Native code. The Bridge may provide a conservative default scorer and expose objective match components. A consuming application can apply stricter domain-specific thresholds.

A reasonable default weighting order is:

1. operation-history correspondence;
2. topology and analytic geometry type;
3. invariant analytic parameters;
4. adjacency signature;
5. measure;
6. center/bounds;
7. runtime index hint.

The runtime index deliberately has the lowest semantic weight.

## Operation history

Bridge 2.6 already exposes generated/modified/removed topology history and now retrieves generated/modified collections through bulk Native ABI calls. Persistent-reference resolution should reuse that information rather than build a second, disconnected history mechanism.

History is local to an operation/session and is not itself a cross-file persistent naming system. When history is unavailable, the resolver falls back to geometry/topology evidence.

## Symmetry and repeated geometry

Symmetric and patterned models are inherently ambiguous if two subshapes have the same geometry and equivalent neighborhoods. The API must surface that ambiguity. The application can then use Feature context, user choice, assembly context, or its own semantic identity to decide.

This is especially important for repeated holes, equal-radius fillets, patterned faces and mirrored features.

## Persistence

The Bridge value should be trivially serializable, but the Bridge must not own the persistence format. Applications may store it in JSON, databases, custom documents, or another format.

A stored record should include the reference algorithm version. Old versions can be upgraded or resolved through compatibility logic without freezing the first fingerprint algorithm forever.

## Implementation phases

### Phase 1 — objective fingerprint

Add internal/native helpers to compute:

- type/orientation/tolerance;
- analytic geometry type and stable parameters;
- measure and center;
- bounds;
- local adjacency counts/types.

No public resolver yet.

### Phase 2 — internal candidate resolver

Implement candidate generation and scoring in `OcctModelingTopologyReference.cpp`. Validate against primitives, Boolean changes, fillets, chamfers, same-domain unification and healing.

### Phase 3 — additive public API

Only after the resolver behavior is covered by deterministic tests, expose versioned public DTOs and resolution methods. Keep runtime indices as hints and return explicit ambiguity.

### Phase 4 — application integration

`demo` or a consuming CAD application can bind its own Feature/Entity IDs to the neutral Bridge reference and persist it in its own document model.

## Required tests before public release

At minimum:

- unchanged topology resolves exactly;
- reordered traversal does not break resolution;
- Boolean `Generated` / `Modified` mapping is preferred over fingerprint matching;
- removed topology reports `Removed`;
- equal-radius/repeated features can return `Ambiguous`;
- transform-only changes do not accidentally invalidate invariant geometry;
- tolerance changes stay within the configured policy;
- import/export round trips do not rely on runtime IDs;
- references from one modeling session cannot be used as live handles in another session.

## Decision

Do not expose a simplistic `PersistentSubshapeId` in Bridge 2.6. The correct next step is a neutral, versioned topology fingerprint and ambiguity-aware resolver, with operation history used as the strongest available evidence and application identity kept above the Bridge boundary.
