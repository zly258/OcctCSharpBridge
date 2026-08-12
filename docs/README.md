# Demo Branch Notes

The `demo` branch is a Windows Binary SDK consumer. It does not contain Bridge source and does not own Avalonia.

## Projects

```text
OcctDemo.Common
├─ OcctDemo.WinForms → OcctNet.WinForms
└─ OcctDemo.Wpf      → OcctNet.Wpf
```

Both UI applications share modeling/session logic from `OcctDemo.Common` while using independent UI hosts.

## SDK workflow

1. Publish a validated Windows SDK on `main`.
2. Switch to `demo`.
3. Run `./sync.ps1` locally.
4. Build or run WinForms/WPF demos.

`dist/win-x64` is intentionally ignored on `demo`; the synchronized files are not a second source of truth.

Avalonia development, Windows/Linux scripts and the cross-platform `OcctAvaloniaViewport` belong to the `avalonia` branch and are intentionally absent here.

No GitHub Actions or NuGet publication flow is used by this demo branch.