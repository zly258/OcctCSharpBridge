using OcctNet;

namespace CadCommon;

public readonly record struct CadCommandAvailability(bool CanExecute, string Message)
{
    public static CadCommandAvailability Available => new(true, string.Empty);

    public static CadCommandAvailability Unavailable(string message) =>
        new(false, message ?? string.Empty);
}

public sealed partial class CadSession
{
    private static readonly HashSet<CadCommandId> SingleShapeCommands =
    [
        CadCommandId.Fillet,
        CadCommandId.Chamfer,
        CadCommandId.Offset,
        CadCommandId.Translate,
        CadCommandId.Rotate,
        CadCommandId.Scale,
        CadCommandId.Mirror,
        CadCommandId.Copy,
        CadCommandId.AnalyzeBounds,
        CadCommandId.AnalyzeMass,
        CadCommandId.AnalyzeTopology,
        CadCommandId.ValidateShape
    ];

    private static readonly HashSet<CadCommandId> TwoShapeCommands =
    [
        CadCommandId.Fuse,
        CadCommandId.Cut,
        CadCommandId.Common,
        CadCommandId.Section,
        CadCommandId.AnalyzeDistance
    ];

    public CadCommandAvailability GetCommandAvailability(CadCommandId commandId)
    {
        var selectedEntries = Engine.SelectedObjects
            .Where(value => Engine.Exists(value))
            .ToArray();
        var selectedShapes = selectedEntries
            .Where(value => value.Kind == OcctObjectKind.Shape)
            .DistinctBy(value => value.Id)
            .Select(value => new OcctShape(value.Id))
            .ToArray();

        if (commandId == CadCommandId.Delete)
            return RequireObjectCount(commandId, selectedEntries.Length, 1, exactly: false);

        if (commandId is CadCommandId.LengthDimension
            or CadCommandId.RadiusDimension
            or CadCommandId.DiameterDimension)
        {
            return RequireSubshapeCount(commandId, selectedEntries, 1);
        }

        if (commandId == CadCommandId.AngleDimension)
            return RequireSubshapeCount(commandId, selectedEntries, 2);

        if (commandId is CadCommandId.Extrude or CadCommandId.Revolve)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 1, exactly: true);
            if (!countCheck.CanExecute) return countCheck;
            var type = Engine.GetShapeType(selectedShapes[0]);
            return IsProfileType(type)
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    $"Select one edge, wire, or face before running {CadLocalization.CommandText(commandId)}.",
                    $"请先选择一条边、一个线框或一个平面，再执行“{CadLocalization.CommandText(commandId)}”。"));
        }

        if (commandId == CadCommandId.Sweep)
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

        if (commandId == CadCommandId.Loft)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 2, exactly: false);
            if (!countCheck.CanExecute) return countCheck;
            return selectedShapes.All(shape => IsProfileType(Engine.GetShapeType(shape)))
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    "All loft sections must be edges, wires, or faces.",
                    "放样所选的全部截面必须是边、线框或平面。"));
        }

        if (commandId is CadCommandId.Shell or CadCommandId.Drill)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 1, exactly: true);
            if (!countCheck.CanExecute) return countCheck;
            var type = Engine.GetShapeType(selectedShapes[0]);
            return type is OcctShapeType.Solid or OcctShapeType.CompSolid
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    $"Select one solid before running {CadLocalization.CommandText(commandId)}.",
                    $"请先选择一个实体，再执行“{CadLocalization.CommandText(commandId)}”。"));
        }

        if (commandId is CadCommandId.Fillet or CadCommandId.Chamfer)
        {
            var countCheck = RequireShapeCount(commandId, selectedShapes.Length, 1, exactly: true);
            if (!countCheck.CanExecute) return countCheck;
            return Engine.GetTopologyCount(selectedShapes[0], OcctShapeType.Edge) > 0
                ? CadCommandAvailability.Available
                : CadCommandAvailability.Unavailable(Local(
                    $"The selected shape has no edges for {CadLocalization.CommandText(commandId)}.",
                    $"当前形体没有可用于“{CadLocalization.CommandText(commandId)}”的边。"));
        }

        if (SingleShapeCommands.Contains(commandId))
            return RequireShapeCount(commandId, selectedShapes.Length, 1, exactly: true);

        if (TwoShapeCommands.Contains(commandId))
            return RequireShapeCount(commandId, selectedShapes.Length, 2, exactly: true);

        return CadCommandAvailability.Available;
    }

    private void EnsureCommandAvailable(CadCommandId commandId)
    {
        var availability = GetCommandAvailability(commandId);
        if (!availability.CanExecute)
            throw new InvalidOperationException(availability.Message);
    }

    private static CadCommandAvailability RequireObjectCount(
        CadCommandId commandId,
        int actual,
        int required,
        bool exactly)
    {
        if ((exactly && actual == required) || (!exactly && actual >= required))
            return CadCommandAvailability.Available;

        var command = CadLocalization.CommandText(commandId);
        return CadCommandAvailability.Unavailable(actual == 0
            ? Local(
                $"Select at least {required} object{(required == 1 ? string.Empty : "s")} before running {command}.",
                $"请先选择至少 {required} 个对象，再执行“{command}”。")
            : Local(
                $"{command} requires exactly {required} selected object{(required == 1 ? string.Empty : "s")}; {actual} are selected.",
                $"“{command}”需要恰好选择 {required} 个对象，当前已选择 {actual} 个。"));
    }

    private static CadCommandAvailability RequireShapeCount(
        CadCommandId commandId,
        int actual,
        int required,
        bool exactly)
    {
        if ((exactly && actual == required) || (!exactly && actual >= required))
            return CadCommandAvailability.Available;

        var command = CadLocalization.CommandText(commandId);
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

    private static CadCommandAvailability RequireSubshapeCount(
        CadCommandId commandId,
        IReadOnlyCollection<OcctObject> selectedEntries,
        int required)
    {
        var command = CadLocalization.CommandText(commandId);
        if (selectedEntries.Count == required
            && selectedEntries.All(value => value.Kind == OcctObjectKind.Shape))
        {
            return CadCommandAvailability.Available;
        }

        return CadCommandAvailability.Unavailable(Local(
            $"Switch to edge selection and select exactly {required} edge{(required == 1 ? string.Empty : "s")} before running {command}.",
            $"请切换到边选择模式，并恰好选择 {required} 条边，再执行“{command}”。"));
    }

    private static bool IsProfileType(OcctShapeType type) =>
        type is OcctShapeType.Edge or OcctShapeType.Wire or OcctShapeType.Face;
}
