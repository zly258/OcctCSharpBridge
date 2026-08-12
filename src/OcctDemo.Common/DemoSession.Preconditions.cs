using OcctNet;

namespace OcctDemo.Common;

public readonly record struct CadCommandAvailability(bool CanExecute, string Message)
{
    public static CadCommandAvailability Available => new(true, string.Empty);

    public static CadCommandAvailability Unavailable(string message) =>
        new(false, message ?? string.Empty);
}

public sealed partial class DemoSession
{
    private static readonly HashSet<DemoCommandId> SingleShapeCommands =
    [
        DemoCommandId.Fillet,
        DemoCommandId.Chamfer,
        DemoCommandId.Offset,
        DemoCommandId.Translate,
        DemoCommandId.Rotate,
        DemoCommandId.Scale,
        DemoCommandId.Mirror,
        DemoCommandId.Copy,
        DemoCommandId.AnalyzeBounds,
        DemoCommandId.AnalyzeMass,
        DemoCommandId.AnalyzeTopology,
        DemoCommandId.ValidateShape
    ];

    private static readonly HashSet<DemoCommandId> TwoShapeCommands =
    [
        DemoCommandId.Fuse,
        DemoCommandId.Cut,
        DemoCommandId.Common,
        DemoCommandId.Section,
        DemoCommandId.AnalyzeDistance
    ];

    public CadCommandAvailability GetCommandAvailability(DemoCommandId commandId)
    {
        var selectedEntries = Engine.SelectedObjects
            .Where(value => Engine.Exists(value))
            .ToArray();
        var selectedShapes = selectedEntries
            .Where(value => value.Kind == OcctObjectKind.Shape)
            .DistinctBy(value => value.Id)
            .Select(value => Engine.GetShape(value.Id))
            .ToArray();

        if (commandId == DemoCommandId.Delete)
            return RequireObjectCount(commandId, selectedEntries.Length, 1, exactly: false);

        if (commandId is DemoCommandId.LengthDimension
            or DemoCommandId.AngleDimension
            or DemoCommandId.RadiusDimension
            or DemoCommandId.DiameterDimension)
        {
            var selectedHits = Engine.GetSelectedHits()
                .Where(hit => Engine.Exists(hit.Owner))
                .DistinctBy(hit => (hit.Owner.Id, hit.SubshapeType, hit.SubshapeIndex))
                .ToArray();
            var required = commandId == DemoCommandId.AngleDimension ? 2 : 1;
            return RequireSubshapeHits(commandId, selectedHits, required, OcctShapeType.Edge);
        }

        if (commandId is DemoCommandId.Extrude or DemoCommandId.Revolve)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 1, exactly: true);
            if (!countCheck.CanExecute) return countCheck;
            var type = Engine.GetShapeType(selectedShapes[0]);
            return IsProfileType(type)
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    $"Select one edge, wire, or face before running {DemoLocalization.CommandText(commandId)}.",
                    $"请先选择一条边、一个线框或一个平面，再执行“{DemoLocalization.CommandText(commandId)}”。"));
        }

        if (commandId == DemoCommandId.Sweep)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 2, exactly: true);
            if (!countCheck.CanExecute) return countCheck;
            var pathType = Engine.GetShapeType(selectedShapes[0]);
            var profileType = Engine.GetShapeType(selectedShapes[1]);
            if (pathType is not (OcctShapeType.Edge or OcctShapeType.Wire))
            {
                return CadCommandAvailability.Unavailable(Local(
                    "Select an edge or wire as the first sweep object (the path).",
                    "扫掠的第一个选择对象必须是边或线框，并作为路径。"));
            }
            return IsProfileType(profileType)
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    "Select an edge, wire, or face as the second sweep object (the profile).",
                    "扫掠的第二个选择对象必须是边、线框或平面，并作为截面。"));
        }

        if (commandId == DemoCommandId.Loft)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 2, exactly: false);
            if (!countCheck.CanExecute) return countCheck;
            return selectedShapes.All(shape => IsProfileType(Engine.GetShapeType(shape)))
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    "All loft sections must be edges, wires, or faces.",
                    "放样所选的全部截面必须是边、线框或平面。"));
        }

        if (commandId is DemoCommandId.Shell or DemoCommandId.Drill)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 1, exactly: true);
            if (!countCheck.CanExecute) return countCheck;
            var type = Engine.GetShapeType(selectedShapes[0]);
            return type is OcctShapeType.Solid or OcctShapeType.CompSolid
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    $"Select one solid before running {DemoLocalization.CommandText(commandId)}.",
                    $"请先选择一个实体，再执行“{DemoLocalization.CommandText(commandId)}”。"));
        }

        if (commandId is DemoCommandId.Fillet or DemoCommandId.Chamfer)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 1, exactly: true);
            if (!countCheck.CanExecute) return countCheck;
            return Engine.GetTopologyCount(selectedShapes[0], OcctShapeType.Edge) > 0
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    $"The selected shape has no edges for {DemoLocalization.CommandText(commandId)}.",
                    $"当前形体没有可用于“{DemoLocalization.CommandText(commandId)}”的边。"));
        }

        if (SingleShapeCommands.Contains(commandId))
            return RequireShapeCount(commandId, selectedShapes.Length, 1, exactly: true);

        if (TwoShapeCommands.Contains(commandId))
            return RequireShapeCount(commandId, selectedShapes.Length, 2, exactly: true);

        return CadCommandAvailability.Available;
    }

    private void EnsureCommandAvailable(DemoCommandId commandId)
    {
        var availability = GetCommandAvailability(commandId);
        if (!availability.CanExecute)
            throw new InvalidOperationException(availability.Message);
    }

    private static CadCommandAvailability RequireObjectCount(
        DemoCommandId commandId,
        int actual,
        int required,
        bool exactly)
    {
        if ((exactly && actual == required) || (!exactly && actual >= required))
            return CadCommandAvailability.Available;

        var command = DemoLocalization.CommandText(commandId);
        return CadCommandAvailability.Unavailable(actual == 0
            ? Local(
                $"Select at least {required} object{(required == 1 ? string.Empty : "s")} before running {command}.",
                $"请先选择至少 {required} 个对象，再执行“{command}”。")
            : Local(
                $"{command} requires exactly {required} selected object{(required == 1 ? string.Empty : "s")}; {actual} are selected.",
                $"“{command}”需要恰好选择 {required} 个对象，当前已选择 {actual} 个。"));
    }

    private static CadCommandAvailability RequireShapeCount(
        DemoCommandId commandId,
        int actual,
        int required,
        bool exactly)
    {
        if ((exactly && actual == required) || (!exactly && actual >= required))
            return CadCommandAvailability.Available;

        var command = DemoLocalization.CommandText(commandId);
        if (actual == 0)
        {
            return CadCommandAvailability.Unavailable(Local(
                $"Select {(exactly ? "exactly" : "at least")} {required} shape{(required == 1 ? string.Empty : "s")} before running {command}.",
                $"请先{(exactly ? "恰好" : "至少")}选择 {required} 个形体，再执行“{command}”。"));
        }

        return CadCommandAvailability.Unavailable(Local(
            $"{command} requires {(exactly ? "exactly" : "at least")} {required} selected shape{(required == 1 ? string.Empty : "s")}; {actual} are selected.",
            $"“{command}”需要{(exactly ? "恰好" : "至少")}选择 {required} 个形体，当前已选择 {actual} 个。"));
    }

    private static CadCommandAvailability RequireSubshapeHits(
        DemoCommandId commandId,
        IReadOnlyCollection<OcctSelectionHit> selectedHits,
        int required,
        OcctShapeType requiredType)
    {
        var command = DemoLocalization.CommandText(commandId);
        var matchingHits = selectedHits
            .Where(hit => hit.IsSubshape && hit.SubshapeType == requiredType)
            .ToArray();

        if (selectedHits.Count == required && matchingHits.Length == required)
            return CadCommandAvailability.Available;

        var typeName = requiredType switch
        {
            OcctShapeType.Edge => Local("edge", "边"),
            OcctShapeType.Face => Local("face", "面"),
            OcctShapeType.Vertex => Local("vertex", "顶点"),
            _ => Local("subshape", "子形")
        };

        return CadCommandAvailability.Unavailable(Local(
            $"Switch to {typeName} selection and select exactly {required} {typeName}{(required == 1 ? string.Empty : "s")} before running {command}.",
            $"请切换到{typeName}选择模式，并恰好选择 {required} 个{typeName}，再执行“{command}”。"));
    }

    private static bool IsProfileType(OcctShapeType type) =>
        type is OcctShapeType.Edge or OcctShapeType.Wire or OcctShapeType.Face;
}
