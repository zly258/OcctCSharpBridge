const translations = {
  en: {
    skipContent: 'Skip to content',
    navBranches: 'Branches',
    navPlatforms: 'Platforms',
    navCapabilities: 'Capabilities',
    navDemo: 'Demo',
    navDocs: 'Docs',

    heroEyebrow: 'BRIDGE 3 · ABI5-ONLY',
    heroTitle: 'A focused OCCT bridge for modern .NET desktop CAD applications',
    heroLead: 'One SDK source, one consumer Demo, explicit Windows/Linux boundaries. Strongly typed modeling, topology, exchange and Viewer APIs sit above OCCT 7.9.0 without exposing application-level document architecture.',
    viewMain: 'Explore SDK',
    viewDemo: 'Open Demo',
    contractBridge: 'Bridge',
    contractAbi: 'Native ABI',

    architectureStack: 'Repository architecture',
    currentArchitecture: 'Current architecture',
    mainStack: 'Native Core · OcctNet · WinForms · WPF · Avalonia',
    demoStack: 'Windows: 3 hosts · Linux: Avalonia only',
    websiteStack: 'Bilingual project overview · canonical links · Demo previews',
    panelNote: 'Generated Binary SDK payloads are build/release artifacts, not a second source tree.',

    branchEyebrow: 'REPOSITORY MODEL',
    branchTitle: 'Three responsibilities, no duplicated Bridge implementation',
    branchLead: 'Development and formal branches stay paired, while SDK implementation, consumer applications and the public website remain intentionally separate.',
    mainText: 'The sole Bridge SDK source. It owns Native Core, OcctNet, WinForms/WPF/Avalonia adapters, tests, ABI checks, documentation and platform Binary SDK production.',
    demoBranchText: 'The single SDK consumer application tree. Windows provides WinForms, WPF and Avalonia; Linux provides Avalonia only. Demo consumes validated SDK outputs and does not vendor Bridge implementation source.',
    websiteText: 'A lightweight bilingual static site that presents the current Bridge contract, platform matrix, canonical Demo screenshots, documentation routes and licensing boundaries.',
    roleLabel: 'Role',
    platformLabel: 'Platforms',
    sourceLabel: 'Sources',
    mainRole: 'SDK source of truth',
    demoRole: 'Integration and packaging example',
    websiteRole: 'Project overview and navigation',
    openBranch: 'Open branch ↗',

    platformEyebrow: 'PLATFORM MATRIX',
    platformTitle: 'Platform support is explicit, not inferred',
    platformLead: 'The SDK source is cross-platform x64. Desktop framework availability differs by operating system, so build, run and publish workflows follow the matrix below.',
    matrixArea: 'Area',
    supported: 'Supported',
    notApplicable: '—',
    demoHosts: 'Demo hosts',
    publishFlow: 'Demo publish',
    viewerBackend: 'Viewer backend',
    windowsNoteTitle: 'Windows x64',
    windowsNote: 'WinForms, WPF and Avalonia are independent hosts over the same Bridge SDK. Release publishing keeps the three application packages separate.',
    linuxNoteTitle: 'Linux x64',
    linuxNote: 'Linux builds Core + Avalonia only. The interactive Viewer requires an X11/XWayland DISPLAY; headless modeling does not.',

    capEyebrow: 'BRIDGE CAPABILITIES',
    capTitle: 'OCCT functionality with clear managed ownership',
    capLead: 'Core APIs focus on reusable geometry and Viewer semantics. Application documents, commands, undo/redo, feature trees, snapping, grips and project persistence remain above the Bridge.',
    f1Title: 'Viewer & AIS',
    f1Text: 'Camera, projection, display modes, material/color/transparency, transforms, text, dimensions, points, lighting and redraw batching.',
    f2Title: 'Headless modeling',
    f2Text: 'Primitives, Boolean operations, extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, healing and operation history.',
    f3Title: 'Selection & interaction',
    f3Text: 'Point and rectangle selection, detection, structured object identity, zoom, pan, rotation and screen/world conversion.',
    f4Title: 'Geometry & topology',
    f4Text: 'Evaluation, curvature, projection, distance, adjacency, inertia, intersections, validation, history and persistent topology references.',
    f5Title: 'Meshing',
    f5Text: 'Configurable triangulation with bulk node/triangle transfer and per-face provenance rather than N+1 interop loops.',
    f6Title: 'STEP assembly exchange',
    f6Text: 'XDE-backed import/export preserves hierarchy, occurrences, transforms, visibility, colors and subshape styles through managed snapshots.',
    f7Title: 'Runtime diagnostics',
    f7Text: 'Platform-aware Native probing, exact Bridge/ABI validation and explicit runtime diagnostics for deployment troubleshooting.',
    f8Title: 'ABI contract checks',
    f8Text: 'Source checks keep Native declarations, definitions and managed LibraryImport bindings aligned while rejecting retired ABI4 compatibility artifacts.',

    demoEyebrow: 'DEMO PREVIEWS',
    demoTitle: 'One consumer tree across Windows and Linux',
    demoLead: 'The three screenshots below are canonical assets from the formal demo branch, captured on Windows x64 for the WinForms, WPF and Avalonia hosts.',
    winformsCaption: 'Classic Windows CAD host.',
    wpfCaption: 'Native HWND viewport host.',
    avaloniaWinCaption: 'Cross-platform UI over the Windows Viewer backend.',

    workflowEyebrow: 'CONSUMER WORKFLOW',
    workflowTitle: 'Build the SDK, validate it, then consume it',
    workflowLead: 'Demo synchronization verifies platform contract, source commit and manifest hashes before applications are built or published.',
    flow1Title: 'Build SDK',
    flow1Text: 'Generate the platform Binary SDK from main.',
    flow2Title: 'Synchronize',
    flow2Text: 'Copy the validated SDK into demo/dist/<rid>.',
    flow3Title: 'Build & run',
    flow3Text: 'Compile only the hosts supported by the current OS.',
    flow4Title: 'Publish',
    flow4Text: 'Create independent application packages with runtime closure.',

    docsEyebrow: 'DOCUMENTATION',
    docsTitle: 'Follow the source of truth for the layer you are using',
    docsLead: 'SDK implementation and ABI rules belong to main. Consumer synchronization, platform builds and application publishing belong to demo.',
    mainDocs: 'main · SDK documentation',
    mainDocsNote: 'Architecture, API conventions, runtime, build and deployment',
    demoDocs: 'demo · Consumer documentation',
    demoDocsNote: 'Windows/Linux synchronization, build, run and publish',
    contractDocs: 'Bridge source contract',
    licenseDocs: 'License & exception',
    licenseDocsNote: 'GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0',

    copy: 'Copy',
    copied: 'Copied',

    licenseEyebrow: 'LICENSING',
    licenseTitle: 'Bridge licensing and application linking are documented separately',
    licenseLead: 'OcctCSharpBridge uses GNU LGPL 2.1 with the OcctCSharpBridge Exception 1.0. The exception covers normal runtime linking by commercial and proprietary applications; modifications to the Bridge itself remain governed by the Bridge license.',
    licenseCardTitle: 'Bridge license',
    licenseCardText: 'Terms for OcctCSharpBridge itself and redistributed modifications.',
    exceptionCardTitle: 'Linking exception',
    exceptionCardText: 'Rules for normal .NET references, dynamic linking, P/Invoke and equivalent runtime linking.',
    readLicense: 'Read license ↗',
    readException: 'Read exception ↗',
    footerLead: 'Bridge 3 · OCCT 7.9.0 · .NET 10 · Windows x64 / Linux x64',
    themeLight: 'Light',
    themeDark: 'Dark'
  },

  zh: {
    skipContent: '跳到正文',
    navBranches: '分支',
    navPlatforms: '平台',
    navCapabilities: '能力',
    navDemo: '案例',
    navDocs: '文档',

    heroEyebrow: 'BRIDGE 3 · ABI5-ONLY',
    heroTitle: '面向现代 .NET 桌面 CAD 应用的清晰 OCCT Bridge',
    heroLead: '一套 SDK 源码、一套 Demo Consumer，并明确区分 Windows/Linux 平台边界。Bridge 在 OCCT 7.9.0 之上提供强类型 Modeling、Topology、Exchange 与 Viewer API，同时不侵入上层应用的 Document 架构。',
    viewMain: '查看 SDK',
    viewDemo: '打开 Demo',
    contractBridge: 'Bridge',
    contractAbi: 'Native ABI',

    architectureStack: '仓库架构',
    currentArchitecture: '当前正式架构',
    mainStack: 'Native Core · OcctNet · WinForms · WPF · Avalonia',
    demoStack: 'Windows：3 个 Host · Linux：仅 Avalonia',
    websiteStack: '双语项目说明 · 正式链接 · Demo 预览',
    panelNote: 'Binary SDK 是构建/发布产物，不是第二套源码树。',

    branchEyebrow: '仓库模型',
    branchTitle: '三类职责清晰分离，不复制 Bridge 实现',
    branchLead: '开发分支与正式分支成对维护；SDK 实现、Consumer 应用和公开官网彼此独立，避免平台代码与产品代码混杂。',
    mainText: '唯一 Bridge SDK 源。负责 Native Core、OcctNet、WinForms/WPF/Avalonia Adapter、测试、ABI 契约检查、文档以及各平台 Binary SDK 生产。',
    demoBranchText: '唯一 SDK Consumer 应用树。Windows 提供 WinForms、WPF、Avalonia；Linux 仅提供 Avalonia。Demo 只消费经过校验的 SDK 产物，不复制 Bridge 实现源码。',
    websiteText: '轻量双语静态官网，用于展示当前 Bridge Contract、平台支持矩阵、Demo 正式截图、文档入口与许可证边界。',
    roleLabel: '职责',
    platformLabel: '平台',
    sourceLabel: '信息来源',
    mainRole: 'SDK 唯一事实源',
    demoRole: '集成与发布示例',
    websiteRole: '项目说明与导航',
    openBranch: '打开分支 ↗',

    platformEyebrow: '平台矩阵',
    platformTitle: '平台能力明确列出，不依靠用户猜测',
    platformLead: 'SDK 源码面向跨平台 x64，但桌面 UI Framework 的可用范围不同，因此构建、运行和发布流程必须按平台矩阵执行。',
    matrixArea: '能力',
    supported: '支持',
    notApplicable: '—',
    demoHosts: 'Demo Host',
    publishFlow: 'Demo 发布',
    viewerBackend: 'Viewer Backend',
    windowsNoteTitle: 'Windows x64',
    windowsNote: 'WinForms、WPF、Avalonia 是基于同一 Bridge SDK 的三个独立 Host；Release 发布时仍保持三个独立应用包。',
    linuxNoteTitle: 'Linux x64',
    linuxNote: 'Linux 仅构建 Core + Avalonia。交互 Viewer 需要 X11/XWayland DISPLAY；Headless Modeling 不要求图形桌面。',

    capEyebrow: 'BRIDGE 能力',
    capTitle: 'OCCT 能力完整，Managed 所有权边界明确',
    capLead: 'Core API 聚焦可复用的几何、拓扑、数据交换与 Viewer 语义；Document、Command、Undo/Redo、Feature Tree、捕捉、夹点和项目持久化仍由上层应用负责。',
    f1Title: 'Viewer & AIS',
    f1Text: 'Camera、Projection、Display Mode、Material/Color/Transparency、Transform、Text、Dimension、Point、Lighting 与 Redraw Batching。',
    f2Title: 'Headless Modeling',
    f2Text: 'Primitive、Boolean、Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、Shelling、Healing 与 Operation History。',
    f3Title: 'Selection & Interaction',
    f3Text: '点选/框选、Detection、结构化 Object Identity、Zoom、Pan、Rotate，以及 Screen/World Conversion。',
    f4Title: 'Geometry & Topology',
    f4Text: 'Evaluation、Curvature、Projection、Distance、Adjacency、Inertia、Intersection、Validation、History 与 Persistent Topology Reference。',
    f5Title: 'Meshing',
    f5Text: '可配置 Triangulation，并通过 Bulk Node/Triangle Transfer 与 per-Face Provenance 避免 N+1 Interop。',
    f6Title: 'STEP Assembly Exchange',
    f6Text: '内部使用 XDE 的装配导入/导出，通过 Managed Snapshot 保留 Hierarchy、Occurrence、Transform、Visibility、Color 与 Subshape Style。',
    f7Title: 'Runtime Diagnostics',
    f7Text: '按平台解析 Native Runtime，严格校验 Bridge/ABI 配对，并提供明确的部署诊断信息。',
    f8Title: 'ABI Contract Checks',
    f8Text: '直接检查 Native Declaration、Definition 与 Managed LibraryImport 一致性，同时拒绝已淘汰的 ABI4 兼容残留。',

    demoEyebrow: '案例预览',
    demoTitle: '一套 Consumer 工程覆盖 Windows 与 Linux',
    demoLead: '以下三张截图全部来自正式 demo 分支，分别在 Windows x64 上截取 WinForms、WPF 与 Avalonia 三个 Host。',
    winformsCaption: '经典 Windows CAD Host。',
    wpfCaption: 'Native HWND Viewport Host。',
    avaloniaWinCaption: 'Avalonia UI + Windows Viewer Backend。',

    workflowEyebrow: 'CONSUMER 流程',
    workflowTitle: '先构建 SDK，再校验并由 Demo 消费',
    workflowLead: 'Demo 在构建或发布应用之前，会校验平台 Contract、sourceCommit 与 Manifest Hash，避免误用过期或不匹配的 Binary SDK。',
    flow1Title: '生成 SDK',
    flow1Text: '从 main 生成当前平台 Binary SDK。',
    flow2Title: '同步校验',
    flow2Text: '将验证后的 SDK 同步到 demo/dist/<rid>。',
    flow3Title: '构建运行',
    flow3Text: '只编译当前操作系统支持的 Host。',
    flow4Title: '应用发布',
    flow4Text: '生成相互独立、包含运行时依赖闭包的应用包。',

    docsEyebrow: '文档',
    docsTitle: '按实际使用层级查阅对应事实源',
    docsLead: 'SDK 实现与 ABI 规则以 main 为准；Consumer 同步、平台构建和应用发布以 demo 为准。',
    mainDocs: 'main · SDK 文档',
    mainDocsNote: '架构、API 约定、Runtime、构建与部署',
    demoDocs: 'demo · Consumer 文档',
    demoDocsNote: 'Windows/Linux 同步、构建、运行与发布',
    contractDocs: 'Bridge 源码契约',
    licenseDocs: '许可证与链接例外',
    licenseDocsNote: 'GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0',

    copy: '复制',
    copied: '已复制',

    licenseEyebrow: '许可证',
    licenseTitle: 'Bridge 许可证与应用链接边界分别说明',
    licenseLead: 'OcctCSharpBridge 采用 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0。Exception 覆盖商业及闭源应用的正常运行时链接；对 Bridge 本身的修改仍受 Bridge 许可证约束。',
    licenseCardTitle: 'Bridge 许可证',
    licenseCardText: '适用于 OcctCSharpBridge 本身及其再分发修改版本的条款。',
    exceptionCardTitle: '链接例外',
    exceptionCardText: '适用于 .NET Assembly Reference、Dynamic Linking、P/Invoke 等正常运行时链接方式的规则。',
    readLicense: '查看许可证 ↗',
    readException: '查看链接例外 ↗',
    footerLead: 'Bridge 3 · OCCT 7.9.0 · .NET 10 · Windows x64 / Linux x64',
    themeLight: '浅色',
    themeDark: '深色'
  }
};

const languageToggle = document.getElementById('languageToggle');
const themeToggle = document.getElementById('themeToggle');
const themeLabel = document.getElementById('themeLabel');
const themeMeta = document.querySelector('meta[name="theme-color"]');
const lightbox = document.getElementById('previewLightbox');
const lightboxImage = document.getElementById('previewLightboxImage');
const previewClose = document.getElementById('previewClose');

let language = localStorage.getItem('occt-language');
if (language !== 'zh' && language !== 'en') {
  language = navigator.language?.toLowerCase().startsWith('zh') ? 'zh' : 'en';
}

function currentTheme() {
  return document.documentElement.dataset.theme === 'dark' ? 'dark' : 'light';
}

function updateThemeLabel() {
  if (!themeLabel) return;
  themeLabel.textContent = currentTheme() === 'dark'
    ? translations[language].themeLight
    : translations[language].themeDark;
}

function applyLanguage(next) {
  language = next;
  const t = translations[language];

  document.documentElement.lang = language === 'zh' ? 'zh-CN' : 'en';

  document.querySelectorAll('[data-i18n]').forEach(node => {
    const value = t[node.dataset.i18n];
    if (value !== undefined) node.textContent = value;
  });

  if (languageToggle) languageToggle.textContent = language === 'zh' ? 'EN' : '中文';
  updateThemeLabel();
  localStorage.setItem('occt-language', language);
}

function applyTheme(theme) {
  const value = theme === 'dark' ? 'dark' : 'light';
  document.documentElement.dataset.theme = value;
  themeMeta?.setAttribute('content', value === 'dark' ? '#0a1018' : '#f6f8fb');
  localStorage.setItem('occt-theme', value);
  updateThemeLabel();
}

languageToggle?.addEventListener('click', () => applyLanguage(language === 'zh' ? 'en' : 'zh'));
themeToggle?.addEventListener('click', () => applyTheme(currentTheme() === 'dark' ? 'light' : 'dark'));

document.querySelectorAll('[data-copy-target]').forEach(button => {
  button.addEventListener('click', async () => {
    const target = document.getElementById(button.dataset.copyTarget);
    if (!target) return;

    try {
      await navigator.clipboard.writeText(target.textContent);
      const old = button.textContent;
      button.textContent = translations[language].copied;
      window.setTimeout(() => { button.textContent = old; }, 1200);
    } catch {
      // Clipboard access may be unavailable on non-secure local previews.
    }
  });
});

function openPreview(img) {
  if (!lightbox || !lightboxImage) return;
  lightboxImage.src = img.src;
  lightboxImage.alt = img.alt;
  lightbox.hidden = false;
  lightbox.setAttribute('aria-hidden', 'false');
  document.body.style.overflow = 'hidden';
  previewClose?.focus();
}

function closePreview() {
  if (!lightbox) return;
  lightbox.hidden = true;
  lightbox.setAttribute('aria-hidden', 'true');
  document.body.style.overflow = '';
}

document.querySelectorAll('.preview-card img').forEach(img => {
  img.tabIndex = 0;
  img.setAttribute('role', 'button');
  img.addEventListener('click', () => openPreview(img));
  img.addEventListener('keydown', event => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      openPreview(img);
    }
  });
});

previewClose?.addEventListener('click', closePreview);
lightbox?.addEventListener('click', event => {
  if (event.target === lightbox) closePreview();
});
document.addEventListener('keydown', event => {
  if (event.key === 'Escape' && lightbox && !lightbox.hidden) closePreview();
});

applyTheme(currentTheme());
applyLanguage(language);
