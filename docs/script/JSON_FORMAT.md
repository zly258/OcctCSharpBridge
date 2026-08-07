# OcctScript JSON format

Current document header:

```json
{
  "format": "OcctScript.Document",
  "version": 1
}
```

Files use UTF-8 JSON and are normally saved with `.json` or `.ocsproj`.

## Document

```json
{
  "format": "OcctScript.Document",
  "version": 1,
  "id": "GUID",
  "name": "Example",
  "description": "",
  "lengthUnit": "mm",
  "angleUnit": "deg",
  "parameters": [],
  "commands": [],
  "outputCommandIds": []
}
```

`outputCommandIds` marks the commands considered final outputs. Intermediate commands may remain visible or hidden through their display settings.

## Parameter

```json
{
  "id": "GUID",
  "name": "Width",
  "displayName": "Width",
  "description": "",
  "category": "",
  "type": "length",
  "expression": "1200",
  "unit": "mm",
  "isReadOnly": false,
  "isHidden": false
}
```

Numeric parameter expressions can reference parameters directly:

```text
Width / 2
max(Height, 500)
Radius * sind(30)
```

`${...}` is intentionally not required.

## Command

```json
{
  "id": "GUID",
  "type": "Extrude",
  "name": "Body",
  "notes": "",
  "isEnabled": true,
  "order": 30,
  "fields": {
    "profile": {
      "expression": "",
      "referenceId": "GUID",
      "referenceIds": [],
      "literal": null
    },
    "direction": {
      "expression": "",
      "referenceId": null,
      "referenceIds": [],
      "literal": "0,0,1"
    },
    "distance": {
      "expression": "Height",
      "referenceId": null,
      "referenceIds": [],
      "literal": null
    }
  },
  "transform": {
    "x": 0,
    "y": 0,
    "z": 0,
    "rotationX": 0,
    "rotationY": 0,
    "rotationZ": 0,
    "scale": 1
  },
  "display": {
    "isVisible": true,
    "color": "#8B5CF6",
    "transparency": 0
  }
}
```

## Compatibility rules

- Command type and field keys are stable English identifiers and do not change when the UI language changes.
- Unknown document versions are rejected instead of silently migrated.
- Command references are GUID-based; display names are editable.
- The build planner ignores list order when dependencies require a different build order.
- Circular references and topology-incompatible references are validation errors.

Ready-to-open examples are in [`../../samples/Scripts`](../../samples/Scripts/README.md).
