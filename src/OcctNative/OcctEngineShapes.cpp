#include "OcctInternal.hxx"

#include <BRepBndLib.hxx>
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <Bnd_Box.hxx>
#include <TopExp_Explorer.hxx>

using namespace OcctBridge;

extern "C"
{
    int occt_shape_type(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h);
        const ObjectEntry* entry = e == nullptr ? nullptr : e->findShape(id);
        return entry == nullptr ? OcctShape_Shape : shapeTypeValue(entry->shape);
    }

    int occt_shape_is_valid(OcctHandle h, OcctObjectId id)
    {
        Engine* e = engineOf(h);
        const ObjectEntry* entry = e == nullptr ? nullptr : e->findShape(id);
        return entry != nullptr && BRepCheck_Analyzer(entry->shape, Standard_True).IsValid() ? 1 : 0;
    }

    int occt_shape_bounds(OcctHandle h, OcctObjectId id, OcctBounds* result)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            Bnd_Box box;
            BRepBndLib::Add(shapeWithPresentationTransformation(*entry), box);
            box.Get(result->minX, result->minY, result->minZ, result->maxX, result->maxY, result->maxZ);
        });
    }

    int occt_shape_linear_properties(OcctHandle h, OcctObjectId id, OcctMassProperties* result)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            GProp_GProps properties;
            BRepGProp::LinearProperties(entry->shape, properties);
            fillMassProperties(properties, result);
        });
    }

    int occt_shape_surface_properties(OcctHandle h, OcctObjectId id, OcctMassProperties* result)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            GProp_GProps properties;
            BRepGProp::SurfaceProperties(entry->shape, properties);
            fillMassProperties(properties, result);
        });
    }

    int occt_shape_volume_properties(OcctHandle h, OcctObjectId id, OcctMassProperties* result)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || result == nullptr) return 0;
        return execute(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            GProp_GProps properties;
            BRepGProp::VolumeProperties(entry->shape, properties);
            fillMassProperties(properties, result);
        });
    }

    int occt_topology_count(OcctHandle h, OcctObjectId id, int type)
    {
        Engine* e = engineOf(h);
        const ObjectEntry* entry = e == nullptr ? nullptr : e->findShape(id);
        if (entry == nullptr) return 0;
        int count = 0;
        for (TopExp_Explorer explorer(entry->shape, shapeEnum(type)); explorer.More(); explorer.Next()) ++count;
        return count;
    }

    OcctObjectId occt_get_subshape(OcctHandle h, OcctObjectId id, int type, int index)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e) || index < 0) return 0;
        return executeObject(e, [&]() -> OcctObjectId
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            int current = 0;
            for (TopExp_Explorer explorer(entry->shape, shapeEnum(type)); explorer.More(); explorer.Next(), ++current)
            {
                if (current == index) return e->addShape(explorer.Current(), false, "Subshape");
            }
            throw std::out_of_range("Subshape index is out of range.");
        });
    }

    OcctObjectId occt_copy_shape(OcctHandle h, OcctObjectId id, int hideInput)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            BRepBuilderAPI_Copy copy(entry->shape);
            if (!copy.IsDone()) throw std::runtime_error("Shape copy failed.");
            const OcctObjectId result = e->addShape(copy.Shape(), false, "Copy");
            if (hideInput) e->hide(id);
            return result;
        });
    }

    OcctObjectId occt_translate(OcctHandle h, OcctObjectId id, OcctVector3d value, int hideInput)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            gp_Trsf transform;
            transform.SetTranslation(vector(value));
            const OcctObjectId result = e->addShape(transformed(entry->shape, transform), false, "Translated");
            if (hideInput) e->hide(id);
            return result;
        });
    }

    OcctObjectId occt_rotate(OcctHandle h, OcctObjectId id, OcctPoint3d pointValue, OcctVector3d directionValue, double degrees, int hideInput)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            gp_Trsf transform;
            transform.SetRotation(
                gp_Ax1(point(pointValue), direction(directionValue)),
                degrees * 3.14159265358979323846 / 180.0);
            const OcctObjectId result = e->addShape(transformed(entry->shape, transform), false, "Rotated");
            if (hideInput) e->hide(id);
            return result;
        });
    }

    OcctObjectId occt_scale(OcctHandle h, OcctObjectId id, OcctPoint3d center, double factor, int hideInput)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            requirePositive(factor, "Scale factor");
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            gp_Trsf transform;
            transform.SetScale(point(center), factor);
            const OcctObjectId result = e->addShape(transformed(entry->shape, transform), false, "Scaled");
            if (hideInput) e->hide(id);
            return result;
        });
    }

    OcctObjectId occt_mirror_plane(OcctHandle h, OcctObjectId id, OcctPoint3d pointValue, OcctVector3d normal, int hideInput)
    {
        Engine* e = engineOf(h);
        if (!validateInitialized(e)) return 0;
        return executeObject(e, [&]
        {
            ObjectEntry* entry = e->findShape(id);
            if (entry == nullptr) throw std::invalid_argument("Shape ID does not exist.");
            gp_Trsf transform;
            transform.SetMirror(gp_Ax2(point(pointValue), direction(normal)));
            const OcctObjectId result = e->addShape(transformed(entry->shape, transform), false, "Mirrored");
            if (hideInput) e->hide(id);
            return result;
        });
    }

    int occt_shape_count(OcctHandle h)
    {
        Engine* e = engineOf(h);
        if (e == nullptr) return 0;
        int count = 0;
        for (const auto& pair : e->objects)
        {
            if (pair.second.kind == OcctObject_Shape) ++count;
        }
        return count;
    }
}
