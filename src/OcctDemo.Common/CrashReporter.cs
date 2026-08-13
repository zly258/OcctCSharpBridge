using System.Text;
using OcctNet;

namespace OcctDemo.Common;

public static class CrashReporter
{
    private static readonly object SyncRoot = new();

    public static string Write(string applicationName, Exception exception, string source)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return WriteCore(applicationName, source, BuildDetails(exception));
    }

    public static string WriteMessage(string applicationName, string source, string message)
    {
        return WriteCore(applicationName, source, message);
    }

    public static string BuildUserMessage(Exception exception, string? logPath = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var chinese = DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified;
        var logLabel = chinese ? "日志" : "Log";
        var logMessage = string.IsNullOrWhiteSpace(logPath)
            ? string.Empty
            : $"{Environment.NewLine}{Environment.NewLine}{logLabel}: {logPath}";

        if (!IsNativeLoadFailure(exception))
        {
            var prefix = chinese
                ? "程序发生未处理异常："
                : "An unhandled application error occurred: ";
            return $"{prefix}{exception.Message}{logMessage}";
        }

        try
        {
            var info = OcctRuntime.GetDiagnosticInfo();
            var bridgeState = FormatFileState(info.ApplicationNativeBridgeExists, chinese);
            var kernelState = FormatFileState(info.ApplicationOcctKernelExists, chinese);
            var guidance = BuildNativeLoadGuidance(info, chinese);
            var title = chinese ? "原生 OCCT 运行库加载失败。" : "The native OCCT runtime could not be loaded.";
            var architectureLabel = chinese ? "进程架构" : "Process architecture";
            var osLabel = chinese ? "操作系统" : "Operating system";
            var baseDirectoryLabel = chinese ? "应用目录" : "Application directory";
            var adviceLabel = chinese ? "建议" : "Recommendation";
            var bridgeName = Path.GetFileName(info.ApplicationNativeBridgePath);
            var kernelName = Path.GetFileName(info.ApplicationOcctKernelPath);

            return
                $"{title}{Environment.NewLine}{Environment.NewLine}" +
                $"{exception.Message}{Environment.NewLine}{Environment.NewLine}" +
                $"{osLabel}: {info.OperatingSystemDescription}{Environment.NewLine}" +
                $"{architectureLabel}: {info.ProcessArchitecture}{Environment.NewLine}" +
                $"{baseDirectoryLabel}: {info.BaseDirectory}{Environment.NewLine}" +
                $"{bridgeName}: {bridgeState} {info.ApplicationNativeBridgePath}{Environment.NewLine}" +
                $"{kernelName}: {kernelState} {info.ApplicationOcctKernelPath}{Environment.NewLine}{Environment.NewLine}" +
                $"{adviceLabel}: {guidance}{logMessage}";
        }
        catch
        {
            var prefix = chinese
                ? "原生运行库加载失败："
                : "The native OCCT runtime could not be loaded: ";
            return $"{prefix}{exception.Message}{logMessage}";
        }
    }

    private static string BuildDetails(Exception exception)
    {
        var builder = new StringBuilder();
        builder.AppendLine(exception.ToString());

        if (!IsNativeLoadFailure(exception))
            return builder.ToString().TrimEnd();

        builder.AppendLine();
        builder.AppendLine("OCCT runtime diagnostics:");
        try
        {
            builder.AppendLine(OcctRuntime.GetDiagnosticReport());
        }
        catch (Exception diagnosticException)
        {
            builder.AppendLine($"Runtime diagnostics failed: {diagnosticException.Message}");
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildNativeLoadGuidance(OcctRuntimeDiagnosticInfo info, bool chinese)
    {
        if (!info.Is64BitProcess)
        {
            return chinese
                ? "当前进程不是 x64。该 Bridge 与 Demo 仅支持 Windows x64 / Linux x64，请使用 x64 配置重新构建。"
                : "The current process is not x64. The Bridge and demo support Windows x64 / Linux x64 only; rebuild for x64.";
        }

        if (!info.ApplicationNativeBridgeExists)
        {
            return chinese
                ? "应用目录缺少 Native Bridge。Windows 请运行 .\\build.ps1 avalonia，Linux 请运行 ./build.sh avalonia，确保原生库复制到 Demo 输出目录。"
                : "The application-local native bridge is missing. Run .\\build.ps1 avalonia on Windows or ./build.sh avalonia on Linux so the native bridge is copied beside the demo.";
        }

        if (!info.ApplicationOcctKernelExists)
        {
            return chinese
                ? "Native Bridge 已存在，但应用目录没有 OCCT Kernel。请确认 OCCT_ROOT/CASROOT 正确，并确保 Windows PATH 或 Linux LD_LIBRARY_PATH 包含 OCCT 运行库目录。"
                : "The native bridge exists, but the app-local OCCT kernel is not present. Verify OCCT_ROOT/CASROOT and ensure the Windows PATH or Linux LD_LIBRARY_PATH contains the OCCT runtime directory.";
        }

        return chinese
            ? "Native Bridge 与 OCCT Kernel 均存在。若仍加载失败，请检查更深层 OCCT/第三方依赖与架构是否匹配，并查看日志中的 OCCT runtime diagnostics。"
            : "The native bridge and OCCT kernel are present. If loading still fails, check deeper OCCT/third-party dependencies and architecture compatibility, then inspect the OCCT runtime diagnostics in the log.";
    }

    private static string FormatFileState(bool exists, bool chinese)
    {
        if (chinese)
            return exists ? "[存在]" : "[缺失]";
        return exists ? "[found]" : "[missing]";
    }

    private static bool IsNativeLoadFailure(Exception exception)
    {
        foreach (var current in EnumerateExceptions(exception))
        {
            if (current is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException)
                return true;

            if (current is FileNotFoundException fileNotFound &&
                (ContainsNativeLibraryName(fileNotFound.Message) || ContainsNativeLibraryName(fileNotFound.FileName)))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Exception> EnumerateExceptions(Exception root)
    {
        var pending = new Stack<Exception>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            yield return current;

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }
    }

    private static bool ContainsNativeLibraryName(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        (value.Contains("OcctNative.dll", StringComparison.OrdinalIgnoreCase) ||
         value.Contains("libOcctNative.so", StringComparison.Ordinal) ||
         value.Contains("TKernel.dll", StringComparison.OrdinalIgnoreCase) ||
         value.Contains("libTKernel.so", StringComparison.Ordinal));

    private static string WriteCore(string applicationName, string source, string details)
    {
        try
        {
            var safeApplicationName = string.Concat(
                (applicationName ?? "CAD").Select(character =>
                    Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OcctCSharpBridge",
                "Logs");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, $"{safeApplicationName}-{DateTime.Now:yyyyMMdd}.log");
            var entry =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {source}{Environment.NewLine}" +
                $"{details}{Environment.NewLine}" +
                $"{new string('-', 80)}{Environment.NewLine}";

            lock (SyncRoot)
            {
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, entry, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                }
                else
                {
                    File.AppendAllText(path, entry, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                }
            }

            return path;
        }
        catch
        {
            return string.Empty;
        }
    }
}
