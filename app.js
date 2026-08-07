(() => {
  const translations = {
    zh: {
      navFeatures: '能力',
      navArchitecture: '架构',
      navDemo: '演示',
      navStart: '开始使用',
      heroEyebrow: 'OPEN CASCADE × .NET',
      heroTitle: '面向 .NET 的轻量级 OCCT 桥接层',
      heroLead: '以稳定 C ABI 连接 Open CASCADE Technology 7.9.0 与 .NET 8，覆盖三维 Viewer、AIS 交互、无窗口建模、拓扑分析、网格与工程文件交换，同时保持应用文档机制与几何内核解耦。',
      viewRepository: '查看仓库',
      viewDemo: '查看 Demo',
      stackApp: '你的 CAD / BIM 应用',
      stackAppSub: '文档 · Command · Undo/Redo · JSON',
      stackManaged: '类型安全 .NET API',
      headless: '无窗口建模',
      statNative: 'Native C ABI',
      statPInvoke: 'P/Invoke 映射',
      statTypes: '公开 .NET 类型',
      statVersion: 'Bridge 版本',
      featuresEyebrow: 'CAPABILITIES',
      featuresTitle: '为轻量 CAD 与工程软件准备的核心能力',
      featuresLead: '桥接层只解决几何、显示与交互问题，把业务对象、文档生命周期和持久化留给上层应用。',
      f1Title: 'Viewer 与 AIS',
      f1Text: '相机、投影、显示模式、材质、颜色、透明度、尺寸、文字、Fit、批量更新与高亮。',
      f2Title: '无窗口建模',
      f2Text: '基础体、布尔、拉伸、旋转、扫掠、放样、圆角、倒角、偏移、抽壳、修复与历史关系。',
      f3Title: 'CAD 选择与交互',
      f3Text: '点选、框选、选择集同步、可选择状态、原始输入转发，以及可关闭的默认视口交互。',
      f4Title: '稳定对象关联',
      f4Text: 'ApplicationTag 将应用层 EntityId 与 Viewer 对象稳定关联；几何可原位更新而不破坏显示状态。',
      f5Title: '几何查询与分析',
      f5Text: '拓扑遍历、解析几何参数、曲线/曲面导数、曲率、质量属性、投影、射线、距离与网格。',
      f6Title: '工程文件交换',
      f6Text: '面向纯 Shape 的 STEP、IGES、BREP、STL 导入导出，避免把应用文档模型耦合到桥接层。',
      archEyebrow: 'DESIGN',
      archTitle: '明确的分层边界',
      archLead: '不使用 OCAF/XDE 承担应用文档职责。桥接层保持通用，上层可以自由实现 AutoCAD 风格文档、撤销重做和 JSON 持久化。',
      p1Title: '应用对象不依赖 Viewer ID',
      p1Text: '使用 ApplicationTag 建立稳定关联，Viewer 对象删除重建也不会改变业务实体身份。',
      p2Title: '几何更新不重建显示对象',
      p2Text: 'UpdateShape 原位替换 AIS_Shape，保留外观、局部变换、选择和可选择状态。',
      p3Title: '应用可以完全接管交互',
      p3Text: 'EnableDefaultInteraction=false 后，宿主只转发原始输入，适合自定义 Tool、捕捉、动态预览和工作平面。',
      p4Title: '转换后的空间状态保持一致',
      p4Text: 'Fit、场景包围盒和定位逻辑基于显示变换后的几何，避免对象移动后镜头仍回到原位置。',
      demoEyebrow: 'WINFORMS / WPF',
      demoTitle: '同一桥接层，两套桌面 Demo',
      demoLead: 'Demo 分支用于验证完整 CAD 交互、菜单、本地化、API Center 与发布流程；main 继续保持纯净可复用。',
      winformsCaption: '经典 CAD 桌面布局与原生 HWND 视口',
      wpfCaption: '复用 OcctWpfViewport 的 WPF 应用',
      branchesEyebrow: 'BRANCHES',
      branchesTitle: '按职责拆分，而不是把所有内容堆在 main',
      branchMain: '可复用 OCCT C++ / C# 桥接核心',
      branchDemo: 'WinForms / WPF 完整示例与发布工具',
      branchScript: '参数化脚本方向的独立实验分支',
      branchWebsite: '本项目静态介绍站点与 GitHub Pages 源码',
      startEyebrow: 'GET STARTED',
      startTitle: '从验证接口到构建项目',
      startLead: '项目目标环境为 Windows x64、.NET 8 与精确版本 OCCT 7.9.0。',
      cnInventory: '中文接口清单 ↗',
      enInventory: 'English API Inventory ↗',
      copy: '复制',
      copied: '已复制',
      aboutEyebrow: 'PROJECT',
      aboutTitle: 'OcctCSharpBridge',
      aboutText: '专注于把 OCCT 几何与 Viewer 能力以稳定、清晰、可复用的方式带到 .NET。',
      author: '作者',
      license: '许可证',
      footerText: 'Built for OCCT 7.9.0 · .NET 8 · Windows x64'
    },
    en: {
      navFeatures: 'Capabilities',
      navArchitecture: 'Architecture',
      navDemo: 'Demo',
      navStart: 'Get Started',
      heroEyebrow: 'OPEN CASCADE × .NET',
      heroTitle: 'A lightweight OCCT bridge for .NET',
      heroLead: 'Connect Open CASCADE Technology 7.9.0 to .NET 8 through a stable C ABI. The bridge covers 3D Viewer, AIS interaction, headless modeling, topology analysis, meshing and engineering file exchange while keeping application documents separate from the geometry kernel.',
      viewRepository: 'View Repository',
      viewDemo: 'View Demo',
      stackApp: 'Your CAD / BIM Application',
      stackAppSub: 'Document · Command · Undo/Redo · JSON',
      stackManaged: 'Type-safe .NET API',
      headless: 'Headless Modeling',
      statNative: 'Native C ABI',
      statPInvoke: 'P/Invoke mappings',
      statTypes: 'Public .NET types',
      statVersion: 'Bridge version',
      featuresEyebrow: 'CAPABILITIES',
      featuresTitle: 'Core building blocks for lightweight CAD and engineering software',
      featuresLead: 'The bridge focuses on geometry, visualization and interaction, leaving domain entities, document lifecycle and persistence to the consuming application.',
      f1Title: 'Viewer & AIS',
      f1Text: 'Camera, projection, display modes, materials, color, transparency, dimensions, text, fitting, batching and highlighting.',
      f2Title: 'Headless Modeling',
      f2Text: 'Primitives, Boolean operations, extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, healing and history.',
      f3Title: 'CAD Selection & Input',
      f3Text: 'Point and rectangle selection, application-driven selection sets, selectability, raw input forwarding and switchable default interaction.',
      f4Title: 'Stable Object Identity',
      f4Text: 'ApplicationTag binds application EntityIds to Viewer objects, while geometry can be updated in place without losing presentation state.',
      f5Title: 'Geometry Queries & Analysis',
      f5Text: 'Topology traversal, analytic geometry, curve/surface derivatives, curvature, mass properties, projection, ray tests, distance and mesh data.',
      f6Title: 'Engineering Exchange',
      f6Text: 'Shape-oriented STEP, IGES, BREP and STL import/export without coupling the bridge to an application document model.',
      archEyebrow: 'DESIGN',
      archTitle: 'Clear architectural boundaries',
      archLead: 'OCAF/XDE is not used as the application document layer. The bridge stays generic, while consumers are free to implement AutoCAD-style documents, undo/redo and JSON persistence.',
      p1Title: 'Application entities do not depend on Viewer IDs',
      p1Text: 'ApplicationTag provides a stable association even when Viewer objects are recreated.',
      p2Title: 'Geometry updates do not rebuild presentations',
      p2Text: 'UpdateShape replaces the TopoDS_Shape in place while preserving appearance, local transformation, selection and selectability.',
      p3Title: 'Applications can fully own interaction',
      p3Text: 'With EnableDefaultInteraction=false, hosts forward raw input without built-in pan/orbit/zoom/selection, enabling custom tools, snapping and dynamic previews.',
      p4Title: 'Transformed spatial state stays consistent',
      p4Text: 'Fit, scene bounds and positioning use presentation-transformed geometry, so moved objects remain spatially correct to the camera.',
      demoEyebrow: 'WINFORMS / WPF',
      demoTitle: 'One bridge, two desktop demos',
      demoLead: 'The demo branch validates full CAD interaction, menus, localization, API Center and packaging while main remains focused and reusable.',
      winformsCaption: 'Classic CAD desktop layout with a native HWND viewport',
      wpfCaption: 'WPF application built around OcctWpfViewport',
      branchesEyebrow: 'BRANCHES',
      branchesTitle: 'Separate responsibilities instead of putting everything on main',
      branchMain: 'Reusable OCCT C++ / C# bridge core',
      branchDemo: 'Complete WinForms / WPF demos and publishing tools',
      branchScript: 'Independent parametric scripting experiment branch',
      branchWebsite: 'Static project website and GitHub Pages source',
      startEyebrow: 'GET STARTED',
      startTitle: 'Validate the API, then build',
      startLead: 'Target environment: Windows x64, .NET 8 and exactly OCCT 7.9.0.',
      cnInventory: '中文接口清单 ↗',
      enInventory: 'English API Inventory ↗',
      copy: 'Copy',
      copied: 'Copied',
      aboutEyebrow: 'PROJECT',
      aboutTitle: 'OcctCSharpBridge',
      aboutText: 'Focused on bringing OCCT geometry and Viewer capabilities to .NET through a stable, clear and reusable bridge.',
      author: 'Author',
      license: 'License',
      footerText: 'Built for OCCT 7.9.0 · .NET 8 · Windows x64'
    }
  };

  const languageToggle = document.getElementById('languageToggle');
  const copyButton = document.getElementById('copyCode');
  const buildCode = document.getElementById('buildCode');
  const previews = [document.getElementById('winformsPreview'), document.getElementById('wpfPreview')];

  let language = localStorage.getItem('occt-website-language') || (navigator.language.toLowerCase().startsWith('zh') ? 'zh' : 'en');

  function applyLanguage(nextLanguage) {
    language = nextLanguage === 'en' ? 'en' : 'zh';
    const dictionary = translations[language];
    document.documentElement.lang = language === 'zh' ? 'zh-CN' : 'en';
    document.querySelectorAll('[data-i18n]').forEach((element) => {
      const key = element.dataset.i18n;
      if (dictionary[key]) element.textContent = dictionary[key];
    });
    previews.forEach((image) => {
      const source = language === 'zh' ? image.dataset.srcZh : image.dataset.srcEn;
      if (source && image.src !== source) image.src = source;
    });
    languageToggle.textContent = language === 'zh' ? 'EN' : '中文';
    languageToggle.setAttribute('aria-label', language === 'zh' ? 'Switch to English' : '切换到中文');
    localStorage.setItem('occt-website-language', language);
  }

  languageToggle.addEventListener('click', () => applyLanguage(language === 'zh' ? 'en' : 'zh'));

  copyButton.addEventListener('click', async () => {
    try {
      await navigator.clipboard.writeText(buildCode.textContent);
      copyButton.textContent = translations[language].copied;
      setTimeout(() => { copyButton.textContent = translations[language].copy; }, 1200);
    } catch {
      const range = document.createRange();
      range.selectNodeContents(buildCode);
      const selection = window.getSelection();
      selection.removeAllRanges();
      selection.addRange(range);
    }
  });

  applyLanguage(language);
})();
