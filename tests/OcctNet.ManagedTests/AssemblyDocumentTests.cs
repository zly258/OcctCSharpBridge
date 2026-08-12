using Microsoft.VisualStudio.TestTools.UnitTesting;
using OcctNet;

namespace OcctNet.ManagedTests;

[TestClass]
public sealed class AssemblyDocumentTests
{
    [TestMethod]
    public void AssemblyTransformReadsThreeByFourMatrix()
    {
        var transform = OcctAssemblyTransform3d.FromArray(
        [
            1, 0, 0, 10,
            0, 1, 0, 20,
            0, 0, 1, 30
        ]);

        Assert.AreEqual(1, transform.M00);
        Assert.AreEqual(10, transform.M03);
        Assert.AreEqual(20, transform.M13);
        Assert.AreEqual(30, transform.M23);
        Assert.AreEqual(OcctAssemblyTransform3d.Identity, OcctAssemblyTransform3d.FromArray([1, 2, 3]));
    }

    [TestMethod]
    public void AssemblyColorKeepsAlphaAsTransparency()
    {
        var color = new OcctAssemblyColor(0.2, 0.4, 0.6, 0.25);
        Assert.AreEqual(0.75, color.Transparency, 1e-12);
    }

    [TestMethod]
    public void AssemblyTreeDoesNotDependOnDisplayNameUniqueness()
    {
        var root = Node("0:1", 0, -1, OcctAssemblyNodeKind.Assembly, "Assembly");
        var first = Node("0:1:1", 1, 0, OcctAssemblyNodeKind.Instance, "SameName");
        var second = Node("0:1:2", 2, 0, OcctAssemblyNodeKind.Instance, "SameName");

        root.AddChild(first);
        root.AddChild(second);

        Assert.AreEqual(2, root.Children.Count);
        Assert.AreEqual("0:1:1", root.Children[0].Id);
        Assert.AreEqual("0:1:2", root.Children[1].Id);
        Assert.AreSame(root, first.Parent);
        Assert.AreSame(root, second.Parent);
    }

    [TestMethod]
    public void SubshapeStylesRemainAttachedToOwningNode()
    {
        var style = new OcctAssemblySubshapeStyle(
            OcctShapeType.Face,
            12,
            new OcctAssemblyStyle(
                true,
                new OcctAssemblyColor(1, 0, 0, 0.8),
                null));
        var node = new OcctAssemblyNode(
            "0:1:1",
            0,
            -1,
            OcctAssemblyNodeKind.Part,
            "Part",
            "Definition",
            null,
            new OcctAssemblyStyle(true, null, null),
            OcctAssemblyTransform3d.Identity,
            OcctAssemblyTransform3d.Identity,
            new[] { style });

        Assert.AreEqual(1, node.SubshapeStyles.Count);
        Assert.AreEqual(OcctShapeType.Face, node.SubshapeStyles[0].ShapeType);
        Assert.AreEqual(12, node.SubshapeStyles[0].SubshapeIndex);
        Assert.AreEqual(0.2, node.SubshapeStyles[0].Style.Transparency, 1e-12);
    }

    private static OcctAssemblyNode Node(
        string id,
        int index,
        int parentIndex,
        OcctAssemblyNodeKind kind,
        string name) =>
        new(
            id,
            index,
            parentIndex,
            kind,
            name,
            string.Empty,
            null,
            new OcctAssemblyStyle(true, null, null),
            OcctAssemblyTransform3d.Identity,
            OcctAssemblyTransform3d.Identity,
            Array.Empty<OcctAssemblySubshapeStyle>());
}
