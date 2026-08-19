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
