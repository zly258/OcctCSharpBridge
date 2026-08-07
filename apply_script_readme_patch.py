from pathlib import Path

for path, zh in [('README.md', False), ('README.zh-CN.md', True)]:
    p = Path(path)
    text = p.read_text(encoding='utf-8-sig')
    if (not zh and '## Clone and configure' in text) or (zh and '## 从克隆开始' in text):
        continue
    if zh:
        anchor = '## 环境要求\n\n'
        block = '''## 从克隆开始\n\n```powershell\ngit clone https://github.com/zly258/OcctCSharpBridge.git\ncd OcctCSharpBridge\ngit switch script\n$env:OCCT_ROOT = "D:\\\\tools\\\\occt-vc144-64"\n```\n\n`script` 分支不是 Demo 分支，它提供 `OcctScript.Editor` 参数化编辑器。第一次使用建议先执行完整 `script` target，再启动 Editor。\n\n### 脚本使用速查\n\n| 命令 | 用途 |\n| --- | --- |\n| `.\\build.ps1 managed Release` | 只构建可复用托管 Bridge/Host |\n| `.\\build.ps1 script Release -OcctRoot <path>` | 校核 Bridge，构建 Native、OcctScript 各层、Editor，并运行 Script Smoke |\n| `.\\run.ps1` | 启动已经构建好的 Editor |\n| `.\\run.ps1 Release -OcctRoot <path>` | 指定 OCCT 路径启动 Editor |\n| `.\\run.ps1 Release -OcctRoot <path> -Build` | 先执行完整 script 构建，再启动 Editor |\n\n`run.ps1` 默认不会重新构建；需要保证输出目录与当前源码一致，或者使用 `-Build`。\n\n'''
    else:
        anchor = '## Requirements\n\n'
        block = '''## Clone and configure\n\n```powershell\ngit clone https://github.com/zly258/OcctCSharpBridge.git\ncd OcctCSharpBridge\ngit switch script\n$env:OCCT_ROOT = "D:\\\\tools\\\\occt-vc144-64"\n```\n\nThe `script` branch is not the desktop demo branch. It provides the `OcctScript.Editor` parametric application. For a first run, build the complete `script` target before starting the editor.\n\n### Script quick reference\n\n| Command | Purpose |\n| --- | --- |\n| `.\\build.ps1 managed Release` | Build only the reusable managed bridge/hosts |\n| `.\\build.ps1 script Release -OcctRoot <path>` | Validate bridge contracts, build native + OcctScript layers + Editor, and run Script Smoke |\n| `.\\run.ps1` | Start an already-built Editor |\n| `.\\run.ps1 Release -OcctRoot <path>` | Start the Editor with an explicit OCCT root |\n| `.\\run.ps1 Release -OcctRoot <path> -Build` | Build the full script target first, then launch the Editor |\n\n`run.ps1` normally does not rebuild. Keep the output synchronized with the current source or use `-Build`.\n\n'''
    if anchor not in text:
        raise RuntimeError(f'anchor not found in {path}')
    text = text.replace(anchor, block + anchor, 1)
    p.write_text(text, encoding='utf-8-sig')
