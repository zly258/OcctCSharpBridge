#include "OcctOcafInternal.hxx"

using namespace OcctOcafInternal;

namespace
{
    std::string asciiValue(const Handle(TCollection_HAsciiString)& value)
    {
        return value.IsNull() ? std::string() : std::string(value->ToCString());
    }

    Handle(TCollection_HAsciiString) asciiHandle(const char* value)
    {
        return new TCollection_HAsciiString(value == nullptr ? "" : value);
    }

    TDF_Label materialLabelForShape(const TDF_Label& shapeLabel)
    {
        Handle(TDataStd_TreeNode) reference;
        if (!shapeLabel.FindAttribute(XCAFDoc::MaterialRefGUID(), reference) || reference.IsNull())
            return {};
        const Handle(TDataStd_TreeNode) materialNode = reference->Father();
        return materialNode.IsNull() ? TDF_Label() : materialNode->Label();
    }

    struct MaterialData
    {
        std::string name;
        std::string description;
        double density = 0.0;
        std::string densityName;
        std::string densityValueType;
    };

    MaterialData readMaterial(const TDF_Label& label)
    {
        Handle(TCollection_HAsciiString) name;
        Handle(TCollection_HAsciiString) description;
        Handle(TCollection_HAsciiString) densityName;
        Handle(TCollection_HAsciiString) densityValueType;
        Standard_Real density = 0.0;
        if (!XCAFDoc_MaterialTool::GetMaterial(label, name, description, density, densityName, densityValueType))
            throw std::runtime_error("Label does not define an XDE material.");
        return {asciiValue(name), asciiValue(description), density, asciiValue(densityName), asciiValue(densityValueType)};
    }
}

extern "C"
{
    int occt_ocaf_xde_set_material(OcctOcafHandle handle, const char* shapeEntry, const char* utf8Name, const char* utf8Description,
                                   double density, const char* utf8DensityName, const char* utf8DensityValueType)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->materialTool()->SetMaterial(
                session->resolve(shapeEntry), asciiHandle(utf8Name), asciiHandle(utf8Description), density,
                asciiHandle(utf8DensityName), asciiHandle(utf8DensityValueType));
        });
    }

    const char* occt_ocaf_xde_material_for_shape(OcctOcafHandle handle, const char* shapeEntry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { return session->entry(materialLabelForShape(session->resolve(shapeEntry))); });
    }

    int occt_ocaf_xde_material_snapshot(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            TDF_LabelSequence labels;
            session->materialTool()->GetMaterialLabels(labels);
            for (int index = 1; index <= labels.Length(); ++index)
                session->stringSnapshot.push_back(session->entry(labels.Value(index)));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_xde_material_name(OcctOcafHandle handle, const char* materialEntry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { return readMaterial(session->resolve(materialEntry)).name; });
    }

    const char* occt_ocaf_xde_material_description(OcctOcafHandle handle, const char* materialEntry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { return readMaterial(session->resolve(materialEntry)).description; });
    }

    double occt_ocaf_xde_material_density(OcctOcafHandle handle, const char* materialEntry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0.0;
        double result = 0.0;
        execute(session, [&] { result = readMaterial(session->resolve(materialEntry)).density; });
        return result;
    }

    const char* occt_ocaf_xde_material_density_name(OcctOcafHandle handle, const char* materialEntry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { return readMaterial(session->resolve(materialEntry)).densityName; });
    }

    const char* occt_ocaf_xde_material_density_value_type(OcctOcafHandle handle, const char* materialEntry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { return readMaterial(session->resolve(materialEntry)).densityValueType; });
    }

    double occt_ocaf_xde_density_for_shape(OcctOcafHandle handle, const char* shapeEntry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0.0;
        double result = 0.0;
        execute(session, [&] { result = XCAFDoc_MaterialTool::GetDensityForShape(session->resolve(shapeEntry)); });
        return result;
    }

    const char* occt_ocaf_xde_material_at(OcctOcafHandle handle, int index)
    {
        return occt_ocaf_xde_shape_at(handle, index);
    }

    int occt_ocaf_xde_set_area(OcctOcafHandle handle, const char* shapeEntry, double area)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { XCAFDoc_Area::Set(session->resolve(shapeEntry), area); });
    }

    int occt_ocaf_xde_get_area(OcctOcafHandle handle, const char* shapeEntry, double* area)
    {
        OcafSession* session = sessionOf(handle);
        if (area == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&] { found = XCAFDoc_Area::Get(session->resolve(shapeEntry), *area) ? 1 : 0; })) return 0;
        return found;
    }

    int occt_ocaf_xde_set_volume(OcctOcafHandle handle, const char* shapeEntry, double volume)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { XCAFDoc_Volume::Set(session->resolve(shapeEntry), volume); });
    }

    int occt_ocaf_xde_get_volume(OcctOcafHandle handle, const char* shapeEntry, double* volume)
    {
        OcafSession* session = sessionOf(handle);
        if (volume == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&] { found = XCAFDoc_Volume::Get(session->resolve(shapeEntry), *volume) ? 1 : 0; })) return 0;
        return found;
    }

    int occt_ocaf_xde_set_centroid(OcctOcafHandle handle, const char* shapeEntry, OcctPoint3d centroid)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { XCAFDoc_Centroid::Set(session->resolve(shapeEntry), gp_Pnt(centroid.x, centroid.y, centroid.z)); });
    }

    int occt_ocaf_xde_get_centroid(OcctOcafHandle handle, const char* shapeEntry, OcctPoint3d* centroid)
    {
        OcafSession* session = sessionOf(handle);
        if (centroid == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            gp_Pnt point;
            found = XCAFDoc_Centroid::Get(session->resolve(shapeEntry), point) ? 1 : 0;
            if (found != 0) *centroid = {point.X(), point.Y(), point.Z()};
        })) return 0;
        return found;
    }
}
