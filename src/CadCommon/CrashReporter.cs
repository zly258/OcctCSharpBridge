using System.Text;

namespace CadCommon;

public static class CrashReporter
{
    private static readonly object SyncRoot = new();

    public static string Write(string applicationName, Exception exception, string source)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return WriteCore(applicationName, source, exception.ToString());
    }

    public static string WriteMessage(string applicationName, string source, string message)
    {
        return WriteCore(applicationName, source, message);
    }

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
