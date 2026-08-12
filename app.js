const translations = {
  en: {
    navCapabilities: 'Capabilities', navArchitecture: 'Architecture', navDemo: 'Demo', navDocs: 'Docs', navLicense: 'License',
    heroTitle: 'OCCT for modern .NET engineering applications',
    heroLead: 'OcctCSharpBridge connects Open CASCADE Technology 7.9.0 to .NET 10 / C# 14 through a stable C ABI, with strongly typed modeling, topology, meshing, STEP assembly exchange, AIS interaction and reusable Windows desktop viewport hosts.',
    viewSource: 'View source', viewDemo: 'View demos', architectureStack: 'Bridge boundary',
    stackApp: 'Your CAD / BIM application', stackAppSub: 'Document · Feature Tree · Command/Tool · Undo/Redo · JSON', stackManaged: 'Strongly typed managed API',
    statNative: 'Native exports', statPInvoke: 'P/Invoke mappings', statTypes: 'Public .NET types', statApi: 'Viewer / Modeling',
    sdkStatusFallback: 'Published SDK: main/dist',
    capEyebrow: 'CAPABILITIES', capTitle: 'A focused foundation for CAD, BIM and engineering software', capLead: 'The Bridge stays close to OCCT geometry and interaction while leaving product documents and domain workflows to the consuming application.',
    f1Title: 'Viewer & AIS', f1Text: 'Camera, projection, display modes, materials, color, transparency, transforms, text, dimensions, lighting and redraw batching.',
    f2Title: 'Headless modeling', f2Text: 'Primitives, Boolean operations, extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, healing and operation history.',
    f3Title: 'Selection & input', f3Text: 'Point/rectangle selection, structured selected and detected identities, selectable state and raw input forwarding for custom CAD tools.',
    f4Title: 'Geometry & topology', f4Text: 'Evaluation, curvature, projection, distance, adjacency, inertia, intersections, shape inspection and persistent topology references.',
    f5Title: 'Meshing', f5Text: 'Configurable triangulation with combined node/triangle arrays and explicit per-face provenance ranges.',
    f6Title: 'STEP assembly exchange', f6Text: 'First-class XDE-derived assembly snapshots preserve hierarchy, occurrences, transforms, visibility, colors and subshape styles.',
    f7Title: 'First-class points', f7Text: 'Real AIS_Point objects and standard OCCT point markers for CAD capture points, grips and lightweight interactive geometry.',
    f8Title: 'Desktop hosts', f8Text: 'Independent WinForms, WPF HwndHost and Avalonia Windows-HWND adapters over the same core engine.',
    archEyebrow: 'ARCHITECTURE', archTitle: 'Clear boundaries instead of a monolithic CAD framework', archLead: 'OcctCSharpBridge provides geometry, presentation and interaction primitives. Product state remains owned by your application.',
    p1Title: 'Application documents stay above the Bridge', p1Text: 'Feature trees, commands, undo/redo, snapping rules and project persistence are application responsibilities.',
    p2Title: 'XDE is internal to STEP exchange', p2Text: 'XDE preserves real STEP product structure and styles, then projects them to OcctAssemblyDocument. It is not exposed as the application document model.',
    p3Title: 'Stable managed semantics', p3Text: 'Strong types, explicit ownership and bulk Native calls keep the managed API predictable and avoid high-cardinality N+1 interop patterns.',
    p4Title: 'UI hosts stay independent', p4Text: 'WinForms, WPF and Avalonia all depend on OcctNet directly. WPF uses native HwndHost rendering with coalesced resize presentation.',
    demoEyebrow: 'DESKTOP DEMOS', demoTitle: 'One shared demo core, three Windows desktop hosts', demoLead: 'The demo branch consumes the actually published Binary SDK locally. Its dist directory is ignored by Git, while screenshots and application source stay versioned.',
    winformsCaption: 'Classic native Windows CAD-style host', wpfCaption: 'Native HwndHost viewport with coalesced resizing', avaloniaCaption: 'Windows HWND host over the same Bridge core',
    docsEyebrow: 'DOCUMENTATION', docsTitle: 'Source, guides and generated API reference', docsLead: 'The source contract is 2.7.0. Published Binary SDK status comes directly from main/dist/win-x64, while generated API pages are refreshed by the release build.',
    englishDocs: 'English guide', englishDocsSub: 'Architecture, modeling, viewer, exchange and deployment', chineseDocs: '中文文档', chineseDocsSub: '架构、建模、Viewer、数据交换与部署', apiDocs: 'Generated API reference', contractSub: 'Machine-readable source contract',
    startEyebrow: 'GET STARTED', startTitle: 'Build the Bridge or run the Binary SDK demos', startLead: 'Publish main on Windows when Binary consumers need the latest source APIs, then synchronize the actual tracked SDK into demo locally.', copy: 'Copy', copied: 'Copied',
    licenseEyebrow: 'LICENSING', licenseTitle: 'Free for non-commercial use; commercial use requires authorization', licenseLead: 'OcctCSharpBridge uses the PolyForm Noncommercial License 1.0.0. Third-party components such as Open CASCADE Technology keep their own licenses.',
    noncommercialKicker: 'NON-COMMERCIAL', noncommercialTitle: 'Free to use', noncommercialText: 'Research, study, evaluation, hobby and other permitted non-commercial uses are free, subject to the PolyForm Noncommercial License terms.', readLicense: 'Read license ↗',
    commercialKicker: 'COMMERCIAL', commercialTitle: 'Separate authorization required', commercialText: 'Commercial use is not granted by the non-commercial license. Contact the author for a commercial license, OEM or redistribution authorization.',
    footerAuthor: 'Author', themeLight: 'Light', themeDark: 'Dark'
  },
  zh: {
    navCapabilities: '能力', navArchitecture: '架构', navDemo: 'Demo', navDocs: '文档', navLicense: '授权',
    heroTitle: '面向现代 .NET 工程软件的 OCCT Bridge',
    heroLead: 'OcctCSharpBridge 通过稳定 C ABI 将 Open CASCADE Technology 7.9.0 接入 .NET 10 / C# 14，提供强类型建模、拓扑、网格、STEP 装配交换、AIS 交互以及可复用的 Windows 桌面视口宿主。',
    viewSource: '查看源码', viewDemo: '查看 Demo', architectureStack: 'Bridge 边界',
    stackApp: '你的 CAD / BIM 应用', stackAppSub: 'Document · Feature Tree · Command/Tool · Undo/Redo · JSON', stackManaged: '强类型托管 API',
    statNative: 'Native 导出', statPInvoke: 'P/Invoke 映射', statTypes: '公开 .NET 类型', statApi: 'Viewer / Modeling',
    sdkStatusFallback: '已发布 SDK：以 main/dist 为准',
    capEyebrow: '核心能力', capTitle: '面向 CAD、BIM 与工程软件的专注基础层', capLead: 'Bridge 聚焦 OCCT 几何与交互能力，把产品级 Document 和业务工作流留给上层应用。',
    f1Title: 'Viewer 与 AIS', f1Text: 'Camera、Projection、显示模式、材质、颜色、透明度、Transform、Text、Dimension、Lighting 和批量刷新。',
    f2Title: 'Headless 建模', f2Text: 'Primitive、Boolean、Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、Shelling、Healing 与 History。',
    f3Title: '选择与输入', f3Text: '点选、框选、Selected/Detected 结构化身份、可选状态和 Raw Input，便于上层实现自己的 CAD Tool。',
    f4Title: '几何与拓扑', f4Text: 'Evaluation、Curvature、Projection、Distance、Adjacency、Inertia、Intersection、Shape Inspection 与持久拓扑引用。',
    f5Title: '网格', f5Text: '可配置三角剖分，返回统一 Node/Triangle 数组以及明确的逐 Face provenance range。',
    f6Title: 'STEP 装配交换', f6Text: '基于 XDE 的一等装配快照保留层级、Occurrence、Transform、显隐、颜色和 Subshape Style。',
    f7Title: '一等 Point', f7Text: '真实 AIS_Point 与 OCCT 标准 Point Marker，适合捕捉点、夹点和轻量交互几何。',
    f8Title: '桌面宿主', f8Text: 'WinForms、WPF HwndHost、Avalonia Windows HWND 三个独立 Adapter，共享同一个核心 Engine。',
    archEyebrow: '架构', archTitle: '保持清晰边界，而不是做成单体 CAD 框架', archLead: 'OcctCSharpBridge 提供几何、显示和交互原语；产品状态仍由你的应用拥有。',
    p1Title: '应用 Document 位于 Bridge 之上', p1Text: 'Feature Tree、Command、Undo/Redo、捕捉规则和项目持久化都属于应用层职责。',
    p2Title: 'XDE 仅用于 STEP 交换内部', p2Text: 'XDE 负责保存真实 STEP 产品结构与样式，再投影成 OcctAssemblyDocument；不会暴露成上层 Document 模型。',
    p3Title: '稳定的托管语义', p3Text: '强类型、明确所有权与 Bulk Native 调用让 API 更可控，并避免高基数 N+1 P/Invoke。',
    p4Title: 'UI Host 彼此独立', p4Text: 'WinForms、WPF、Avalonia 都直接依赖 OcctNet；WPF 使用原生 HwndHost 并合并 Resize 刷新。',
    demoEyebrow: '桌面 DEMO', demoTitle: '一套共享 Demo Core，三个 Windows 桌面宿主', demoLead: 'demo 分支在本地消费 main 实际已发布的 Binary SDK；dist 被 Git 忽略，应用源码和截图继续正常版本管理。',
    winformsCaption: '经典 Windows CAD 风格宿主', wpfCaption: '原生 HwndHost 视口与合并 Resize', avaloniaCaption: '基于同一 Bridge Core 的 Windows HWND 宿主',
    docsEyebrow: '文档', docsTitle: '源码、专题指南与生成式 API Reference', docsLead: '当前源码契约是 2.7.0；已发布 Binary SDK 状态直接读取 main/dist/win-x64，生成式 API 页面由发布构建重新生成。',
    englishDocs: 'English Guide', englishDocsSub: '架构、建模、Viewer、交换与部署', chineseDocs: '中文文档', chineseDocsSub: '架构、建模、Viewer、数据交换与部署', apiDocs: '生成式 API Reference', contractSub: '机器可读源码契约',
    startEyebrow: '快速开始', startTitle: '构建 Bridge 或运行 Binary SDK Demo', startLead: '当 Binary 消费者需要最新源码 API 时，在 Windows 正式发布 main，再把实际跟踪的 SDK 同步到 demo 本地。', copy: '复制', copied: '已复制',
    licenseEyebrow: '许可与授权', licenseTitle: '非商业使用免费；商业使用需要授权', licenseLead: 'OcctCSharpBridge 采用 PolyForm Noncommercial License 1.0.0。Open CASCADE Technology 等第三方组件继续遵循各自许可证。',
    noncommercialKicker: '非商业使用', noncommercialTitle: '免费使用', noncommercialText: '研究、学习、评估、个人兴趣以及许可证允许的其它非商业用途可免费使用，但需要遵循 PolyForm Noncommercial License 的具体条款。', readLicense: '查看许可证 ↗',
    commercialKicker: '商业使用', commercialTitle: '需要单独授权', commercialText: '非商业许可证并未授予商业使用权。商业项目、OEM 或再分发请联系作者取得商业许可证/授权。',
    footerAuthor: '作者', themeLight: '浅色', themeDark: '深色'
  }
};

const publishedContractUrl = 'https://raw.githubusercontent.com/zly258/OcctCSharpBridge/main/dist/win-x64/bridge-contract.json';
const languageToggle = document.getElementById('languageToggle');
const themeToggle = document.getElementById('themeToggle');
const themeLabel = document.getElementById('themeLabel');
const copyCode = document.getElementById('copyCode');
const buildCode = document.getElementById('buildCode');
const themeMeta = document.querySelector('meta[name="theme-color"]');

let publishedSdkVersion = null;
let language = localStorage.getItem('occt-language');
if (language !== 'zh' && language !== 'en') {
  language = navigator.language?.toLowerCase().startsWith('zh') ? 'zh' : 'en';
}

function ensureReleaseStatusBadge() {
  const badges = document.querySelector('.badges');
  if (!badges) return null;
  let badge = document.getElementById('publishedSdkStatus');
  if (!badge) {
    badge = document.createElement('span');
    badge.id = 'publishedSdkStatus';
    badges.appendChild(badge);
  }
  return badge;
}

function updatePublishedSdkStatus() {
  const badge = ensureReleaseStatusBadge();
  if (!badge) return;
  const t = translations[language];
  badge.textContent = publishedSdkVersion
    ? (language === 'zh' ? `已发布 SDK ${publishedSdkVersion}` : `Published SDK ${publishedSdkVersion}`)
    : t.sdkStatusFallback;
}

async function loadPublishedSdkStatus() {
  try {
    const response = await fetch(publishedContractUrl, { cache: 'no-store' });
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    const contract = await response.json();
    if (typeof contract.bridgeVersion === 'string' && contract.bridgeVersion.trim()) {
      publishedSdkVersion = contract.bridgeVersion.trim();
    }
  } catch {
    publishedSdkVersion = null;
  }
  updatePublishedSdkStatus();
}

function applyLanguage(nextLanguage) {
  language = nextLanguage;
  const t = translations[language];
  document.documentElement.lang = language === 'zh' ? 'zh-CN' : 'en';
  document.querySelectorAll('[data-i18n]').forEach((node) => {
    const key = node.dataset.i18n;
    if (Object.prototype.hasOwnProperty.call(t, key)) node.textContent = t[key];
  });
  languageToggle.textContent = language === 'zh' ? 'EN' : '中文';
  languageToggle.setAttribute('aria-label', language === 'zh' ? 'Switch to English' : '切换为中文');
  updatePreviewImages();
  updateThemeLabel();
  updatePublishedSdkStatus();
  localStorage.setItem('occt-language', language);
}

function currentTheme() {
  return document.documentElement.dataset.theme === 'dark' ? 'dark' : 'light';
}

function applyTheme(theme) {
  const normalized = theme === 'dark' ? 'dark' : 'light';
  document.documentElement.dataset.theme = normalized;
  localStorage.setItem('occt-theme', normalized);
  themeMeta?.setAttribute('content', normalized === 'dark' ? '#0b1018' : '#f7f9fc');
  updateThemeLabel();
}

function updateThemeLabel() {
  const t = translations[language];
  const theme = currentTheme();
  themeLabel.textContent = theme === 'dark' ? t.themeLight : t.themeDark;
  themeToggle.setAttribute('aria-label', theme === 'dark' ? 'Switch to light theme' : 'Switch to dark theme');
}

function updatePreviewImages() {
  ['winformsPreview', 'wpfPreview', 'avaloniaPreview'].forEach((id) => {
    const image = document.getElementById(id);
    if (!image) return;
    image.src = language === 'zh' ? image.dataset.srcZh : image.dataset.srcEn;
    image.alt = language === 'zh'
      ? `${image.closest('figure')?.querySelector('strong')?.textContent || ''} Demo 预览`
      : `${image.closest('figure')?.querySelector('strong')?.textContent || ''} demo preview`;
  });
}

languageToggle.addEventListener('click', () => applyLanguage(language === 'en' ? 'zh' : 'en'));
themeToggle.addEventListener('click', () => applyTheme(currentTheme() === 'dark' ? 'light' : 'dark'));

copyCode.addEventListener('click', async () => {
  try {
    await navigator.clipboard.writeText(buildCode.textContent);
    const original = translations[language].copy;
    copyCode.textContent = translations[language].copied;
    window.setTimeout(() => { copyCode.textContent = original; }, 1200);
  } catch {
    const range = document.createRange();
    range.selectNodeContents(buildCode);
    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
  }
});

ensureReleaseStatusBadge();
applyLanguage(language);
applyTheme(currentTheme());
void loadPublishedSdkStatus();
