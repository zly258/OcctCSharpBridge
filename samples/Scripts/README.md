# OcctScript sample documents

All sample files use the same versioned JSON document format and can be opened directly in `OcctScript.Editor`.

| File | Demonstrates |
| --- | --- |
| `01-Curves.json` | Line, polyline, circle, arc, ellipse, regular polygon, Bezier and B-Spline |
| `02-Extrude.json` | Wire → face → solid and edge → face extrusion |
| `03-Revolve.json` | Closed profile → face → revolved solid |
| `04-Sweep.json` | Circular profile swept along a B-Spline spine |
| `05-Loft.json` | Multi-section solid loft |
| `06-Booleans.json` | Cut followed by fuse |
| `07-Primitives-Transforms.json` | Cone, sphere, torus, wedge, move, mirror and scale |
| `08-Edge-Features.json` | Fillet, chamfer and shell operations |

Notes:

- Length values are interpreted in the document's `lengthUnit`; the initial editor uses millimeters.
- Angles are in degrees.
- Expressions reference parameters directly, for example `Width / 2`; `${...}` is not required.
- Command references are stored by GUID, while the editor lets you enter command names.
- Edge/face indices used by fillet, chamfer and shell are zero-based topology indices and may need adjustment if upstream topology changes.
