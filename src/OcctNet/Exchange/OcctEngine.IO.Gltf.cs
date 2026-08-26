namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>
    /// Imports a glTF 2.0 JSON (.gltf) or binary (.glb) file through an isolated
    /// headless modeling session and creates the resulting Viewer shape on the
    /// native surface thread.
    /// </summary>
    public Task<OcctShape> ImportGltfAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        ImportInBackgroundAsync(
            filePath,
            static (session, path) => session.ImportGltf(path),
            cancellationToken);
}
