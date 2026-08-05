using System.Drawing;
using System.Reflection;
using OcctNet;

namespace CadCommon;

public enum ApiDemoExecutionMode
{
    Automated,
    Interactive,
    FileRequired,
    EnvironmentRequired,
    CatalogOnly
}

public sealed record ApiDemoMember(
    string Area,
    string TypeName,
    string Kind,
    string Name,
    string Signature,
    ApiDemoExecutionMode ExecutionMode,
    string Requirement);

public sealed record ApiDemoScenario(
    string Id,
    string Category,
    string Title,
    string Description,
    bool RequiresUiThread,
    Func<CadSession, IProgress<string>, CancellationToken, ApiDemoResult> Execute);

public sealed record ApiDemoResult(
    string ScenarioId,
    bool Succeeded,
    string Summary,
    TimeSpan Duration,
    IReadOnlyList<string> Details);

public static class ApiDemoCatalog
{
    private static readonly HashSet<string> InteractiveMembers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Initialize", "Resize", "Redraw", "WindowFit", "ScreenToWorld", "WorldToScreen", "MoveTo",
        "Select", "SelectRectangle", "SelectObject", "ClearSelection", "SetSelectionMode"
    };

    public static IReadOnlyList<ApiDemoMember> Members { get; } = BuildMembers();

    public static IReadOnlyList<ApiDemoScenario> Scenarios { get; } = new ApiDemoScenario[]
    {
        new("catalog", "基础", "公共 API 目录校验", "反射读取 OcctNet 程序集中的全部公共类型、构造函数、属性、字段、事件和方法。", false, RunCatalogAudit),
        new("viewer", "Viewer", "Viewer、相机与显示", "演示背景、视图、投影、显示精度、材质、光照、选择容差和基础实体显示。", true, RunViewerScenario),
        new("cad-samples", "Viewer", "WinForms/WPF 共享 CAD 场景", "执行现有的基础实体、布尔、放样和注释示例，验证 CadCommon 与 Viewer 封装协作。", true, RunCadSampleScenario),
        new("headless", "Headless", "无窗口建模、拓扑、分析与网格", "演示实体构造、布尔运算、拓扑查询、网格、射线求交、放样、修复与同域统一。", false, RunHeadlessScenario),
        new("interop", "Headless", "Headless Shape 送入 Viewer", "通过稳定的 Shape 复制边界，将无窗口建模结果显示到当前 Viewer。", true, RunInteropScenario),
        new("exchange", "数据交换", "BREP 文件往返", "在临时目录执行 BREP 导出与导入，验证纯 Shape 文件交换接口。", false, RunExchangeScenario),
        new("ocaf", "OCAF/XDE", "OCAF 属性、事务与 XDE 持久化", "演示 Label、数组、变量、表达式、关系式、颜色、图层、材料、验证属性、Undo/Redo 和 BinXCAF 重开。", false, RunOcafScenario),
        new("tnaming", "OCAF/XDE", "TNaming 与持久选择", "演示 GeneratedFrom 历史、NamedShape 版本、历史对和 Selector 持久选择。", false, RunNamingScenario),
        new("assembly", "OCAF/XDE", "XDE 装配、组件与元数据", "演示顶层装配、组件引用、用户反查、颜色定义、图层定义和材料定义复用。", false, RunAssemblyScenario)
    };

    public static string CoverageSummary
    {
        get
        {
            var typeCount = Members.Select(item => item.TypeName).Distinct(StringComparer.Ordinal).Count();
            var automated = Members.Count(item => item.ExecutionMode == ApiDemoExecutionMode.Automated);
            var interactive = Members.Count(item => item.ExecutionMode == ApiDemoExecutionMode.Interactive);
            var file = Members.Count(item => item.ExecutionMode == ApiDemoExecutionMode.FileRequired);
            return $"{typeCount} public types, {Members.Count} public members; automated {automated}, interactive {interactive}, file-dependent {file}.";
        }
    }

    public static ApiDemoScenario GetScenario(string id) =>
        Scenarios.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Unknown API demo scenario: {id}");

    private static IReadOnlyList<ApiDemoMember> BuildMembers()
    {
        var assembly = typeof(OcctEngine).Assembly;
        var result = new List<ApiDemoMember>();
        foreach (var type in assembly.GetExportedTypes()
                     .Where(type => string.Equals(type.Namespace, "OcctNet", StringComparison.Ordinal))
                     .OrderBy(type => type.Name, StringComparer.Ordinal))
        {
            var area = GetArea(type);
            result.Add(new ApiDemoMember(area, type.Name, "Type", type.Name, DescribeType(type),
                type.IsEnum || type.IsValueType ? ApiDemoExecutionMode.CatalogOnly : ApiDemoExecutionMode.EnvironmentRequired,
                type.IsEnum || type.IsValueType ? "类型、枚举或数据结构" : "由对应场景创建实例"));

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var constructor in type.GetConstructors(flags).OrderBy(FormatMember, StringComparer.Ordinal))
                result.Add(Member(area, type, "Constructor", constructor.Name, FormatMember(constructor)));
            foreach (var property in type.GetProperties(flags).OrderBy(property => property.Name, StringComparer.Ordinal))
                result.Add(Member(area, type, "Property", property.Name, $"{FriendlyName(property.PropertyType)} {property.Name}"));
            foreach (var field in type.GetFields(flags).OrderBy(field => field.Name, StringComparer.Ordinal))
                result.Add(Member(area, type, "Field", field.Name, $"{FriendlyName(field.FieldType)} {field.Name}"));
            foreach (var eventInfo in type.GetEvents(flags).OrderBy(eventInfo => eventInfo.Name, StringComparer.Ordinal))
                result.Add(Member(area, type, "Event", eventInfo.Name, $"event {FriendlyName(eventInfo.EventHandlerType ?? typeof(Delegate))} {eventInfo.Name}"));
            foreach (var method in type.GetMethods(flags)
                         .Where(method => !method.IsSpecialName)
                         .OrderBy(FormatMember, StringComparer.Ordinal))
                result.Add(Member(area, type, "Method", method.Name, FormatMember(method)));
        }
        return result;
    }

    private static ApiDemoMember Member(string area, Type type, string kind, string name, string signature)
    {
        var (mode, requirement) = Classify(type, name, kind);
        return new ApiDemoMember(area, type.Name, kind, name, signature, mode, requirement);
    }

    private static (ApiDemoExecutionMode Mode, string Requirement) Classify(Type type, string name, string kind)
    {
        if (kind == "Constructor" || name == nameof(IDisposable.Dispose) || type == typeof(OcctRuntime))
            return (ApiDemoExecutionMode.EnvironmentRequired, "需要正确的 OCCT 7.9.0 运行环境或对象生命周期");

        if (InteractiveMembers.Contains(name) || type == typeof(OcctViewportControl))
            return (ApiDemoExecutionMode.Interactive, "需要已初始化的窗口、视口或鼠标/选择状态");

        if (name.Contains("Import", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Export", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Open", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Save", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Dump", StringComparison.OrdinalIgnoreCase))
            return (ApiDemoExecutionMode.FileRequired, "需要输入文件、输出路径或相应 OCCT 资源");

        if (kind is "Field" or "Type")
            return (ApiDemoExecutionMode.CatalogOnly, "数据结构或枚举，由其他接口使用");

        return (ApiDemoExecutionMode.Automated, "可由 API 场景、现有 CAD 命令或专项参数测试调用");
    }

    private static string GetArea(Type type)
    {
        if (type.Name.StartsWith("Ocaf", StringComparison.Ordinal)) return "OCAF/XDE";
        if (type.Name.StartsWith("OcctModel", StringComparison.Ordinal)) return "Headless";
        if (type == typeof(OcctModelingSession)) return "Headless";
        if (type == typeof(OcctEngine) || type == typeof(OcctViewportControl)) return "Viewer";
        if (type == typeof(OcctRuntime)) return "Runtime";
        return "Common";
    }

    private static string DescribeType(Type type)
    {
        if (type.IsEnum) return $"enum {type.Name}";
        if (type.IsValueType) return $"struct {type.Name}";
        if (type.IsInterface) return $"interface {type.Name}";
        return $"class {type.Name}";
    }

    private static string FormatMember(MethodBase method)
    {
        var parameters = string.Join(", ", method.GetParameters().Select(parameter =>
            $"{FriendlyName(parameter.ParameterType)} {parameter.Name}"));
        if (method is MethodInfo methodInfo)
            return $"{FriendlyName(methodInfo.ReturnType)} {methodInfo.Name}({parameters})";
        return $"{method.DeclaringType?.Name}({parameters})";
    }

    private static string FriendlyName(Type type)
    {
        if (type.IsByRef) return $"ref {FriendlyName(type.GetElementType()!)}";
        if (type.IsArray) return $"{FriendlyName(type.GetElementType()!)}[]";
        if (!type.IsGenericType) return type.Name;
        var name = type.Name[..type.Name.IndexOf('`')];
        return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(FriendlyName))}>";
    }

    private static ApiDemoResult RunCatalogAudit(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        token.ThrowIfCancellationRequested();
        progress.Report(CoverageSummary);
        var groups = Members.GroupBy(item => item.Area).OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => $"{group.Key}: {group.Count()} members").ToArray();
        foreach (var group in groups) progress.Report(group);
        return Success("catalog", started, CoverageSummary, groups);
    }

    private static ApiDemoResult RunViewerScenario(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        token.ThrowIfCancellationRequested();
        var engine = session.Engine;
        engine.SetGradientBackground(Color.White, Color.FromArgb(202, 221, 238));
        engine.SetTriedronVisible(true);
        engine.SetViewCubeVisible(true);
        engine.SetAntialiasing(true);
        engine.SetSelectionTolerance(4);
        engine.SetDefaultMaterial(OcctMaterial.Plastified, false);
        engine.SetDisplayPrecision(0.001, 20.0, true);
        engine.SetSceneLighting(0.35, 0.85, new OcctVector3d(-1, -1, -1), true);
        engine.SetProjection(OcctProjectionType.Orthographic);
        engine.SetView(OcctViewOrientation.Isometric);
        var box = engine.MakeBox(90, 65, 45, -45, -32.5, 0);
        engine.Fit(box);
        progress.Report($"Viewer initialized; displayed {box}.");
        return Success("viewer", started, "Viewer display, camera, precision, material and lighting APIs completed.",
            new[] { $"Object count: {engine.ObjectCount}", $"Shape count: {engine.ShapeCount}" });
    }

    private static ApiDemoResult RunCadSampleScenario(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        var commands = new[]
        {
            CadCommandId.DemoPrimitives,
            CadCommandId.DemoBoolean,
            CadCommandId.DemoLoft,
            CadCommandId.DemoAnnotations
        };
        foreach (var command in commands)
        {
            token.ThrowIfCancellationRequested();
            progress.Report($"Running {CadLocalization.CommandText(command)}...");
            session.Execute(command);
        }
        session.Engine.FitAll();
        return Success("cad-samples", started, "Shared CAD scenarios completed.",
            commands.Select(CadLocalization.CommandText).ToArray());
    }

    private static ApiDemoResult RunHeadlessScenario(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        using var model = new OcctModelingSession();
        progress.Report(OcctModelingSession.Capabilities);
        var box = model.MakeBox(100, 80, 60);
        var cylinder = model.MakeCylinder(new OcctPoint3d(50, 40, -10), OcctVector3d.UnitZ, 12, 80);
        var cut = model.Cut(box, cylinder);
        if (!cut.Succeeded || !model.IsValid(cut.Shape)) throw new InvalidOperationException("Boolean result is invalid.");
        token.ThrowIfCancellationRequested();

        var faceCount = model.GetTopologyCount(cut.Shape, OcctShapeType.Face);
        var edgeCount = model.GetTopologyCount(cut.Shape, OcctShapeType.Edge);
        model.Mesh(cut.Shape, OcctModelMeshParameters.Default);
        var face = model.GetSubshape(cut.Shape, OcctShapeType.Face, 0);
        var mesh = model.GetFaceMesh(face);
        var hits = model.IntersectRay(cut.Shape, new OcctPoint3d(50, 40, 100), new OcctVector3d(0, 0, -1));

        var lower = model.MakeCircle(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 10);
        var upper = model.MakeCircle(new OcctPoint3d(0, 0, 25), OcctVector3d.UnitZ, 16);
        var loft = model.Loft(new[] { model.MakeWire(new[] { lower }), model.MakeWire(new[] { upper }) });
        var healed = model.FixShape(cut.Shape);
        var unified = model.UnifySameDomain(healed.Shape);
        if (!model.IsValid(loft.Shape) || !model.IsValid(unified.Shape))
            throw new InvalidOperationException("Feature or healing result is invalid.");

        var details = new[]
        {
            $"Faces: {faceCount}; edges: {edgeCount}",
            $"Mesh: {mesh.Nodes.Count} nodes, {mesh.Triangles.Count} triangles",
            $"Ray hits: {hits.Count}",
            $"Shape hash: {model.GetShapeHash(cut.Shape)}",
            $"Maximum tolerance: {model.GetMaximumTolerance(cut.Shape):G6}",
            $"Operation report: {cut.Report}"
        };
        foreach (var detail in details) progress.Report(detail);
        return Success("headless", started, "Headless modeling, topology, analysis, mesh and healing APIs completed.", details);
    }

    private static ApiDemoResult RunInteropScenario(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        using var model = new OcctModelingSession();
        var body = model.MakeBox(120, 80, 45);
        var tool = model.MakeCylinder(new OcctPoint3d(60, 40, -10), OcctVector3d.UnitZ, 15, 70);
        var cut = model.Cut(body, tool);
        token.ThrowIfCancellationRequested();
        var displayed = session.Engine.Display(model, cut.Shape, fit: true);
        progress.Report($"Copied {cut.Shape} to viewer object {displayed}.");
        return Success("interop", started, "Headless-to-viewer shape copy completed.", new[] { displayed.ToString() });
    }

    private static ApiDemoResult RunExchangeScenario(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        using var model = new OcctModelingSession();
        var shape = model.MakeBox(50, 40, 30);
        var path = Path.Combine(Path.GetTempPath(), $"occt-demo-{Guid.NewGuid():N}.brep");
        try
        {
            model.ExportBrep(shape, path);
            token.ThrowIfCancellationRequested();
            var imported = model.ImportBrep(path);
            if (!model.IsValid(imported)) throw new InvalidOperationException("BREP round trip produced an invalid shape.");
            var detail = $"BREP round trip: {new FileInfo(path).Length:N0} bytes; imported {imported}.";
            progress.Report(detail);
            return Success("exchange", started, "BREP export/import completed.", new[] { detail });
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static ApiDemoResult RunOcafScenario(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        using var model = new OcctModelingSession();
        var body = model.MakeBox(100, 80, 60);
        var face = model.GetSubshape(body, OcctShapeType.Face, 0);
        var path = Path.Combine(Path.GetTempPath(), $"occt-demo-{Guid.NewGuid():N}.xbf");
        try
        {
            using (var document = new OcafDocument())
            {
                document.UndoLimit = 10;
                OcafLabel shapeLabel;
                using (var command = document.BeginCommand())
                {
                    var data = document.NewChild(document.Main);
                    document.SetName(data, "API Demo");
                    document.SetComment(data, "OCAF/XDE 综合演示");
                    document.SetInteger(data, 42);
                    document.SetRealArray(data, new[] { 1.25, 2.5, 5.0 }, -1);

                    var width = document.NewChild(data);
                    var height = document.NewChild(data);
                    var expression = document.NewChild(data);
                    var relation = document.NewChild(data);
                    document.SetVariable(width, "Width", 100, "mm", true);
                    document.SetVariable(height, "Height", 80, "mm");
                    document.SetExpression(expression, "Width + Height", new[] { width, height });
                    document.SetRelation(relation, "Height <= Width", new[] { height, width });
                    document.AssignVariableExpression(height, "Width * 0.8", new[] { width });

                    shapeLabel = document.AddShape(model, body, makeAssembly: false);
                    document.SetName(shapeLabel, "Demo body");
                    var subshape = document.AddSubshape(shapeLabel, model, face);
                    document.SetName(subshape, "First face");
                    var color = document.AddColorDefinition(new OcafColor(0.2, 0.45, 0.8));
                    document.SetColor(shapeLabel, OcafColorType.Surface, color);
                    var layer = document.AddLayer("Demo Layer");
                    document.SetLayer(shapeLabel, layer, true);
                    var material = document.AddMaterialDefinition("Steel", "Demo material", 7.85, "density", "g/cm3");
                    document.SetMaterial(shapeLabel, material);
                    document.SetArea(shapeLabel, model.GetSurfaceProperties(body).Mass);
                    document.SetVolume(shapeLabel, model.GetVolumeProperties(body).Mass);
                    document.SetCentroid(shapeLabel, model.GetVolumeProperties(body).CenterOfMass);
                    command.Commit();
                }

                document.MarkModified(shapeLabel);
                var modified = document.GetModifiedLabels().Count;
                document.PurgeModified();
                if (document.AvailableUndos <= 0 || !document.Undo() || !document.Redo())
                    throw new InvalidOperationException("OCAF Undo/Redo failed.");
                document.SaveAs(path);
                progress.Report($"Saved BinXCAF document; modified-label snapshot contained {modified} label(s).");
            }

            token.ThrowIfCancellationRequested();
            using var reopened = OcafDocument.Open(path);
            var shapes = reopened.GetShapes(true);
            if (shapes.Count != 1 || reopened.GetSubshapes(shapes[0]).Count != 1)
                throw new InvalidOperationException("BinXCAF round trip lost shape metadata.");
            var details = new[]
            {
                $"Native version: {OcafDocument.NativeVersion}",
                $"Capabilities: {OcafDocument.Capabilities}",
                $"Storage version: {reopened.StorageFormatVersion}",
                $"Shapes after reopen: {shapes.Count}",
                $"Name: {reopened.GetName(shapes[0])}"
            };
            foreach (var detail in details) progress.Report(detail);
            return Success("ocaf", started, "OCAF attributes, transactions, XDE metadata and persistence completed.", details);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static ApiDemoResult RunNamingScenario(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        using var model = new OcctModelingSession();
        using var document = new OcafDocument();
        var source = model.MakeBox(80, 60, 40);
        var generated = model.MakeBox(90, 70, 50);
        var face = model.GetSubshape(source, OcctShapeType.Face, 0);
        var historyLabel = document.NewChild(document.Main);
        var selectorLabel = document.NewChild(document.Main);
        using (var command = document.BeginCommand())
        {
            document.NamingGeneratedFrom(historyLabel, model, source, generated);
            document.SetNamedShapeVersion(historyLabel, 2);
            if (!document.SelectPersistentShape(selectorLabel, model, face, source, geometryMode: true))
                throw new InvalidOperationException("TNaming selector could not create a persistent selection.");
            command.Commit();
        }
        token.ThrowIfCancellationRequested();
        var history = document.GetNamedShapeHistory(historyLabel, model);
        var solved = document.SolvePersistentSelection(selectorLabel);
        var details = new[]
        {
            $"Evolution: {document.GetNamedShapeEvolution(historyLabel)}",
            $"Version: {document.GetNamedShapeVersion(historyLabel)}",
            $"History pairs: {history.Count}",
            $"Selector solved: {solved}",
            $"Face identified: {document.IsShapeIdentified(selectorLabel, model, face)}"
        };
        foreach (var detail in details) progress.Report(detail);
        return Success("tnaming", started, "TNaming history and persistent-selection APIs completed.", details);
    }

    private static ApiDemoResult RunAssemblyScenario(CadSession session, IProgress<string> progress, CancellationToken token)
    {
        var started = DateTime.UtcNow;
        using var model = new OcctModelingSession();
        using var document = new OcafDocument();
        var firstShape = model.MakeBox(50, 40, 30);
        var secondShape = model.MakeCylinder(new OcctPoint3d(0, 0, 0), OcctVector3d.UnitZ, 12, 50);
        OcafLabel assembly;
        OcafLabel firstPart;
        OcafLabel firstComponent;
        using (var command = document.BeginCommand())
        {
            assembly = document.NewShapeLabel();
            document.SetName(assembly, "Demo assembly");
            firstPart = document.AddShape(model, firstShape, false);
            var secondPart = document.AddShape(model, secondShape, false);
            document.SetName(firstPart, "Housing");
            document.SetName(secondPart, "Pin");
            firstComponent = document.AddComponent(assembly, firstPart, OcctModelLocation.Identity);
            document.AddComponent(assembly, secondPart, OcctModelLocation.Identity);
            var color = document.AddColorDefinition(new OcafColor(0.75, 0.32, 0.12));
            document.SetColor(firstPart, OcafColorType.Surface, color);
            var layer = document.AddLayer("Assembly Parts");
            document.SetLayer(firstPart, layer);
            document.SetLayer(secondPart, layer);
            var material = document.AddMaterialDefinition("Steel", "Reusable assembly material", 7.85, "density", "g/cm3");
            document.SetMaterial(firstPart, material);
            document.SetMaterial(secondPart, material);
            document.UpdateAssemblies();
            command.Commit();
        }
        token.ThrowIfCancellationRequested();
        var components = document.GetComponents(assembly, true);
        var users = document.GetUsers(firstPart, true);
        var details = new[]
        {
            $"Assembly: {document.IsAssembly(assembly)}",
            $"Components: {components.Count}",
            $"First component refers to: {document.GetReferredShape(firstComponent)}",
            $"Users of first part: {users.Count}",
            $"Shapes on layer: {document.GetShapesOnLayer(document.FindLayer("Assembly Parts")!.Value).Count}"
        };
        foreach (var detail in details) progress.Report(detail);
        return Success("assembly", started, "XDE assembly, component and reusable metadata APIs completed.", details);
    }

    private static ApiDemoResult Success(string id, DateTime started, string summary, IReadOnlyList<string> details) =>
        new(id, true, summary, DateTime.UtcNow - started, details);
}
