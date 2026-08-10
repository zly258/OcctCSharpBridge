internal static partial class Program
{
    private const string ProductAuthor = "zly258";
    private const string ProductVersion = "2.6.0";
    private const string ProductStack = "OCCT 7.9.0 · .NET 10 · C# 14 · C++17 · Avalonia 12.1.0 · Windows x64";

    static Program()
    {
        Zh = Zh with
        {
            GeneratedNotice = Zh.GeneratedNotice + $" Author: **{ProductAuthor}**。",
            ManagedIntro = $"Bridge **{ProductVersion}** · Native ABI **4** · {ProductStack}。" + Environment.NewLine + Environment.NewLine + Zh.ManagedIntro,
            NativeIntro = $"Author: **{ProductAuthor}**。Bridge **{ProductVersion}** · Native ABI **4** · {ProductStack}。" + Environment.NewLine + Environment.NewLine + Zh.NativeIntro
        };

        En = En with
        {
            GeneratedNotice = En.GeneratedNotice + $" Author: **{ProductAuthor}**.",
            ManagedIntro = $"Bridge **{ProductVersion}** · Native ABI **4** · {ProductStack}." + Environment.NewLine + Environment.NewLine + En.ManagedIntro,
            NativeIntro = $"Author: **{ProductAuthor}**. Bridge **{ProductVersion}** · Native ABI **4** · {ProductStack}." + Environment.NewLine + Environment.NewLine + En.NativeIntro
        };
    }
}
