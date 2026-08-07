(() => {
  const translations = {
    en: {
      navFeatures: 'Capabilities',
      navArchitecture: 'Architecture',
      navDemo: 'Demo',
      navStart: 'Get Started',
      heroTitle: 'A lightweight OCCT bridge for .NET',
      heroLead: 'Connect Open CASCADE Technology 7.9.0 to .NET 8 through a stable C ABI. The bridge covers 3D Viewer, AIS interaction, headless modeling, topology analysis, meshing and engineering file exchange while keeping application documents separate from the geometry kernel.',
      viewRepository: 'View Repository',
      viewDemo: 'View Demo',
      stackApp: 'Your CAD / BIM Application',
      stackAppSub: 'Document · Command · Undo/Redo · JSON',
      stackManaged: 'Type-safe .NET API',
      statNative: 'Native C ABI',
      statPInvoke: 'P/Invoke mappings',
      statTypes: 'Public .NET types',
      statVersion: 'Bridge version',
      featuresEyebrow: 'CAPABILITIES',
      featuresTitle: 'Core building blocks for CAD and engineering software',
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
      f5Text: 'Topology traversal, analytic geometry, derivatives, curvature, mass properties, projection, ray tests, distance and mesh data.',
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
      p3Text: 'With EnableDefaultInteraction=false, hosts can expose raw input for custom tools, snapping, dynamic previews and work planes.',
      p4Title: 'Transformed spatial state stays consistent',
      p4Text: 'Fit, scene bounds and positioning use presentation-transformed geometry, so moved objects remain spatially correct to the camera.',
      demoEyebrow: 'DESKTOP HOSTS',
      demoTitle: 'One bridge, multiple desktop hosts',
      demoLead: 'The demo branch validates CAD interaction, localization and packaging across WinForms, WPF and Avalonia while main remains focused and reusable.',
      winformsCaption: 'Classic CAD desktop layout with a native HWND viewport',
      wpfCaption: 'WPF application built around OcctWpfViewport',
      avaloniaCaption: 'Full CAD demo using the native Avalonia HWND host',
      branchesEyebrow: 'BRANCHES',
      branchesTitle: 'Clear responsibilities across branches',
      branchesLead: 'Keep the reusable bridge core, desktop demos and project website in clearly separated branches.',
      branchMain: 'Reusable OCCT C++ / C# bridge core',
      branchDemo: 'WinForms, WPF and Avalonia demos',
      branchWebsite: 'Static project website and GitHub Pages source',
      startEyebrow: 'GET STARTED',
      startTitle: 'Clone, build, run and publish',
      startLead: 'Start from a fresh clone, switch to the demo branch, configure OCCT_ROOT, then use the PowerShell scripts for validation, build, run, smoke testing and packaging.',
      enInventory: 'English API Inventory ↗',
      cnInventory: 'Chinese API Inventory ↗',
      copy: 'Copy',
      copied: 'Copied',
      aboutEyebrow: 'PROJECT',
      aboutText: 'A reusable C# bridge for Open CASCADE Technology, focused on a stable and explicit boundary between .NET applications and OCCT.',
      author: 'Author',
      license: 'License',
      footerText: 'Built for OCCT 7.9.0 · .NET 8 · Windows x64'
    },
    zh: {
      navFeatures: '能力',
      navArchitecture: '架构',
      navDemo: '演示',
      navStart: '开始使用',
      heroTitle: '面向 .NET 的轻量级 OCCT 桥接层',
      heroLead: '通过稳定 C ABI 将 Open CASCADE Technology 7.9.0 接入 .NET 8，覆盖三维 Viewer、AIS 交互、无窗口建模、拓扑分析、网格与工程文件交换，同时保持应用文档机制与几何内核解耦。',
      viewRepository: '查看仓库',
      viewDemo: '查看 Demo',
      stackApp: '你的 CAD / BIM 应用',
      stackAppSub: '文档 · Command · Undo/Redo · JSON',
      stackManaged: '类型安全 .NET API',
      statNative: 'Native C ABI',
      statPInvoke: 'P/Invoke 映射',
      statTypes: '公开 .NET 类型',
      statVersion: 'Bridge 版本',
      featuresEyebrow: 'CAPABILITIES',
      featuresTitle: '面向 CAD 与工程软件的核心构建能力',
      featuresLead: '桥接层聚焦几何、显示与交互，把业务对象、文档生命周期和持久化留给上层应用。',
      f1Title: 'Viewer 与 AIS',
      f1Text: '相机、投影、显示模式、材质、颜色、透明度、尺寸、文字、Fit、批量更新与高亮。',
      f2Title: '无窗口建模',
      f2Text: '基础体、布尔、拉伸、旋转、扫掠、放样、圆角、倒角、偏移、抽壳、修复与历史关系。',
      f3Title: 'CAD 选择与输入',
      f3Text: '点选、框选、选择集同步、可选择状态、原始输入转发，以及可切换的默认交互。',
      f4Title: '稳定对象关联',
      f4Text: 'ApplicationTag 将应用层 EntityId 与 Viewer 对象稳定关联，几何可原位更新而不破坏显示状态。',
      f5Title: '几何查询与分析',
      f5Text: '拓扑遍历、解析几何、导数、曲率、质量属性、投影、射线、距离和网格数据。',
      f6Title: '工程文件交换',
      f6Text: '面向 Shape 的 STEP、IGES、BREP、STL 导入导出，不把桥接层耦合到应用文档模型。',
      archEyebrow: 'DESIGN',
      archTitle: '清晰的架构边界',
      archLead: '不使用 OCAF/XDE 承担应用文档职责。桥接层保持通用，上层可自由实现 AutoCAD 风格文档、撤销重做与 JSON 持久化。',
      p1Title: '应用对象不依赖 Viewer ID',
      p1Text: 'ApplicationTag 提供稳定关联，即使 Viewer 对象被重建也不影响业务实体身份。',
      p2Title: '几何更新不重建显示对象',
      p2Text: 'UpdateShape 原位替换 TopoDS_Shape，并保留外观、局部变换、选择和可选择状态。',
      p3Title: '应用可以完全接管交互',
      p3Text: '关闭默认交互后，宿主可提供原始输入，用于自定义 Tool、捕捉、动态预览与工作平面。',
      p4Title: '变换后的空间状态保持一致',
      p4Text: 'Fit、场景包围盒和定位逻辑基于显示变换后的几何，对象移动后镜头状态仍保持正确。',
      demoEyebrow: 'DESKTOP HOSTS',
      demoTitle: '同一桥接层，多种桌面宿主',
      demoLead: 'demo 分支验证 WinForms、WPF 与 Avalonia 的 CAD 交互、本地化和构建发布；main 继续保持纯净可复用。',
      winformsCaption: '经典 CAD 桌面布局与原生 HWND 视口',
      wpfCaption: '基于 OcctWpfViewport 的 WPF 应用',
      avaloniaCaption: '使用 Avalonia 原生 HWND 宿主的完整 CAD Demo',
      branchesEyebrow: 'BRANCHES',
      branchesTitle: '不同分支保持清晰职责',
      branchesLead: '将可复用桥接核心、桌面 Demo 与项目网站分别维护，保持分支边界清晰。',
      branchMain: '可复用 OCCT C++ / C# 桥接核心',
      branchDemo: 'WinForms、WPF 与 Avalonia 示例',
      branchWebsite: '静态项目网站与 GitHub Pages 源码',
      startEyebrow: 'GET STARTED',
      startTitle: '从克隆开始：构建、运行与发布',
      startLead: '建议从全新克隆开始，切换到 demo 分支，配置 OCCT_ROOT，再分别使用 PowerShell 脚本完成校核、构建、运行、Smoke 测试和发布。',
      enInventory: '英文接口清单 ↗',
      cnInventory: '中文接口清单 ↗',
      copy: '复制',
      copied: '已复制',
      aboutEyebrow: 'PROJECT',
      aboutText: '面向 Open CASCADE Technology 的可复用 C# 桥接层，强调 .NET 应用与 OCCT 之间稳定、明确的集成边界。',
      author: 'Author',
      license: '许可证',
      footerText: 'Built for OCCT 7.9.0 · .NET 8 · Windows x64'
    }
  };
  const languageToggle = document.getElementById('languageToggle');
  const copyButton = document.getElementById('copyCode');
  const buildCode = document.getElementById('buildCode');
  const previews = [
    document.getElementById('winformsPreview'),
    document.getElementById('wpfPreview'),
    document.getElementById('avaloniaPreview')
  ].filter(Boolean);

  const storageKey = 'occt-website-language-v2';
  let language = localStorage.getItem(storageKey) === 'zh' ? 'zh' : 'en';

  function placeholderSource(image) {
    const host = image.closest('figure')?.querySelector('figcaption strong')?.textContent?.trim() || 'Demo';
    const pending = language === 'zh' ? '预览图待上传' : 'Preview pending';
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="1000" viewBox="0 0 1600 1000"><rect width="1600" height="1000" fill="#eef2f7"/><rect x="40" y="40" width="1520" height="920" rx="24" fill="#f8fafc" stroke="#cbd5e1" stroke-width="2"/><text x="800" y="470" text-anchor="middle" font-family="Segoe UI,Arial,sans-serif" font-size="46" font-weight="600" fill="#334155">${host}</text><text x="800" y="535" text-anchor="middle" font-family="Segoe UI,Arial,sans-serif" font-size="24" fill="#64748b">${pending}</text></svg>`;
    return `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}`;
  }

  function setPreviewLanguage(image) {
    image.dataset.fallbackUsed = 'false';
    image.dataset.placeholderUsed = 'false';
    image.alt = language === 'zh' ? (image.dataset.altZh || image.alt) : (image.dataset.altEn || image.alt);
    const source = language === 'zh' ? image.dataset.srcZh : image.dataset.srcEn;
    if (source) image.src = source;
  }

  previews.forEach((image) => {
    image.addEventListener('error', () => {
      if (image.dataset.placeholderUsed === 'true') return;
      const fallback = language === 'zh' ? image.dataset.fallbackZh : image.dataset.fallbackEn;
      if (fallback && image.dataset.fallbackUsed !== 'true') {
        image.dataset.fallbackUsed = 'true';
        image.src = fallback;
        return;
      }
      image.dataset.placeholderUsed = 'true';
      image.src = placeholderSource(image);
    });
  });

  function applyLanguage(nextLanguage) {
    language = nextLanguage === 'zh' ? 'zh' : 'en';
    const dictionary = translations[language];

    document.documentElement.lang = language === 'zh' ? 'zh-CN' : 'en';

    document.querySelectorAll('[data-i18n]').forEach((element) => {
      const key = element.dataset.i18n;
      if (dictionary[key]) element.textContent = dictionary[key];
    });

    previews.forEach(setPreviewLanguage);

    languageToggle.textContent = language === 'zh' ? 'EN' : '中文';
    languageToggle.setAttribute('aria-label', language === 'zh' ? 'Switch to English' : 'Switch to Chinese');
    localStorage.setItem(storageKey, language);
  }

  languageToggle.addEventListener('click', () => {
    applyLanguage(language === 'zh' ? 'en' : 'zh');
  });

  copyButton.addEventListener('click', async () => {
    try {
      await navigator.clipboard.writeText(buildCode.textContent);
      copyButton.textContent = translations[language].copied;
      window.setTimeout(() => {
        copyButton.textContent = translations[language].copy;
      }, 1200);
    } catch {
      const range = document.createRange();
      range.selectNodeContents(buildCode);
      const selection = window.getSelection();
      selection.removeAllRanges();
      selection.addRange(range);
    }
  });

  const previewImages = Array.from(document.querySelectorAll('.preview-card img'));
  if (previewImages.length) {
    const lightbox = document.createElement('div');
    lightbox.className = 'image-lightbox';
    lightbox.setAttribute('role', 'dialog');
    lightbox.setAttribute('aria-modal', 'true');
    lightbox.setAttribute('aria-label', 'Image preview');
    lightbox.hidden = true;
    lightbox.innerHTML = `
      <button class="image-lightbox-close" type="button" aria-label="Close image preview">×</button>
      <img class="image-lightbox-image" alt="" />
      <div class="image-lightbox-caption"></div>`;
    document.body.appendChild(lightbox);

    const lightboxImage = lightbox.querySelector('.image-lightbox-image');
    const lightboxCaption = lightbox.querySelector('.image-lightbox-caption');
    const lightboxClose = lightbox.querySelector('.image-lightbox-close');
    let lastFocusedImage = null;

    const closeLightbox = () => {
      if (lightbox.hidden) return;
      lightbox.hidden = true;
      document.body.classList.remove('lightbox-open');
      lightboxImage.removeAttribute('src');
      if (lastFocusedImage) lastFocusedImage.focus();
    };

    const openLightbox = (image) => {
      lastFocusedImage = image;
      lightboxImage.src = image.currentSrc || image.src;
      lightboxImage.alt = image.alt || '';
      const caption = image.closest('figure')?.querySelector('figcaption')?.innerText?.trim() || image.alt || '';
      lightboxCaption.textContent = caption;
      lightbox.hidden = false;
      document.body.classList.add('lightbox-open');
      lightboxClose.focus();
    };

    previewImages.forEach((image) => {
      image.classList.add('zoomable-image');
      image.tabIndex = 0;
      image.setAttribute('role', 'button');
      image.setAttribute('aria-label', `${image.alt || 'Preview image'} — open full size`);
      image.addEventListener('click', () => openLightbox(image));
      image.addEventListener('keydown', (event) => {
        if (event.key === 'Enter' || event.key === ' ') {
          event.preventDefault();
          openLightbox(image);
        }
      });
    });

    lightboxClose.addEventListener('click', closeLightbox);
    lightbox.addEventListener('click', (event) => {
      if (event.target === lightbox) closeLightbox();
    });
    document.addEventListener('keydown', (event) => {
      if (event.key === 'Escape' && !lightbox.hidden) closeLightbox();
    });
  }

  applyLanguage(language);
})();