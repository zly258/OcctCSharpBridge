#pragma once

#include "OcctNative.h"

#include <Graphic3d_HorizontalTextAlignment.hxx>
#include <Graphic3d_VerticalTextAlignment.hxx>
#include <TopoDS_Shape.hxx>

#include <functional>

namespace OcctModelingInternal
{
    TopoDS_Shape buildBRepText(
        const char* utf8Text,
        const char* fontName,
        OcctPoint3d position,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double height,
        double extrusionDepth,
        bool bold,
        bool italic,
        Graphic3d_HorizontalTextAlignment horizontalAlignment,
        Graphic3d_VerticalTextAlignment verticalAlignment);
}
