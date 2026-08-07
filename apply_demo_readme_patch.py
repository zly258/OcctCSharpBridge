from pathlib import Path


def patch(path, english=True):
    p = Path(path)
    text = p.read_text(encoding='utf-8-sig')
    if english:
        anchor = '## Build and run\n\n'
        block = '''## First-time setup\n\nClone the repository, switch to the demo branch, then configure the OCCT 7.9.0 SDK:\n\n```powershell\ngit clone https://github.com/zly258/OcctCSharpBridge.git\ncd OcctCSharpBridge\ngit switch demo\n$env:OCCT_ROOT = "D:\\\\tools\\\\occt-vc144-64"\n```\n\n### PowerShell scripts\n\n`build.ps1` is the build/validation entry point. Supported targets are `validate`, `native`, `managed`, `smoke`, `winform`, `wpf`, `avalonia`, and `all`. `validate` does not require an OCCT SDK; native/demo/smoke targets do.\n\n```powershell\n.\\build.ps1 validate Release\n.\\build.ps1 managed Release\n.\\build.ps1 winform Release\n.\\build.ps1 wpf Release\n.\\build.ps1 avalonia Release\n.\\build.ps1 all Release\n.\\build.ps1 smoke Release\n```\n\n`run.ps1` starts an **already-built** executable; it does not rebuild the project. Syntax:\n\n```powershell\n.\\run.ps1 <winform|wpf|avalonia> [Release] [-OcctRoot <path>]\n```\n\nExamples:\n\n```powershell\n.\\run.ps1 winform\n.\\run.ps1 wpf\n.\\run.ps1 avalonia\n```\n\n`publish.ps1` creates deployment-complete packages for WinForms and WPF. Avalonia is currently covered by build/run/CI but is not yet part of the formal publish target.\n\n```powershell\n.\\publish.ps1 all Release -Zip -OcctRoot "D:\\\\tools\\\\occt-vc144-64"\n.\\publish.ps1 winform Release -Zip -OcctRoot "D:\\\\tools\\\\occt-vc144-64"\n.\\publish.ps1 wpf Release -Zip -OcctRoot "D:\\\\tools\\\\occt-vc144-64"\n```\n\n### Display defaults\n\nWinForms and WPF start in shaded mode with face edges enabled. Use **View → Visual Styles → Shaded with Edges** to toggle face-boundary drawing independently from Shaded/Wireframe. The lightweight Avalonia demo also enables face boundaries by default.\n\n'''
    else:
        anchor = '## 构建与运行\n\n'
        block = '''## 第一次使用\n\n先克隆仓库、切换到 `demo` 分支，再配置 OCCT 7.9.0：\n\n```powershell\ngit clone https://github.com/zly258/OcctCSharpBridge.git\ncd OcctCSharpBridge\ngit switch demo\n$env:OCCT_ROOT = "D:\\\\tools\\\\occt-vc144-64"\n```\n\n### PowerShell 脚本说明\n\n`build.ps1` 是统一构建与校核入口，支持 `validate`、`native`、`managed`、`smoke`、`winform`、`wpf`、`avalonia`、`all`。其中 `validate` 不要求 OCCT SDK；涉及 Native、Demo 或 Smoke 的目标需要 OCCT。\n\n```powershell\n.\\build.ps1 validate Release\n.\\build.ps1 managed Release\n.\\build.ps1 winform Release\n.\\build.ps1 wpf Release\n.\\build.ps1 avalonia Release\n.\\build.ps1 all Release\n.\\build.ps1 smoke Release\n```\n\n`run.ps1` **只启动已经构建好的程序，不会自动重新编译**。格式：\n\n```powershell\n.\\run.ps1 <winform|wpf|avalonia> [Release] [-OcctRoot <path>]\n```\n\n常用示例：\n\n```powershell\n.\\run.ps1 winform\n.\\run.ps1 wpf\n.\\run.ps1 avalonia\n```\n\n`publish.ps1` 用于生成 WinForms/WPF 可部署发布包。Avalonia 当前已纳入 build/run/CI，但尚未纳入正式 publish 目标。\n\n```powershell\n.\\publish.ps1 all Release -Zip -OcctRoot "D:\\\\tools\\\\occt-vc144-64"\n.\\publish.ps1 winform Release -Zip -OcctRoot "D:\\\\tools\\\\occt-vc144-64"\n.\\publish.ps1 wpf Release -Zip -OcctRoot "D:\\\\tools\\\\occt-vc144-64"\n```\n\n### 默认显示样式\n\nWinForms 与 WPF 默认使用着色并显示实体边线，可在 **视图 → 视觉样式 → 着色并显示边线** 独立开关面边界显示；它不会改变 Shaded/Wireframe 本身。轻量 Avalonia Demo 也默认开启面边界。\n\n'''
    if '## First-time setup' in text or '## 第一次使用' in text:
        return
    if anchor not in text:
        raise RuntimeError(f'anchor not found in {path}')
    text = text.replace(anchor, block + anchor, 1)
    if english:
        trouble = '''\n## Troubleshooting\n\n- If `run.ps1` starts an old executable, rebuild the relevant target first; the runner does not compile.\n- If Avalonia exits during startup, inspect `src\\CadAvalonia\\bin\\x64\\<Configuration>\\net8.0-windows\\CAD-Avalonia.log`.\n- If native loading fails, rebuild with the correct `-OcctRoot` and make sure OCCT/third-party runtime DLLs are available.\n- `build.ps1 validate` is the fastest check after API/menu/host changes; `build.ps1 smoke` verifies real native modeling.\n'''
        text = text.replace('\n## Publish\n', trouble + '\n## Publish\n', 1)
    else:
        trouble = '''\n## 常见问题\n\n- `run.ps1` 运行的是旧程序：先重新执行对应 `build.ps1` target；运行脚本不会编译。\n- Avalonia 启动退出：查看 `src\\CadAvalonia\\bin\\x64\\<Configuration>\\net8.0-windows\\CAD-Avalonia.log`。\n- Native DLL 加载失败：使用正确 `-OcctRoot` 重新构建，并确认 OCCT/第三方 Runtime DLL 完整。\n- 修改 API、菜单或 Host 后优先执行 `build.ps1 validate`；需要验证真实 OCCT 建模时执行 `build.ps1 smoke`。\n'''
        text = text.replace('\n## 发布\n', trouble + '\n## 发布\n', 1)
    p.write_text(text, encoding='utf-8-sig')

patch('README.md', True)
patch('README.zh-CN.md', False)
