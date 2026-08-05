#include "OcctOcafExtended.h"
#include "OcctOcafInternal.hxx"

#include <TDataStd_Expression.hxx>
#include <TDataStd_Relation.hxx>
#include <TDataStd_Variable.hxx>
#include <TDF_AttributeList.hxx>
#include <TDF_LabelMap.hxx>
#include <TDocStd_FormatVersion.hxx>

using namespace OcctOcafInternal;

namespace
{
    template<typename Attribute>
    Handle(Attribute) findAttribute(const TDF_Label& label, const Standard_GUID& id)
    {
        Handle(Attribute) attribute;
        if (!label.FindAttribute(id, attribute) || attribute.IsNull())
            return {};
        return attribute;
    }

    void assignVariables(OcafSession* session, TDF_AttributeList& variables,
                         const char* const* variableEntries, int variableCount)
    {
        if (variableCount < 0)
            throw std::invalid_argument("Variable count must not be negative.");
        if (variableCount > 0 && variableEntries == nullptr)
            throw std::invalid_argument("Variable entries must not be null.");

        variables.Clear();
        for (int index = 0; index < variableCount; ++index)
        {
            const TDF_Label variableLabel = session->resolve(variableEntries[index]);
            const Handle(TDataStd_Variable) variable =
                findAttribute<TDataStd_Variable>(variableLabel, TDataStd_Variable::GetID());
            if (variable.IsNull())
                throw std::invalid_argument(std::string("Label is not a TDataStd_Variable: ") + variableEntries[index]);
            variables.Append(variable);
        }
    }

    int expressionVariableSnapshot(OcafSession* session, const TDF_Label& label, bool relation)
    {
        session->stringSnapshot.clear();
        TDF_AttributeList* variables = nullptr;
        if (relation)
        {
            const Handle(TDataStd_Relation) attribute =
                findAttribute<TDataStd_Relation>(label, TDataStd_Relation::GetID());
            if (attribute.IsNull()) return 0;
            variables = &attribute->GetVariables();
        }
        else
        {
            const Handle(TDataStd_Expression) attribute =
                findAttribute<TDataStd_Expression>(label, TDataStd_Expression::GetID());
            if (attribute.IsNull()) return 0;
            variables = &attribute->GetVariables();
        }

        for (TDF_ListIteratorOfAttributeList iterator(*variables); iterator.More(); iterator.Next())
        {
            const Handle(TDF_Attribute)& attribute = iterator.Value();
            if (!attribute.IsNull())
                session->stringSnapshot.push_back(session->entry(attribute->Label()));
        }
        return static_cast<int>(session->stringSnapshot.size());
    }
}

extern "C"
{
    int occt_ocaf_storage_format_version(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&]
        {
            session->requireDocument();
            result = static_cast<int>(session->document->StorageFormatVersion());
        });
        return result;
    }

    int occt_ocaf_set_storage_format_version(OcctOcafHandle handle, int version)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            if (version < TDocStd_FormatVersion_LOWER || version > TDocStd_FormatVersion_UPPER)
                throw std::out_of_range("OCAF storage format version must be between 2 and 12 for OCCT 7.9.0.");
            session->document->ChangeStorageFormatVersion(static_cast<TDocStd_FormatVersion>(version));
        });
    }

    int occt_ocaf_mark_modified(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&] { session->document->SetModified(session->resolve(entry)); });
    }

    int occt_ocaf_purge_modified(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            session->requireDocument();
            session->document->PurgeModified();
        });
    }

    int occt_ocaf_modified_snapshot(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        if (!execute(session, [&]
        {
            session->requireDocument();
            session->stringSnapshot.clear();
            const TDF_LabelMap& labels = session->document->GetModified();
            for (TDF_MapIteratorOfLabelMap iterator(labels); iterator.More(); iterator.Next())
                session->stringSnapshot.push_back(session->entry(iterator.Key()));
        })) return 0;
        return static_cast<int>(session->stringSnapshot.size());
    }

    const char* occt_ocaf_modified_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->stringSnapshot.size())) return "";
        return session->stringSnapshot[static_cast<std::size_t>(index)].c_str();
    }

    int occt_ocaf_init_delta_compaction(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        if (!execute(session, [&]
        {
            session->requireDocument();
            result = session->document->InitDeltaCompaction() ? 1 : 0;
        })) return 0;
        return result;
    }

    int occt_ocaf_perform_delta_compaction(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        if (!execute(session, [&]
        {
            session->requireDocument();
            result = session->document->PerformDeltaCompaction() ? 1 : 0;
        })) return 0;
        return result;
    }

    int occt_ocaf_remove_first_undo(OcctOcafHandle handle)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int removed = 0;
        if (!execute(session, [&]
        {
            session->requireDocument();
            if (session->document->GetAvailableUndos() > 0)
            {
                session->document->RemoveFirstUndo();
                removed = 1;
            }
        })) return 0;
        return removed;
    }

    int occt_ocaf_label_child_count(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = session->resolve(entry).NbChildren(); });
        return result;
    }

    int occt_ocaf_label_attribute_count(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = session->resolve(entry).NbAttributes(); });
        return result;
    }

    int occt_ocaf_label_transaction(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = session->resolve(entry).Transaction(); });
        return result;
    }

    int occt_ocaf_label_may_be_modified(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = session->resolve(entry).MayBeModified() ? 1 : 0; });
        return result;
    }

    int occt_ocaf_label_attributes_modified(OcctOcafHandle handle, const char* entry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&] { result = session->resolve(entry).AttributesModified() ? 1 : 0; });
        return result;
    }

    int occt_ocaf_label_is_descendant(OcctOcafHandle handle, const char* entry, const char* ancestorEntry)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        execute(session, [&]
        {
            result = session->resolve(entry).IsDescendant(session->resolve(ancestorEntry)) ? 1 : 0;
        });
        return result;
    }

    int occt_ocaf_set_variable(OcctOcafHandle handle, const char* entry, const char* utf8Name,
                               double value, const char* utf8Unit, int isConstant)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            Handle(TDataStd_Variable) variable = TDataStd_Variable::Set(session->resolve(entry));
            variable->Name(extended(utf8Name));
            variable->Set(value);
            variable->Unit(TCollection_AsciiString(utf8Unit == nullptr ? "" : utf8Unit));
            variable->Constant(isConstant != 0);
        });
    }

    int occt_ocaf_get_variable(OcctOcafHandle handle, const char* entry, const char** utf8Name,
                               double* value, const char** utf8Unit, int* isConstant,
                               int* isValued, int* isAssigned)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || utf8Name == nullptr || value == nullptr || utf8Unit == nullptr ||
            isConstant == nullptr || isValued == nullptr || isAssigned == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            const Handle(TDataStd_Variable) variable =
                findAttribute<TDataStd_Variable>(session->resolve(entry), TDataStd_Variable::GetID());
            if (variable.IsNull()) return;
            session->stringSnapshot = {utf8(variable->Name()), variable->Unit().ToCString()};
            *utf8Name = session->stringSnapshot[0].c_str();
            *utf8Unit = session->stringSnapshot[1].c_str();
            *isValued = variable->IsValued() ? 1 : 0;
            *value = *isValued != 0 ? variable->Get() : 0.0;
            *isConstant = variable->IsConstant() ? 1 : 0;
            *isAssigned = variable->IsAssigned() ? 1 : 0;
            found = 1;
        })) return 0;
        return found;
    }

    int occt_ocaf_assign_variable_expression(OcctOcafHandle handle, const char* variableEntry,
                                             const char* utf8Expression,
                                             const char* const* variableEntries, int variableCount)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            Handle(TDataStd_Variable) variable = TDataStd_Variable::Set(session->resolve(variableEntry));
            Handle(TDataStd_Expression) expression = variable->Assign();
            expression->SetExpression(extended(utf8Expression));
            assignVariables(session, expression->GetVariables(), variableEntries, variableCount);
        });
    }

    int occt_ocaf_desassign_variable(OcctOcafHandle handle, const char* variableEntry)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            const Handle(TDataStd_Variable) variable =
                findAttribute<TDataStd_Variable>(session->resolve(variableEntry), TDataStd_Variable::GetID());
            if (variable.IsNull()) throw std::runtime_error("Label has no TDataStd_Variable attribute.");
            variable->Desassign();
        });
    }

    int occt_ocaf_set_expression(OcctOcafHandle handle, const char* entry, const char* utf8Expression,
                                 const char* const* variableEntries, int variableCount)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            Handle(TDataStd_Expression) expression = TDataStd_Expression::Set(session->resolve(entry));
            expression->SetExpression(extended(utf8Expression));
            assignVariables(session, expression->GetVariables(), variableEntries, variableCount);
        });
    }

    int occt_ocaf_get_expression(OcctOcafHandle handle, const char* entry, const char** utf8Expression)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || utf8Expression == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            const Handle(TDataStd_Expression) expression =
                findAttribute<TDataStd_Expression>(session->resolve(entry), TDataStd_Expression::GetID());
            if (expression.IsNull()) return;
            *utf8Expression = session->setScratch(utf8(expression->GetExpression()));
            found = 1;
        })) return 0;
        return found;
    }

    int occt_ocaf_set_relation(OcctOcafHandle handle, const char* entry, const char* utf8Relation,
                               const char* const* variableEntries, int variableCount)
    {
        OcafSession* session = sessionOf(handle);
        return execute(session, [&]
        {
            Handle(TDataStd_Relation) relation = TDataStd_Relation::Set(session->resolve(entry));
            relation->SetRelation(extended(utf8Relation));
            assignVariables(session, relation->GetVariables(), variableEntries, variableCount);
        });
    }

    int occt_ocaf_get_relation(OcctOcafHandle handle, const char* entry, const char** utf8Relation)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || utf8Relation == nullptr) return 0;
        int found = 0;
        if (!execute(session, [&]
        {
            const Handle(TDataStd_Relation) relation =
                findAttribute<TDataStd_Relation>(session->resolve(entry), TDataStd_Relation::GetID());
            if (relation.IsNull()) return;
            *utf8Relation = session->setScratch(utf8(relation->GetRelation()));
            found = 1;
        })) return 0;
        return found;
    }

    int occt_ocaf_expression_variable_snapshot(OcctOcafHandle handle, const char* entry, int relation)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr) return 0;
        int result = 0;
        if (!execute(session, [&]
        {
            result = expressionVariableSnapshot(session, session->resolve(entry), relation != 0);
        })) return 0;
        return result;
    }

    const char* occt_ocaf_expression_variable_at(OcctOcafHandle handle, int index)
    {
        OcafSession* session = sessionOf(handle);
        if (session == nullptr || index < 0 || index >= static_cast<int>(session->stringSnapshot.size())) return "";
        return session->stringSnapshot[static_cast<std::size_t>(index)].c_str();
    }
}
