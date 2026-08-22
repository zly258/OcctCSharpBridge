using System.Globalization;
using OcctNet;

namespace OcctDemo.Common;

public sealed partial class DemoSession
{
    private const string StepPathPrefix = "step-path:";
    private const char StepPathSeparator = '\u001F';

    public IReadOnlyList<string> GetHierarchyPath(IOcctObject value)
    {
        if (value.Kind != OcctObjectKind.Shape) return Array.Empty<string>();
        var tag = Engine.GetApplicationTag(value);
        if (!tag.StartsWith(StepPathPrefix, StringComparison.Ordinal)) return Array.Empty<string>();
        return tag[StepPathPrefix.Length..]
            .Split(StepPathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public IReadOnlyList<KeyValuePair<string, string>> DescribeObjectLightweight(IOcctObject value) =>
        DescribeObject(value, includeGeometryDetails: false);

    public IReadOnlyList<KeyValuePair<string, string>> DescribeObject(IOcctObject value, bool includeGeometryDetails = false)
    {
        var rows = new List<KeyValuePair<string, string>>
        {
            new(DemoLocalization.Text("Object.Id"), value.Id.ToString(CultureInfo.InvariantCulture)),
            new(DemoLocalization.Text("Object.Name"), SafeName(value)),
            new(DemoLocalization.Text("Object.Kind"), DemoLocalization.ObjectKind(value.Kind))
        };

        var hierarchy = GetHierarchyPath(value);
        if (hierarchy.Count > 1)
        {
            rows.Add(new(
                Local("Assembly Path", "装配路径"),
                string.Join(" / ", hierarchy.Take(hierarchy.Count - 1))));
        }

        if (value is OcctShape shape && Engine.ContainsObject(shape.Id))
        {
            var shapeType = Engine.GetShapeType(shape);
            rows.Add(new(DemoLocalization.Text("Object.Topology"), DemoLocalization.ShapeType(shapeType)));

            if (!includeGeometryDetails)
            {
                rows.Add(new(
                    DemoLocalization.Text("Object.GeometryDetails"),
                    DemoLocalization.Text("Object.ClickToLoadDetails")));
            }
            else
            {
                try
                {
                    var valid = Engine.IsShapeValid(shape);
                    rows.Add(new(DemoLocalization.Text("Object.Validity"), valid ? DemoLocalization.Text("Object.Valid") : DemoLocalization.Text("Object.Invalid")));

                    var bounds = Engine.GetShapeBounds(shape);
                    rows.Add(new(DemoLocalization.Text("Object.SizeX"), $"{bounds.SizeX:F3} mm"));
                    rows.Add(new(DemoLocalization.Text("Object.SizeY"), $"{bounds.SizeY:F3} mm"));
                    rows.Add(new(DemoLocalization.Text("Object.SizeZ"), $"{bounds.SizeZ:F3} mm"));
                    rows.Add(new(DemoLocalization.Text("Object.Center"), $"({bounds.Center.X:F2}, {bounds.Center.Y:F2}, {bounds.Center.Z:F2})"));

                    var vCount = Engine.GetTopologyCount(shape, OcctShapeType.Vertex);
                    var eCount = Engine.GetTopologyCount(shape, OcctShapeType.Edge);
                    var fCount = Engine.GetTopologyCount(shape, OcctShapeType.Face);
                    var sCount = Engine.GetTopologyCount(shape, OcctShapeType.Solid);

                    rows.Add(new(DemoLocalization.Text("Object.Vertices"), vCount.ToString(CultureInfo.InvariantCulture)));
                    rows.Add(new(DemoLocalization.Text("Object.Edges"), eCount.ToString(CultureInfo.InvariantCulture)));
                    rows.Add(new(DemoLocalization.Text("Object.Faces"), fCount.ToString(CultureInfo.InvariantCulture)));
                    if (sCount > 0)
                    {
                        rows.Add(new(Local("Solids", "实体数"), sCount.ToString(CultureInfo.InvariantCulture)));
                        var vol = Engine.GetShapeVolumeProperties(shape);
                        rows.Add(new(Local("Volume", "体积"), $"{vol.Mass:F3} mm³"));
                    }
                    else if (fCount > 0)
                    {
                        var area = Engine.GetShapeSurfaceProperties(shape);
                        rows.Add(new(Local("Surface Area", "表面积"), $"{area.Mass:F3} mm²"));
                    }
                    else if (eCount > 0)
                    {
                        var len = Engine.GetShapeLinearProperties(shape);
                        rows.Add(new(Local("Length", "长度"), $"{len.Mass:F3} mm"));
                    }
                }
                catch (Exception ex)
                {
                    rows.Add(new(DemoLocalization.Text("Object.GeometryDetails"), $"{Local("Compute error", "计算失败")}: {ex.Message}"));
                }
            }
        }

        return rows;
    }
}
