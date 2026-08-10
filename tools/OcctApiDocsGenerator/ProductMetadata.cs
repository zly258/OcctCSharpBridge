internal static partial class Program
{
    private const string ProductAuthor = "zly258";

    static Program()
    {
        Zh = Zh with
        {
            GeneratedNotice = Zh.GeneratedNotice + $" Author: **{ProductAuthor}**。"
        };

        En = En with
        {
            GeneratedNotice = En.GeneratedNotice + $" Author: **{ProductAuthor}**."
        };
    }
}
