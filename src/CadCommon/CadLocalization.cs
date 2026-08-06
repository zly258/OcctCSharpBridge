using System.Collections.ObjectModel;

namespace CadCommon;

public enum CadLanguage
{
    English,
    ChineseSimplified
}

public static class CadLocalization
{
    private static CadLanguage _currentLanguage = CadLanguage.English;

    private static readonly IReadOnlyDictionary<string, string> EnglishUi = new ReadOnlyDictionary<string, string>(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AppTitle.WinForms"] = "OCCT CAD - WinForms",
            ["AppTitle.Wpf"] = "OCCT CAD - WPF",
            ["Menu.File"] = "&File",
            ["Menu.Edit"] = "&Edit",
            ["Menu.Draw"] = "&Draw",
            ["Menu.Solid"] = "&Solid",
            ["Menu.Annotate"] = "&Annotate",
            ["Menu.View"] = "&View",
            ["Menu.Tools"] = "&Tools",
            ["Menu.Samples"] = "&Samples",
            ["Menu.Help"] = "&Help",
            ["Menu.Language"] = "&Language",
            ["Menu.New"] = "New",
            ["Menu.Open"] = "Open...",
            ["Menu.Save"] = "Save",
            ["Menu.SaveAs"] = "Save As...",
            ["Menu.Import"] = "Import...",
            ["Menu.ExportSelected"] = "Export Selected...",
            ["Menu.ExportImage"] = "Export View Image...",
            ["Menu.Exit"] = "Exit",
            ["Menu.Undo"] = "Undo",
            ["Menu.Redo"] = "Redo",
            ["Menu.ClearSelection"] = "Deselect All",
            ["Menu.ShowAll"] = "Show All",
            ["Menu.HideAll"] = "Hide All",
            ["Menu.Primitives"] = "3D Primitives",
            ["Menu.Features"] = "Solid Features",
            ["Menu.Boolean"] = "Boolean Operations",
            ["Menu.Details"] = "Edges and Shell",
            ["Menu.Display"] = "Visual Styles",
            ["Menu.StandardViews"] = "Standard Views",
            ["Menu.Projection"] = "Projection",
            ["Menu.FitAll"] = "Zoom Extents",
            ["Menu.FitSelected"] = "Zoom Selected",
            ["Menu.Shaded"] = "Shaded",
            ["Menu.Wireframe"] = "2D Wireframe",
            ["Menu.Hlr"] = "Hidden Line",
            ["Menu.Antialiasing"] = "Anti-aliasing",
            ["Menu.Triedron"] = "UCS Icon",
            ["Menu.ViewCube"] = "ViewCube",
            ["Menu.Front"] = "Front",
            ["Menu.Back"] = "Back",
            ["Menu.Left"] = "Left",
            ["Menu.Right"] = "Right",
            ["Menu.Top"] = "Top",
            ["Menu.Bottom"] = "Bottom",
            ["Menu.Isometric"] = "SW Isometric",
            ["Menu.NorthEast"] = "NE Isometric",
            ["Menu.NorthWest"] = "NW Isometric",
            ["Menu.SouthEast"] = "SE Isometric",
            ["Menu.SouthWest"] = "SW Isometric",
            ["Menu.Orthographic"] = "Parallel Projection",
            ["Menu.Perspective"] = "Perspective Projection",
            ["Menu.PerspectiveFov"] = "Perspective Field of View...",
            ["Menu.DisplayPrecision"] = "Display Resolution...",
            ["Menu.Lighting"] = "Scene Lighting...",
            ["Menu.ResetLighting"] = "Reset Lighting",
            ["Menu.Material"] = "Default Material",
            ["Menu.SelectionMode"] = "Selection Filter",
            ["Menu.SelectionTolerance"] = "Selection Aperture...",
            ["Menu.WindowSelection"] = "Window Selection",
            ["Menu.Background"] = "Background Color...",
            ["Menu.GradientBackground"] = "Gradient Background",
            ["Menu.DepthHandling"] = "Depth and Coplanar Display",
            ["Menu.AutoZFit"] = "Automatic Z Range",
            ["Menu.AutoZFitNow"] = "Recalculate Z Range",
            ["Menu.DepthForward"] = "Bring Selected Forward",
            ["Menu.DepthBackward"] = "Push Selected Back",
            ["Menu.DepthReset"] = "Reset Selected Depth Bias",
            ["Menu.MouseHelp"] = "Mouse Controls",
            ["Menu.About"] = "About",
            ["Menu.English"] = "English",
            ["Menu.Chinese"] = "简体中文",
            ["Toolbar.New"] = "New",
            ["Toolbar.Open"] = "Open",
            ["Toolbar.Save"] = "Save",
            ["Toolbar.Undo"] = "Undo",
            ["Toolbar.Redo"] = "Redo",
            ["Toolbar.Line"] = "Line",
            ["Toolbar.Circle"] = "Circle",
            ["Toolbar.Box"] = "Box",
            ["Toolbar.Cylinder"] = "Cylinder",
            ["Toolbar.Shaded"] = "Shaded",
            ["Toolbar.Wireframe"] = "Wireframe",
            ["Toolbar.Extents"] = "Extents",
            ["Toolbar.Isometric"] = "Isometric",
            ["Toolbar.Selection"] = "Selection:",
            ["Panel.ModelExplorer"] = "Model Explorer",
            ["Panel.Properties"] = "Properties",
            ["Panel.CommandLine"] = "Command Line",
            ["Property.Name"] = "Property",
            ["Property.Value"] = "Value",
            ["Status.Initializing"] = "Initializing...",
            ["Status.Ready"] = "Ready - OCCT {0}",
            ["Status.NoneSelected"] = "No selection",
            ["Status.Selected"] = "{0} object(s) selected",
            ["Status.WindowSelectionOn"] = "Window selection enabled",
            ["Status.WindowSelectionOff"] = "Window selection disabled",
            ["Status.AutoZFitOn"] = "Automatic Z-range fitting enabled",
            ["Status.AutoZFitOff"] = "Automatic Z-range fitting disabled",
            ["Status.DepthBiasApplied"] = "Depth bias updated for {0} object(s).",
            ["Status.DepthBiasNoShape"] = "Select one or more shapes first.",
            ["Dialog.OpenTitle"] = "Open Drawing",
            ["Dialog.ImportTitle"] = "Import Model",
            ["Dialog.SaveTitle"] = "Save Drawing",
            ["Dialog.ExportTitle"] = "Export Selected Objects",
            ["Dialog.ExportImageTitle"] = "Export View Image",
            ["Dialog.Ok"] = "OK",
            ["Dialog.Cancel"] = "Cancel",
            ["Dialog.Yes"] = "Yes",
            ["Dialog.No"] = "No",
            ["Dialog.ErrorTitle"] = "CAD Operation Failed",
            ["Dialog.ConfirmDiscard"] = "The drawing has unsaved changes. Save before continuing?",
            ["Dialog.ConfirmDiscardTitle"] = "Unsaved Drawing",
            ["Dialog.ApplyExistingMaterial"] = "Apply this material to existing solids?",
            ["Dialog.AboutText"] = "OCCT CAD demonstration application\nOpen CASCADE Technology 7.9.0\nWinForms / WPF native viewport bridge\nPolyForm Noncommercial License 1.0.0",
            ["Dialog.MouseText"] = "Left click: select\nDrag left button: window selection\nCtrl + selection: add to selection\nRight drag: orbit\nMiddle drag: pan\nMouse wheel: zoom",
            ["Selection.Object"] = "Object",
            ["Selection.Vertex"] = "Vertex",
            ["Selection.Edge"] = "Edge",
            ["Selection.Wire"] = "Wire",
            ["Selection.Face"] = "Face",
            ["Selection.Shell"] = "Shell",
            ["Selection.Solid"] = "Solid",
            ["History.Undo"] = "Undo {0}",
            ["History.Redo"] = "Redo {0}",
            ["History.NothingToUndo"] = "Nothing to undo.",
            ["History.NothingToRedo"] = "Nothing to redo.",
            ["History.Undone"] = "Undo completed: {0}",
            ["History.Redone"] = "Redo completed: {0}",
            ["History.NotTracked"] = "This command is not included in undo history.",
            ["Session.New"] = "New drawing created.",
            ["Session.Open"] = "Opened: {0}",
            ["Session.Import"] = "Imported: {0}",
            ["Session.Save"] = "Saved: {0}",
            ["Session.Export"] = "Exported selected object: {0}",
            ["Session.Created"] = "Created {0}.",
            ["Session.Deleted"] = "Deleted {0} object(s).",
            ["Session.SelectOne"] = "Select a shape in the viewport or Model Explorer first.",
            ["Session.SelectMany"] = "This command requires at least {0} shapes. Hold Ctrl to add objects to the selection set.",
            ["Session.SelectSubshape"] = "Set the appropriate vertex, edge, or face selection filter and select a subobject in the viewport.",
            ["Session.NoExportShape"] = "The drawing contains no shape that can be exported.",
            ["Session.UnsupportedSave"] = "Supported save formats are STEP, IGES, BREP, and STL.",
            ["Session.UnsupportedExport"] = "Supported export formats are STEP, IGES, BREP, and STL.",
            ["Object.Id"] = "ID",
            ["Object.Name"] = "Name",
            ["Object.Kind"] = "Object Type",
            ["Object.Topology"] = "Topology Type",
            ["Object.Validity"] = "Validity",
            ["Object.Valid"] = "Valid",
            ["Object.Invalid"] = "Invalid",
            ["Object.SizeX"] = "Size X",
            ["Object.SizeY"] = "Size Y",
            ["Object.SizeZ"] = "Size Z",
            ["Object.Center"] = "Center",
            ["Object.Vertices"] = "Vertices",
            ["Object.Edges"] = "Edges",
            ["Object.Faces"] = "Faces"
        });

    private static readonly IReadOnlyDictionary<string, string> ChineseUi = new ReadOnlyDictionary<string, string>(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AppTitle.WinForms"] = "OCCT CAD - WinForms",
            ["AppTitle.Wpf"] = "OCCT CAD - WPF",
            ["Menu.File"] = "文件(&F)", ["Menu.Edit"] = "编辑(&E)", ["Menu.Draw"] = "绘图(&D)", ["Menu.Solid"] = "实体(&S)",
            ["Menu.Annotate"] = "注释(&A)", ["Menu.View"] = "视图(&V)", ["Menu.Tools"] = "工具(&T)", ["Menu.Samples"] = "示例(&X)",
            ["Menu.Help"] = "帮助(&H)", ["Menu.Language"] = "语言(&L)", ["Menu.New"] = "新建", ["Menu.Open"] = "打开...", ["Menu.Save"] = "保存",
            ["Menu.SaveAs"] = "另存为...", ["Menu.Import"] = "导入...", ["Menu.ExportSelected"] = "导出选中对象...", ["Menu.ExportImage"] = "导出视图图片...",
            ["Menu.Exit"] = "退出", ["Menu.Undo"] = "撤销", ["Menu.Redo"] = "重做", ["Menu.ClearSelection"] = "取消全部选择", ["Menu.ShowAll"] = "全部显示",
            ["Menu.HideAll"] = "全部隐藏", ["Menu.Primitives"] = "三维基本体", ["Menu.Features"] = "实体特征", ["Menu.Boolean"] = "布尔运算",
            ["Menu.Details"] = "边处理与薄壁", ["Menu.Display"] = "视觉样式", ["Menu.StandardViews"] = "标准视图", ["Menu.Projection"] = "投影方式",
            ["Menu.FitAll"] = "缩放至范围", ["Menu.FitSelected"] = "缩放至选中对象", ["Menu.Shaded"] = "着色", ["Menu.Wireframe"] = "二维线框",
            ["Menu.Hlr"] = "隐藏线", ["Menu.Antialiasing"] = "抗锯齿", ["Menu.Triedron"] = "显示坐标图标", ["Menu.ViewCube"] = "显示视图立方体",
            ["Menu.Front"] = "前视图", ["Menu.Back"] = "后视图", ["Menu.Left"] = "左视图", ["Menu.Right"] = "右视图", ["Menu.Top"] = "俯视图",
            ["Menu.Bottom"] = "仰视图", ["Menu.Isometric"] = "西南轴测", ["Menu.NorthEast"] = "东北轴测", ["Menu.NorthWest"] = "西北轴测",
            ["Menu.SouthEast"] = "东南轴测", ["Menu.SouthWest"] = "西南轴测", ["Menu.Orthographic"] = "平行投影", ["Menu.Perspective"] = "透视投影",
            ["Menu.PerspectiveFov"] = "透视视场角...", ["Menu.DisplayPrecision"] = "显示精度...", ["Menu.Lighting"] = "场景光照...",
            ["Menu.ResetLighting"] = "恢复默认光照", ["Menu.Material"] = "默认材质", ["Menu.SelectionMode"] = "选择过滤器",
            ["Menu.SelectionTolerance"] = "选择容差...", ["Menu.WindowSelection"] = "框选", ["Menu.Background"] = "背景颜色...",
            ["Menu.GradientBackground"] = "渐变背景", ["Menu.DepthHandling"] = "深度与共面显示", ["Menu.AutoZFit"] = "自动 Z 范围", ["Menu.AutoZFitNow"] = "重新计算 Z 范围", ["Menu.DepthForward"] = "将选中对象前移", ["Menu.DepthBackward"] = "将选中对象后移", ["Menu.DepthReset"] = "恢复选中对象深度偏移", ["Menu.MouseHelp"] = "鼠标操作", ["Menu.About"] = "关于", ["Menu.English"] = "English",
            ["Menu.Chinese"] = "简体中文", ["Toolbar.New"] = "新建", ["Toolbar.Open"] = "打开", ["Toolbar.Save"] = "保存", ["Toolbar.Undo"] = "撤销",
            ["Toolbar.Redo"] = "重做", ["Toolbar.Line"] = "直线", ["Toolbar.Circle"] = "圆", ["Toolbar.Box"] = "长方体", ["Toolbar.Cylinder"] = "圆柱",
            ["Toolbar.Shaded"] = "着色", ["Toolbar.Wireframe"] = "线框", ["Toolbar.Extents"] = "范围", ["Toolbar.Isometric"] = "轴测",
            ["Toolbar.Selection"] = "选择：", ["Panel.ModelExplorer"] = "模型浏览器", ["Panel.Properties"] = "特性", ["Panel.CommandLine"] = "命令行",
            ["Property.Name"] = "属性", ["Property.Value"] = "值", ["Status.Initializing"] = "正在初始化...", ["Status.Ready"] = "就绪 - OCCT {0}",
            ["Status.NoneSelected"] = "未选择", ["Status.Selected"] = "已选择 {0} 个对象", ["Status.WindowSelectionOn"] = "框选已启用",
            ["Status.WindowSelectionOff"] = "框选已关闭", ["Status.AutoZFitOn"] = "自动 Z 范围调整已启用", ["Status.AutoZFitOff"] = "自动 Z 范围调整已关闭", ["Status.DepthBiasApplied"] = "已更新 {0} 个对象的深度偏移。", ["Status.DepthBiasNoShape"] = "请先选择一个或多个 Shape。", ["Dialog.OpenTitle"] = "打开图形", ["Dialog.ImportTitle"] = "导入模型", ["Dialog.SaveTitle"] = "保存图形",
            ["Dialog.ExportTitle"] = "导出选中对象", ["Dialog.ExportImageTitle"] = "导出视图图片", ["Dialog.Ok"] = "确定", ["Dialog.Cancel"] = "取消",
            ["Dialog.Yes"] = "是", ["Dialog.No"] = "否", ["Dialog.ErrorTitle"] = "CAD 操作失败", ["Dialog.ConfirmDiscard"] = "当前图形包含未保存的修改，是否在继续前保存？",
            ["Dialog.ConfirmDiscardTitle"] = "未保存图形", ["Dialog.ApplyExistingMaterial"] = "是否将该材质同时应用到现有实体？",
            ["Dialog.AboutText"] = "OCCT CAD 演示程序\nOpen CASCADE Technology 7.9.0\nWinForms / WPF 原生视口桥接\nPolyForm Noncommercial License 1.0.0",
            ["Dialog.MouseText"] = "左键单击：选择\n按住左键拖动：框选\nCtrl + 选择：追加选择\n按住右键拖动：动态旋转\n按住中键拖动：平移\n滚轮：缩放",
            ["Selection.Object"] = "对象", ["Selection.Vertex"] = "顶点", ["Selection.Edge"] = "边", ["Selection.Wire"] = "线框", ["Selection.Face"] = "面",
            ["Selection.Shell"] = "壳", ["Selection.Solid"] = "实体", ["History.Undo"] = "撤销 {0}", ["History.Redo"] = "重做 {0}",
            ["History.NothingToUndo"] = "没有可撤销的操作。", ["History.NothingToRedo"] = "没有可重做的操作。", ["History.Undone"] = "已撤销：{0}",
            ["History.Redone"] = "已重做：{0}", ["History.NotTracked"] = "该命令不加入撤销历史。", ["Session.New"] = "已新建空白图形。",
            ["Session.Open"] = "已打开：{0}", ["Session.Import"] = "已导入：{0}", ["Session.Save"] = "已保存：{0}", ["Session.Export"] = "已导出选中对象：{0}",
            ["Session.Created"] = "已创建{0}。", ["Session.Deleted"] = "已删除 {0} 个对象。", ["Session.SelectOne"] = "请先在视图区或模型浏览器中选择一个形体。",
            ["Session.SelectMany"] = "该命令至少需要选择 {0} 个形体，可按 Ctrl 追加选择。", ["Session.SelectSubshape"] = "请切换到相应的顶点、边或面选择过滤器，并在视图区选择子元素。",
            ["Session.NoExportShape"] = "当前图形中没有可导出的形体。", ["Session.UnsupportedSave"] = "保存格式仅支持 STEP、IGES、BREP 和 STL。",
            ["Session.UnsupportedExport"] = "导出格式仅支持 STEP、IGES、BREP 和 STL。", ["Object.Id"] = "ID", ["Object.Name"] = "名称",
            ["Object.Kind"] = "对象类型", ["Object.Topology"] = "拓扑类型", ["Object.Validity"] = "有效性", ["Object.Valid"] = "有效", ["Object.Invalid"] = "无效",
            ["Object.SizeX"] = "尺寸 X", ["Object.SizeY"] = "尺寸 Y", ["Object.SizeZ"] = "尺寸 Z", ["Object.Center"] = "中心",
            ["Object.Vertices"] = "顶点数", ["Object.Edges"] = "边数", ["Object.Faces"] = "面数"
        });

    private static readonly IReadOnlyDictionary<CadCommandId, (string Text, string Description)> EnglishCommands =
        new ReadOnlyDictionary<CadCommandId, (string Text, string Description)>(new Dictionary<CadCommandId, (string, string)>
        {
            [CadCommandId.Point] = ("Point", "Creates a point at the specified coordinates."),
            [CadCommandId.Line] = ("Line", "Creates a line segment between two points."),
            [CadCommandId.Polyline] = ("Polyline", "Creates an open or closed polyline from coordinate points."),
            [CadCommandId.Circle] = ("Circle", "Creates a circle on the XY plane."),
            [CadCommandId.ArcThreePoints] = ("3-Point Arc", "Creates an arc through three points."),
            [CadCommandId.ArcCenter] = ("Center Arc", "Creates an arc from center, radius, start angle, and end angle."),
            [CadCommandId.Ellipse] = ("Ellipse", "Creates an ellipse on the XY plane."),
            [CadCommandId.Rectangle] = ("Rectangle", "Creates a rectangular wire or planar face."),
            [CadCommandId.Polygon] = ("Polygon", "Creates a regular polygon wire or planar face."),
            [CadCommandId.Bezier] = ("Bezier Curve", "Creates a Bezier curve from control points."),
            [CadCommandId.BSpline] = ("B-Spline", "Creates an interpolated B-spline curve."),
            [CadCommandId.Box] = ("Box", "Creates a rectangular solid."),
            [CadCommandId.Cylinder] = ("Cylinder", "Creates a cylindrical solid along the Z axis."),
            [CadCommandId.Frustum] = ("Conical Frustum", "Creates a conical frustum."),
            [CadCommandId.Cone] = ("Cone", "Creates a conical solid."),
            [CadCommandId.Torus] = ("Torus", "Creates a toroidal solid."),
            [CadCommandId.Sphere] = ("Sphere", "Creates a spherical solid."),
            [CadCommandId.Wedge] = ("Wedge", "Creates a wedge solid."),
            [CadCommandId.Pipe] = ("Tube", "Creates a straight hollow cylindrical tube."),
            [CadCommandId.Extrude] = ("Extrude", "Extrudes the selected wire or face along a vector."),
            [CadCommandId.Revolve] = ("Revolve", "Revolves the selected profile about an axis."),
            [CadCommandId.Sweep] = ("Sweep", "Sweeps a profile along a selected path."),
            [CadCommandId.Loft] = ("Loft", "Creates a loft through two or more selected sections."),
            [CadCommandId.Fuse] = ("Union", "Combines the first two selected solids."),
            [CadCommandId.Cut] = ("Subtract", "Subtracts the second selected solid from the first."),
            [CadCommandId.Common] = ("Intersect", "Creates the common volume of the first two selected solids."),
            [CadCommandId.Section] = ("Section", "Creates intersection curves between two selected shapes."),
            [CadCommandId.Fillet] = ("Fillet", "Rounds all edges of the selected shape."),
            [CadCommandId.Chamfer] = ("Chamfer", "Chamfers all edges of the selected shape."),
            [CadCommandId.Offset] = ("Offset", "Offsets the selected shape."),
            [CadCommandId.Shell] = ("Shell", "Removes a face and creates a thin-walled solid."),
            [CadCommandId.Drill] = ("Hole", "Cuts a cylindrical hole along the Z axis."),
            [CadCommandId.Translate] = ("Move", "Moves the selected object by a displacement vector."),
            [CadCommandId.Rotate] = ("Rotate", "Rotates the selected object about an axis."),
            [CadCommandId.Scale] = ("Scale", "Scales the selected object about a base point."),
            [CadCommandId.Mirror] = ("Mirror", "Mirrors the selected object about a plane."),
            [CadCommandId.Copy] = ("Copy", "Creates a copy of the selected object."),
            [CadCommandId.Delete] = ("Erase", "Erases the selected objects."),
            [CadCommandId.Text] = ("Vector Text", "Creates scalable BRep vector text that stays sharp when zoomed."),
            [CadCommandId.LengthDimension] = ("Vector Linear Dimension", "Creates one BRep result containing dimension lines, arrows, and vector text."),
            [CadCommandId.AngleDimension] = ("Vector Angular Dimension", "Creates one BRep result containing an arc, arrows, and vector text."),
            [CadCommandId.RadiusDimension] = ("Vector Radius Dimension", "Creates one BRep result containing a leader, arrow, and vector text."),
            [CadCommandId.DiameterDimension] = ("Vector Diameter Dimension", "Creates one BRep result containing dimension lines, arrows, and vector text."),
            [CadCommandId.AnalyzeBounds] = ("Extents", "Reports the bounding box of the selected shape."),
            [CadCommandId.AnalyzeMass] = ("Mass Properties", "Reports length, area, volume, and centroid."),
            [CadCommandId.AnalyzeTopology] = ("Topology Statistics", "Counts vertices, edges, wires, faces, shells, and solids."),
            [CadCommandId.AnalyzeDistance] = ("Minimum Distance", "Calculates the minimum distance between two selected shapes."),
            [CadCommandId.ValidateShape] = ("Validate Shape", "Checks BREP topology and geometry validity."),
            [CadCommandId.DemoPrimitives] = ("Primitive Gallery", "Creates a gallery of common 2D and 3D primitives."),
            [CadCommandId.DemoBracket] = ("Mechanical Bracket", "Creates a drilled and filleted mechanical bracket."),
            [CadCommandId.DemoFlange] = ("Eight-Hole Flange", "Creates a flange with a bore and eight bolt holes."),
            [CadCommandId.DemoPipe] = ("Swept Pipe", "Creates a pipe by sweeping a circular profile along a 3D curve."),
            [CadCommandId.DemoTee] = ("Pipe Tee", "Creates a hollow pipe tee using Boolean operations."),
            [CadCommandId.DemoReducer] = ("Reducer", "Creates a hollow reducer using outer and inner lofts."),
            [CadCommandId.DemoLoft] = ("Lofted Body", "Creates a multi-section lofted solid."),
            [CadCommandId.DemoBoolean] = ("Boolean Examples", "Creates union, subtract, intersect, and section examples."),
            [CadCommandId.DemoElements] = ("Comprehensive Elements", "Creates representative curve, face, solid, and feature results."),
            [CadCommandId.DemoGear] = ("Complex Gear", "Creates a complete gear with teeth, a bore, and relief holes."),
            [CadCommandId.DemoManifold] = ("Multi-Port Manifold", "Creates a complex body with multi-directional ports and internal channels."),
            [CadCommandId.DemoTwistedDuct] = ("Twisted Duct", "Creates a hollow multi-section twisted transition duct."),
            [CadCommandId.DemoAnnotations] = ("Vector Annotations", "Creates BRep text and linear, angular, radius, and diameter annotations.")
        });

    private static readonly IReadOnlyDictionary<string, string> EnglishParameterLabels = new ReadOnlyDictionary<string, string>(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["x"] = "X Coordinate", ["y"] = "Y Coordinate", ["z"] = "Z Coordinate",
            ["x1"] = "Start X", ["y1"] = "Start Y", ["z1"] = "Start Z",
            ["x2"] = "Second X", ["y2"] = "Second Y", ["z2"] = "Second Z",
            ["x3"] = "End X", ["y3"] = "End Y", ["z3"] = "End Z",
            ["px"] = "Axis Point X", ["py"] = "Axis Point Y", ["pz"] = "Axis Point Z",
            ["ax"] = "Axis Direction X", ["ay"] = "Axis Direction Y", ["az"] = "Axis Direction Z",
            ["nx"] = "Normal X", ["ny"] = "Normal Y", ["nz"] = "Normal Z",
            ["dx"] = "Delta X", ["dy"] = "Delta Y", ["dz"] = "Delta Z",
            ["radius"] = "Radius", ["r1"] = "Base Radius", ["r2"] = "Top Radius",
            ["major"] = "Major Radius", ["minor"] = "Minor Radius", ["outer"] = "Outer Radius",
            ["wall"] = "Wall Thickness", ["height"] = "Height", ["width"] = "Width",
            ["sides"] = "Number of Sides", ["start"] = "Start Angle", ["end"] = "End Angle",
            ["angle"] = "Angle", ["factor"] = "Scale Factor", ["distance"] = "Chamfer Distance",
            ["offset"] = "Offset Distance", ["tolerance"] = "Tolerance", ["thickness"] = "Thickness",
            ["depth"] = "Depth", ["face"] = "Face Index", ["flyout"] = "Dimension Offset",
            ["points"] = "Point Coordinates", ["text"] = "Text", ["closed"] = "Closed",
            ["periodic"] = "Periodic", ["solid"] = "Create Solid", ["ruled"] = "Ruled Loft",
            ["hide"] = "Hide Source Objects", ["zoomable"] = "Zoom with View", ["ltx"] = "Top Length",
            ["font"] = "Font", ["bold"] = "Bold", ["italic"] = "Italic",
            ["textHeight"] = "Text Height", ["arrowSize"] = "Arrow Size",
            ["fov"] = "Vertical Field of View", ["coefficient"] = "Deviation Coefficient",
            ["existing"] = "Apply to Existing Objects", ["ambient"] = "Ambient Intensity",
            ["directional"] = "Directional Intensity", ["headlight"] = "Camera Headlight", ["pixels"] = "Aperture Size"
        });

    public static CadLanguage CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage == value) return;
            _currentLanguage = value;
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static event EventHandler? LanguageChanged;

    public static string Text(string key, params object[] arguments)
    {
        var dictionary = CurrentLanguage == CadLanguage.ChineseSimplified ? ChineseUi : EnglishUi;
        if (!dictionary.TryGetValue(key, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(System.Globalization.CultureInfo.CurrentCulture, value, arguments);
    }

    public static string CommandText(CadCommandId id)
    {
        if (CurrentLanguage == CadLanguage.English && EnglishCommands.TryGetValue(id, out var command)) return command.Text;
        return CadCommandCatalog.Get(id).Text;
    }

    public static CadCommandDefinition Localize(CadCommandDefinition definition)
    {
        if (CurrentLanguage == CadLanguage.ChineseSimplified) return definition;
        var command = EnglishCommands.TryGetValue(definition.Id, out var english)
            ? english
            : (Text: definition.Text, Description: definition.Description);
        var parameters = definition.Parameters
            .Select(parameter => parameter with
            {
                Label = EnglishParameterLabels.TryGetValue(parameter.Key, out var label) ? label : parameter.Key
            })
            .ToArray();
        return definition with
        {
            Category = TranslateCategory(definition.Category),
            Text = command.Text,
            Description = command.Description,
            Parameters = parameters
        };
    }

    public static string SelectionMode(OcctNet.OcctSelectionMode mode) => Text($"Selection.{mode}");

    public static string ObjectKind(OcctNet.OcctObjectKind kind)
    {
        if (CurrentLanguage == CadLanguage.English) return kind.ToString();
        return kind switch
        {
            OcctNet.OcctObjectKind.Shape => "形体",
            OcctNet.OcctObjectKind.Text => "文字",
            OcctNet.OcctObjectKind.Dimension => "尺寸",
            _ => kind.ToString()
        };
    }

    public static string ShapeType(OcctNet.OcctShapeType type)
    {
        if (CurrentLanguage == CadLanguage.English) return type.ToString();
        return type switch
        {
            OcctNet.OcctShapeType.Compound => "复合体",
            OcctNet.OcctShapeType.CompSolid => "组合实体",
            OcctNet.OcctShapeType.Solid => "实体",
            OcctNet.OcctShapeType.Shell => "壳",
            OcctNet.OcctShapeType.Face => "面",
            OcctNet.OcctShapeType.Wire => "线框",
            OcctNet.OcctShapeType.Edge => "边",
            OcctNet.OcctShapeType.Vertex => "顶点",
            _ => "形体"
        };
    }

    private static string TranslateCategory(string category) => category switch
    {
        "二维" => "Draw",
        "三维" => "Solid",
        "编辑" => "Modify",
        "注释" => "Annotate",
        "工具" => "Tools",
        "示例" => "Samples",
        _ => category
    };
}
