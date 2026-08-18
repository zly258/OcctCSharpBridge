using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class ViewerAnnotationContractTests
{
    [TestMethod]
    public void LengthDimensionSupportsHostOwnedDraftingPlane()
    {
        var overload = typeof(OcctEngine)
            .GetMethods()
            .SingleOrDefault(method =>
            {
                if (!string.Equals(method.Name, nameof(OcctEngine.AddLengthDimension), StringComparison.Ordinal))
                    return false;
                var parameters = method.GetParameters();
                return parameters.Length == 4 &&
                       parameters[0].ParameterType == typeof(OcctShape) &&
                       parameters[1].ParameterType == typeof(OcctVector3d) &&
                       parameters[2].ParameterType == typeof(double) &&
                       parameters[3].ParameterType == typeof(System.Drawing.Color?);
            });

        Assert.IsNotNull(overload, "Viewer length dimensions must expose an explicit drafting-plane overload.");
        Assert.AreEqual(typeof(OcctDimension), overload.ReturnType);
    }
}
