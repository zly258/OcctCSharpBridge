#include "OcctOcafInternal.hxx"

using namespace OcctOcafInternal;

extern "C"
{
    OcctOcafHandle occt_ocaf_create()
    {
        try { return new OcafSession(); }
        catch (...) { return nullptr; }
    }

    void occt_ocaf_destroy(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session != nullptr && !session->document.IsNull())
        {
            try { session->application->Close(session->document); }
            catch (...) {}
        }
        delete session;
    }

    const char* occt_ocaf_last_error(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session == nullptr ? "Invalid OCAF handle." : session->lastError.c_str();
    }

    const char* occt_ocaf_version()
    {
        return "7.9.0";
    }

    const char* occt_ocaf_capabilities()
    {
        return "occt=7.9.0;ocaf-document;tdf-labels;tdata-attributes;transactions;undo-redo;tnaming;xde-shapes;assemblies;colors;layers;materials;validation-properties;binxcaf;xmlxcaf;stepcaf;igescaf;modeling-interop";
    }

    int occt_ocaf_new_document(OcctOcafHandle handle, const char* utf8Format)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            if (!session->document.IsNull()) session->application->Close(session->document);
            const TCollection_ExtendedString format = extended(utf8Format == nullptr || *utf8Format == '\0' ? "BinXCAF" : utf8Format);
            session->application->NewDocument(format, session->document);
            if (session->document.IsNull()) throw std::runtime_error("OCAF document creation failed.");
            session->path.clear();
        });
    }

    int occt_ocaf_open_document(OcctOcafHandle handle, const char* utf8Path)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            const std::filesystem::path path = pathFromUtf8(utf8Path);
            if (!std::filesystem::exists(path)) throw std::invalid_argument("OCAF document file does not exist.");
            if (!session->document.IsNull()) session->application->Close(session->document);
            Handle(TDocStd_Document) document;
            const PCDM_ReaderStatus status = session->application->Open(extendedPath(path), document);
            if (status != PCDM_RS_OK || document.IsNull())
                throw std::runtime_error("OCAF document open failed with PCDM status " + std::to_string(static_cast<int>(status)) + ".");
            session->document = document;
            session->path = std::filesystem::absolute(path);
        });
    }

    int occt_ocaf_save_document(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            if (session->path.empty()) throw std::runtime_error("The document has no path. Use SaveAs first.");
            const PCDM_StoreStatus status = session->application->SaveAs(session->document, extendedPath(session->path));
            if (status != PCDM_SS_OK)
                throw std::runtime_error("OCAF document save failed with PCDM status " + std::to_string(static_cast<int>(status)) + ".");
        });
    }

    int occt_ocaf_save_as(OcctOcafHandle handle, const char* utf8Path)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            const std::filesystem::path path = std::filesystem::absolute(pathFromUtf8(utf8Path));
            if (path.has_parent_path()) std::filesystem::create_directories(path.parent_path());
            const PCDM_StoreStatus status = session->application->SaveAs(session->document, extendedPath(path));
            if (status != PCDM_SS_OK)
                throw std::runtime_error("OCAF document save failed with PCDM status " + std::to_string(static_cast<int>(status)) + ".");
            session->path = path;
        });
    }

    int occt_ocaf_close_document(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            session->application->Close(session->document);
            session->document.Nullify();
            session->path.clear();
            session->stringSnapshot.clear();
            session->attributeSnapshot.clear();
            session->integerSnapshot.clear();
            session->realSnapshot.clear();
            session->arrayStringSnapshot.clear();
            session->namedShapePairs.clear();
        });
    }

    int occt_ocaf_is_open(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() ? 1 : 0;
    }

    const char* occt_ocaf_document_path(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return "";
        return executeString(session, [&] { return session->path.empty() ? std::string() : session->path.u8string(); });
    }

    const char* occt_ocaf_storage_format(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            session->requireDocument();
            return utf8(session->document->StorageFormat());
        });
    }

    int occt_ocaf_change_storage_format(OcctOcafHandle handle, const char* utf8Format)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            session->document->ChangeStorageFormat(extended(utf8Format));
        });
    }

    int occt_ocaf_is_saved(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() && session->document->IsSaved() ? 1 : 0;
    }

    int occt_ocaf_is_changed(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() && session->document->IsChanged() ? 1 : 0;
    }

    int occt_ocaf_is_empty(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() && session->document->IsEmpty() ? 1 : 0;
    }

    int occt_ocaf_is_valid(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() && session->document->IsValid() ? 1 : 0;
    }

    const char* occt_ocaf_document_json(OcctOcafHandle handle, int depth)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            session->requireDocument();
            std::ostringstream stream;
            session->document->DumpJson(stream, depth);
            return stream.str();
        });
    }

    int occt_ocaf_new_command(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->NewCommand(); });
    }

    int occt_ocaf_open_command(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->OpenCommand(); });
    }

    int occt_ocaf_commit_command(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int committed = 0;
        if (!execute(session, [&] { session->requireDocument(); committed = session->document->CommitCommand() ? 1 : 0; })) return 0;
        return committed;
    }

    int occt_ocaf_abort_command(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->AbortCommand(); });
    }

    int occt_ocaf_has_open_command(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() && session->document->HasOpenCommand() ? 1 : 0;
    }

    int occt_ocaf_get_undo_limit(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || session->document.IsNull()) return 0;
        return session->document->GetUndoLimit();
    }

    int occt_ocaf_set_undo_limit(OcctOcafHandle handle, int limit)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->SetUndoLimit(limit); });
    }

    int occt_ocaf_available_undos(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session == nullptr || session->document.IsNull() ? 0 : session->document->GetAvailableUndos();
    }

    int occt_ocaf_available_redos(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session == nullptr || session->document.IsNull() ? 0 : session->document->GetAvailableRedos();
    }

    int occt_ocaf_undo(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        if (!execute(session, [&] { session->requireDocument(); result = session->document->Undo() ? 1 : 0; })) return 0;
        return result;
    }

    int occt_ocaf_redo(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        if (!execute(session, [&] { session->requireDocument(); result = session->document->Redo() ? 1 : 0; })) return 0;
        return result;
    }

    int occt_ocaf_clear_undos(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->ClearUndos(); });
    }

    int occt_ocaf_clear_redos(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->ClearRedos(); });
    }

    int occt_ocaf_set_nested_transaction_mode(OcctOcafHandle handle, int enabled)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->SetNestedTransactionMode(enabled != 0); });
    }

    int occt_ocaf_nested_transaction_mode(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() && session->document->IsNestedTransactionMode() ? 1 : 0;
    }

    int occt_ocaf_set_modification_mode(OcctOcafHandle handle, int enabled)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->SetModificationMode(enabled != 0); });
    }

    int occt_ocaf_modification_mode(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() && session->document->ModificationMode() ? 1 : 0;
    }

    int occt_ocaf_set_empty_labels_saving_mode(OcctOcafHandle handle, int enabled)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->requireDocument(); session->document->SetEmptyLabelsSavingMode(enabled != 0); });
    }

    int occt_ocaf_empty_labels_saving_mode(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return session != nullptr && !session->document.IsNull() && session->document->EmptyLabelsSavingMode() ? 1 : 0;
    }

    const char* occt_ocaf_root_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { session->requireDocument(); return session->entry(session->document->GetData()->Root()); });
    }

    const char* occt_ocaf_main_entry(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { session->requireDocument(); return session->entry(session->document->Main()); });
    }

    int occt_ocaf_label_exists(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || session->document.IsNull() || entry == nullptr) return 0;
        try
        {
            TDF_Label label;
            TDF_Tool::Label(session->document->GetData(), TCollection_AsciiString(entry), label, Standard_False);
            return label.IsNull() ? 0 : 1;
        }
        catch (...) { return 0; }
    }

    int occt_ocaf_create_label(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { (void)session->resolve(entry, true); });
    }

    const char* occt_ocaf_new_child(OcctOcafHandle handle, const char* parentEntry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { return session->entry(session->resolve(parentEntry).NewChild()); });
    }

    const char* occt_ocaf_find_child(OcctOcafHandle handle, const char* parentEntry, int tag, int create)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            TDF_Label child = session->resolve(parentEntry).FindChild(tag, create != 0);
            if (child.IsNull()) throw std::runtime_error("Child label does not exist.");
            return session->entry(child);
        });
    }

    const char* occt_ocaf_father(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&] { return session->entry(session->resolve(entry).Father()); });
    }

    int occt_ocaf_label_tag(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = session->resolve(entry).Tag(); });
        return result;
    }

    int occt_ocaf_label_depth(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return -1;
        int result = -1;
        execute(session, [&] { result = session->resolve(entry).Depth(); });
        return result;
    }

    int occt_ocaf_label_is_root(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = session->resolve(entry).IsRoot() ? 1 : 0; });
        return result;
    }

    int occt_ocaf_label_is_imported(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = session->resolve(entry).IsImported() ? 1 : 0; });
        return result;
    }

    int occt_ocaf_set_label_imported(OcctOcafHandle handle, const char* entry, int imported)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->resolve(entry).Imported(imported != 0); });
    }

    int occt_ocaf_child_snapshot(OcctOcafHandle handle, const char* entry, int recursive)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->stringSnapshot.clear();
            for (TDF_ChildIterator iterator(session->resolve(entry), recursive != 0); iterator.More(); iterator.Next())
                session->stringSnapshot.push_back(session->entry(iterator.Value()));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_child_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->stringSnapshot.size())) return "";
        return session->stringSnapshot[static_cast<std::size_t>(index)].c_str();
    }

    int occt_ocaf_attribute_snapshot(OcctOcafHandle handle, const char* entry, int includeForgotten)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->attributeSnapshot.clear();
            for (TDF_AttributeIterator iterator(session->resolve(entry), includeForgotten == 0); iterator.More(); iterator.Next())
            {
                const Handle(TDF_Attribute) attribute = iterator.Value();
                if (attribute.IsNull()) continue;
                const char* typeName = attribute->DynamicType()->Name();
                session->attributeSnapshot.push_back({attribute, typeName == nullptr ? "" : typeName, guidString(attribute->ID())});
            }
        })) return 0;
        return static_cast<int>(session->attributeSnapshot.size());
    }

    const char* occt_ocaf_attribute_type_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->attributeSnapshot.size())) return "";
        return session->attributeSnapshot[static_cast<std::size_t>(index)].type.c_str();
    }

    const char* occt_ocaf_attribute_guid_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->attributeSnapshot.size())) return "";
        return session->attributeSnapshot[static_cast<std::size_t>(index)].guid.c_str();
    }

    const char* occt_ocaf_attribute_json_at(OcctOcafHandle handle, int index, int depth)
    {
        OcafSession* session = sessionOf(handle);
        return executeString(session, [&]
        {
            if (index < 0 || index >= static_cast<int>(session->attributeSnapshot.size()))
                throw std::out_of_range("Attribute snapshot index is out of range.");
            return attributeJson(session->attributeSnapshot[static_cast<std::size_t>(index)].attribute, depth);
        });
    }

    int occt_ocaf_forget_attribute(OcctOcafHandle handle, const char* entry, const char* guid)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int removed = 0;
        if (!execute(session, [&] { removed = session->resolve(entry).ForgetAttribute(parseGuid(guid)) ? 1 : 0; })) return 0;
        return removed;
    }

    int occt_ocaf_forget_all_attributes(OcctOcafHandle handle, const char* entry, int clearChildren)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->resolve(entry).ForgetAllAttributes(clearChildren != 0); });
    }
}
