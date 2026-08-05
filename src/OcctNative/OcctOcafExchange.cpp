#include "OcctOcafInternal.hxx"

#include <IFSelect_ReturnStatus.hxx>
#include <STEPControl_StepModelType.hxx>

using namespace OcctOcafInternal;

namespace
{
    std::string exchangePath(const char* utf8Path, bool createParent)
    {
        std::filesystem::path path = std::filesystem::absolute(pathFromUtf8(utf8Path));
        if (createParent && path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
        return path.u8string();
    }
}

extern "C"
{
    int occt_ocaf_import_step(OcctOcafHandle handle, const char* utf8Path)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            const std::string path = exchangePath(utf8Path, false);
            if (!std::filesystem::exists(std::filesystem::u8path(path)))
                throw std::invalid_argument("STEP file does not exist.");
            STEPCAFControl_Reader reader;
            const IFSelect_ReturnStatus status = reader.ReadFile(path.c_str());
            if (status != IFSelect_RetDone)
                throw std::runtime_error("STEPCAF read failed with IFSelect status " + std::to_string(static_cast<int>(status)) + ".");
            if (!reader.Transfer(session->document))
                throw std::runtime_error("STEPCAF transfer into the XDE document failed.");
            session->shapeTool()->UpdateAssemblies();
        });
    }

    int occt_ocaf_export_step(OcctOcafHandle handle, const char* utf8Path)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            const std::string path = exchangePath(utf8Path, true);
            session->shapeTool()->UpdateAssemblies();
            STEPCAFControl_Writer writer;
            if (!writer.Transfer(session->document, STEPControl_AsIs))
                throw std::runtime_error("STEPCAF document transfer failed.");
            const IFSelect_ReturnStatus status = writer.Write(path.c_str());
            if (status != IFSelect_RetDone)
                throw std::runtime_error("STEPCAF write failed with IFSelect status " + std::to_string(static_cast<int>(status)) + ".");
        });
    }

    int occt_ocaf_import_iges(OcctOcafHandle handle, const char* utf8Path)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            const std::string path = exchangePath(utf8Path, false);
            if (!std::filesystem::exists(std::filesystem::u8path(path)))
                throw std::invalid_argument("IGES file does not exist.");
            IGESCAFControl_Reader reader;
            if (!reader.Perform(path.c_str(), session->document))
                throw std::runtime_error("IGESCAF import failed.");
            session->shapeTool()->UpdateAssemblies();
        });
    }

    int occt_ocaf_export_iges(OcctOcafHandle handle, const char* utf8Path)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            const std::string path = exchangePath(utf8Path, true);
            session->shapeTool()->UpdateAssemblies();
            IGESCAFControl_Writer writer;
            if (!writer.Perform(session->document, path.c_str()))
                throw std::runtime_error("IGESCAF export failed.");
        });
    }
}
