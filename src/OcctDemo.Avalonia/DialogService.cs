using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using OcctDemo.Common;
using DrawingColor = System.Drawing.Color;

namespace OcctDemo.Avalonia;

internal enum DemoDialogChoice
{
    Cancel = 0,
    Ok = 1,
    Yes = 2,
    No = 3
}

internal static class DialogService
{
    public static async Task ShowMessageAsync(Window owner, string title, string message)
    {
        _ = await ShowDialogAsync(owner, title, message, includeNo: false, includeCancel: false);
    }

    public static Task<DemoDialogChoice> ShowQuestionAsync(Window owner, string title, string message, bool includeCancel) =>
        ShowDialogAsync(owner, title, message, includeNo: true, includeCancel: includeCancel);

    public static Task<DrawingColor?> PickColorAsync(Window owner, string title, DrawingColor initialColor) =>
        ClassicColorDialog.ShowAsync(owner, title, initialColor);

    private static async Task<DemoDialogChoice> ShowDialogAsync(Window owner, string title, string message, bool includeNo, bool includeCancel)
    {
        var messageBlock = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 620,
            Margin = new Thickness(0, 0, 0, 16)
        };
        var scroll = new ScrollViewer
        {
            MaxHeight = 420,
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = messageBlock
        };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };
        var dialog = CreateDialogWindow(title, 520, new StackPanel
        {
            Margin = new Thickness(18),
            Children = { scroll, buttons }
        });

        if (includeCancel)
        {
            var cancel = CreateButton(Local("Cancel", "取消"));
            cancel.IsCancel = true;
            cancel.Click += (_, _) => dialog.Close(DemoDialogChoice.Cancel);
            buttons.Children.Add(cancel);
        }
        if (includeNo)
        {
            var no = CreateButton(Local("No", "否"));
            no.Click += (_, _) => dialog.Close(DemoDialogChoice.No);
            buttons.Children.Add(no);
        }
        var primary = CreateButton(includeNo ? Local("Yes", "是") : Local("OK", "确定"));
        primary.IsDefault = true;
        primary.Click += (_, _) => dialog.Close(includeNo ? DemoDialogChoice.Yes : DemoDialogChoice.Ok);
        buttons.Children.Add(primary);

        return await dialog.ShowDialog<DemoDialogChoice>(owner);
    }

    private static Window CreateDialogWindow(string title, double width, Control content) => new()
    {
        Title = title,
        Width = width,
        MinWidth = 360,
        MaxWidth = 720,
        SizeToContent = SizeToContent.Height,
        CanResize = false,
        ShowInTaskbar = false,
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Content = content
    };

    private static Button CreateButton(string text) => new()
    {
        Content = text,
        MinWidth = 96,
        Padding = new Thickness(12, 6),
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center
    };

    private static string Local(string english, string chinese) =>
        DemoLocalization.CurrentLanguage == DemoLanguage.ChineseSimplified ? chinese : english;
}
