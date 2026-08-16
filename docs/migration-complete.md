# Avalonia Branch Migration

The standalone Avalonia branch model has been replaced by the unified Demo consumer model.

Absorbed into `demo`:

- Avalonia Windows Demo host.
- Avalonia Linux Demo host.
- classic Avalonia color dialog.
- Windows Avalonia publishing through the unified `publish.ps1`.
- Linux Binary SDK synchronization, build, run and portable publish workflows.
- Windows and Linux Avalonia preview assets.

The `avalonia` and `avalonia-dev` branch names are no longer part of the supported architecture. Avalonia itself remains fully supported as an adapter and Demo host.
