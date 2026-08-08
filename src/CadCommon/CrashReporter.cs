using System.Text;
using OcctNet;

namespace CadCommon;

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

        var chinese = CadLocalization.CurrentLanguage == CadLanguage.ChineseSimplified;
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
            var title = chinese ? "原生运行库加载失败。" : "The native OCCT runtime could not be loaded.";
            var architectureLabel = chinese ? "进程架构" : "Process architecture";
            var baseDirectoryLabel = chinese ? "应用目录" : "Application directory";
            var adviceLabel = chinese ? "建议" : "Recommendation";

            return
                $"{title}{Environment.NewLine}{Environment.NewLine}" +
                $"{exception.Message}{Environment.NewLine}{Environment.NewLine}" +
                $"{architectureLabel}: {info.ProcessArchitecture}{Environment.NewLine}" +
                $"{baseDirectoryLabel}: {info.BaseDirectory}{Environment.NewLine}" +
                $"OcctNative.dll: {bridgeState} {info.ApplicationNativeBridgePath}{Environment.NewLine}" +
                $"TKernel.dll: {kernelState} {info.ApplicationOcctKernelPath}{Environment.NewLine}{Environment.NewLine}" +
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
                ? "当前进程不是 x64。该桥接层和发布包仅支持 Windows x64，请重新使用 x64 配置构建并发布。"
                : "The current process is not x64. This bridge and its redistributable package require Windows x64; rebuild and publish for x64.";
        }

        if (!info.ApplicationNativeBridgeExists)
        {
            return chinese
                ? "程序目录缺少 OcctNative.dll。请使用最新 demo/publish.ps1 重新生成发布包，不要手工复制单个 DLL。"
                : "OcctNative.dll is missing from the application directory. Republish with the latest demo/publish.ps1 instead of copying individual DLLs.";
        }

        if (!info.ApplicationOcctKernelExists)
        {
            return chinese
                ? "OcctNative.dll 已存在，但程序目录缺少 TKernel.dll，说明原生依赖闭包不完整。请使用最新 demo/publish.ps1 重新发布。"
                : "OcctNative.dll exists, but TKernel.dll is missing from the application directory, so the native dependency closure is incomplete. Republish with the latest demo/publish.ps1.";
        }

        return chinese
            ? "OcctNative.dll 与 TKernel.dll 均存在。若仍出现 Win32 126，通常是更深层的 OCCT、第三方或 Visual C++ 运行库依赖缺失/版本不匹配。请使用最新 demo/publish.ps1 生成完整包，并查看日志中的 OCCT runtime diagnostics。"
            : "OcctNative.dll and TKernel.dll are both present. If Win32 126 still occurs, a deeper OCCT, third-party, or Visual C++ runtime dependency is usually missing or mismatched. Republish with the latest demo/publish.ps1 and inspect the OCCT runtime diagnostics in the log.";
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
         value.Contains("TKernel.dll", StringComparison.OrdinalIgnoreCase));

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
