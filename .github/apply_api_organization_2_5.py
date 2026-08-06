from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    return (ROOT / path).read_bytes().decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    normalized = text.replace("\r\n", "\n").rstrip() + "\n"
    target.write_bytes(BOM + normalized.replace("\n", "\r\n").encode("utf-8"))


def replace_once(text: str, old: str, new: str, label: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block not found: {label}")
    return text.replace(old, new, 1)


# -----------------------------------------------------------------------------
# Native API: differential geometry and strict local OCCT configuration.
# -----------------------------------------------------------------------------
header = read("src/OcctNative/OcctModeling.h")
new_structs = """
    struct OcctModelParameterRange
    {
        double firstParameter;
        double lastParameter;
        int isClosed;
        int isPeriodic;
        double period;
    };

    struct OcctModelCurveDifferential
    {
        double parameter;
        OcctPoint3d point;
        OcctVector3d firstDerivative;
        OcctVector3d secondDerivative;
    };

    struct OcctModelCurveCurvature
    {
        double parameter;
        OcctPoint3d point;
        OcctVector3d tangent;
        OcctVector3d normal;
        OcctPoint3d centerOfCurvature;
        double curvature;
        int hasTangent;
        int hasNormal;
        int hasCenterOfCurvature;
    };

    struct OcctModelSurfacePeriodicity
    {
        int isUClosed;
        int isVClosed;
        int isUPeriodic;
        int isVPeriodic;
        double uPeriod;
        double vPeriod;
    };

    struct OcctModelSurfaceDifferential
    {
        double u;
        double v;
        OcctPoint3d point;
        OcctVector3d normal;
        OcctVector3d uDerivative;
        OcctVector3d vDerivative;
        OcctVector3d uSecondDerivative;
        OcctVector3d vSecondDerivative;
        OcctVector3d uvDerivative;
        int hasNormal;
    };

    struct OcctModelSurfaceCurvature
    {
        double u;
        double v;
        OcctPoint3d point;
        OcctVector3d normal;
        OcctVector3d maximumDirection;
        OcctVector3d minimumDirection;
        double maximumCurvature;
        double minimumCurvature;
        double meanCurvature;
        double gaussianCurvature;
        int isUmbilic;
        int hasNormal;
        int hasCurvature;
    };
"""
header = replace_once(
    header,
    "    struct OcctModelLocation\n    {\n",
    new_structs + "\n    struct OcctModelLocation\n    {\n",
    "differential geometry structs")
new_declarations = """    OCCTBRIDGE_API int occt_model_edge_parameter_range(OcctModelHandle handle, OcctObjectId edgeId, OcctModelParameterRange* result);
    OCCTBRIDGE_API int occt_model_edge_differential(OcctModelHandle handle, OcctObjectId edgeId, double parameter, OcctModelCurveDifferential* result);
    OCCTBRIDGE_API int occt_model_edge_curvature(OcctModelHandle handle, OcctObjectId edgeId, double parameter, double resolution, OcctModelCurveCurvature* result);
    OCCTBRIDGE_API int occt_model_face_periodicity(OcctModelHandle handle, OcctObjectId faceId, OcctModelSurfacePeriodicity* result);
    OCCTBRIDGE_API int occt_model_face_differential(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceDifferential* result);
    OCCTBRIDGE_API int occt_model_face_curvature(OcctModelHandle handle, OcctObjectId faceId, double u, double v, double resolution, OcctModelSurfaceCurvature* result);
"""
header = replace_once(
    header,
    "    OCCTBRIDGE_API int occt_model_face_torus_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);\n",
    "    OCCTBRIDGE_API int occt_model_face_torus_geometry(OcctModelHandle handle, OcctObjectId faceId, OcctModelTorusGeometry* result);\n\n" + new_declarations,
    "differential geometry declarations")
write("src/OcctNative/OcctModeling.h", header)

native_differential = r'''#include "OcctModelingInternal.hxx"

#include <BRepLProp_CLProps.hxx>
#include <BRepLProp_SLProps.hxx>
#include <Precision.hxx>

using namespace OcctModelingInternal;

namespace
{
    TopoDS_Edge requireEdge(ModelSession* model, OcctObjectId edgeId)
    {
        const TopoDS_Shape& shape = model->requireShape(edgeId);
        if (shape.ShapeType() != TopAbs_EDGE)
            throw std::invalid_argument("Input must be an edge.");
        return TopoDS::Edge(shape);
    }

    TopoDS_Face requireFace(ModelSession* model, OcctObjectId faceId)
    {
        const TopoDS_Shape& shape = model->requireShape(faceId);
        if (shape.ShapeType() != TopAbs_FACE)
            throw std::invalid_argument("Input must be a face.");
        return TopoDS::Face(shape);
    }

    OcctPoint3d toNativePoint(const gp_Pnt& point)
    {
        return {point.X(), point.Y(), point.Z()};
    }

    OcctVector3d toNativeVector(const gp_Vec& vector)
    {
        return {vector.X(), vector.Y(), vector.Z()};
    }

    OcctVector3d toNativeVector(const gp_Dir& direction)
    {
        return {direction.X(), direction.Y(), direction.Z()};
    }

    void validateCurveParameter(const BRepAdaptor_Curve& curve, double parameter)
    {
        const double first = curve.FirstParameter();
        const double last = curve.LastParameter();
        const double tolerance = Precision::PConfusion();
        if ((std::isfinite(first) && parameter < first - tolerance) ||
            (std::isfinite(last) && parameter > last + tolerance))
        {
            throw std::out_of_range("Curve parameter is outside the edge range.");
        }
    }

    bool isReversed(const TopoDS_Face& face)
    {
        return face.Orientation() == TopAbs_REVERSED;
    }
}

extern "C"
{
    int occt_model_edge_parameter_range(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        OcctModelParameterRange* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            result->firstParameter = curve.FirstParameter();
            result->lastParameter = curve.LastParameter();
            result->isClosed = curve.IsClosed() ? 1 : 0;
            result->isPeriodic = curve.IsPeriodic() ? 1 : 0;
            result->period = result->isPeriodic != 0 ? curve.Period() : 0.0;
        });
    }

    int occt_model_edge_differential(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        double parameter,
        OcctModelCurveDifferential* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            validateCurveParameter(curve, parameter);
            gp_Pnt point;
            gp_Vec firstDerivative;
            gp_Vec secondDerivative;
            curve.D2(parameter, point, firstDerivative, secondDerivative);
            result->parameter = parameter;
            result->point = toNativePoint(point);
            result->firstDerivative = toNativeVector(firstDerivative);
            result->secondDerivative = toNativeVector(secondDerivative);
        });
    }

    int occt_model_edge_curvature(
        OcctModelHandle handle,
        OcctObjectId edgeId,
        double parameter,
        double resolution,
        OcctModelCurveCurvature* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            requirePositive(resolution, "Resolution");
            const BRepAdaptor_Curve curve(requireEdge(model, edgeId));
            validateCurveParameter(curve, parameter);
            BRepLProp_CLProps properties(curve, parameter, 2, resolution);

            result->parameter = parameter;
            result->point = toNativePoint(properties.Value());
            result->tangent = {0.0, 0.0, 0.0};
            result->normal = {0.0, 0.0, 0.0};
            result->centerOfCurvature = result->point;
            result->curvature = 0.0;
            result->hasTangent = 0;
            result->hasNormal = 0;
            result->hasCenterOfCurvature = 0;

            if (properties.IsTangentDefined())
            {
                gp_Dir tangent;
                properties.Tangent(tangent);
                result->tangent = toNativeVector(tangent);
                result->hasTangent = 1;
            }

            result->curvature = properties.Curvature();
            if (std::abs(result->curvature) > resolution)
            {
                gp_Dir normal;
                gp_Pnt center;
                properties.Normal(normal);
                properties.CentreOfCurvature(center);
                result->normal = toNativeVector(normal);
                result->centerOfCurvature = toNativePoint(center);
                result->hasNormal = 1;
                result->hasCenterOfCurvature = 1;
            }
        });
    }

    int occt_model_face_periodicity(
        OcctModelHandle handle,
        OcctObjectId faceId,
        OcctModelSurfacePeriodicity* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            const BRepAdaptor_Surface surface(requireFace(model, faceId));
            result->isUClosed = surface.IsUClosed() ? 1 : 0;
            result->isVClosed = surface.IsVClosed() ? 1 : 0;
            result->isUPeriodic = surface.IsUPeriodic() ? 1 : 0;
            result->isVPeriodic = surface.IsVPeriodic() ? 1 : 0;
            result->uPeriod = result->isUPeriodic != 0 ? surface.UPeriod() : 0.0;
            result->vPeriod = result->isVPeriodic != 0 ? surface.VPeriod() : 0.0;
        });
    }

    int occt_model_face_differential(
        OcctModelHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        double resolution,
        OcctModelSurfaceDifferential* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            requirePositive(resolution, "Resolution");
            const TopoDS_Face face = requireFace(model, faceId);
            const BRepAdaptor_Surface surface(face);
            gp_Pnt point;
            gp_Vec uDerivative;
            gp_Vec vDerivative;
            gp_Vec uSecondDerivative;
            gp_Vec vSecondDerivative;
            gp_Vec uvDerivative;
            surface.D2(
                u,
                v,
                point,
                uDerivative,
                vDerivative,
                uSecondDerivative,
                vSecondDerivative,
                uvDerivative);

            result->u = u;
            result->v = v;
            result->point = toNativePoint(point);
            result->normal = {0.0, 0.0, 0.0};
            result->uDerivative = toNativeVector(uDerivative);
            result->vDerivative = toNativeVector(vDerivative);
            result->uSecondDerivative = toNativeVector(uSecondDerivative);
            result->vSecondDerivative = toNativeVector(vSecondDerivative);
            result->uvDerivative = toNativeVector(uvDerivative);
            result->hasNormal = 0;

            const gp_Vec cross = uDerivative.Crossed(vDerivative);
            if (cross.SquareMagnitude() > resolution * resolution)
            {
                gp_Dir normal(cross);
                if (isReversed(face)) normal.Reverse();
                result->normal = toNativeVector(normal);
                result->hasNormal = 1;
            }
        });
    }

    int occt_model_face_curvature(
        OcctModelHandle handle,
        OcctObjectId faceId,
        double u,
        double v,
        double resolution,
        OcctModelSurfaceCurvature* result)
    {
        ModelSession* model = modelOf(handle);
        if (result == nullptr) return 0;
        return execute(model, [&]
        {
            requirePositive(resolution, "Resolution");
            const TopoDS_Face face = requireFace(model, faceId);
            const BRepAdaptor_Surface surface(face);
            BRepLProp_SLProps properties(surface, u, v, 2, resolution);

            result->u = u;
            result->v = v;
            result->point = toNativePoint(properties.Value());
            result->normal = {0.0, 0.0, 0.0};
            result->maximumDirection = {0.0, 0.0, 0.0};
            result->minimumDirection = {0.0, 0.0, 0.0};
            result->maximumCurvature = 0.0;
            result->minimumCurvature = 0.0;
            result->meanCurvature = 0.0;
            result->gaussianCurvature = 0.0;
            result->isUmbilic = 0;
            result->hasNormal = 0;
            result->hasCurvature = 0;

            if (properties.IsNormalDefined())
            {
                gp_Dir normal = properties.Normal();
                if (isReversed(face)) normal.Reverse();
                result->normal = toNativeVector(normal);
                result->hasNormal = 1;
            }

            if (!properties.IsCurvatureDefined()) return;

            gp_Dir maximumDirection;
            gp_Dir minimumDirection;
            properties.CurvatureDirections(maximumDirection, minimumDirection);
            const double maximumCurvature = properties.MaxCurvature();
            const double minimumCurvature = properties.MinCurvature();

            if (isReversed(face))
            {
                result->maximumDirection = toNativeVector(minimumDirection);
                result->minimumDirection = toNativeVector(maximumDirection);
                result->maximumCurvature = -minimumCurvature;
                result->minimumCurvature = -maximumCurvature;
                result->meanCurvature = -properties.MeanCurvature();
            }
            else
            {
                result->maximumDirection = toNativeVector(maximumDirection);
                result->minimumDirection = toNativeVector(minimumDirection);
                result->maximumCurvature = maximumCurvature;
                result->minimumCurvature = minimumCurvature;
                result->meanCurvature = properties.MeanCurvature();
            }

            result->gaussianCurvature = properties.GaussianCurvature();
            result->isUmbilic = properties.IsUmbilic() ? 1 : 0;
            result->hasCurvature = 1;
        });
    }
}
'''
write("src/OcctNative/OcctModelingDifferentialGeometry.cpp", native_differential)

cmake = read("src/OcctNative/CMakeLists.txt")
cmake = replace_once(
    cmake,
    'set(_occt_default_root "$ENV{OCCT_ROOT}")\nif(NOT _occt_default_root)\n    set(_occt_default_root "D:/tools/occt-vc144-64")\nendif()\n\nset(OCCT_ROOT "${_occt_default_root}" CACHE PATH "OCCT installation root")\n',
    'set(OCCT_ROOT "$ENV{OCCT_ROOT}" CACHE PATH "OCCT installation root")\nif(NOT OCCT_ROOT)\n    message(FATAL_ERROR "OCCT_ROOT is not configured. Pass -DOCCT_ROOT=<path> or set the OCCT_ROOT environment variable.")\nendif()\n',
    "CMake OCCT root policy")
cmake = replace_once(
    cmake,
    "    OcctModelingAnalyticGeometry.cpp\n",
    "    OcctModelingAnalyticGeometry.cpp\n    OcctModelingDifferentialGeometry.cpp\n",
    "differential geometry CMake source")
write("src/OcctNative/CMakeLists.txt", cmake)

core = read("src/OcctNative/OcctModelingCore.cpp")
core = core.replace("analytic-geometry;topology", "analytic-geometry;differential-geometry;topology")
write("src/OcctNative/OcctModelingCore.cpp", core)

engine = read("src/OcctNative/OcctEngine.cpp").replace("2.4.0", "2.5.0")
write("src/OcctNative/OcctEngine.cpp", engine)

# -----------------------------------------------------------------------------
# Managed API: categorized partials, canonical naming and new query types.
# -----------------------------------------------------------------------------
interop_types = '''using System.Runtime.InteropServices;

namespace OcctNet;

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelParameterRange
{
    public double FirstParameter;
    public double LastParameter;
    public int NativeIsClosed;
    public int NativeIsPeriodic;
    public double Period;

    public readonly bool IsClosed => NativeIsClosed != 0;
    public readonly bool IsPeriodic => NativeIsPeriodic != 0;
    public readonly double Length => LastParameter - FirstParameter;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelCurveDifferential
{
    public double Parameter;
    public OcctPoint3d Point;
    public OcctVector3d FirstDerivative;
    public OcctVector3d SecondDerivative;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelCurveCurvature
{
    public double Parameter;
    public OcctPoint3d Point;
    public OcctVector3d Tangent;
    public OcctVector3d Normal;
    public OcctPoint3d CenterOfCurvature;
    public double Curvature;
    public int NativeHasTangent;
    public int NativeHasNormal;
    public int NativeHasCenterOfCurvature;

    public readonly bool HasTangent => NativeHasTangent != 0;
    public readonly bool HasNormal => NativeHasNormal != 0;
    public readonly bool HasCenterOfCurvature => NativeHasCenterOfCurvature != 0;
    public readonly double RadiusOfCurvature => Math.Abs(Curvature) > double.Epsilon
        ? 1.0 / Math.Abs(Curvature)
        : double.PositiveInfinity;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelSurfacePeriodicity
{
    public int NativeIsUClosed;
    public int NativeIsVClosed;
    public int NativeIsUPeriodic;
    public int NativeIsVPeriodic;
    public double UPeriod;
    public double VPeriod;

    public readonly bool IsUClosed => NativeIsUClosed != 0;
    public readonly bool IsVClosed => NativeIsVClosed != 0;
    public readonly bool IsUPeriodic => NativeIsUPeriodic != 0;
    public readonly bool IsVPeriodic => NativeIsVPeriodic != 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelSurfaceDifferential
{
    public double U;
    public double V;
    public OcctPoint3d Point;
    public OcctVector3d Normal;
    public OcctVector3d UDerivative;
    public OcctVector3d VDerivative;
    public OcctVector3d USecondDerivative;
    public OcctVector3d VSecondDerivative;
    public OcctVector3d UvDerivative;
    public int NativeHasNormal;

    public readonly bool HasNormal => NativeHasNormal != 0;
}

[StructLayout(LayoutKind.Sequential)]
public struct OcctModelSurfaceCurvature
{
    public double U;
    public double V;
    public OcctPoint3d Point;
    public OcctVector3d Normal;
    public OcctVector3d MaximumDirection;
    public OcctVector3d MinimumDirection;
    public double MaximumCurvature;
    public double MinimumCurvature;
    public double MeanCurvature;
    public double GaussianCurvature;
    public int NativeIsUmbilic;
    public int NativeHasNormal;
    public int NativeHasCurvature;

    public readonly bool IsUmbilic => NativeIsUmbilic != 0;
    public readonly bool HasNormal => NativeHasNormal != 0;
    public readonly bool HasCurvature => NativeHasCurvature != 0;
}
'''
write("src/OcctNet/OcctDifferentialGeometryTypes.cs", interop_types)

pinvoke = '''using System.Runtime.InteropServices;

namespace OcctNet;

internal static partial class ModelNativeMethods
{
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_parameter_range(IntPtr handle, long edgeId, out OcctModelParameterRange result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_differential(IntPtr handle, long edgeId, double parameter, out OcctModelCurveDifferential result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_edge_curvature(IntPtr handle, long edgeId, double parameter, double resolution, out OcctModelCurveCurvature result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_periodicity(IntPtr handle, long faceId, out OcctModelSurfacePeriodicity result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_differential(IntPtr handle, long faceId, double u, double v, double resolution, out OcctModelSurfaceDifferential result);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int occt_model_face_curvature(IntPtr handle, long faceId, double u, double v, double resolution, out OcctModelSurfaceCurvature result);
}
'''
write("src/OcctNet/ModelNativeMethods.DifferentialGeometry.cs", pinvoke)

session_differential = '''namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctModelParameterRange GetEdgeParameterRange(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_parameter_range(_handle, edge.Id, out var result));
        return result;
    }

    public OcctModelCurveDifferential EvaluateEdgeAtParameter(OcctModelShape edge, double parameter)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_differential(_handle, edge.Id, parameter, out var result));
        return result;
    }

    public OcctModelCurveCurvature GetEdgeCurvature(
        OcctModelShape edge,
        double parameter,
        double resolution = 1e-9)
    {
        EnsureShape(edge);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(resolution);
        Check(ModelNativeMethods.occt_model_edge_curvature(
            _handle, edge.Id, parameter, resolution, out var result));
        return result;
    }

    public OcctModelSurfacePeriodicity GetFacePeriodicity(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_periodicity(_handle, face.Id, out var result));
        return result;
    }

    public OcctModelSurfaceDifferential EvaluateFaceDifferential(
        OcctModelShape face,
        double u,
        double v,
        double resolution = 1e-9)
    {
        EnsureShape(face);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(resolution);
        Check(ModelNativeMethods.occt_model_face_differential(
            _handle, face.Id, u, v, resolution, out var result));
        return result;
    }

    public OcctModelSurfaceCurvature GetFaceCurvature(
        OcctModelShape face,
        double u,
        double v,
        double resolution = 1e-9)
    {
        EnsureShape(face);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(resolution);
        Check(ModelNativeMethods.occt_model_face_curvature(
            _handle, face.Id, u, v, resolution, out var result));
        return result;
    }
}
'''
write("src/OcctNet/OcctModelingSession.DifferentialGeometry.cs", session_differential)

base_session = '''using System.Runtime.InteropServices;
using System.Threading;

namespace OcctNet;

/// <summary>
/// Headless OCCT modeling session. No HWND, AIS context, or viewer is required.
/// </summary>
public sealed partial class OcctModelingSession : IDisposable
{
    private IntPtr _handle;

    public OcctModelingSession()
    {
        OcctRuntime.Configure();
        OcctBridgeInfo.EnsureCompatible();
        _handle = ModelNativeMethods.occt_model_create();
        if (_handle == IntPtr.Zero)
            throw new OcctException("Unable to create the native OCCT modeling session.");
    }

    internal IntPtr NativeHandle
    {
        get
        {
            EnsureNotDisposed();
            return _handle;
        }
    }

    public static string Capabilities =>
        Marshal.PtrToStringUTF8(ModelNativeMethods.occt_model_capabilities()) ?? string.Empty;

    public int ShapeCount
    {
        get
        {
            EnsureNotDisposed();
            return ModelNativeMethods.occt_model_shape_count(_handle);
        }
    }

    public IReadOnlyList<OcctModelShape> Shapes
    {
        get
        {
            EnsureNotDisposed();
            return Enumerable.Range(0, ShapeCount)
                .Select(index => ModelNativeMethods.occt_model_shape_id_at(_handle, index))
                .Where(id => id > 0)
                .Select(id => new OcctModelShape(id))
                .ToArray();
        }
    }

    public bool Exists(OcctModelShape shape)
    {
        EnsureNotDisposed();
        return shape.IsValid && ModelNativeMethods.occt_model_shape_exists(_handle, shape.Id) != 0;
    }

    public void Delete(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_delete_shape(_handle, shape.Id));
    }

    public void Clear() => Check(ModelNativeMethods.occt_model_clear(NativeHandle));

    public OcctModelShape Copy(OcctModelShape shape)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_copy_shape(_handle, shape.Id));
    }

    private delegate long ImportCall(IntPtr handle, string path);

    private OcctModelShape ImportSpecific(string filePath, ImportCall call)
    {
        ValidatePath(filePath);
        return CheckShape(call(_handle, Path.GetFullPath(filePath)));
    }

    private delegate int ExportCall(IntPtr handle, long shapeId, string path);

    private void ExportShape(OcctModelShape shape, string filePath, ExportCall call)
    {
        EnsureShape(shape);
        ValidatePath(filePath);
        Check(call(_handle, shape.Id, Path.GetFullPath(filePath)));
    }

    private static void ValidatePath(string path) => ArgumentException.ThrowIfNullOrWhiteSpace(path);

    private delegate int PropertyCall(IntPtr handle, long id, out OcctMassProperties result);

    private OcctMassProperties GetProperties(OcctModelShape shape, PropertyCall call)
    {
        EnsureShape(shape);
        Check(call(_handle, shape.Id, out var result));
        return result;
    }

    private static T[] RequiredArray<T>(IEnumerable<T> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var result = values.ToArray();
        if (result.Length == 0)
            throw new ArgumentException("Collection must not be empty.", parameterName);
        return result;
    }

    private long[] ShapeIds(IEnumerable<OcctModelShape> shapes)
    {
        var array = RequiredArray(shapes, nameof(shapes));
        foreach (var shape in array) EnsureShape(shape);
        return array.Select(shape => shape.Id).ToArray();
    }

    private void EnsureShape(OcctModelShape shape)
    {
        EnsureNotDisposed();
        if (!shape.IsValid || ModelNativeMethods.occt_model_shape_exists(_handle, shape.Id) == 0)
            throw new ArgumentException("Shape does not belong to this modeling session.", nameof(shape));
    }

    private OcctModelShape CheckShape(long id)
    {
        if (id <= 0) throw CreateException();
        return new OcctModelShape(id);
    }

    private OcctModelAlgorithmResult CheckAlgorithm(NativeModelAlgorithmResult native)
    {
        if (native.Succeeded == 0 || native.ShapeId <= 0) throw CreateException();
        return new OcctModelAlgorithmResult(this, native);
    }

    private void Check(int result)
    {
        if (result == 0) throw CreateException();
    }

    private OcctException CreateException()
    {
        var pointer = _handle == IntPtr.Zero
            ? IntPtr.Zero
            : ModelNativeMethods.occt_model_last_error(_handle);
        var message = pointer == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(pointer);
        return new OcctException(string.IsNullOrWhiteSpace(message)
            ? "The native OCCT modeling operation failed."
            : message);
    }

    private void EnsureNotDisposed() =>
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _handle) == IntPtr.Zero, this);

    public void Dispose()
    {
        ReleaseHandle(throwOnError: true);
        GC.SuppressFinalize(this);
    }

    private void ReleaseHandle(bool throwOnError)
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle == IntPtr.Zero) return;

        if (throwOnError)
        {
            ModelNativeMethods.occt_model_destroy(handle);
            return;
        }

        try
        {
            ModelNativeMethods.occt_model_destroy(handle);
        }
        catch
        {
            // Finalizers must not allow native unload failures to terminate the process.
        }
    }

    ~OcctModelingSession() => ReleaseHandle(throwOnError: false);
}
'''
write("src/OcctNet/OcctModelingSession.cs", base_session)

shape_queries = '''namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public long GetShapeHash(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_hash(_handle, shape.Id);
    }

    public OcctShapeType GetShapeType(OcctModelShape shape)
    {
        EnsureShape(shape);
        return (OcctShapeType)ModelNativeMethods.occt_model_shape_type(_handle, shape.Id);
    }

    public OcctModelOrientation GetShapeOrientation(OcctModelShape shape)
    {
        EnsureShape(shape);
        return (OcctModelOrientation)ModelNativeMethods.occt_model_shape_orientation(_handle, shape.Id);
    }

    public OcctModelOrientation GetOrientation(OcctModelShape shape) => GetShapeOrientation(shape);

    public bool IsClosed(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_is_closed(_handle, shape.Id) != 0;
    }

    public bool IsValid(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_is_valid(_handle, shape.Id) != 0;
    }

    public double GetMaximumTolerance(OcctModelShape shape)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_shape_tolerance(_handle, shape.Id);
    }

    public string GetCheckReport(OcctModelShape shape)
    {
        EnsureShape(shape);
        return Marshal.PtrToStringUTF8(
            ModelNativeMethods.occt_model_check_report(_handle, shape.Id)) ?? string.Empty;
    }

    public OcctBounds GetShapeBounds(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_shape_bounds(_handle, shape.Id, out var result));
        return result;
    }

    public OcctBounds GetBounds(OcctModelShape shape) => GetShapeBounds(shape);

    public OcctMassProperties GetLinearProperties(OcctModelShape shape) =>
        GetProperties(shape, ModelNativeMethods.occt_model_shape_linear_properties);

    public OcctMassProperties GetSurfaceProperties(OcctModelShape shape) =>
        GetProperties(shape, ModelNativeMethods.occt_model_shape_surface_properties);

    public OcctMassProperties GetVolumeProperties(OcctModelShape shape) =>
        GetProperties(shape, ModelNativeMethods.occt_model_shape_volume_properties);

    public OcctDistanceResult GetShapeDistance(OcctModelShape first, OcctModelShape second)
    {
        EnsureShape(first);
        EnsureShape(second);
        Check(ModelNativeMethods.occt_model_shape_distance(
            _handle, first.Id, second.Id, out var result));
        return result;
    }

    public OcctDistanceResult Distance(OcctModelShape first, OcctModelShape second) =>
        GetShapeDistance(first, second);

    public OcctModelLocation GetShapeLocation(OcctModelShape shape)
    {
        EnsureShape(shape);
        Check(ModelNativeMethods.occt_model_get_location(_handle, shape.Id, out var result));
        return result;
    }

    public OcctModelLocation GetLocation(OcctModelShape shape) => GetShapeLocation(shape);

    public OcctModelShape SetShapeLocation(
        OcctModelShape shape,
        OcctModelLocation location,
        bool copyShape = true)
    {
        EnsureShape(shape);
        return CheckShape(ModelNativeMethods.occt_model_set_location(
            _handle, shape.Id, in location, copyShape ? 1 : 0));
    }

    public OcctModelShape SetLocation(
        OcctModelShape shape,
        OcctModelLocation location,
        bool copyShape = true) => SetShapeLocation(shape, location, copyShape);
}
'''
write("src/OcctNet/OcctModelingSession.ShapeQueries.cs", shape_queries)

topology = '''namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public int GetTopologyCount(OcctModelShape shape, OcctShapeType type)
    {
        EnsureShape(shape);
        return ModelNativeMethods.occt_model_topology_count(_handle, shape.Id, (int)type);
    }

    public OcctModelShape GetSubshapeAt(OcctModelShape shape, OcctShapeType type, int index)
    {
        EnsureShape(shape);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        return CheckShape(ModelNativeMethods.occt_model_get_subshape(
            _handle, shape.Id, (int)type, index));
    }

    public OcctModelShape GetSubshape(OcctModelShape shape, OcctShapeType type, int index) =>
        GetSubshapeAt(shape, type, index);

    public IReadOnlyList<OcctModelShape> GetSubshapes(
        OcctModelShape shape,
        OcctShapeType type) =>
        Enumerable.Range(0, GetTopologyCount(shape, type))
            .Select(index => GetSubshapeAt(shape, type, index))
            .ToArray();

    public OcctModelShape GetOuterWire(OcctModelShape face)
    {
        EnsureShape(face);
        return CheckShape(ModelNativeMethods.occt_model_outer_wire(_handle, face.Id));
    }

    public IReadOnlyList<OcctModelShape> GetInnerWires(OcctModelShape face)
    {
        EnsureShape(face);
        var count = ModelNativeMethods.occt_model_inner_wire_count(_handle, face.Id);
        return Enumerable.Range(0, count)
            .Select(index => CheckShape(
                ModelNativeMethods.occt_model_inner_wire_at(_handle, face.Id, index)))
            .ToArray();
    }

    public IReadOnlyList<OcctModelShape> GetAncestors(
        OcctModelShape root,
        OcctModelShape child,
        OcctShapeType ancestorType)
    {
        EnsureShape(root);
        EnsureShape(child);
        var count = ModelNativeMethods.occt_model_ancestor_count(
            _handle, root.Id, child.Id, (int)ancestorType);
        return Enumerable.Range(0, count)
            .Select(index => CheckShape(ModelNativeMethods.occt_model_ancestor_at(
                _handle, root.Id, child.Id, (int)ancestorType, index)))
            .ToArray();
    }
}
'''
write("src/OcctNet/OcctModelingSession.Topology.cs", topology)

geometry_queries = '''using System.Runtime.InteropServices;

namespace OcctNet;

public sealed partial class OcctModelingSession
{
    public OcctPoint3d GetVertexPoint(OcctModelShape vertex)
    {
        EnsureShape(vertex);
        Check(ModelNativeMethods.occt_model_vertex_point(_handle, vertex.Id, out var result));
        return result;
    }

    public (OcctPoint3d Start, OcctPoint3d End) GetEdgeEndpoints(OcctModelShape edge)
    {
        EnsureShape(edge);
        Check(ModelNativeMethods.occt_model_edge_endpoints(
            _handle, edge.Id, out var start, out var end));
        return (start, end);
    }

    public OcctEdgeEvaluation EvaluateEdgeNormalized(
        OcctModelShape edge,
        double normalizedParameter)
    {
        EnsureShape(edge);
        if (normalizedParameter < 0.0 || normalizedParameter > 1.0)
            throw new ArgumentOutOfRangeException(
                nameof(normalizedParameter),
                "Normalized edge parameter must be in the range [0, 1].");
        Check(ModelNativeMethods.occt_model_edge_point_at(
            _handle,
            edge.Id,
            normalizedParameter,
            out var point,
            out var tangent));
        return new OcctEdgeEvaluation(point, tangent);
    }

    public OcctEdgeEvaluation EvaluateEdge(
        OcctModelShape edge,
        double normalizedParameter) => EvaluateEdgeNormalized(edge, normalizedParameter);

    public OcctCurveType GetEdgeCurveType(OcctModelShape edge)
    {
        EnsureShape(edge);
        return (OcctCurveType)ModelNativeMethods.occt_model_edge_curve_type(_handle, edge.Id);
    }

    public OcctCurveType GetCurveType(OcctModelShape edge) => GetEdgeCurveType(edge);

    public OcctSurfaceType GetFaceSurfaceType(OcctModelShape face)
    {
        EnsureShape(face);
        return (OcctSurfaceType)ModelNativeMethods.occt_model_face_surface_type(_handle, face.Id);
    }

    public OcctSurfaceType GetSurfaceType(OcctModelShape face) => GetFaceSurfaceType(face);

    public OcctUvBounds GetFaceUvBounds(OcctModelShape face)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_uv_bounds(_handle, face.Id, out var result));
        return result;
    }

    public OcctUvBounds GetUvBounds(OcctModelShape face) => GetFaceUvBounds(face);

    public OcctFaceEvaluation EvaluateFaceAtParameters(
        OcctModelShape face,
        double u,
        double v)
    {
        EnsureShape(face);
        Check(ModelNativeMethods.occt_model_face_point_normal(
            _handle, face.Id, u, v, out var point, out var normal));
        return new OcctFaceEvaluation(point, normal);
    }

    public OcctFaceEvaluation EvaluateFace(OcctModelShape face, double u, double v) =>
        EvaluateFaceAtParameters(face, u, v);
}
'''
write("src/OcctNet/OcctModelingSession.GeometryQueries.cs", geometry_queries)

# Every bridge P/Invoke uses Cdecl plus exact symbol spelling.
for path in (ROOT / "src/OcctNet").glob("*NativeMethods*.cs"):
    text = path.read_bytes().decode("utf-8-sig").replace("\r\n", "\n")
    text = re.sub(
        r"CallingConvention\s*=\s*CallingConvention\.Cdecl(?!\s*,\s*ExactSpelling)",
        "CallingConvention = CallingConvention.Cdecl, ExactSpelling = true",
        text)
    write(str(path.relative_to(ROOT)), text)

bridge_info = read("src/OcctNet/OcctBridgeInfo.cs").replace("2.4.0", "2.5.0")
write("src/OcctNet/OcctBridgeInfo.cs", bridge_info)

# -----------------------------------------------------------------------------
# Smoke coverage for canonical naming and differential queries.
# -----------------------------------------------------------------------------
smoke = read("tests/OcctNet.Smoke/Program.cs")
smoke = smoke.replace(
    "var firstFace = model.GetSubshape(cut.Shape, OcctShapeType.Face, 0);",
    "var firstFace = model.GetSubshapeAt(cut.Shape, OcctShapeType.Face, 0);")
smoke = replace_once(
    smoke,
    "var lowerCircle = model.MakeCircle(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 10);\n",
    "var lowerCircle = model.MakeCircle(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 10);\n"
    "var circleRange = model.GetEdgeParameterRange(lowerCircle);\n"
    "var circleParameter = (circleRange.FirstParameter + circleRange.LastParameter) * 0.5;\n"
    "var circleDifferential = model.EvaluateEdgeAtParameter(lowerCircle, circleParameter);\n"
    "var circleCurvature = model.GetEdgeCurvature(lowerCircle, circleParameter);\n"
    "if (!circleCurvature.HasTangent || Math.Abs(circleCurvature.Curvature - 0.1) > 1e-6)\n"
    "    throw new InvalidOperationException(\"Circle differential geometry is invalid.\");\n"
    "if (circleDifferential.FirstDerivative.X == 0 && circleDifferential.FirstDerivative.Y == 0)\n"
    "    throw new InvalidOperationException(\"Circle first derivative is invalid.\");\n",
    "circle differential smoke")
smoke = replace_once(
    smoke,
    "var faceMesh = model.GetFaceMesh(firstFace);\n",
    "var faceMesh = model.GetFaceMesh(firstFace);\n"
    "var faceUv = model.GetFaceUvBounds(firstFace);\n"
    "var faceU = (faceUv.UMin + faceUv.UMax) * 0.5;\n"
    "var faceV = (faceUv.VMin + faceUv.VMax) * 0.5;\n"
    "var facePeriodicity = model.GetFacePeriodicity(firstFace);\n"
    "var faceDifferential = model.EvaluateFaceDifferential(firstFace, faceU, faceV);\n"
    "var faceCurvature = model.GetFaceCurvature(firstFace, faceU, faceV);\n"
    "if (!faceDifferential.HasNormal || !faceCurvature.HasNormal)\n"
    "    throw new InvalidOperationException(\"Face differential geometry has no normal.\");\n"
    "_ = facePeriodicity;\n",
    "face differential smoke")
write("tests/OcctNet.Smoke/Program.cs", smoke)

# -----------------------------------------------------------------------------
# Validation scripts and normalized build orchestration.
# -----------------------------------------------------------------------------
differential_check = r'''param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$contracts = [ordered]@{
    "src/OcctNative/OcctModelingDifferentialGeometry.cpp" = @(
        "BRepLProp_CLProps",
        "BRepLProp_SLProps",
        "occt_model_edge_parameter_range",
        "occt_model_edge_differential",
        "occt_model_edge_curvature",
        "occt_model_face_periodicity",
        "occt_model_face_differential",
        "occt_model_face_curvature"
    )
    "src/OcctNet/ModelNativeMethods.DifferentialGeometry.cs" = @(
        "occt_model_edge_parameter_range",
        "occt_model_face_curvature",
        "CallingConvention.Cdecl",
        "ExactSpelling = true"
    )
    "src/OcctNet/OcctModelingSession.DifferentialGeometry.cs" = @(
        "GetEdgeParameterRange",
        "EvaluateEdgeAtParameter",
        "GetEdgeCurvature",
        "GetFacePeriodicity",
        "EvaluateFaceDifferential",
        "GetFaceCurvature"
    )
    "src/OcctNet/OcctDifferentialGeometryTypes.cs" = @(
        "OcctModelParameterRange",
        "OcctModelCurveDifferential",
        "OcctModelCurveCurvature",
        "OcctModelSurfacePeriodicity",
        "OcctModelSurfaceDifferential",
        "OcctModelSurfaceCurvature",
        "StructLayout(LayoutKind.Sequential)"
    )
}

foreach ($contract in $contracts.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $contract.Key
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Differential geometry API file was not found: $($contract.Key)"
    }
    $text = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Differential geometry token is missing from $($contract.Key): $token"
        }
    }
}

Write-Host "[differential-geometry] Curve and surface derivatives, periodicity and curvature contracts validated." -ForegroundColor Green
'''
write("tests/check-differential-geometry-api.ps1", differential_check)

organization_check = r'''param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$requiredFiles = @(
    "src/OcctNet/OcctModelingSession.cs",
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs",
    "src/OcctNet/OcctModelingSession.Topology.cs",
    "src/OcctNet/OcctModelingSession.GeometryQueries.cs",
    "src/OcctNet/OcctModelingSession.AnalyticGeometry.cs",
    "src/OcctNet/OcctModelingSession.DifferentialGeometry.cs",
    "src/OcctNet/OcctModelingSession.Geometry.cs",
    "src/OcctNet/OcctModelingSession.Algorithms.cs",
    "src/OcctNet/OcctModelingSession.Analysis.cs",
    "src/OcctNet/OcctModelingSession.History.cs"
)
foreach ($relativePath in $requiredFiles) {
    if (-not (Test-Path (Join-Path $RepositoryRoot $relativePath) -PathType Leaf)) {
        throw "Managed API category file is missing: $relativePath"
    }
}

$baseText = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot "src/OcctNet/OcctModelingSession.cs"))
foreach ($forbidden in @("GetShapeHash(", "GetTopologyCount(", "GetVertexPoint(", "GetEdgeCurveType(")) {
    if ($baseText.Contains($forbidden)) {
        throw "OcctModelingSession.cs contains a categorized API method: $forbidden"
    }
}

$canonicalContracts = [ordered]@{
    "src/OcctNet/OcctModelingSession.ShapeQueries.cs" = @(
        "GetShapeOrientation",
        "GetShapeBounds",
        "GetShapeDistance",
        "GetShapeLocation",
        "SetShapeLocation"
    )
    "src/OcctNet/OcctModelingSession.Topology.cs" = @("GetSubshapeAt")
    "src/OcctNet/OcctModelingSession.GeometryQueries.cs" = @(
        "EvaluateEdgeNormalized",
        "GetEdgeCurveType",
        "GetFaceSurfaceType",
        "GetFaceUvBounds",
        "EvaluateFaceAtParameters"
    )
}
foreach ($contract in $canonicalContracts.GetEnumerator()) {
    $text = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $contract.Key))
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Canonical managed API is missing from $($contract.Key): $token"
        }
    }
}

$nativeMethodFiles = Get-ChildItem (Join-Path $RepositoryRoot "src/OcctNet") -Filter "*NativeMethods*.cs" -File
foreach ($file in $nativeMethodFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    $attributes = [regex]::Matches($text, '\[DllImport\(LibraryName(?<body>.*?)\)\]', 'Singleline')
    foreach ($attribute in $attributes) {
        $body = $attribute.Groups['body'].Value
        if ($body -notmatch 'CallingConvention\s*=\s*CallingConvention\.Cdecl') {
            throw "Bridge P/Invoke does not declare Cdecl: $($file.Name)"
        }
        if ($body -notmatch 'ExactSpelling\s*=\s*true') {
            throw "Bridge P/Invoke does not use exact symbol spelling: $($file.Name)"
        }
    }
}

$docs = @(Get-ChildItem (Join-Path $RepositoryRoot "docs") -File | Select-Object -ExpandProperty Name | Sort-Object)
$expectedDocs = @("API_COVERAGE.md", "API_COVERAGE.zh-CN.md")
if (Compare-Object $expectedDocs $docs) {
    throw "The docs directory must contain only API_COVERAGE.md and API_COVERAGE.zh-CN.md."
}

Write-Host "[organization] Managed categories, canonical naming, P/Invoke attributes and documentation layout validated." -ForegroundColor Green
'''
write("tests/check-api-organization.ps1", organization_check)

version_check = r'''param(
    [string]$RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$expectedVersion = "2.5.0"
$expectedNativeCount = 327
$expectedPublicTypeCount = 75

$contracts = [ordered]@{
    "src/OcctNative/OcctEngine.cpp" = @("occt_bridge_version()", $expectedVersion)
    "src/OcctNet/OcctBridgeInfo.cs" = @("ManagedVersion", $expectedVersion)
    "README.md" = @("Bridge version: `$expectedVersion`")
    "README.zh-CN.md" = @("Bridge 版本：`$expectedVersion`")
    "docs/API_COVERAGE.md" = @(
        "Native exports: `$expectedNativeCount`",
        "Managed P/Invoke declarations: `$expectedNativeCount`",
        "Public .NET types: `$expectedPublicTypeCount`",
        "Native bridge version: `$expectedVersion`"
    )
    "docs/API_COVERAGE.zh-CN.md" = @(
        "Native exports: `$expectedNativeCount`",
        "Managed P/Invoke declarations: `$expectedNativeCount`",
        "Public .NET types: `$expectedPublicTypeCount`",
        "原生桥接版本：`$expectedVersion`"
    )
}

foreach ($contract in $contracts.GetEnumerator()) {
    $path = Join-Path $RepositoryRoot $contract.Key
    if (-not (Test-Path $path -PathType Leaf)) {
        throw "Version contract file was not found: $($contract.Key)"
    }
    $text = [System.IO.File]::ReadAllText($path)
    foreach ($token in $contract.Value) {
        if (-not $text.Contains($token)) {
            throw "Version contract is stale in $($contract.Key): $token"
        }
    }
}

foreach ($path in @("build.ps1", "src/OcctNative/CMakeLists.txt")) {
    $text = [System.IO.File]::ReadAllText((Join-Path $RepositoryRoot $path))
    if ($text -match '(?i)D:[\\/]tools[\\/]occt') {
        throw "A machine-specific OCCT path remains in $path."
    }
}

Write-Host "[version] Bridge $expectedVersion, ABI 2 and API inventory counts validated." -ForegroundColor Green
'''
write("tests/check-version-contract.ps1", version_check)

build_script = r'''param(
    [Parameter(Position = 0)]
    [ValidateSet("validate", "native", "managed", "smoke", "all")]
    [string]$Target = "all",

    [Parameter(Position = 1)]
    [ValidateSet("Debug", "Release", "RelWithDebInfo")]
    [string]$Configuration = "Release",

    [string]$OcctRoot = $env:OCCT_ROOT
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
$env:DOTNET_CLI_UI_LANGUAGE = "en-US"
$env:VSLANG = "1033"

if (Test-Path "$env:SystemRoot\System32\chcp.com") {
    & "$env:SystemRoot\System32\chcp.com" 65001 | Out-Null
}

$Target = $Target.ToLowerInvariant()
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$NativeSource = Join-Path $RepoRoot "src\OcctNative"
$NativeBuild = Join-Path $RepoRoot "build\native"
$NativeDll = Join-Path $NativeBuild "bin\$Configuration\OcctNative.dll"

$Projects = [ordered]@{
    Core = "src\OcctNet\OcctNet.csproj"
    WinForms = "src\OcctNet.WinForms\OcctNet.WinForms.csproj"
    Wpf = "src\OcctNet.Wpf\OcctNet.Wpf.csproj"
    Smoke = "tests\OcctNet.Smoke\OcctNet.Smoke.csproj"
}

$Checks = [ordered]@{
    Version = "tests\check-version-contract.ps1"
    Organization = "tests\check-api-organization.ps1"
    AnalyticGeometry = "tests\check-analytic-geometry-api.ps1"
    DifferentialGeometry = "tests\check-differential-geometry-api.ps1"
    UiHosts = "tests\check-ui-hosts.ps1"
    Viewport = "tests\check-viewport-api.ps1"
    Selection = "tests\check-selection-contract.ps1"
    NativeBuild = "tests\check-native-build-structure.ps1"
    ApiSurface = "tests\check-api-surface.ps1"
}

function Assert-Path {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path $Path)) {
        throw "Required path was not found: $Path"
    }
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "$Name was not found in PATH."
    }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][object[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$ErrorMessage
    )
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw $ErrorMessage
    }
}

function Invoke-ContractChecks {
    foreach ($check in $Checks.GetEnumerator()) {
        $path = Join-Path $RepoRoot $check.Value
        Assert-Path $path
        Write-Host ("[{0}] Running {1}..." -f $check.Key.ToLowerInvariant(), $check.Value) -ForegroundColor Cyan
        & $path -RepositoryRoot $RepoRoot
        if (-not $?) {
            throw "$($check.Key) validation failed."
        }
    }
}

function Resolve-OcctConfiguration {
    if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
        throw "OCCT_ROOT is not configured. Pass -OcctRoot <path> or set the OCCT_ROOT environment variable."
    }

    $script:OcctRoot = [System.IO.Path]::GetFullPath($OcctRoot)
    $script:OcctIncludeDir = Join-Path $script:OcctRoot "inc"
    $script:OcctLibDir = Join-Path $script:OcctRoot "win64\vc14\lib"
    $script:OcctBinDir = Join-Path $script:OcctRoot "win64\vc14\bin"

    foreach ($path in @(
        $script:OcctIncludeDir,
        $script:OcctLibDir,
        $script:OcctBinDir,
        (Join-Path $script:OcctIncludeDir "Standard.hxx"),
        (Join-Path $script:OcctLibDir "TKernel.lib"),
        (Join-Path $script:OcctBinDir "TKernel.dll")
    )) {
        Assert-Path $path
    }
}

function Build-Native {
    Assert-Command "cmake"
    Resolve-OcctConfiguration

    Write-Host "[native] Configuring OCCT 7.9.0 bridge..." -ForegroundColor Cyan
    Invoke-Checked "cmake" @(
        "-S", $NativeSource,
        "-B", $NativeBuild,
        "-G", "Visual Studio 17 2022",
        "-A", "x64",
        "-DOCCT_ROOT=$script:OcctRoot",
        "-DOCCT_INCLUDE_DIR=$script:OcctIncludeDir",
        "-DOCCT_LIB_DIR=$script:OcctLibDir",
        "-DOCCT_BIN_DIR=$script:OcctBinDir"
    ) "CMake configure failed."

    Write-Host "[native] Building $Configuration..." -ForegroundColor Cyan
    Invoke-Checked "cmake" @(
        "--build", $NativeBuild,
        "--config", $Configuration,
        "--parallel"
    ) "Native build failed."

    Assert-Path $NativeDll
    Write-Host "Native: $NativeDll" -ForegroundColor Green
}

function Build-Project {
    param([Parameter(Mandatory = $true)][string]$Name)

    Assert-Command "dotnet"
    $relativePath = $Projects[$Name]
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        throw "Unknown project key: $Name"
    }

    $project = Join-Path $RepoRoot $relativePath
    Assert-Path $project
    $projectDirectory = Split-Path -Parent $project
    Remove-Item (Join-Path $projectDirectory "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $projectDirectory "obj") -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host "[$($Name.ToLowerInvariant())] Building $Configuration..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $project,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "$Name build failed."
}

function Build-Managed {
    Build-Project "Core"
    Build-Project "WinForms"
    Build-Project "Wpf"
}

function Run-Smoke {
    Assert-Path $NativeDll
    Build-Project "Smoke"

    $smokeProject = Join-Path $RepoRoot $Projects.Smoke
    $smokeOutput = Join-Path (Split-Path -Parent $smokeProject) "bin\x64\$Configuration\net8.0-windows"
    Copy-Item $NativeDll (Join-Path $smokeOutput "OcctNative.dll") -Force

    $previousNativeDirectory = $env:OCCT_BRIDGE_NATIVE_DIR
    try {
        $env:OCCT_BRIDGE_NATIVE_DIR = $smokeOutput
        Write-Host "[smoke] Running native modeling scenarios..." -ForegroundColor Cyan
        Invoke-Checked "dotnet" @(
            "run",
            "--project", $smokeProject,
            "-c", $Configuration,
            "-p:Platform=x64",
            "--no-build"
        ) "Smoke test failed."
    }
    finally {
        $env:OCCT_BRIDGE_NATIVE_DIR = $previousNativeDirectory
    }
}

Write-Host "Target:        $Target"
Write-Host "Configuration: $Configuration"
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    Write-Host "OCCT root:     not configured (valid for validate/managed)" -ForegroundColor DarkGray
}
else {
    Write-Host "OCCT root:     $OcctRoot" -ForegroundColor DarkGray
}

Invoke-ContractChecks

switch ($Target) {
    "validate" { }
    "native" { Build-Native }
    "managed" { Build-Managed }
    "smoke" {
        Build-Native
        Build-Managed
        Run-Smoke
    }
    "all" {
        Build-Native
        Build-Managed
    }
}

Write-Host "Build completed." -ForegroundColor Green
'''
write("build.ps1", build_script)

# Strengthen API validation: require exact symbol spelling and category counts in docs.
api_check = read("tests/check-api-surface.ps1")
api_check = replace_once(
    api_check,
    "$cdeclPInvokes = Get-Matches $pinvokeText '(?s)\\[DllImport\\([^\\]]*CallingConvention\\s*=\\s*CallingConvention\\.Cdecl[^\\]]*\\)\\]\\s*internal\\s+static\\s+extern\\s+[A-Za-z0-9_<>,\\[\\]?]+\\s+(occt_[a-z0-9_]+)\\s*\\('\n",
    "$cdeclPInvokes = Get-Matches $pinvokeText '(?s)\\[DllImport\\([^\\]]*CallingConvention\\s*=\\s*CallingConvention\\.Cdecl[^\\]]*\\)\\]\\s*internal\\s+static\\s+extern\\s+[A-Za-z0-9_<>,\\[\\]?]+\\s+(occt_[a-z0-9_]+)\\s*\\('\n"
    "$exactPInvokes = Get-Matches $pinvokeText '(?s)\\[DllImport\\([^\\]]*ExactSpelling\\s*=\\s*true[^\\]]*\\)\\]\\s*internal\\s+static\\s+extern\\s+[A-Za-z0-9_<>,\\[\\]?]+\\s+(occt_[a-z0-9_]+)\\s*\\('\n",
    "exact PInvoke extraction")
api_check = replace_once(
    api_check,
    'Assert-SetEqual "Cdecl P/Invoke declarations" $pinvokes $cdeclPInvokes\n',
    'Assert-SetEqual "Cdecl P/Invoke declarations" $pinvokes $cdeclPInvokes\nAssert-SetEqual "exact-name P/Invoke declarations" $pinvokes $exactPInvokes\n',
    "exact PInvoke comparison")
write("tests/check-api-surface.ps1", api_check)

# -----------------------------------------------------------------------------
# Documentation: retain two detailed inventories and add explicit design rules.
# -----------------------------------------------------------------------------
english = read("docs/API_COVERAGE.md")
english = english.replace("Native exports: `321`", "Native exports: `327`")
english = english.replace("Managed P/Invoke declarations: `321`", "Managed P/Invoke declarations: `327`")
english = english.replace("Public .NET types: `69`", "Public .NET types: `75`")
english = english.replace("### OcctModeling.h (112)", "### OcctModeling.h (118)")
english = english.replace("Native bridge version: `2.4.0`", "Native bridge version: `2.5.0`")
english_rules = '''## API design and naming rules

The public wrapper follows one vocabulary across native C ABI, P/Invoke, and managed APIs:

| Layer | Rule | Example |
|---|---|---|
| Native C ABI | `occt_model_<subject>_<operation>` | `occt_model_face_curvature` |
| P/Invoke | Preserve the native symbol exactly; use Cdecl and exact spelling | `occt_model_edge_differential` |
| Managed query | `Get<Subject><Result>` | `GetFaceCurvature()` |
| Managed evaluation | `Evaluate<Subject><ParameterMeaning>` | `EvaluateEdgeAtParameter()` |
| Collection indexing | Use the `At` suffix for zero-based indexed access | `GetSubshapeAt()` |
| Compatibility alias | Existing ambiguous names remain forwarding aliases | `GetBounds()` forwards to `GetShapeBounds()` |

Managed modeling APIs are organized by responsibility: session/core, shape queries, topology, geometry queries, analytic geometry, differential geometry, construction, algorithms, analysis, mesh, exchange, and operation history. A method must be placed in the corresponding partial-class file instead of the session core file.

Parameter semantics are explicit. `EvaluateEdgeNormalized()` accepts `[0, 1]`; `EvaluateEdgeAtParameter()` accepts the exact OCCT curve parameter. Face evaluation methods use exact surface `U` and `V` parameters.

'''
english = replace_once(english, "## Choose an assembly\n", english_rules + "## Choose an assembly\n", "English API rules")
english_diff = '''### Differential geometry

Differential queries expose exact curve and surface derivatives, periodicity, normals, and curvature without converting the model to a mesh.

| Managed API | Input semantics | Returned data |
|---|---|---|
| `GetEdgeParameterRange()` | Edge | Exact first/last parameters, closed/periodic flags and period |
| `EvaluateEdgeAtParameter()` | Exact curve parameter | Point, first derivative and second derivative |
| `GetEdgeCurvature()` | Exact curve parameter | Tangent, normal, center and scalar curvature with definition flags |
| `GetFacePeriodicity()` | Face | U/V closed and periodic flags plus periods |
| `EvaluateFaceDifferential()` | Exact U/V | Point, oriented normal, first and second partial derivatives |
| `GetFaceCurvature()` | Exact U/V | Principal, mean and Gaussian curvature, principal directions and umbilic state |

```csharp
var range = model.GetEdgeParameterRange(edge);
var parameter = (range.FirstParameter + range.LastParameter) * 0.5;
var differential = model.EvaluateEdgeAtParameter(edge, parameter);
var curvature = model.GetEdgeCurvature(edge, parameter);

var uv = model.GetFaceUvBounds(face);
var u = (uv.UMin + uv.UMax) * 0.5;
var v = (uv.VMin + uv.VMax) * 0.5;
var surface = model.EvaluateFaceDifferential(face, u, v);
var surfaceCurvature = model.GetFaceCurvature(face, u, v);
```

Normals follow the topological face orientation. For reversed faces, principal curvatures and mean curvature are sign-adjusted and principal maximum/minimum values are reordered; Gaussian curvature is unchanged. Undefined tangents, normals, and curvature are represented by explicit `Has...` flags.

'''
english = replace_once(english, "### Geometry and feature modeling\n", english_diff + "### Geometry and feature modeling\n", "English differential section")
english = replace_once(
    english,
    "- `occt_model_face_torus_geometry`\n",
    "- `occt_model_face_torus_geometry`\n"
    "- `occt_model_edge_parameter_range`\n"
    "- `occt_model_edge_differential`\n"
    "- `occt_model_edge_curvature`\n"
    "- `occt_model_face_periodicity`\n"
    "- `occt_model_face_differential`\n"
    "- `occt_model_face_curvature`\n",
    "English native differential inventory")
new_types = """- `OcctModelParameterRange`
- `OcctModelCurveDifferential`
- `OcctModelCurveCurvature`
- `OcctModelSurfacePeriodicity`
- `OcctModelSurfaceDifferential`
- `OcctModelSurfaceCurvature`
"""
english = english.replace("- `OcctTorusGeometry`\n", "- `OcctTorusGeometry`\n" + new_types)
write("docs/API_COVERAGE.md", english)

chinese = read("docs/API_COVERAGE.zh-CN.md")
chinese = chinese.replace("Native exports: `321`", "Native exports: `327`")
chinese = chinese.replace("Managed P/Invoke declarations: `321`", "Managed P/Invoke declarations: `327`")
chinese = chinese.replace("Public .NET types: `69`", "Public .NET types: `75`")
chinese = chinese.replace("### OcctModeling.h (112)", "### OcctModeling.h (118)")
chinese = chinese.replace("原生桥接版本：`2.4.0`", "原生桥接版本：`2.5.0`")
chinese_rules = '''## 接口设计与命名规范

原生 C ABI、P/Invoke 与托管接口使用统一词汇：

| 层级 | 规则 | 示例 |
|---|---|---|
| 原生 C ABI | `occt_model_<对象>_<操作>` | `occt_model_face_curvature` |
| P/Invoke | 完整保留原生符号，统一 Cdecl 和精确名称 | `occt_model_edge_differential` |
| 托管查询 | `Get<对象><结果>` | `GetFaceCurvature()` |
| 托管求值 | `Evaluate<对象><参数含义>` | `EvaluateEdgeAtParameter()` |
| 集合索引 | 零基索引访问统一使用 `At` 后缀 | `GetSubshapeAt()` |
| 兼容别名 | 已发布的含义不够明确的方法保留为转发别名 | `GetBounds()` 转发到 `GetShapeBounds()` |

建模接口按职责划分为：会话与生命周期、形状查询、拓扑、几何查询、解析几何、微分几何、构造、算法、分析、网格、文件交换和操作历史。新增方法必须进入对应的 partial class 文件，不再堆入会话核心文件。

参数含义必须体现在方法名中。`EvaluateEdgeNormalized()` 接收 `[0, 1]` 归一化参数；`EvaluateEdgeAtParameter()` 接收 OCCT 原始曲线参数；面求值接口使用原始 `U`、`V` 参数。

'''
chinese = replace_once(chinese, "## 选择程序集\n", chinese_rules + "## 选择程序集\n", "Chinese API rules")
chinese_diff = '''### 微分几何

微分几何接口直接读取曲线、曲面的导数、周期性、法向和曲率，不需要先转换为三角网格。

| 托管接口 | 参数含义 | 返回内容 |
|---|---|---|
| `GetEdgeParameterRange()` | 边 | 原始首尾参数、闭合/周期标志和周期 |
| `EvaluateEdgeAtParameter()` | 原始曲线参数 | 点、一阶导数和二阶导数 |
| `GetEdgeCurvature()` | 原始曲线参数 | 切向、法向、曲率中心、曲率及定义标志 |
| `GetFacePeriodicity()` | 面 | U/V 闭合、周期标志及周期 |
| `EvaluateFaceDifferential()` | 原始 U/V | 点、按面方向修正的法向、一阶及二阶偏导 |
| `GetFaceCurvature()` | 原始 U/V | 主曲率、平均曲率、高斯曲率、主方向和脐点状态 |

```csharp
var range = model.GetEdgeParameterRange(edge);
var parameter = (range.FirstParameter + range.LastParameter) * 0.5;
var differential = model.EvaluateEdgeAtParameter(edge, parameter);
var curvature = model.GetEdgeCurvature(edge, parameter);

var uv = model.GetFaceUvBounds(face);
var u = (uv.UMin + uv.UMax) * 0.5;
var v = (uv.VMin + uv.VMax) * 0.5;
var surface = model.EvaluateFaceDifferential(face, u, v);
var surfaceCurvature = model.GetFaceCurvature(face, u, v);
```

法向遵循拓扑面的方向。面为反向时，主曲率和平均曲率会进行符号修正，并重新排列最大、最小主曲率；高斯曲率保持不变。切向、法向或曲率不可定义时，通过明确的 `Has...` 属性表达，不使用无意义数值冒充有效结果。

'''
chinese = replace_once(chinese, "### 几何与特征建模\n", chinese_diff + "### 几何与特征建模\n", "Chinese differential section")
chinese = replace_once(
    chinese,
    "- `occt_model_face_torus_geometry`\n",
    "- `occt_model_face_torus_geometry`\n"
    "- `occt_model_edge_parameter_range`\n"
    "- `occt_model_edge_differential`\n"
    "- `occt_model_edge_curvature`\n"
    "- `occt_model_face_periodicity`\n"
    "- `occt_model_face_differential`\n"
    "- `occt_model_face_curvature`\n",
    "Chinese native differential inventory")
chinese = chinese.replace("- `OcctTorusGeometry`\n", "- `OcctTorusGeometry`\n" + new_types)
write("docs/API_COVERAGE.zh-CN.md", chinese)

readme = read("README.md")
readme = readme.replace(
    "Exact line, circle, ellipse, plane, cylinder, cone, sphere, and torus parameters support feature recognition and engineering automation.",
    "Exact analytic parameters plus curve/surface derivatives, periodicity and curvature support feature recognition, engineering rules and parametric reconstruction.")
readme = replace_once(
    readme,
    "- Managed target: `.NET 8`, Windows x64.\n",
    "- Managed target: `.NET 8`, Windows x64.\n- Bridge version: `2.5.0`; ABI: `2`.\n",
    "English README version")
write("README.md", readme)

readme_zh = read("README.zh-CN.md")
readme_zh = readme_zh.replace(
    "精确的直线、圆、椭圆、平面、圆柱、圆锥、球面和圆环面参数可用于特征识别与工程自动化。",
    "解析几何参数以及曲线、曲面的导数、周期性和曲率可用于特征识别、工程规则判断与参数化重建。")
readme_zh = replace_once(
    readme_zh,
    "- 托管目标：`.NET 8`，Windows x64。\n",
    "- 托管目标：`.NET 8`，Windows x64。\n- Bridge 版本：`2.5.0`；ABI：`2`。\n",
    "Chinese README version")
write("README.zh-CN.md", readme_zh)

# Remove this one-shot migration script from the produced commit.
Path(__file__).unlink()
