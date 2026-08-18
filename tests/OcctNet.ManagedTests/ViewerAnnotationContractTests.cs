using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class ViewerAnnotationContractTests
{
    [TestMethod]
    public void LengthDimensionSupportsHostOwnedDraftingPlane()
    {
        var overload = FindOverload(
            nameof(OcctEngine.AddLengthDimension),
            typeof(OcctShape),
            typeof(OcctVector3d),
            typeof(double),
            typeof(System.Drawing.Color?));

        Assert.IsNotNull(overload, "Viewer length dimensions must expose an explicit drafting-plane overload.");
        Assert.AreEqual(typeof(OcctDimension), overload.ReturnType);
    }

    [TestMethod]
    public void AngleDimensionSupportsHostOwnedDraftingPlane()
    {
        var overload = FindOverload(
            nameof(OcctEngine.AddAngleDimension),
            typeof(OcctShape),
            typeof(OcctShape),
            typeof(OcctVector3d),
            typeof(double),
            typeof(System.Drawing.Color?));

        Assert.IsNotNull(overload, "Viewer angle dimensions must expose an explicit drafting-plane overload.");
        Assert.AreEqual(typeof(OcctDimension), overload.ReturnType);
    }

    private static System.Reflection.MethodInfo? FindOverload(string name, params Type[] parameterTypes) =>
        typeof(OcctEngine)
            .GetMethods()
            .SingleOrDefault(method =>
            {
                if (!string.Equals(method.Name, name, StringComparison.Ordinal)) return false;
                var parameters = method.GetParameters();
                return parameters.Length == parameterTypes.Length &&
                       parameters.Select(parameter => parameter.ParameterType).SequenceEqual(parameterTypes);
            });
}
