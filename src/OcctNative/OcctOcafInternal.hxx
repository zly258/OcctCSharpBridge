#pragma once

#include "OcctOcaf.h"
#include "OcctModelingInternal.hxx"

#include <Standard_Version.hxx>

static_assert(OCC_VERSION_MAJOR == 7 && OCC_VERSION_MINOR == 9 && OCC_VERSION_MAINTENANCE == 0,
              "OcctCSharpBridge OCAF wrapper requires exactly OCCT 7.9.0.");

#include <PCDM_ReaderStatus.hxx>
#include <PCDM_StoreStatus.hxx>
#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TCollection_HAsciiString.hxx>
#include <TDF_Attribute.hxx>
#include <TDF_AttributeIterator.hxx>
#include <TDF_ChildIterator.hxx>
#include <TDF_Label.hxx>
#include <TDF_LabelSequence.hxx>
#include <TDF_Reference.hxx>
#include <TDF_Tool.hxx>
#include <TDataStd_AsciiString.hxx>
#include <TDataStd_BooleanArray.hxx>
#include <TDataStd_ByteArray.hxx>
#include <TDataStd_Comment.hxx>
#include <TDataStd_ExtStringArray.hxx>
#include <TDataStd_Integer.hxx>
#include <TDataStd_IntegerArray.hxx>
#include <TDataStd_Name.hxx>
#include <TDataStd_Real.hxx>
#include <TDataStd_RealArray.hxx>
#include <TDataStd_UAttribute.hxx>
#include <TDataStd_TreeNode.hxx>
#include <TDataXtd_Position.hxx>
#include <TDataXtd_Shape.hxx>
#include <TDocStd_Application.hxx>
#include <TDocStd_Document.hxx>
#include <TNaming_Builder.hxx>
#include <TNaming_Iterator.hxx>
#include <TNaming_NamedShape.hxx>
#include <TDF_LabelMap.hxx>
#include <TNaming_Selector.hxx>
#include <TopLoc_Location.hxx>
#include <XCAFApp_Application.hxx>
#include <XCAFDoc.hxx>
#include <XCAFDoc_Centroid.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_LayerTool.hxx>
#include <XCAFDoc_Material.hxx>
#include <XCAFDoc_MaterialTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFDoc_Volume.hxx>
#include <XCAFDoc_Area.hxx>
#include <IGESCAFControl_Reader.hxx>
#include <IGESCAFControl_Writer.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <Quantity_Color.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <Standard_GUID.hxx>
#include <Standard_Type.hxx>
#include <gp_Trsf.hxx>

#include <algorithm>
#include <array>
#include <filesystem>
#include <functional>
#include <sstream>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>

namespace OcctOcafInternal
{
    struct AttributeInfo
    {
        Handle(TDF_Attribute) attribute;
        std::string type;
        std::string guid;
    };

    struct NamedShapePair
    {
        OcctObjectId oldShapeId = 0;
        OcctObjectId newShapeId = 0;
    };

    struct OcafSession
    {
        Handle(XCAFApp_Application) application;
        Handle(TDocStd_Document) document;
        std::filesystem::path path;
        std::string lastError;
        std::string scratchString;
        std::vector<std::string> stringSnapshot;
        std::vector<AttributeInfo> attributeSnapshot;
        std::vector<int> integerSnapshot;
        std::vector<double> realSnapshot;
        std::vector<std::string> arrayStringSnapshot;
        int arrayLower = 0;
        std::vector<NamedShapePair> namedShapePairs;

        OcafSession()
            : application(XCAFApp_Application::GetApplication())
        {
            if (application.IsNull())
                throw std::runtime_error("XCAFApp_Application is unavailable.");
        }

        void requireDocument() const
        {
            if (document.IsNull()) throw std::runtime_error("No OCAF document is open.");
        }

        TDF_Label resolve(const char* entry, bool create = false) const
        {
            requireDocument();
            if (entry == nullptr || *entry == '\0') throw std::invalid_argument("Label entry must not be empty.");
            TDF_Label label;
            TDF_Tool::Label(document->GetData(), TCollection_AsciiString(entry), label, create);
            if (label.IsNull())
                throw std::invalid_argument(std::string("Label does not exist: ") + entry);
            return label;
        }

        std::string entry(const TDF_Label& label) const
        {
            if (label.IsNull()) return {};
            TCollection_AsciiString value;
            TDF_Tool::Entry(label, value);
            return value.ToCString();
        }

        const char* setScratch(std::string value)
        {
            scratchString = std::move(value);
            return scratchString.c_str();
        }

        Handle(XCAFDoc_ShapeTool) shapeTool() const
        {
            requireDocument();
            return XCAFDoc_DocumentTool::ShapeTool(document->Main());
        }

        Handle(XCAFDoc_ColorTool) colorTool() const
        {
            requireDocument();
            return XCAFDoc_DocumentTool::ColorTool(document->Main());
        }

        Handle(XCAFDoc_LayerTool) layerTool() const
        {
            requireDocument();
            return XCAFDoc_DocumentTool::LayerTool(document->Main());
        }

        Handle(XCAFDoc_MaterialTool) materialTool() const
        {
            requireDocument();
            return XCAFDoc_DocumentTool::MaterialTool(document->Main());
        }
    };

    inline OcafSession* sessionOf(OcctOcafHandle handle)
    {
        return static_cast<OcafSession*>(handle);
    }

    inline OcctModelingInternal::ModelSession* modelOf(OcctModelHandle handle)
    {
        return OcctModelingInternal::modelOf(handle);
    }

    inline std::string failureMessage(const Standard_Failure& failure)
    {
        const char* text = failure.GetMessageString();
        return text == nullptr ? "Open CASCADE OCAF operation failed." : text;
    }

    template<typename Function>
    inline int execute(OcafSession* session, Function&& function)
    {
        if (session == nullptr) return 0;
        session->lastError.clear();
        try
        {
            function();
            return 1;
        }
        catch (const Standard_Failure& failure)
        {
            session->lastError = failureMessage(failure);
        }
        catch (const std::exception& exception)
        {
            session->lastError = exception.what();
        }
        catch (...)
        {
            session->lastError = "Unknown native OCAF error.";
        }
        return 0;
    }

    template<typename Function>
    inline const char* executeString(OcafSession* session, Function&& function)
    {
        if (session == nullptr) return "";
        session->scratchString.clear();
        execute(session, [&] { session->scratchString = function(); });
        return session->scratchString.c_str();
    }

    inline TCollection_ExtendedString extended(const char* utf8)
    {
        if (utf8 == nullptr) throw std::invalid_argument("UTF-8 string must not be null.");
        return TCollection_ExtendedString(utf8, Standard_True);
    }

    inline std::string utf8(const TCollection_ExtendedString& value)
    {
        const int length = value.LengthOfCString();
        std::vector<char> buffer(static_cast<std::size_t>(std::max(length, 0)) + 1, '\0');
        value.ToUTF8CString(buffer.data());
        return buffer.data();
    }

    inline std::string guidString(const Standard_GUID& guid)
    {
        std::array<char, 64> buffer{};
        guid.ToCString(buffer.data());
        return buffer.data();
    }

    inline Standard_GUID parseGuid(const char* guid)
    {
        if (guid == nullptr || *guid == '\0') throw std::invalid_argument("GUID must not be empty.");
        return Standard_GUID(guid);
    }

    inline std::filesystem::path pathFromUtf8(const char* utf8Path)
    {
        if (utf8Path == nullptr || *utf8Path == '\0') throw std::invalid_argument("File path must not be empty.");
        return std::filesystem::u8path(utf8Path);
    }

    inline TCollection_ExtendedString extendedPath(const std::filesystem::path& path)
    {
        const std::string value = path.u8string();
        return TCollection_ExtendedString(value.c_str(), Standard_True);
    }

    inline gp_Trsf transformOf(const OcctModelLocation& value)
    {
        gp_Trsf transform;
        transform.SetValues(
            value.m11, value.m12, value.m13, value.m14,
            value.m21, value.m22, value.m23, value.m24,
            value.m31, value.m32, value.m33, value.m34);
        return transform;
    }

    inline void fillLocation(const TopLoc_Location& location, OcctModelLocation& result)
    {
        const gp_Trsf transform = location.Transformation();
        result.m11 = transform.Value(1, 1); result.m12 = transform.Value(1, 2); result.m13 = transform.Value(1, 3); result.m14 = transform.Value(1, 4);
        result.m21 = transform.Value(2, 1); result.m22 = transform.Value(2, 2); result.m23 = transform.Value(2, 3); result.m24 = transform.Value(2, 4);
        result.m31 = transform.Value(3, 1); result.m32 = transform.Value(3, 2); result.m33 = transform.Value(3, 3); result.m34 = transform.Value(3, 4);
        result.m41 = 0.0; result.m42 = 0.0; result.m43 = 0.0; result.m44 = 1.0;
    }

    inline XCAFDoc_ColorType colorType(int value)
    {
        switch (value)
        {
            case OcctOcafColor_General: return XCAFDoc_ColorGen;
            case OcctOcafColor_Surface: return XCAFDoc_ColorSurf;
            case OcctOcafColor_Curve: return XCAFDoc_ColorCurv;
            default: throw std::invalid_argument("Unknown XCAF color type.");
        }
    }

    inline Quantity_ColorRGBA toColor(OcctOcafColor value)
    {
        const auto clamp = [](double component) { return static_cast<float>(std::clamp(component, 0.0, 1.0)); };
        return Quantity_ColorRGBA(clamp(value.red), clamp(value.green), clamp(value.blue), clamp(value.alpha));
    }

    inline void fillColor(const Quantity_ColorRGBA& value, OcctOcafColor& result)
    {
        result.red = value.GetRGB().Red();
        result.green = value.GetRGB().Green();
        result.blue = value.GetRGB().Blue();
        result.alpha = value.Alpha();
    }

    inline Handle(TNaming_NamedShape) namedShape(const TDF_Label& label)
    {
        Handle(TNaming_NamedShape) attribute;
        if (!label.FindAttribute(TNaming_NamedShape::GetID(), attribute) || attribute.IsNull())
            throw std::runtime_error("Label has no TNaming_NamedShape attribute.");
        return attribute;
    }

    inline TopoDS_Shape modelShape(OcctModelHandle modelHandle, OcctObjectId shapeId)
    {
        auto* model = modelOf(modelHandle);
        if (model == nullptr) throw std::invalid_argument("Invalid modeling session handle.");
        return model->requireShape(shapeId);
    }

    inline OcctObjectId addModelShape(OcctModelHandle modelHandle, const TopoDS_Shape& shape)
    {
        auto* model = modelOf(modelHandle);
        if (model == nullptr) throw std::invalid_argument("Invalid modeling session handle.");
        return model->addShape(shape);
    }

    inline std::string attributeJson(const Handle(TDF_Attribute)& attribute, int depth)
    {
        if (attribute.IsNull()) return "{}";
        std::ostringstream stream;
        attribute->DumpJson(stream, depth);
        return stream.str();
    }
}
