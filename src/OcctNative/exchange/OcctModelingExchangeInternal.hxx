#pragma once

#include <BRep_Builder.hxx>
#include <BRepTools.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IGESControl_Reader.hxx>
#include <IGESControl_Writer.hxx>
#include <STEPControl_Reader.hxx>
#include <STEPControl_Writer.hxx>
#include <StlAPI_Reader.hxx>
#include <TopoDS_Shape.hxx>

#include <filesystem>
#include <fstream>
#include <stdexcept>
#include <string>

namespace OcctModelingInternal
{
    inline std::ifstream modelInputStream(const std::filesystem::path& path)
    {
        std::ifstream stream(path, std::ios::binary);
        if (!stream) throw std::runtime_error("Unable to open input file.");
        return stream;
    }

    inline std::ofstream modelOutputStream(const std::filesystem::path& path)
    {
        if (path.empty()) throw std::invalid_argument("Path is empty.");
        if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
        std::ofstream stream(path, std::ios::binary | std::ios::trunc);
        if (!stream) throw std::runtime_error("Unable to open output file.");
        return stream;
    }

    inline TopoDS_Shape readModelStep(const std::filesystem::path& path)
    {
        auto stream = modelInputStream(path);
        STEPControl_Reader reader;
        const IFSelect_ReturnStatus status = reader.ReadStream(path.filename().string().c_str(), stream);
        if (status != IFSelect_RetDone) throw std::runtime_error("STEP file could not be read.");
        if (reader.TransferRoots() <= 0) throw std::runtime_error("STEP roots could not be transferred.");
        const TopoDS_Shape shape = reader.OneShape();
        if (shape.IsNull()) throw std::runtime_error("STEP file contains no transferable shape.");
        return shape;
    }

    inline TopoDS_Shape readModelIges(const std::filesystem::path& path)
    {
        auto stream = modelInputStream(path);
        IGESControl_Reader reader;
        const IFSelect_ReturnStatus status = reader.ReadStream(path.filename().string().c_str(), stream);
        if (status != IFSelect_RetDone) throw std::runtime_error("IGES file could not be read.");
        if (reader.TransferRoots() <= 0) throw std::runtime_error("IGES roots could not be transferred.");
        const TopoDS_Shape shape = reader.OneShape();
        if (shape.IsNull()) throw std::runtime_error("IGES file contains no transferable shape.");
        return shape;
    }

    inline TopoDS_Shape readModelBrep(const std::filesystem::path& path)
    {
        auto stream = modelInputStream(path);
        BRep_Builder builder;
        TopoDS_Shape shape;
        BRepTools::Read(shape, stream, builder);
        if (shape.IsNull()) throw std::runtime_error("BREP file contains no readable shape.");
        return shape;
    }

    inline TopoDS_Shape readModelStl(const std::filesystem::path& path)
    {
        TopoDS_Shape shape;
        StlAPI_Reader reader;
        if (!reader.Read(shape, path.string().c_str()))
            throw std::runtime_error("STL file could not be read. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
        if (shape.IsNull()) throw std::runtime_error("STL file contains no readable shape.");
        return shape;
    }

    inline void writeModelStep(const TopoDS_Shape& shape, const std::filesystem::path& path)
    {
        STEPControl_Writer writer;
        if (writer.Transfer(shape, STEPControl_AsIs) != IFSelect_RetDone)
            throw std::runtime_error("Shape could not be transferred to STEP.");
        auto stream = modelOutputStream(path);
        if (writer.WriteStream(stream) != IFSelect_RetDone)
            throw std::runtime_error("STEP file could not be written.");
    }

    inline void writeModelIges(const TopoDS_Shape& shape, const std::filesystem::path& path)
    {
        IGESControl_Writer writer("MM", 1);
        if (!writer.AddShape(shape)) throw std::runtime_error("Shape could not be transferred to IGES.");
        writer.ComputeModel();
        auto stream = modelOutputStream(path);
        if (!writer.Write(stream)) throw std::runtime_error("IGES file could not be written.");
    }
}
