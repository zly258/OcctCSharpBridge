# 03 API Coverage and Design Conventions

The public managed API is split between viewer-oriented and headless-modeling capabilities.

## Main facades

### `OcctEngine`

Covers AIS/viewer lifecycle, registered scene objects, display state, camera, selection, structured hit data, annotations, transforms, appearance and viewport interaction.

### `OcctModelingSession`

Covers primitives, curves, surfaces, B-Rep topology, Boolean and feature operations, healing, inspection, inertia, history, topology references, meshing and STEP/IGES/BREP/STL exchange.

## ABI rules

The native boundary uses fixed-width integers, `double`, plain structs, UTF-8 strings and explicit buffer/capacity contracts. C++ exceptions do not cross the ABI. Managed code converts native status/error information into strong .NET results or exceptions.

The current contract records 344 native exports and 344 managed P/Invoke declarations. Count parity detects missing additions/removals, but signature-level validation remains the stronger long-term goal: return types, parameter order/types, struct size/offset and enum numeric values.

## Ownership

Objects are owner-aware. IDs from one `OcctEngine` or `OcctModelingSession` cannot be passed to another owner merely because their numeric values match.

## Collection transfer

High-volume collections use bulk C ABI calls. New public APIs should not reintroduce legacy indexed loops across the managed/native boundary unless the underlying operation is inherently scalar.

## Compatibility policy

The project is maintained as a current SDK, not a compatibility museum. Deleted aliases, legacy wrappers and aggregate compatibility headers are not recreated to support old demo code. Callers move to the current API.

For exact public types and members, use the generated [Complete API Reference](api/README.md).