using OcctNet;

namespace OcctDemo.Common;

public enum DemoCommandId
{
    Point, Line, Polyline, Circle, ArcThreePoints, ArcCenter, Ellipse, Rectangle, Polygon, Bezier, BSpline,
    Box, Cylinder, Frustum, Cone, Torus, Sphere, Wedge, Pipe,
    Extrude, Revolve, Sweep, Loft, Fuse, Cut, Common, Section, Fillet, Chamfer, Offset, Shell, Drill,
    Translate, Rotate, Scale, Mirror, Copy, Delete,
    Text, LengthDimension, AngleDimension, RadiusDimension, DiameterDimension,
    AnalyzeBounds, AnalyzeMass, AnalyzeTopology, AnalyzeDistance, ValidateShape,
    DemoPrimitives, DemoBracket, DemoFlange, DemoPipe, DemoTee, DemoReducer, DemoLoft, DemoBoolean,
    DemoElements, DemoGear, DemoManifold, DemoTwistedDuct, DemoAnnotations
}

public enum DemoParameterKind { Number, Integer, Text, Boolean, Choice }

public sealed record DemoParameterDefinition(
    string Key,
    string Label,
    DemoParameterKind Kind,
    string DefaultValue,
    string? Unit = null,
    IReadOnlyList<string>? Options = null);

public sealed record DemoCommandDefinition(
    DemoCommandId Id,
    string Category,
    string Text,
    string Description,
    IReadOnlyList<DemoParameterDefinition> Parameters,
    string? Shortcut = null);

public sealed record DemoCommandResult(
    string Message,
    IReadOnlyList<IOcctObject> CreatedObjects,
    string? AnalysisText = null)
{
    public static DemoCommandResult Empty(string message) => new(message, Array.Empty<IOcctObject>());
    public static DemoCommandResult Created(string message, params IOcctObject[] objects) => new(message, objects);
}

public static class DemoCommandCatalog
{
    private static DemoParameterDefinition N(string key, string label, double value, string? unit = "mm") => new(key, label, DemoParameterKind.Number, value.ToString(System.Globalization.CultureInfo.InvariantCulture), unit);
    private static DemoParameterDefinition I(string key, string label, int value) => new(key, label, DemoParameterKind.Integer, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    private static DemoParameterDefinition T(string key, string label, string value) => new(key, label, DemoParameterKind.Text, value);
    private static DemoParameterDefinition B(string key, string label, bool value) => new(key, label, DemoParameterKind.Boolean, value ? "true" : "false");

    public static IReadOnlyList<DemoCommandDefinition> All { get; } = new DemoCommandDefinition[]
    {
        new(DemoCommandId.Point, "二维", "点", "按三维坐标创建顶点。", new[] { N("x","X",0), N("y","Y",0), N("z","Z",0) }),
        new(DemoCommandId.Line, "二维", "直线", "创建两点直线。", new[] { N("x1","起点 X",0), N("y1","起点 Y",0), N("z1","起点 Z",0), N("x2","终点 X",100), N("y2","终点 Y",0), N("z2","终点 Z",0) }),
        new(DemoCommandId.Polyline, "二维", "多段线", "根据坐标串创建多段线，格式：x,y,z;x,y,z。", new[] { T("points","坐标点","0,0,0;100,0,0;100,60,0"), B("closed","闭合",false) }),
        new(DemoCommandId.Circle, "二维", "圆", "在 XY 平面创建圆。", new[] { N("x","圆心 X",0), N("y","圆心 Y",0), N("z","圆心 Z",0), N("radius","半径",50) }),
        new(DemoCommandId.ArcThreePoints, "二维", "三点圆弧", "通过起点、中间点和终点创建圆弧。", new[] { N("x1","起点 X",0), N("y1","起点 Y",0), N("x2","中间点 X",50), N("y2","中间点 Y",30), N("x3","终点 X",100), N("y3","终点 Y",0), N("z","Z",0) }),
        new(DemoCommandId.ArcCenter, "二维", "圆心圆弧", "按圆心、半径和角度创建圆弧。", new[] { N("x","圆心 X",0), N("y","圆心 Y",0), N("z","圆心 Z",0), N("radius","半径",50), N("start","起始角",0,"°"), N("end","终止角",120,"°") }),
        new(DemoCommandId.Ellipse, "二维", "椭圆", "在 XY 平面创建椭圆。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","中心 Z",0), N("major","长半轴",80), N("minor","短半轴",40) }),
        new(DemoCommandId.Rectangle, "二维", "矩形", "创建矩形线框或平面。", new[] { N("x","原点 X",0), N("y","原点 Y",0), N("z","原点 Z",0), N("width","宽度",100), N("height","高度",60), B("face","生成平面",false) }),
        new(DemoCommandId.Polygon, "二维", "正多边形", "创建正多边形线框或平面。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","中心 Z",0), N("radius","外接圆半径",50), I("sides","边数",6), B("face","生成平面",false) }),
        new(DemoCommandId.Bezier, "二维", "Bezier 曲线", "按控制点创建 Bezier 曲线。", new[] { T("points","控制点","0,0,0;40,70,0;80,-20,0;120,40,0") }),
        new(DemoCommandId.BSpline, "二维", "B 样条", "按插值点创建 B 样条曲线。", new[] { T("points","插值点","0,0,0;30,40,0;60,-20,0;90,50,0;120,0,0"), B("periodic","周期曲线",false) }),

        new(DemoCommandId.Box, "三维", "长方体", "创建长方体。", new[] { N("x","原点 X",0), N("y","原点 Y",0), N("z","原点 Z",0), N("dx","长度",100), N("dy","宽度",70), N("dz","高度",50) }),
        new(DemoCommandId.Cylinder, "三维", "圆柱", "创建 Z 轴圆柱。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","底面 Z",0), N("radius","半径",40), N("height","高度",100) }),
        new(DemoCommandId.Frustum, "三维", "圆台", "创建圆台。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","底面 Z",0), N("r1","底半径",50), N("r2","顶半径",30), N("height","高度",100) }),
        new(DemoCommandId.Cone, "三维", "圆锥", "创建圆锥。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","底面 Z",0), N("radius","底半径",50), N("height","高度",100) }),
        new(DemoCommandId.Torus, "三维", "圆环", "创建圆环体。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","中心 Z",0), N("major","主半径",60), N("minor","管半径",15) }),
        new(DemoCommandId.Sphere, "三维", "球", "创建球体。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","中心 Z",0), N("radius","半径",50) }),
        new(DemoCommandId.Wedge, "三维", "楔体", "创建楔体。", new[] { N("dx","长度",100), N("dy","宽度",70), N("dz","高度",60), N("ltx","顶部长度",40) }),
        new(DemoCommandId.Pipe, "三维", "圆管", "创建直圆管。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","底面 Z",0), N("outer","外半径",50), N("wall","壁厚",5), N("height","长度",150) }),
        new(DemoCommandId.Extrude, "三维", "拉伸", "拉伸当前选择的线框或平面。", new[] { N("dx","方向 X",0), N("dy","方向 Y",0), N("dz","方向 Z",100), B("hide","隐藏输入",true) }),
        new(DemoCommandId.Revolve, "三维", "旋转", "绕轴旋转当前选择的轮廓。", new[] { N("px","轴点 X",0), N("py","轴点 Y",0), N("pz","轴点 Z",0), N("ax","轴向 X",0), N("ay","轴向 Y",1), N("az","轴向 Z",0), N("angle","角度",360,"°"), B("hide","隐藏输入",true) }),
        new(DemoCommandId.Sweep, "三维", "扫掠", "按选择顺序使用路径和截面扫掠。", new[] { B("hide","隐藏输入",true) }),
        new(DemoCommandId.Loft, "三维", "放样", "对两个以上选中截面放样。", new[] { B("solid","生成实体",true), B("ruled","直纹放样",false), B("hide","隐藏输入",true) }),
        new(DemoCommandId.Fuse, "三维", "布尔并集", "对前两个选中实体执行并集。", new[] { B("hide","隐藏输入",true) }),
        new(DemoCommandId.Cut, "三维", "布尔差集", "用第二个选中实体切除第一个。", new[] { B("hide","隐藏输入",true) }),
        new(DemoCommandId.Common, "三维", "布尔交集", "求前两个选中实体的交集。", new[] { B("hide","隐藏输入",true) }),
        new(DemoCommandId.Section, "三维", "布尔截交线", "生成前两个选中形体的截交线。", new[] { B("hide","隐藏输入",false) }),
        new(DemoCommandId.Fillet, "三维", "圆角", "对当前选中形体的全部边倒圆。", new[] { N("radius","圆角半径",5), B("hide","隐藏输入",true) }),
        new(DemoCommandId.Chamfer, "三维", "倒角", "对当前选中形体的全部边倒角。", new[] { N("distance","倒角距离",5), B("hide","隐藏输入",true) }),
        new(DemoCommandId.Offset, "三维", "偏移", "对当前选择形体执行偏移。", new[] { N("offset","偏移量",5), N("tolerance","容差",0.0001,null), B("hide","隐藏输入",true) }),
        new(DemoCommandId.Shell, "三维", "抽壳", "移除指定面并生成薄壁实体，面索引从 0 开始。", new[] { I("face","移除面索引",0), N("thickness","壁厚",5), B("hide","隐藏输入",true) }),
        new(DemoCommandId.Drill, "三维", "钻孔", "在选中实体上沿 Z 轴钻孔。", new[] { N("x","孔中心 X",0), N("y","孔中心 Y",0), N("z","起点 Z",0), N("radius","孔半径",10), N("depth","深度",100), B("hide","隐藏输入",true) }),

        new(DemoCommandId.Translate, "编辑", "移动", "平移当前选择形体。", new[] { N("dx","X",50), N("dy","Y",0), N("dz","Z",0), B("hide","隐藏原对象",true) }),
        new(DemoCommandId.Rotate, "编辑", "旋转", "绕指定轴旋转当前选择形体。", new[] { N("px","轴点 X",0), N("py","轴点 Y",0), N("pz","轴点 Z",0), N("ax","轴向 X",0), N("ay","轴向 Y",0), N("az","轴向 Z",1), N("angle","角度",45,"°"), B("hide","隐藏原对象",true) }),
        new(DemoCommandId.Scale, "编辑", "缩放", "以指定中心缩放当前选择形体。", new[] { N("x","中心 X",0), N("y","中心 Y",0), N("z","中心 Z",0), N("factor","比例",1.5,null), B("hide","隐藏原对象",true) }),
        new(DemoCommandId.Mirror, "编辑", "镜像", "关于指定平面镜像当前选择形体。", new[] { N("x","平面点 X",0), N("y","平面点 Y",0), N("z","平面点 Z",0), N("nx","法向 X",1), N("ny","法向 Y",0), N("nz","法向 Z",0), B("hide","隐藏原对象",false) }),
        new(DemoCommandId.Copy, "编辑", "复制", "复制当前选择形体。", new[] { B("hide","隐藏原对象",false) }),
        new(DemoCommandId.Delete, "编辑", "删除", "删除全部选中对象。", Array.Empty<DemoParameterDefinition>(), "Delete"),

        new(DemoCommandId.Text, "注释", "矢量文字", "通过 Headless Modeling 生成 BRep 矢量文字并显示到 Viewer。", new[] { T("text","文字","OCCT CAD"), N("x","X",0), N("y","Y",0), N("z","Z",0), N("height","文字高度",18), N("depth","挤出厚度",0), T("font","字体",DemoFonts.OcctSansSerif), B("bold","粗体",false), B("italic","斜体",false) }),
        new(DemoCommandId.LengthDimension, "注释", "线性尺寸", "基于当前选中边创建交互式 Viewer 线性尺寸。", new[] { N("flyout","引出距离",20) }),
        new(DemoCommandId.AngleDimension, "注释", "角度尺寸", "基于当前选择的两条边创建交互式 Viewer 角度尺寸。", new[] { N("flyout","圆弧半径",30) }),
        new(DemoCommandId.RadiusDimension, "注释", "半径尺寸", "基于当前选中圆边创建交互式 Viewer 半径尺寸。", new[] { N("flyout","引出距离",20) }),
        new(DemoCommandId.DiameterDimension, "注释", "直径尺寸", "基于当前选中圆边创建交互式 Viewer 直径尺寸。", new[] { N("flyout","引出距离",20) }),

        new(DemoCommandId.AnalyzeBounds, "工具", "包围盒", "查询当前形体包围盒。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.AnalyzeMass, "工具", "几何属性", "查询长度、面积、体积和重心。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.AnalyzeTopology, "工具", "拓扑统计", "统计顶点、边、线框、面、壳和实体。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.AnalyzeDistance, "工具", "最短距离", "计算前两个选中形体的最短距离。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.ValidateShape, "工具", "形体检查", "检查当前形体是否有效。", Array.Empty<DemoParameterDefinition>()),

        new(DemoCommandId.DemoPrimitives, "示例", "基本体陈列", "生成二维和三维基本体陈列。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoBracket, "示例", "机械支架", "生成带孔、圆角的机械支架。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoFlange, "示例", "八孔法兰", "生成中心孔、八个螺栓孔和圆角的法兰盘。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoPipe, "示例", "扫掠弯管", "生成三维路径和圆截面的扫掠弯管。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoTee, "示例", "管道三通", "通过多方向圆柱布尔运算生成空心三通。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoReducer, "示例", "异径管", "通过内外双层放样和布尔差集生成异径管。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoLoft, "示例", "放样壳体", "生成多截面放样体。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoBoolean, "示例", "布尔运算", "生成并集、差集、交集和截交线示例。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoElements, "示例", "综合元素测试", "生成曲线、平面、实体和特征的代表性结果。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoGear, "示例", "复杂齿轮", "生成带轮齿、中心孔和减重孔的完整齿轮。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoManifold, "示例", "多通道阀体", "生成带多方向接口和内部孔道的复杂阀体。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoTwistedDuct, "示例", "扭转风管", "生成多截面扭转过渡的中空风管。", Array.Empty<DemoParameterDefinition>()),
        new(DemoCommandId.DemoAnnotations, "示例", "矢量注释标注", "通过 Headless Modeling 生成 BRep 文字、线性、角度、半径和直径标注。", Array.Empty<DemoParameterDefinition>())
    };

    public static DemoCommandDefinition Get(DemoCommandId id) => All.First(command => command.Id == id);
}
