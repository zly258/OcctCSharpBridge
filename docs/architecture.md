# Unified Demo Architecture

`main` produces Binary SDKs. `demo` consumes them.

```text
main
├─ dist/win-x64   → demo: WinForms / WPF / Avalonia
└─ dist/linux-x64 → demo: Avalonia
```

`OcctDemo.Common` is shared by all Demo hosts.

No Demo project owns Bridge implementation code, ABI declarations, or a separate Avalonia branch-specific SDK copy.
