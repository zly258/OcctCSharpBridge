using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OcctNet;

/// <summary>
/// OCCT 核心引擎抽象接口，供上层测试 Mock 与解耦
/// </summary>
public interface IOcctEngine : IDisposable
{
    bool IsDisposed { get; }
    bool IsInitialized { get; }
    IReadOnlyList<IOcctObject> GetObjects();
    IReadOnlyList<IOcctObject> SelectedObjects { get; }
    OcctShape Import(string filePath);
    Task<OcctShape> ImportAsync(string filePath, CancellationToken ct = default);
    void Delete(IOcctObject value);
    void Delete(IEnumerable<IOcctObject> values);
}
