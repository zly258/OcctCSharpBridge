using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class AlgorithmResourceTests
{
    [TestMethod]
    public void AlgorithmDiagnosticsRemainAvailableAfterSessionDisposal()
    {
        OcctAlgorithmResource resource;
        long operationId;

        using (var session = new OcctModelingSession())
        {
            var left = session.MakeBox(20, 20, 20);
            var right = session.MakeBox(20, 20, 20, 10, 0, 0);
            var result = session.Fuse(left, right);
            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(result.HasErrors);

            operationId = result.OperationId;
            resource = session.AcquireAlgorithm(result);
            Assert.AreEqual(operationId, resource.OperationId);
            Assert.AreEqual(result.HasWarnings, resource.HasWarnings);
            Assert.AreEqual(result.HasErrors, resource.HasErrors);
            Assert.AreEqual(result.Report, resource.Report);
        }

        using (resource)
        {
            Assert.AreEqual(operationId, resource.OperationId);
            Assert.IsFalse(resource.HasErrors);
            _ = resource.Report;
        }
    }
}
