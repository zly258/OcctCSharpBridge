internal static partial class Program
{
    static Program()
    {
        var author = ResolveProductAuthor();
        Zh = Zh with
        {
            GeneratedNotice = Zh.GeneratedNotice + $" Author: **{author}**。"
        };
        En = En with
        {
            GeneratedNotice = En.GeneratedNotice + $" Author: **{author}**."
        };
    }

    private static string ResolveProductAuthor()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                var contractPath = Path.Combine(directory.FullName, "bridge-contract.json");
                if (File.Exists(contractPath))
                {
                    try
                    {
                        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(contractPath));
                        if (document.RootElement.TryGetProperty("author", out var authorElement))
                        {
                            var author = authorElement.GetString();
                            if (!string.IsNullOrWhiteSpace(author)) return author;
                        }
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        // Main performs strict contract parsing and reports malformed JSON.
                    }
                }
                directory = directory.Parent;
            }
        }
        return "zly258";
    }
}
