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
    f1Title: 'Viewer & AIS', f1Text: 'Camera, Projection, Display Mode, Material, Color, Transparency, Transform, Text, Dimension, Lighting and Redraw Batching.',
    f2Title: 'Headless Modeling', f2Text: 'Primitive, Boolean, Extrude, Revolve, Sweep, Loft, Fillet, Chamfer, Offset, Shelling, Healing and Operation History.',
    f3Title: 'Selection & Input', f3Text: 'Point/rectangle selection, structured Selected/Detected identities, selectable state and Raw Input forwarding for custom CAD Tools.',
    f4Title: 'Geometry & Topology', f4Text: 'Evaluation, Curvature, Projection, Distance, Adjacency, Inertia, Intersection, Shape Inspection and persistent Topology References.',
    f5Title: 'Meshing', f5Text: 'Configurable triangulation with combined Node/Triangle arrays and explicit per-Face provenance ranges.',
    f6Title: 'STEP Assembly Exchange', f6Text: 'First-class XDE-derived assembly snapshots preserve hierarchy, Occurrences, Transforms, Visibility, Colors and Subshape Styles.',
    f7Title: 'First-class Point', f7Text: 'Real AIS_Point objects and standard OCCT Point Markers for CAD capture points, grips and lightweight interactive geometry.',
    f8Title: 'Desktop UI Hosts', f8Text: 'Independent WinForms, WPF HwndHost and Avalonia Windows-HWND adapters over the same Bridge Core.',
    archEyebrow: 'ARCHITECTURE', archTitle: 'Clear boundaries instead of a monolithic CAD framework', archLead: 'OcctCSharpBridge provides geometry, presentation and interaction primitives. Product state remains owned by your application.',
    p1Title: 'Application Document stays above the Bridge', p1Text: 'Feature Tree, Command, Undo/Redo, snapping rules and project persistence are application responsibilities.',
    p2Title: 'XDE stays internal to STEP exchange', p2Text: 'XDE preserves real STEP product structure and styles, then projects them to OcctAssemblyDocument. It is not exposed as the application Document model.',
    p3Title: 'Stable managed semantics', p3Text: 'Strong types, explicit ownership and Bulk Native calls keep the managed API predictable and avoid high-cardinality N+1 P/Invoke patterns.',
    p4Title: 'UI Hosts stay independent', p4Text: 'WinForms, WPF and Avalonia all depend on OcctNet directly. WPF uses Native HwndHost rendering with coalesced Resize presentation.',
    demoEyebrow: 'DESKTOP DEMOS', demoTitle: 'One shared Demo Core, three Windows desktop UI Hosts', demoLead: 'The demo branch consumes the actually published Binary SDK locally. Its dist directory is ignored by Git, while screenshots and application source stay versioned. Click any preview to enlarge it.',
    winformsCaption: 'Classic Windows CAD-style UI Host', wpfCaption: 'Native HwndHost Viewport with coalesced Resize', avaloniaCaption: 'Windows HWND UI Host over the same Bridge Core',
    docsEyebrow: 'DOCUMENTATION', docsTitle: 'Source, guides and generated API Reference', docsLead: 'The source contract is 2.7.0. Published Binary SDK status comes directly from main/dist/win-x64, while generated API pages are refreshed by the release build.',
    englishDocs: 'English Guide', englishDocsSub: 'Architecture, Modeling, Viewer, Exchange and Deployment', chineseDocs: '中文文档', chineseDocsSub: 'Architecture、Modeling、Viewer、Data Exchange 与 Deployment', apiDocs: 'Generated API Reference', contractSub: 'Machine-readable source contract',
    startEyebrow: 'GET STARTED', startTitle: 'Build the Bridge or run the Binary SDK demos', startLead: 'Publish main on Windows when Binary consumers need the latest source APIs, then synchronize the actual tracked SDK into demo locally.', copy: 'Copy', copied: 'Copied',
    licenseEyebrow: 'LICENSING', licenseTitle: 'Free for non-commercial use; commercial use requires authorization', licenseLead: 'OcctCSharpBridge uses the PolyForm Noncommercial License 1.0.0. Third-party components such as Open CASCADE Technology keep their own licenses.',
    noncommercialKicker: 'NON-COMMERCIAL', noncommercialTitle: 'Free to use', noncommercialText: 'Research, study, evaluation, hobby and other permitted non-commercial uses are free, subject to the PolyForm Noncommercial License terms.', readLicense: 'Read license ↗',
    commercialKicker: 'COMMERCIAL', commercialTitle: 'Separate authorization required', commercialText: 'Commercial use is not granted by the non-commercial license. Contact the author for a commercial license, OEM or redistribution authorization.',
    footerAuthor: 'Author', themeLight: 'Light', themeDark: 'Dark', previewOpen: 'Open full-size preview', previewClose: 'Close preview'
  },
  zh: {
    navCapabilities: '能力', navArchitecture: '架构', navDemo: 'Demo', navDocs: '文档', navLicense: '授权',
    heroTitle: '面向现代 .NET 工程软件的 OCCT Bridge',
    heroLead: 'OcctCSharpBridge 通过稳定 C ABI 将 Open CASCADE Technology 7.9.0 接入 .NET 10 / C# 14，提供强类型 Modeling、Topology、Meshing、STEP Assembly Exchange、AIS Interaction，以及可复用的 Windows Desktop UI Host。',
    viewSource: '查看源码', viewDemo: '查看 Demo', architectureStack: 'Bridge Boundary',
    stackApp: '你的 CAD / BIM 应用', stackAppSub: 'Document · Feature Tree · Command/Tool · Undo/Redo · JSON', stackManaged: '强类型 Managed API',
    statNative: 'Native Exports', statPInvoke: 'P/Invoke Mappings', statTypes: 'Public .NET Types', statApi: 'Viewer / Modeling',
    sdkStatusFallback: 'Published SDK：以 main/dist 为准',
    capEyebrow: 'CAPABILITIES', capTitle: '面向 CAD、BIM 与工程软件的专注基础层', capLead: 'Bridge 聚焦 OCCT Geometry 与 Interaction，把产品级 Document 和业务 Workflow 留给上层应用。',
    f1Title: 'Viewer & AIS', f1Text: 'Camera、Projection、Display Mode、Material、Color、Transparency、Transform、Text、Dimension、Lighting 与 Redraw Batching。',
    f2Title: 'Headless Modeling', f2Text: 'Primitive、Boolean、Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、Shelling、Healing 与 Operation History。',
    f3Title: 'Selection & Input', f3Text: '支持点选、框选、结构化 Selected/Detected Identity、Selectable State 与 Raw Input，便于上层实现自己的 CAD Tool。',
    f4Title: 'Geometry & Topology', f4Text: 'Evaluation、Curvature、Projection、Distance、Adjacency、Inertia、Intersection、Shape Inspection 与 Persistent Topology Reference。',
    f5Title: 'Meshing', f5Text: '可配置 Triangulation，返回统一 Node/Triangle Array，并提供明确的 per-Face provenance range。',
    f6Title: 'STEP Assembly Exchange', f6Text: '基于 XDE 的 first-class Assembly Snapshot，保留 Hierarchy、Occurrence、Transform、Visibility、Color 与 Subshape Style。',
    f7Title: 'First-class Point', f7Text: '使用真实 AIS_Point 与 OCCT 标准 Point Marker，适合 CAD Capture Point、Grip 和轻量交互 Geometry。',
    f8Title: 'Desktop UI Hosts', f8Text: 'WinForms、WPF HwndHost、Avalonia Windows HWND 三个独立 Adapter，共享同一个 Bridge Core。',
    archEyebrow: 'ARCHITECTURE', archTitle: '保持清晰边界，而不是做成单体 CAD Framework', archLead: 'OcctCSharpBridge 提供 Geometry、Presentation 与 Interaction Primitive；产品状态仍由上层应用负责。',
    p1Title: 'Application Document 位于 Bridge 之上', p1Text: 'Feature Tree、Command、Undo/Redo、Snapping Rule 与 Project Persistence 都属于应用层职责。',
    p2Title: 'XDE 仅用于 STEP Exchange 内部', p2Text: 'XDE 保存真实 STEP Product Structure 与 Style，再投影为 OcctAssemblyDocument；不会暴露成上层 Application Document。',
    p3Title: '稳定的 Managed Semantics', p3Text: 'Strong Type、明确 Ownership 与 Bulk Native Call 让 API 更可控，并避免高基数 N+1 P/Invoke。',
    p4Title: 'UI Hosts 彼此独立', p4Text: 'WinForms、WPF、Avalonia 都直接依赖 OcctNet；WPF 使用 Native HwndHost，并对 Resize Presentation 做 Coalescing。',
    demoEyebrow: 'DESKTOP DEMOS', demoTitle: '一套共享 Demo Core，三个 Windows Desktop UI Host', demoLead: 'demo 分支在本地消费 main 实际发布的 Binary SDK；dist 被 Git 忽略，应用源码和截图正常版本管理。三个预览图均可点击放大。',
    winformsCaption: '经典 Windows CAD 风格 UI Host', wpfCaption: 'Native HwndHost Viewport + Resize Coalescing', avaloniaCaption: '基于同一 Bridge Core 的 Windows HWND UI Host',
    docsEyebrow: 'DOCUMENTATION', docsTitle: '源码、专题 Guide 与 Generated API Reference', docsLead: '当前 Source Contract 为 2.7.0；Published Binary SDK 状态直接读取 main/dist/win-x64，API Reference 由 Release Build 自动生成。',
    englishDocs: 'English Guide', englishDocsSub: 'Architecture、Modeling、Viewer、Exchange 与 Deployment', chineseDocs: '中文文档', chineseDocsSub: 'Architecture、Modeling、Viewer、Data Exchange 与 Deployment', apiDocs: 'Generated API Reference', contractSub: 'Machine-readable Source Contract',
    startEyebrow: 'GET STARTED', startTitle: '构建 Bridge 或运行 Binary SDK Demo', startLead: '当 Binary Consumer 需要最新 Source API 时，在 Windows 正式发布 main，再把实际跟踪的 SDK 同步到 demo 本地。', copy: '复制', copied: '已复制',
    licenseEyebrow: 'LICENSING', licenseTitle: '非商业使用免费；商业使用需要授权', licenseLead: 'OcctCSharpBridge 使用 PolyForm Noncommercial License 1.0.0。Open CASCADE Technology 等 Third-party Component 继续遵循各自 License。',
    noncommercialKicker: 'NON-COMMERCIAL', noncommercialTitle: '非商业使用免费', noncommercialText: '研究、学习、评估、个人兴趣以及 License 允许的其它非商业用途可免费使用，但需遵循 PolyForm Noncommercial License 的具体条款。', readLicense: '查看 License ↗',
    commercialKicker: 'COMMERCIAL', commercialTitle: '商业使用需要单独授权', commercialText: 'Noncommercial License 不授予商业使用权。商业项目、OEM 或 Redistribution 请联系作者取得 Commercial License / Authorization。',
    footerAuthor: '作者', themeLight: '浅色', themeDark: '深色', previewOpen: '点击查看原图', previewClose: '关闭预览'
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
    ? (language === 'zh' ? `Published SDK ${publishedSdkVersion}` : `Published SDK ${publishedSdkVersion}`)
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
  updateLightboxLanguage();
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
    const product = image.closest('figure')?.querySelector('strong')?.textContent || '';
    image.alt = language === 'zh' ? `${product} Demo 预览` : `${product} demo preview`;
    image.setAttribute('aria-label', `${translations[language].previewOpen}: ${product}`);
  });
}

let lightbox = null;
let lightboxImage = null;
let lightboxCaption = null;
let lightboxClose = null;
let activePreview = null;

function ensureLightbox() {
  if (lightbox) return;

  const style = document.createElement('style');
  style.textContent = `
    .preview-card img { cursor: zoom-in; transition: opacity .18s ease, transform .18s ease; }
    .preview-card img:hover { opacity: .94; transform: scale(1.004); }
    .preview-card img:focus-visible { outline: 3px solid var(--accent); outline-offset: -3px; }
    .preview-lightbox { position: fixed; inset: 0; z-index: 200; display: none; align-items: center; justify-content: center; padding: 32px; background: rgba(3, 8, 16, .88); backdrop-filter: blur(10px); }
    .preview-lightbox.is-open { display: flex; }
    .preview-lightbox__content { position: relative; display: flex; max-width: min(96vw, 1600px); max-height: 94vh; flex-direction: column; gap: 10px; }
    .preview-lightbox__image { max-width: 100%; max-height: calc(94vh - 46px); width: auto; height: auto; object-fit: contain; border-radius: 10px; background: #fff; box-shadow: 0 28px 90px rgba(0,0,0,.45); }
    .preview-lightbox__caption { margin: 0; color: #e8eef7; text-align: center; font-size: 13px; }
    .preview-lightbox__close { position: fixed; top: 18px; right: 22px; width: 42px; height: 42px; border: 1px solid rgba(255,255,255,.22); border-radius: 999px; background: rgba(12,18,28,.74); color: #fff; cursor: pointer; font-size: 24px; line-height: 1; }
    .preview-lightbox__close:hover { background: rgba(28,38,52,.94); }
    body.preview-lightbox-open { overflow: hidden; }
    @media (max-width: 720px) { .preview-lightbox { padding: 14px; } .preview-lightbox__close { top: 10px; right: 10px; } }
  `;
  document.head.appendChild(style);

  lightbox = document.createElement('div');
  lightbox.className = 'preview-lightbox';
  lightbox.setAttribute('role', 'dialog');
  lightbox.setAttribute('aria-modal', 'true');
  lightbox.setAttribute('aria-hidden', 'true');
  lightbox.innerHTML = `
    <button class="preview-lightbox__close" type="button" aria-label="Close preview">×</button>
    <div class="preview-lightbox__content">
      <img class="preview-lightbox__image" alt="" />
      <p class="preview-lightbox__caption"></p>
    </div>
  `;
  document.body.appendChild(lightbox);

  lightboxImage = lightbox.querySelector('.preview-lightbox__image');
  lightboxCaption = lightbox.querySelector('.preview-lightbox__caption');
  lightboxClose = lightbox.querySelector('.preview-lightbox__close');

  lightboxClose.addEventListener('click', closePreviewLightbox);
  lightbox.addEventListener('click', (event) => {
    if (event.target === lightbox) closePreviewLightbox();
  });
  document.addEventListener('keydown', (event) => {
    if (!lightbox?.classList.contains('is-open')) return;
    if (event.key === 'Escape') closePreviewLightbox();
  });
}

function previewCaption(image) {
  const figure = image.closest('figure');
  const title = figure?.querySelector('strong')?.textContent || '';
  const caption = figure?.querySelector('figcaption span')?.textContent || '';
  return caption ? `${title} — ${caption}` : title;
}

function openPreviewLightbox(image) {
  ensureLightbox();
  activePreview = image;
  lightboxImage.src = image.currentSrc || image.src;
  lightboxImage.alt = image.alt;
  lightboxCaption.textContent = previewCaption(image);
  lightbox.classList.add('is-open');
  lightbox.setAttribute('aria-hidden', 'false');
  document.body.classList.add('preview-lightbox-open');
  updateLightboxLanguage();
  lightboxClose.focus();
}

function closePreviewLightbox() {
  if (!lightbox) return;
  lightbox.classList.remove('is-open');
  lightbox.setAttribute('aria-hidden', 'true');
  document.body.classList.remove('preview-lightbox-open');
  lightboxImage.removeAttribute('src');
  lightboxCaption.textContent = '';
  activePreview?.focus();
  activePreview = null;
}

function updateLightboxLanguage() {
  if (!lightboxClose) return;
  lightboxClose.setAttribute('aria-label', translations[language].previewClose);
  if (activePreview && lightbox?.classList.contains('is-open')) {
    lightboxImage.src = activePreview.currentSrc || activePreview.src;
    lightboxImage.alt = activePreview.alt;
    lightboxCaption.textContent = previewCaption(activePreview);
  }
}

function enablePreviewLightbox() {
  ensureLightbox();
  ['winformsPreview', 'wpfPreview', 'avaloniaPreview'].forEach((id) => {
    const image = document.getElementById(id);
    if (!image) return;
    image.tabIndex = 0;
    image.setAttribute('role', 'button');
    image.addEventListener('click', () => openPreviewLightbox(image));
    image.addEventListener('keydown', (event) => {
      if (event.key !== 'Enter' && event.key !== ' ') return;
      event.preventDefault();
      openPreviewLightbox(image);
    });
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

enablePreviewLightbox();
ensureReleaseStatusBadge();
applyLanguage(language);
applyTheme(currentTheme());
void loadPublishedSdkStatus();