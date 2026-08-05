from pathlib import Path

path = Path("src/OcctNative/OcctModelingGeometry.cpp")
text = path.read_text(encoding="utf-8-sig")

replacements = {
    '            BRepPrimAPI_MakeBox maker(gp_Pnt(x, y, z), dx, dy, dz);\n            if (!maker.IsDone()) throw std::runtime_error("Box creation failed.");\n            return maker.Shape();':
    '            BRepPrimAPI_MakeBox maker(gp_Pnt(x, y, z), dx, dy, dz);\n            maker.Build();\n            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Box creation failed.");\n            return maker.Shape();',
    '            BRepPrimAPI_MakeCylinder maker(toAxis2(origin, axis), radius, height);\n            if (!maker.IsDone()) throw std::runtime_error("Cylinder creation failed.");\n            return maker.Shape();':
    '            BRepPrimAPI_MakeCylinder maker(toAxis2(origin, axis), radius, height);\n            maker.Build();\n            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Cylinder creation failed.");\n            return maker.Shape();',
    '            BRepPrimAPI_MakeCone maker(toAxis2(origin, axis), radius1, radius2, height);\n            if (!maker.IsDone()) throw std::runtime_error("Cone creation failed.");\n            return maker.Shape();':
    '            BRepPrimAPI_MakeCone maker(toAxis2(origin, axis), radius1, radius2, height);\n            maker.Build();\n            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Cone creation failed.");\n            return maker.Shape();',
    '            BRepPrimAPI_MakeSphere maker(toPoint(center), radius);\n            if (!maker.IsDone()) throw std::runtime_error("Sphere creation failed.");\n            return maker.Shape();':
    '            BRepPrimAPI_MakeSphere maker(toPoint(center), radius);\n            maker.Build();\n            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Sphere creation failed.");\n            return maker.Shape();',
    '            BRepPrimAPI_MakeTorus maker(toAxis2(center, axis), majorRadius, minorRadius);\n            if (!maker.IsDone()) throw std::runtime_error("Torus creation failed.");\n            return maker.Shape();':
    '            BRepPrimAPI_MakeTorus maker(toAxis2(center, axis), majorRadius, minorRadius);\n            maker.Build();\n            if (!maker.IsDone() || maker.Shape().IsNull()) throw std::runtime_error("Torus creation failed.");\n            return maker.Shape();',
}

for old, new in replacements.items():
    if old in text:
        text = text.replace(old, new)
    elif new not in text:
        raise SystemExit(f"Expected primitive block not found: {old.splitlines()[0].strip()}")

path.write_text(text, encoding="utf-8", newline="\n")
print("Updated lazy BRepPrimAPI primitive construction.")
