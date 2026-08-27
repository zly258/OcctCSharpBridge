namespace OcctNet;

public sealed partial class OcctEngine
{
    /// <summary>
    /// Imports a Wavefront OBJ file through an isolated headless modeling session
    /// and creates the resulting Viewer shape on the native surface thread.
    /// </summary>
    public Task<OcctShape> ImportObjAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        ImportInBackgroundAsync(
            filePath,
            static (session, path) => session.ImportObj(path),
            cancellationToken);

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
