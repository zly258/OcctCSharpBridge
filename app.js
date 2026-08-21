const translations = {
  en: {
    skipContent: 'Skip to content',
    navBranches: 'Branches',
    navPlatforms: 'Platforms',
    navCapabilities: 'Capabilities',
    navDemo: 'Demo',
    navDocs: 'Docs',

    heroEyebrow: 'BRIDGE 3 · ABI5-ONLY',
    heroTitle: 'A focused OCCT bridge for .NET desktop CAD',
    heroLead: 'One SDK, one Demo, clear Windows/Linux boundaries — typed modeling, topology, exchange and Viewer APIs over OCCT 7.9.0.',
    viewMain: 'SDK',
    viewDemo: 'Demo',
    contractBridge: 'Bridge',
    contractAbi: 'Native ABI',

    branchEyebrow: 'REPOSITORY',
    branchTitle: 'Three responsibilities, one Bridge',
    branchLead: 'SDK, consumer app and website stay separate — never duplicating the Bridge implementation.',
    mainText: 'The sole Bridge SDK source: Native Core, OcctNet and the WinForms/WPF/Avalonia adapters.',
    demoBranchText: 'The single consumer app. Windows ships three hosts; Linux ships Avalonia only.',
    websiteText: 'This bilingual static site: contract, matrix, previews and docs.',
    openBranch: 'Open ↗',

    platformEyebrow: 'PLATFORM MATRIX',
    platformTitle: 'Platform support, explicit',
    platformLead: 'Cross-platform x64 SDK; desktop frameworks differ by OS.',
    matrixArea: 'Area',
    supported: 'Yes',
    notApplicable: '—',
    demoHosts: 'Demo hosts',
    publishFlow: 'Publish',
    viewerBackend: 'Viewer',
    windowsNoteTitle: 'Windows',
    windowsNote: 'WinForms, WPF and Avalonia are independent hosts over one Bridge SDK.',
    linuxNoteTitle: 'Linux',
    linuxNote: 'Core + Avalonia only. The Viewer needs X11/XWayland; headless modeling does not.',

    capEyebrow: 'CAPABILITIES',
    capTitle: 'What the Bridge covers',
    capLead: 'Reusable geometry and Viewer semantics — app concerns stay above the Bridge.',
    f1Title: 'Viewer & AIS',
    f2Title: 'Headless modeling',
    f3Title: 'Selection & interaction',
    f4Title: 'Geometry & topology',
    f5Title: 'Meshing',
    f6Title: 'STEP assembly exchange',
    f7Title: 'Runtime diagnostics',
    f8Title: 'ABI contract checks',

    demoEyebrow: 'DEMO',
    demoTitle: 'Demo previews',
    demoLead: 'Canonical screenshots from the demo branch, on Windows x64.',

    workflowEyebrow: 'WORKFLOW',
    workflowTitle: 'Build, validate, consume',
    workflowLead: 'Demo verifies the contract before building or publishing.',
    flow1Title: 'Build SDK',
    flow1Text: 'Generate the Binary SDK from main.',
    flow2Title: 'Sync',
    flow2Text: 'Copy the SDK into demo/dist/<rid>.',
    flow3Title: 'Build & run',
    flow3Text: 'Compile the hosts for the current OS.',
    flow4Title: 'Publish',
    flow4Text: 'Package apps with their runtime closure.',

    docsEyebrow: 'DOCS',
    docsTitle: 'Documentation',
    docsLead: 'Read the source of truth for your layer.',
    mainDocs: 'SDK docs',
    mainDocsNote: 'Architecture, API, build, deploy',
    demoDocs: 'Demo docs',
    demoDocsNote: 'Sync, build, run, publish',
    contractDocs: 'Bridge contract',
    licenseDocs: 'License',
    licenseDocsNote: 'LGPL 2.1 + Exception 1.0',

    copy: 'Copy',
    copied: 'Copied',

    licenseEyebrow: 'LICENSE',
    licenseTitle: 'Licensing',
    licenseLead: 'LGPL 2.1 with the OcctCSharpBridge linking Exception 1.0.',
    licenseCardTitle: 'Bridge license',
    licenseCardText: 'Terms for the project and redistributed modifications.',
    exceptionCardTitle: 'Linking exception',
    exceptionCardText: 'Covers normal .NET and P/Invoke runtime linking.',
    readLicense: 'Read ↗',
    readException: 'Read ↗',
    footerLead: 'OCCT 7.9.0 · .NET 10 · Windows / Linux',
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
    heroTitle: '面向现代 .NET 桌面 CAD 的清晰 OCCT Bridge',
    heroLead: '一套 SDK、一套 Demo，明确区分 Windows/Linux 边界——在 OCCT 7.9.0 之上提供强类型建模、拓扑、交换与 Viewer API。',
    viewMain: 'SDK',
    viewDemo: 'Demo',
    contractBridge: 'Bridge',
    contractAbi: 'Native ABI',

    branchEyebrow: '仓库模型',
    branchTitle: '三类职责清晰分离，不复制 Bridge 实现',
    branchLead: 'SDK、Consumer 应用与官网彼此独立，绝不重复 Bridge 实现。',
    mainText: '唯一 Bridge SDK 源：Native Core、OcctNet 及 WinForms/WPF/Avalonia Adapter。',
    demoBranchText: '唯一 Consumer 应用树。Windows 提供三个 Host；Linux 仅提供 Avalonia。',
    websiteText: '轻量双语静态官网：契约、矩阵、预览与文档。',
    openBranch: '打开 ↗',

    platformEyebrow: '平台矩阵',
    platformTitle: '平台能力明确列出',
    platformLead: 'SDK 源码跨平台 x64；桌面框架按操作系统区分。',
    matrixArea: '能力',
    supported: '支持',
    notApplicable: '—',
    demoHosts: 'Demo Host',
    publishFlow: '发布',
    viewerBackend: 'Viewer',
    windowsNoteTitle: 'Windows',
    windowsNote: 'WinForms、WPF、Avalonia 是基于同一 Bridge SDK 的独立 Host。',
    linuxNoteTitle: 'Linux',
    linuxNote: '仅 Core + Avalonia。交互 Viewer 需要 X11/XWayland；Headless 建模不需要。',

    capEyebrow: 'BRIDGE 能力',
    capTitle: 'Bridge 覆盖的能力',
    capLead: '可复用的几何与 Viewer 语义；应用层职责仍由上层负责。',
    f1Title: 'Viewer & AIS',
    f2Title: 'Headless 建模',
    f3Title: '选择与交互',
    f4Title: '几何与拓扑',
    f5Title: '网格剖分',
    f6Title: 'STEP 装配交换',
    f7Title: '运行时诊断',
    f8Title: 'ABI 契约检查',

    demoEyebrow: '案例',
    demoTitle: 'Demo 预览',
    demoLead: '来自 demo 分支的正式截图，均截取于 Windows x64。',

    workflowEyebrow: 'CONSUMER 流程',
    workflowTitle: '先构建、再校验、后消费',
    workflowLead: 'Demo 在构建或发布前校验契约。',
    flow1Title: '生成 SDK',
    flow1Text: '从 main 生成当前平台 Binary SDK。',
    flow2Title: '同步',
    flow2Text: '将 SDK 同步到 demo/dist/<rid>。',
    flow3Title: '构建运行',
    flow3Text: '只编译当前操作系统支持的 Host。',
    flow4Title: '应用发布',
    flow4Text: '生成包含运行时依赖闭包的应用包。',

    docsEyebrow: '文档',
    docsTitle: '文档',
    docsLead: '按实际使用层级查阅对应事实源。',
    mainDocs: 'SDK 文档',
    mainDocsNote: '架构、API、构建、部署',
    demoDocs: 'Demo 文档',
    demoDocsNote: '同步、构建、运行、发布',
    contractDocs: 'Bridge 契约',
    licenseDocs: '许可证',
    licenseDocsNote: 'LGPL 2.1 + Exception 1.0',

    copy: '复制',
    copied: '已复制',

    licenseEyebrow: '许可证',
    licenseTitle: '许可证',
    licenseLead: 'GNU LGPL 2.1 + OcctCSharpBridge 链接例外 1.0。',
    licenseCardTitle: 'Bridge 许可证',
    licenseCardText: '适用于项目本身及其再分发修改版本。',
    exceptionCardTitle: '链接例外',
    exceptionCardText: '覆盖 .NET 引用、P/Invoke 等正常运行时链接方式。',
    readLicense: '查看 ↗',
    readException: '查看 ↗',
    footerLead: 'OCCT 7.9.0 · .NET 10 · Windows / Linux',
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
