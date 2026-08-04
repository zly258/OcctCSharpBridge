#include "OcctInternal.hxx"

#include <BRep_Builder.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRepTools.hxx>
#include <IGESControl_Reader.hxx>
#include <IGESControl_Writer.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <STEPControl_Reader.hxx>
#include <STEPControl_Writer.hxx>
#include <StlAPI_Reader.hxx>
#include <StlAPI_Writer.hxx>
#include <TopoDS_Compound.hxx>

#include <fstream>

using namespace OcctBridge;

namespace
{
    ObjectEntry& requiredShape(Engine* engine, OcctObjectId id)
    {
        ObjectEntry* entry=engine->findShape(id);if(entry==nullptr)throw std::invalid_argument("Shape ID does not exist.");return *entry;
    }

    std::ifstream inputStream(const std::filesystem::path& path)
    {
        std::ifstream stream(path,std::ios::binary);if(!stream)throw std::runtime_error("Unable to open input file.");return stream;
    }

    std::ofstream outputStream(const std::filesystem::path& path)
    {
        if(path.has_parent_path())std::filesystem::create_directories(path.parent_path());
        std::ofstream stream(path,std::ios::binary|std::ios::trunc);if(!stream)throw std::runtime_error("Unable to open output file.");return stream;
    }

    TopoDS_Shape allShapes(Engine* engine)
    {
        BRep_Builder builder;TopoDS_Compound compound;builder.MakeCompound(compound);int count=0;
        for(const auto& pair:engine->objects){if(pair.second.kind==OcctObject_Shape&&!pair.second.shape.IsNull()){builder.Add(compound,pair.second.shape);++count;}}
        if(count==0)throw std::runtime_error("There are no shapes to export.");return compound;
    }

    TopoDS_Shape readStep(const std::filesystem::path& path)
    {
        auto stream=inputStream(path);STEPControl_Reader reader;const IFSelect_ReturnStatus status=reader.ReadStream(path.filename().string().c_str(),stream);
        if(status!=IFSelect_RetDone)throw std::runtime_error("STEP file could not be read.");if(reader.TransferRoots()<=0)throw std::runtime_error("STEP roots could not be transferred.");TopoDS_Shape shape=reader.OneShape();if(shape.IsNull())throw std::runtime_error("STEP file contains no transferable shape.");return shape;
    }

    TopoDS_Shape readIges(const std::filesystem::path& path)
    {
        auto stream=inputStream(path);IGESControl_Reader reader;const IFSelect_ReturnStatus status=reader.ReadStream(path.filename().string().c_str(),stream);
        if(status!=IFSelect_RetDone)throw std::runtime_error("IGES file could not be read.");if(reader.TransferRoots()<=0)throw std::runtime_error("IGES roots could not be transferred.");TopoDS_Shape shape=reader.OneShape();if(shape.IsNull())throw std::runtime_error("IGES file contains no transferable shape.");return shape;
    }

    TopoDS_Shape readBrep(const std::filesystem::path& path)
    {
        auto stream=inputStream(path);BRep_Builder builder;TopoDS_Shape shape;BRepTools::Read(shape,stream,builder);if(shape.IsNull())throw std::runtime_error("BREP file contains no readable shape.");return shape;
    }

    TopoDS_Shape readStl(const std::filesystem::path& path)
    {
        TopoDS_Shape shape;StlAPI_Reader reader;if(!reader.Read(shape,path.string().c_str()))throw std::runtime_error("STL file could not be read. Use an ASCII-only file path if the OCCT package lacks wide-path support.");return shape;
    }

    void writeStep(const TopoDS_Shape& shape,const std::filesystem::path& path)
    {
        STEPControl_Writer writer;if(writer.Transfer(shape,STEPControl_AsIs)!=IFSelect_RetDone)throw std::runtime_error("Shape could not be transferred to STEP.");auto stream=outputStream(path);if(writer.WriteStream(stream)!=IFSelect_RetDone)throw std::runtime_error("STEP file could not be written.");
    }

    void writeIges(const TopoDS_Shape& shape,const std::filesystem::path& path)
    {
        IGESControl_Writer writer("MM",1);if(!writer.AddShape(shape))throw std::runtime_error("Shape could not be transferred to IGES.");writer.ComputeModel();auto stream=outputStream(path);if(!writer.Write(stream))throw std::runtime_error("IGES file could not be written.");
    }
}

extern "C"
{
    OcctObjectId occt_import_step(OcctHandle h,const char* utf8Path){Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{const auto path=pathFromUtf8(utf8Path);if(path.empty())throw std::invalid_argument("Path is empty.");return e->addShape(readStep(path),true,path.stem().u8string());});}
    OcctObjectId occt_import_iges(OcctHandle h,const char* utf8Path){Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{const auto path=pathFromUtf8(utf8Path);if(path.empty())throw std::invalid_argument("Path is empty.");return e->addShape(readIges(path),true,path.stem().u8string());});}
    OcctObjectId occt_import_brep(OcctHandle h,const char* utf8Path){Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{const auto path=pathFromUtf8(utf8Path);if(path.empty())throw std::invalid_argument("Path is empty.");return e->addShape(readBrep(path),true,path.stem().u8string());});}
    OcctObjectId occt_import_stl(OcctHandle h,const char* utf8Path){Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return executeObject(e,[&]{const auto path=pathFromUtf8(utf8Path);if(path.empty())throw std::invalid_argument("Path is empty.");return e->addShape(readStl(path),true,path.stem().u8string());});}

    OcctObjectId occt_import_file(OcctHandle h,const char* utf8Path)
    {
        const auto path=pathFromUtf8(utf8Path);const std::string extension=lowerExtension(path);
        if(extension==".step"||extension==".stp")return occt_import_step(h,utf8Path);
        if(extension==".iges"||extension==".igs")return occt_import_iges(h,utf8Path);
        if(extension==".brep"||extension==".rle")return occt_import_brep(h,utf8Path);
        if(extension==".stl")return occt_import_stl(h,utf8Path);
        Engine* e=engineOf(h);if(e)e->setError("Unsupported file extension. Supported: STEP, IGES, BREP and STL.");return 0;
    }

    int occt_export_step(OcctHandle h,OcctObjectId id,const char* utf8Path){Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{writeStep(requiredShape(e,id).shape,pathFromUtf8(utf8Path));});}
    int occt_export_all_step(OcctHandle h,const char* utf8Path){Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{writeStep(allShapes(e),pathFromUtf8(utf8Path));});}
    int occt_export_iges(OcctHandle h,OcctObjectId id,const char* utf8Path){Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{writeIges(requiredShape(e,id).shape,pathFromUtf8(utf8Path));});}
    int occt_export_all_iges(OcctHandle h,const char* utf8Path){Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{writeIges(allShapes(e),pathFromUtf8(utf8Path));});}

    int occt_export_brep(OcctHandle h,OcctObjectId id,const char* utf8Path)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]{auto stream=outputStream(pathFromUtf8(utf8Path));BRepTools::Write(requiredShape(e,id).shape,stream);if(!stream)throw std::runtime_error("BREP file could not be written.");});
    }

    int occt_export_stl(OcctHandle h,OcctObjectId id,const char* utf8Path,double linearDeflection,double angularDeflection,int asciiMode)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;return execute(e,[&]
        {
            requirePositive(linearDeflection,"Linear deflection");requirePositive(angularDeflection,"Angular deflection");ObjectEntry& shape=requiredShape(e,id);BRepMesh_IncrementalMesh mesh(shape.shape,linearDeflection,Standard_False,angularDeflection,Standard_True);mesh.Perform();if(!mesh.IsDone())throw std::runtime_error("STL meshing failed.");
            const auto path=pathFromUtf8(utf8Path);if(path.has_parent_path())std::filesystem::create_directories(path.parent_path());StlAPI_Writer writer;writer.ASCIIMode()=asciiMode!=0;if(!writer.Write(shape.shape,path.string().c_str()))throw std::runtime_error("STL file could not be written. Use an ASCII-only file path if the OCCT package lacks wide-path support.");
        });
    }
}
