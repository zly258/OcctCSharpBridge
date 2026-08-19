using System.Drawing;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class AppearanceContractTests
{
    [TestMethod]
    public void HighlightModesAndStylesRemainStable()
    {
        var enumValues = EnumValues<OcctHighlightMode>();
        Assert.AreEqual(0, enumValues[nameof(OcctHighlightMode.BoundingBox)]);
        Assert.AreEqual(1, enumValues[nameof(OcctHighlightMode.Wireframe)]);
        Assert.AreEqual(2, enumValues[nameof(OcctHighlightMode.Shaded)]);

        var style = new OcctViewerHighlightStyle(OcctHighlightMode.Shaded, Color.Orange);
        Assert.AreEqual(OcctHighlightMode.Shaded, style.Mode);
        Assert.AreEqual(Color.Orange.ToArgb(), style.Color.ToArgb());

        RequireEngineMethod(nameof(OcctEngine.SetSelectionHighlightColor), typeof(Color));
        RequireEngineMethod(nameof(OcctEngine.SetHoverHighlightColor), typeof(Color));
        RequireEngineMethod(nameof(OcctEngine.SetSelectionHighlightMode), typeof(OcctHighlightMode));
        RequireEngineMethod(nameof(OcctEngine.SetHoverHighlightMode), typeof(OcctHighlightMode));
        RequireEngineMethod(nameof(OcctEngine.SetSelectionHighlightStyle), typeof(OcctViewerHighlightStyle));
        RequireEngineMethod(nameof(OcctEngine.SetHoverHighlightStyle), typeof(OcctViewerHighlightStyle));
    }

    [TestMethod]
    public void ViewerDecorationPositionApisRemainStable()
    {
        var corners = EnumValues<OcctCornerPosition>();
        Assert.AreEqual(0, corners[nameof(OcctCornerPosition.LeftLower)]);
        Assert.AreEqual(1, corners[nameof(OcctCornerPosition.LeftUpper)]);
        Assert.AreEqual(2, corners[nameof(OcctCornerPosition.RightLower)]);
        Assert.AreEqual(3, corners[nameof(OcctCornerPosition.RightUpper)]);

        var triedron = new OcctTriedronOptions
        {
            Visible = true,
            Position = OcctCornerPosition.RightLower,
            Scale = 0.12,
            Color = Color.White
        };
        Assert.IsTrue(triedron.Visible);
        Assert.AreEqual(OcctCornerPosition.RightLower, triedron.Position);
        Assert.AreEqual(0.12, triedron.Scale, 1e-12);
        Assert.AreEqual(Color.White.ToArgb(), triedron.Color.ToArgb());

        var viewCube = new OcctViewCubeOptions
        {
            Visible = true,
            Position = OcctCornerPosition.LeftUpper,
            SizePixels = 100,
            OffsetX = 12,
            OffsetY = 16
        };
        Assert.IsTrue(viewCube.Visible);
        Assert.AreEqual(OcctCornerPosition.LeftUpper, viewCube.Position);
        Assert.AreEqual(100, viewCube.SizePixels);
        Assert.AreEqual(12, viewCube.OffsetX);
        Assert.AreEqual(16, viewCube.OffsetY);

        RequireEngineMethod(nameof(OcctEngine.SetTriedron), typeof(OcctTriedronOptions));
        RequireEngineMethod(nameof(OcctEngine.SetTriedronPosition), typeof(OcctCornerPosition));
        RequireEngineMethod(nameof(OcctEngine.SetTriedronScale), typeof(double));
        RequireEngineMethod(nameof(OcctEngine.SetTriedronColor), typeof(Color));
        RequireEngineMethod(nameof(OcctEngine.SetViewCube), typeof(OcctViewCubeOptions));
        RequireEngineMethod(nameof(OcctEngine.SetViewCubeOptions), typeof(OcctViewCubeOptions));
        RequireEngineMethod(nameof(OcctEngine.SetViewCubePosition), typeof(OcctCornerPosition));
        RequireEngineMethod(nameof(OcctEngine.SetViewCubeSize), typeof(int));
        RequireEngineMethod(nameof(OcctEngine.SetViewCubeOffset), typeof(int), typeof(int));
    }

    private static IReadOnlyDictionary<string, int> EnumValues<TEnum>()
        where TEnum : struct, Enum =>
        typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .ToDictionary(
                field => field.Name,
                field => Convert.ToInt32(field.GetRawConstantValue(), null));

    private static void RequireEngineMethod(string name, params Type[] parameterTypes)
    {
        var method = typeof(OcctEngine).GetMethod(name, parameterTypes);
        Assert.IsNotNull(method, $"OcctEngine must expose {name}({string.Join(", ", parameterTypes.Select(type => type.Name))}).");
        Assert.IsTrue(method.IsPublic, $"OcctEngine.{name} must remain public.");
    }
}
