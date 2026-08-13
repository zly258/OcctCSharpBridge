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

    public IReadOnlyList<KeyValuePair<string, string>> DescribeObjectLightweight(IOcctObject value)
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

        if (value.Kind == OcctObjectKind.Shape)
        {
            rows.Add(new(
                Local("Geometry Details", "几何详情"),
                Local("Use Analysis commands on demand", "请按需使用“分析”命令")));
        }

        return rows;
    }
}
