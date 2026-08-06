from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
BOM = b"\xef\xbb\xbf"


def read(path: str) -> str:
    data = (ROOT / path).read_bytes()
    return data.decode("utf-8-sig").replace("\r\n", "\n")


def write(path: str, text: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    normalized = text.replace("\r\n", "\n").replace("\n", "\r\n")
    target.write_bytes(BOM + normalized.encode("utf-8"))


def replace_once(text: str, old: str, new: str, name: str) -> str:
    if old not in text:
        raise RuntimeError(f"Expected block was not found: {name}")
    return text.replace(old, new, 1)


# Move the native HWND host out of the UI-independent wrapper.
source = ROOT / "src/OcctNet/OcctViewportControl.cs"
target = ROOT / "src/OcctNet.WinForms/OcctViewportControl.cs"
target.parent.mkdir(parents=True, exist_ok=True)
target.write_bytes(source.read_bytes())
source.unlink()

write(
    "src/OcctNet.WinForms/OcctNet.WinForms.csproj",
    """<Project Sdk=\"Microsoft.NET.Sdk\">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Platforms>x64</Platforms>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=\"..\\OcctNet\\OcctNet.csproj\" />
  </ItemGroup>
</Project>
""",
)

core_project = read("src/OcctNet/OcctNet.csproj")
core_project = re.sub(
    r"^\s*<UseWindowsForms>true</UseWindowsForms>\n", "", core_project, flags=re.MULTILINE
)
write("src/OcctNet/OcctNet.csproj", core_project)

solution = read("OcctBridge.sln")
project_guid = "{7EA6CC4D-34D3-4A91-9C6A-CFC4E339CE58}"
if "OcctNet.WinForms" not in solution:
    project = (
        'Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = '
        '"OcctNet.WinForms", "src\\OcctNet.WinForms\\OcctNet.WinForms.csproj", '
        f'"{project_guid}"\nEndProject\n'
    )
    anchor = 'Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "OcctNet.Smoke"'
    solution = solution.replace(anchor, project + anchor, 1)
    configurations = (
        f"        {project_guid}.Debug|x64.ActiveCfg = Debug|x64\n"
        f"        {project_guid}.Debug|x64.Build.0 = Debug|x64\n"
        f"        {project_guid}.Release|x64.ActiveCfg = Release|x64\n"
        f"        {project_guid}.Release|x64.Build.0 = Release|x64\n"
    )
    config_anchor = "        {A5E5B359-A6C0-40AE-8F83-ADCF59BC9F6E}.Debug|x64.ActiveCfg"
    solution = solution.replace(config_anchor, configurations + config_anchor, 1)
write("OcctBridge.sln", solution)

build = read("build.ps1")
build = replace_once(
    build,
    '$ManagedProject = Join-Path $RepoRoot "src\\OcctNet\\OcctNet.csproj"\n',
    '$ManagedProject = Join-Path $RepoRoot "src\\OcctNet\\OcctNet.csproj"\n'
    '$WinFormsProject = Join-Path $RepoRoot "src\\OcctNet.WinForms\\OcctNet.WinForms.csproj"\n',
    "managed project variable",
)
build = replace_once(
    build,
    '$ManagedOutput = Join-Path $RepoRoot "src\\OcctNet\\bin\\x64\\$Configuration\\net8.0-windows"\n',
    '$ManagedOutput = Join-Path $RepoRoot "src\\OcctNet\\bin\\x64\\$Configuration\\net8.0-windows"\n'
    '$WinFormsOutput = Join-Path $RepoRoot "src\\OcctNet.WinForms\\bin\\x64\\$Configuration\\net8.0-windows"\n',
    "managed output variable",
)
start = build.index("function Build-Managed {")
end = build.index("\nfunction Run-Smoke {", start)
managed_block = """function Build-Managed {
    Assert-Command "dotnet"

    foreach ($project in @($ManagedProject, $WinFormsProject)) {
        Remove-Item (Join-Path (Split-Path -Parent $project) "bin") -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item (Join-Path (Split-Path -Parent $project) "obj") -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Host "[managed] Building core wrapper ($Configuration)..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $ManagedProject,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "OcctNet build failed."

    Write-Host "[managed] Building optional WinForms host ($Configuration)..." -ForegroundColor Cyan
    Invoke-Checked "dotnet" @(
        "build", $WinFormsProject,
        "-c", $Configuration,
        "-p:Platform=x64",
        "--nologo"
    ) "OcctNet.WinForms build failed."

    Assert-Path (Join-Path $ManagedOutput "OcctNet.dll")
    Assert-Path (Join-Path $WinFormsOutput "OcctNet.WinForms.dll")

    if (Test-Path $NativeDll) {
        Copy-Item $NativeDll (Join-Path $ManagedOutput "OcctNative.dll") -Force
    }
    else {
        Write-Warning "OcctNative.dll was not found. Build target 'all' or 'native' before running a consumer application."
    }

    Write-Host "Managed core:     $ManagedOutput" -ForegroundColor Green
    Write-Host "Managed WinForms: $WinFormsOutput" -ForegroundColor Green
}
"""
build = build[:start] + managed_block + build[end:]
write("build.ps1", build)

engine = read("src/OcctNet/OcctEngine.cs")
engine = engine.replace(
    "using System.Runtime.InteropServices;\n",
    "using System.Runtime.InteropServices;\nusing System.Threading;\n",
    1,
)
engine = engine.replace(
    "    public bool IsInitialized => _initialized;\n",
    "    public bool IsInitialized => Volatile.Read(ref _initialized) && Volatile.Read(ref _handle) != IntPtr.Zero;\n",
    1,
)
engine = engine.replace(
    "        if (_initialized) return;\n"
    "        Check(NativeMethods.occt_initialize(_handle, windowHandle));\n"
    "        _initialized = true;\n",
    "        if (Volatile.Read(ref _initialized)) return;\n"
    "        Check(NativeMethods.occt_initialize(_handle, windowHandle));\n"
    "        Volatile.Write(ref _initialized, true);\n",
    1,
)
engine = engine.replace(
    '        if (!_initialized) throw new InvalidOperationException("Initialize the OCCT engine with a valid window handle first.");\n',
    '        if (!Volatile.Read(ref _initialized)) throw new InvalidOperationException("Initialize the OCCT engine with a valid window handle first.");\n',
    1,
)
engine = replace_once(
    engine,
    """    private void EnsureNotDisposed() => ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

    public void Dispose()
    {
        if (_handle == IntPtr.Zero) return;
        NativeMethods.occt_destroy(_handle);
        _handle = IntPtr.Zero;
        _initialized = false;
        GC.SuppressFinalize(this);
    }

    ~OcctEngine() => Dispose();
""",
    """    private void EnsureNotDisposed() =>
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _handle) == IntPtr.Zero, this);

    public void Dispose()
    {
        ReleaseHandle(throwOnError: true);
        GC.SuppressFinalize(this);
    }

    private void ReleaseHandle(bool throwOnError)
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        Volatile.Write(ref _initialized, false);
        if (handle == IntPtr.Zero) return;

        if (throwOnError)
        {
            NativeMethods.occt_destroy(handle);
            return;
        }

        try
        {
            NativeMethods.occt_destroy(handle);
        }
        catch
        {
            // Finalizers must not allow native unload failures to terminate the process.
        }
    }

    ~OcctEngine() => ReleaseHandle(throwOnError: false);
""",
    "OcctEngine disposal",
)
write("src/OcctNet/OcctEngine.cs", engine)

model = read("src/OcctNet/OcctModelingSession.cs")
model = model.replace(
    "using System.Runtime.InteropServices;\n",
    "using System.Runtime.InteropServices;\nusing System.Threading;\n",
    1,
)
model = replace_once(
    model,
    """    private void EnsureNotDisposed() => ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

    public void Dispose()
    {
        if (_handle == IntPtr.Zero) return;
        ModelNativeMethods.occt_model_destroy(_handle);
        _handle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    ~OcctModelingSession() => Dispose();
""",
    """    private void EnsureNotDisposed() =>
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _handle) == IntPtr.Zero, this);

    public void Dispose()
    {
        ReleaseHandle(throwOnError: true);
        GC.SuppressFinalize(this);
    }

    private void ReleaseHandle(bool throwOnError)
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle == IntPtr.Zero) return;

        if (throwOnError)
        {
            ModelNativeMethods.occt_model_destroy(handle);
            return;
        }

        try
        {
            ModelNativeMethods.occt_model_destroy(handle);
        }
        catch
        {
            // Finalizers must not allow native unload failures to terminate the process.
        }
    }

    ~OcctModelingSession() => ReleaseHandle(throwOnError: false);
""",
    "OcctModelingSession disposal",
)
write("src/OcctNet/OcctModelingSession.cs", model)

ocaf = read("src/OcctNet/OcafDocument.cs")
ocaf = ocaf.replace(
    "using System.Runtime.InteropServices;\n",
    "using System.Runtime.InteropServices;\nusing System.Threading;\n",
    1,
)
ocaf = replace_once(
    ocaf,
    """    private void EnsureNotDisposed() => ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);

    public void Dispose()
    {
        if (_handle == IntPtr.Zero) return;
        OcafNativeMethods.occt_ocaf_destroy(_handle);
        _handle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    ~OcafDocument() => Dispose();
""",
    """    private void EnsureNotDisposed() =>
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _handle) == IntPtr.Zero, this);

    public void Dispose()
    {
        ReleaseHandle(throwOnError: true);
        GC.SuppressFinalize(this);
    }

    private void ReleaseHandle(bool throwOnError)
    {
        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle == IntPtr.Zero) return;

        if (throwOnError)
        {
            OcafNativeMethods.occt_ocaf_destroy(handle);
            return;
        }

        try
        {
            OcafNativeMethods.occt_ocaf_destroy(handle);
        }
        catch
        {
            // Finalizers must not allow native unload failures to terminate the process.
        }
    }

    ~OcafDocument() => ReleaseHandle(throwOnError: false);
""",
    "OcafDocument disposal",
)
write("src/OcctNet/OcafDocument.cs", ocaf)

ocaf_types = read("src/OcctNet/OcafTypes.cs")
ocaf_types = replace_once(
    ocaf_types,
    """public sealed class OcafCommandScope : IDisposable
{
    private OcafDocument? _document;
    private bool _completed;

    internal OcafCommandScope(OcafDocument document)
    {
        _document = document;
        document.NewCommand();
    }

    /// <summary>Commits the command and returns whether an undo delta was created.</summary>
    public bool Commit()
    {
        var document = _document ?? throw new ObjectDisposedException(nameof(OcafCommandScope));
        var producedDelta = document.CommitCommand();
        _completed = true;
        return producedDelta;
    }

    public void Abort()
    {
        var document = _document ?? throw new ObjectDisposedException(nameof(OcafCommandScope));
        document.AbortCommand();
        _completed = true;
    }

    public void Dispose()
    {
        var document = _document;
        _document = null;
        if (document is null || _completed) return;
        if (document.HasOpenCommand) document.AbortCommand();
    }
}
""",
    """public sealed class OcafCommandScope : IDisposable
{
    private readonly object _syncRoot = new();
    private OcafDocument? _document;
    private bool _completed;

    internal OcafCommandScope(OcafDocument document)
    {
        _document = document;
        document.NewCommand();
    }

    /// <summary>Commits the command and returns whether an undo delta was created.</summary>
    public bool Commit()
    {
        lock (_syncRoot)
        {
            var document = GetActiveDocument();
            var producedDelta = document.CommitCommand();
            _completed = true;
            _document = null;
            return producedDelta;
        }
    }

    public void Abort()
    {
        lock (_syncRoot)
        {
            var document = GetActiveDocument();
            document.AbortCommand();
            _completed = true;
            _document = null;
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            var document = _document;
            _document = null;
            if (document is null || _completed) return;
            if (document.HasOpenCommand) document.AbortCommand();
            _completed = true;
        }
    }

    private OcafDocument GetActiveDocument() =>
        _document is not null && !_completed
            ? _document
            : throw new ObjectDisposedException(nameof(OcafCommandScope));
}
""",
    "OcafCommandScope completion",
)
write("src/OcctNet/OcafTypes.cs", ocaf_types)

readme = read("README.md")
readme = readme.replace(
    "src/OcctNative   C++17 native bridge and stable C ABI\n"
    "src/OcctNet      Type-safe .NET wrapper\n",
    "src/OcctNative         C++17 native bridge and stable C ABI\n"
    "src/OcctNet            UI-independent, type-safe .NET wrapper\n"
    "src/OcctNet.WinForms   Optional WinForms OCCT viewport control\n",
    1,
)
readme = readme.replace(
    "- `OcctEngine`: HWND viewer, AIS objects, selection, camera, display attributes, text, and dimensions.\n",
    "- `OcctEngine`: HWND viewer, AIS objects, selection, camera, display attributes, text, and dimensions.\n"
    "- `OcctViewportControl` is provided separately by `OcctNet.WinForms`; the core wrapper no longer depends on WinForms.\n",
    1,
)
readme = readme.replace(
    '  <ProjectReference Include="..\\OcctCSharpBridge\\src\\OcctNet\\OcctNet.csproj" />\n',
    '  <ProjectReference Include="..\\OcctCSharpBridge\\src\\OcctNet\\OcctNet.csproj" />\n'
    '  <!-- Add only when a WinForms/WPF host needs OcctViewportControl. -->\n'
    '  <ProjectReference Include="..\\OcctCSharpBridge\\src\\OcctNet.WinForms\\OcctNet.WinForms.csproj" />\n',
    1,
)
readme = readme.replace(
    "`build.ps1 validate` fails",
    "Session disposal is idempotent and finalizer-safe. Instances still represent native mutable state and should not be used concurrently from multiple threads.\n\n`build.ps1 validate` fails",
    1,
)
write("README.md", readme)

readme_zh = read("README.zh-CN.md")
readme_zh = readme_zh.replace(
    "src/OcctNative   C++17 原生桥接与稳定 C ABI\n"
    "src/OcctNet      类型安全的 .NET 封装\n",
    "src/OcctNative         C++17 原生桥接与稳定 C ABI\n"
    "src/OcctNet            不依赖 UI 的类型安全 .NET 封装\n"
    "src/OcctNet.WinForms   可选的 WinForms OCCT 视口控件\n",
    1,
)
readme_zh = readme_zh.replace(
    "- `OcctEngine`：HWND 视口、AIS 对象、选择、相机、显示属性、文字与尺寸。\n",
    "- `OcctEngine`：HWND 视口、AIS 对象、选择、相机、显示属性、文字与尺寸。\n"
    "- `OcctViewportControl` 已独立到 `OcctNet.WinForms`，核心封装不再依赖 WinForms。\n",
    1,
)
readme_zh = readme_zh.replace(
    '  <ProjectReference Include="..\\OcctCSharpBridge\\src\\OcctNet\\OcctNet.csproj" />\n',
    '  <ProjectReference Include="..\\OcctCSharpBridge\\src\\OcctNet\\OcctNet.csproj" />\n'
    '  <!-- 仅 WinForms/WPF 宿主需要 OcctViewportControl 时引用。 -->\n'
    '  <ProjectReference Include="..\\OcctCSharpBridge\\src\\OcctNet.WinForms\\OcctNet.WinForms.csproj" />\n',
    1,
)
readme_zh = readme_zh.replace(
    "`build.ps1 validate` 会在",
    "会话释放已改为幂等且终结器安全。各会话仍封装可变的原生状态，不应由多个线程并发调用。\n\n`build.ps1 validate` 会在",
    1,
)
write("README.zh-CN.md", readme_zh)

for path, note in (
    (
        "docs/API_COVERAGE.md",
        "\nThe `OcctViewport*` types above are provided by the optional `OcctNet.WinForms` assembly; all other managed types remain in the UI-independent `OcctNet` assembly.\n",
    ),
    (
        "docs/API_COVERAGE.zh-CN.md",
        "\n上述 `OcctViewport*` 类型由可选的 `OcctNet.WinForms` 程序集提供；其余托管类型仍位于不依赖 UI 的 `OcctNet` 程序集中。\n",
    ),
):
    inventory = read(path)
    inventory = inventory.replace(
        "- `OcctViewportWorldPointEventArgs`\n",
        "- `OcctViewportWorldPointEventArgs`\n" + note,
        1,
    )
    write(path, inventory)
