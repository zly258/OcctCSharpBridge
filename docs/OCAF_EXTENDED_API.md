# Extended OCAF / XDE API (OCCT 7.9.0)

This module extends the stable `OcafDocument` facade without exposing OCCT C++ handles or label pointers. Native declarations are in `OcctOcafExtended.h`; .NET consumers use the additional members of `OcafDocument` directly.

## Added document and TDF workflows

- storage format version 2–12 selection;
- modified-label tracking and purge;
- delta compaction initialization/execution and oldest-undo removal;
- label child count, attribute count and transaction index;
- subtree/attribute modification checks and ancestor relationships.

## Added parametric-data workflows

- `TDataStd_Variable` name, numeric value, unit and constant state;
- variable expression assignment and removal;
- standalone `TDataStd_Expression` text and referenced-variable labels;
- `TDataStd_Relation` text and referenced-variable labels.

Variable references are stored as OCAF attribute references, not only as text. Callers should keep the variable names used in expression text consistent with the referenced variable attributes.

## Added XDE shape workflows

- empty top-level shape labels;
- top-level and compound classification;
- component count and reverse user lookup;
- configurable shape search across instances, components and subshapes;
- explicit subshape label creation, lookup and enumeration.

## Added XDE metadata workflows

- reusable color definition creation and lookup;
- assignment by color-definition label and assignment-state queries;
- SHUO instance color set/get and instance visibility query;
- layer lookup, definition checks, assignment checks and reverse shape lookup;
- reusable physical-material definitions and assignment by material label.

## Native boundary

The extended module follows the same ABI rules as the core bridge:

- labels are UTF-8 TDF entry strings;
- shapes are copied through `OcctModelingSession` IDs;
- returned strings and snapshots are valid until the next call using the same `OcafDocument` session;
- one `OcafDocument` instance is not thread-safe and should be externally synchronized;
- exact OCCT version remains 7.9.0.

## Remaining high-value modules

The next independent extension should cover:

1. `XCAFDoc_DimTolTool` and strongly typed PMI/GD&T;
2. `XCAFDoc_VisMaterialTool` and PBR material/texture properties;
3. `XCAFDoc_ViewTool`, notes and clipping planes;
4. additional `TDataStd` list/reference/named-data attributes;
5. TDF copy/relocation workflows and stronger topology-naming diagnostics.

Keeping these as separate modules limits ABI growth and allows each area to have focused persistence and exchange tests.
