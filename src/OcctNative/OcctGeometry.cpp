#include "OcctInternal.hxx"

#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepBuilderAPI_MakeSolid.hxx>
#include <BRepBuilderAPI_MakeVertex.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_Sewing.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeTorus.hxx>
#include <BRepPrimAPI_MakeWedge.hxx>
#include <GC_MakeArcOfCircle.hxx>
#include <GeomAPI_Interpolate.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_Circle.hxx>
#include <Geom_Ellipse.hxx>
#include <TColgp_Array1OfPnt.hxx>
#include <TColgp_HArray1OfPnt.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Shell.hxx>
#include <TopoDS_Wire.hxx>

using namespace OcctBridge;

namespace
{
    void hideInputs(Engine* engine, const OcctObjectId* ids, int count)
    {
        for (int index = 0; index < count; ++index) engine->hide(ids[index]);
    }

    std::vector<gp_Pnt> pointsFrom(const OcctPoint3d* points, int count)
    {
        if (points == nullptr) throw std::invalid_argument("Point array is null.");
        std::vector<gp_Pnt> result;
        result.reserve(static_cast<std::size_t>(count));
        for (int index = 0; index < count; ++index) result.push_back(point(points[index]));
        return result;
    }

    TopoDS_Wire rectangleWire(OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height)
    {
        requirePositive(width, "Width");
        requirePositive(height, "Height");
        const gp_Ax2 plane = axis2(origin, normal, xDirection);
        const gp_Pnt p0 = plane.Location();
        const gp_Vec xVector(plane.XDirection());
        const gp_Vec yVector(plane.YDirection());
        const gp_Pnt p1 = p0.Translated(xVector * width);
        const gp_Pnt p2 = p1.Translated(yVector * height);
        const gp_Pnt p3 = p0.Translated(yVector * height);
        BRepBuilderAPI_MakePolygon polygon;
        polygon.Add(p0); polygon.Add(p1); polygon.Add(p2); polygon.Add(p3); polygon.Close();
        if (!polygon.IsDone()) throw std::runtime_error("Rectangle wire creation failed.");
        return polygon.Wire();
    }
}

extern "C"
{
    OcctObjectId occt_make_vertex(OcctHandle h, OcctPoint3d p)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{return e->addShape(BRepBuilderAPI_MakeVertex(point(p)).Shape(),false,"Vertex");});
    }

    OcctObjectId occt_make_line(OcctHandle h, OcctPoint3d start, OcctPoint3d end)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{if(point(start).Distance(point(end))<=Precision::Confusion())throw std::invalid_argument("Line endpoints must be different.");BRepBuilderAPI_MakeEdge maker(point(start),point(end));if(!maker.IsDone())throw std::runtime_error("Line creation failed.");return e->addShape(maker.Shape(),false,"Line");});
    }

    OcctObjectId occt_make_polyline(OcctHandle h, const OcctPoint3d* input, int count, int closed)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requireCount(count,2,"Polyline");const auto points=pointsFrom(input,count);BRepBuilderAPI_MakePolygon maker;for(const gp_Pnt& p:points)maker.Add(p);if(closed)maker.Close();if(!maker.IsDone())throw std::runtime_error("Polyline creation failed.");return e->addShape(maker.Wire(),false,closed?"Polygon":"Polyline");});
    }

    OcctObjectId occt_make_circle(OcctHandle h, OcctPoint3d center, OcctVector3d normal, double radius)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requirePositive(radius,"Radius");Handle(Geom_Circle) curve=new Geom_Circle(axis2(center,normal),radius);BRepBuilderAPI_MakeEdge maker(curve);if(!maker.IsDone())throw std::runtime_error("Circle creation failed.");return e->addShape(maker.Shape(),false,"Circle");});
    }

    OcctObjectId occt_make_arc_three_points(OcctHandle h, OcctPoint3d start, OcctPoint3d middle, OcctPoint3d end)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{GC_MakeArcOfCircle arc(point(start),point(middle),point(end));if(!arc.IsDone())throw std::runtime_error("Arc creation failed.");BRepBuilderAPI_MakeEdge maker(arc.Value());if(!maker.IsDone())throw std::runtime_error("Arc edge creation failed.");return e->addShape(maker.Shape(),false,"Arc");});
    }

    OcctObjectId occt_make_ellipse(OcctHandle h, OcctPoint3d center, OcctVector3d normal, double majorRadius, double minorRadius)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requirePositive(majorRadius,"Major radius");requirePositive(minorRadius,"Minor radius");if(majorRadius<minorRadius)throw std::invalid_argument("Major radius must be greater than or equal to minor radius.");Handle(Geom_Ellipse) curve=new Geom_Ellipse(axis2(center,normal),majorRadius,minorRadius);BRepBuilderAPI_MakeEdge maker(curve);if(!maker.IsDone())throw std::runtime_error("Ellipse creation failed.");return e->addShape(maker.Shape(),false,"Ellipse");});
    }

    OcctObjectId occt_make_bezier(OcctHandle h, const OcctPoint3d* input, int count)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requireCount(count,2,"Bezier curve");if(input==nullptr)throw std::invalid_argument("Pole array is null.");TColgp_Array1OfPnt poles(1,count);for(int i=0;i<count;++i)poles.SetValue(i+1,point(input[i]));Handle(Geom_BezierCurve) curve=new Geom_BezierCurve(poles);BRepBuilderAPI_MakeEdge maker(curve);if(!maker.IsDone())throw std::runtime_error("Bezier edge creation failed.");return e->addShape(maker.Shape(),false,"Bezier");});
    }

    OcctObjectId occt_make_bspline_interpolated(OcctHandle h, const OcctPoint3d* input, int count, int periodic, double tolerance)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requireCount(count,2,"B-spline");if(input==nullptr)throw std::invalid_argument("Point array is null.");requirePositive(tolerance,"Tolerance");Handle(TColgp_HArray1OfPnt) points=new TColgp_HArray1OfPnt(1,count);for(int i=0;i<count;++i)points->SetValue(i+1,point(input[i]));GeomAPI_Interpolate interpolation(points,periodic!=0,tolerance);interpolation.Perform();if(!interpolation.IsDone())throw std::runtime_error("B-spline interpolation failed.");BRepBuilderAPI_MakeEdge maker(interpolation.Curve());if(!maker.IsDone())throw std::runtime_error("B-spline edge creation failed.");return e->addShape(maker.Shape(),false,"BSpline");});
    }

    OcctObjectId occt_make_rectangle_wire(OcctHandle h, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{return e->addShape(rectangleWire(origin,xDirection,normal,width,height),false,"Rectangle");});
    }

    OcctObjectId occt_make_face_from_wire(OcctHandle h, OcctObjectId wireId, int onlyPlane)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{ObjectEntry* wire=e->findShape(wireId);if(!wire||wire->shape.ShapeType()!=TopAbs_WIRE)throw std::invalid_argument("Input must be a wire.");BRepBuilderAPI_MakeFace maker(TopoDS::Wire(wire->shape),onlyPlane!=0);if(!maker.IsDone())throw std::runtime_error("Face creation failed.");return e->addShape(maker.Shape(),false,"Face");});
    }

    OcctObjectId occt_make_plane_face(OcctHandle h, OcctPoint3d origin, OcctVector3d xDirection, OcctVector3d normal, double width, double height)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{BRepBuilderAPI_MakeFace maker(rectangleWire(origin,xDirection,normal,width,height),Standard_True);if(!maker.IsDone())throw std::runtime_error("Planar face creation failed.");return e->addShape(maker.Shape(),false,"PlaneFace");});
    }

    OcctObjectId occt_make_box(OcctHandle h, double x, double y, double z, double dx, double dy, double dz)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requirePositive(dx,"Box X size");requirePositive(dy,"Box Y size");requirePositive(dz,"Box Z size");return e->addShape(BRepPrimAPI_MakeBox(gp_Pnt(x,y,z),dx,dy,dz).Shape(),true,"Box");});
    }

    OcctObjectId occt_make_cylinder(OcctHandle h, OcctPoint3d origin, OcctVector3d axis, double radius, double height)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requirePositive(radius,"Radius");requirePositive(height,"Height");return e->addShape(BRepPrimAPI_MakeCylinder(axis2(origin,axis),radius,height).Shape(),true,"Cylinder");});
    }

    OcctObjectId occt_make_sphere(OcctHandle h, OcctPoint3d center, double radius)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requirePositive(radius,"Radius");return e->addShape(BRepPrimAPI_MakeSphere(point(center),radius).Shape(),true,"Sphere");});
    }

    OcctObjectId occt_make_cone(OcctHandle h, OcctPoint3d origin, OcctVector3d axis, double radius1, double radius2, double height)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{if(radius1<0.0||radius2<0.0||radius1+radius2<=0.0)throw std::invalid_argument("Cone radii are invalid.");requirePositive(height,"Height");return e->addShape(BRepPrimAPI_MakeCone(axis2(origin,axis),radius1,radius2,height).Shape(),true,"Cone");});
    }

    OcctObjectId occt_make_torus(OcctHandle h, OcctPoint3d center, OcctVector3d axis, double majorRadius, double minorRadius)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requirePositive(majorRadius,"Major radius");requirePositive(minorRadius,"Minor radius");if(minorRadius>=majorRadius)throw std::invalid_argument("Minor radius must be less than major radius.");return e->addShape(BRepPrimAPI_MakeTorus(axis2(center,axis),majorRadius,minorRadius).Shape(),true,"Torus");});
    }

    OcctObjectId occt_make_wedge(OcctHandle h, double dx, double dy, double dz, double ltx)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requirePositive(dx,"Wedge X size");requirePositive(dy,"Wedge Y size");requirePositive(dz,"Wedge Z size");return e->addShape(BRepPrimAPI_MakeWedge(dx,dy,dz,ltx).Shape(),true,"Wedge");});
    }

    OcctObjectId occt_make_compound(OcctHandle h, const OcctObjectId* ids, int count, int hide)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requireCount(count,1,"Compound");if(!ids)throw std::invalid_argument("Shape ID array is null.");BRep_Builder builder;TopoDS_Compound compound;builder.MakeCompound(compound);for(int i=0;i<count;++i){ObjectEntry* item=e->findShape(ids[i]);if(!item)throw std::invalid_argument("Compound contains an invalid shape ID.");builder.Add(compound,item->shape);}const auto result=e->addShape(compound,false,"Compound");if(hide)hideInputs(e,ids,count);return result;});
    }

    OcctObjectId occt_make_wire(OcctHandle h, const OcctObjectId* ids, int count, int hide)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requireCount(count,1,"Wire");if(!ids)throw std::invalid_argument("Edge ID array is null.");BRepBuilderAPI_MakeWire maker;for(int i=0;i<count;++i){ObjectEntry* item=e->findShape(ids[i]);if(!item||item->shape.ShapeType()!=TopAbs_EDGE)throw std::invalid_argument("Wire inputs must be edges.");maker.Add(TopoDS::Edge(item->shape));}if(!maker.IsDone())throw std::runtime_error("Wire assembly failed.");const auto result=e->addShape(maker.Wire(),false,"Wire");if(hide)hideInputs(e,ids,count);return result;});
    }

    OcctObjectId occt_sew_shapes(OcctHandle h, const OcctObjectId* ids, int count, double tolerance, int hide)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{requireCount(count,1,"Sewing");requirePositive(tolerance,"Tolerance");if(!ids)throw std::invalid_argument("Shape ID array is null.");BRepBuilderAPI_Sewing sewing(tolerance);for(int i=0;i<count;++i){ObjectEntry* item=e->findShape(ids[i]);if(!item)throw std::invalid_argument("Sewing contains an invalid shape ID.");sewing.Add(item->shape);}sewing.Perform();const TopoDS_Shape resultShape=sewing.SewedShape();if(resultShape.IsNull())throw std::runtime_error("Sewing failed.");const auto result=e->addShape(resultShape,false,"SewnShape");if(hide)hideInputs(e,ids,count);return result;});
    }

    OcctObjectId occt_make_solid_from_shell(OcctHandle h, OcctObjectId shellId, int hideInput)
    {
        Engine* e=engineOf(h);if(!validateInitialized(e))return 0;
        return executeObject(e,[&]{ObjectEntry* shell=e->findShape(shellId);if(!shell||shell->shape.ShapeType()!=TopAbs_SHELL)throw std::invalid_argument("Input must be a shell.");BRepBuilderAPI_MakeSolid maker(TopoDS::Shell(shell->shape));if(!maker.IsDone())throw std::runtime_error("Solid creation failed.");const auto result=e->addShape(maker.Solid(),false,"Solid");if(hideInput)e->hide(shellId);return result;});
    }
}
