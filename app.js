const translations = {
  en: {
    navBranches:'Branches', navCapabilities:'Capabilities', navDemo:'Demo', navDocs:'Docs', navLicense:'License',
    heroTitle:'A clean OCCT bridge for Windows and cross-platform Avalonia applications',
    heroLead:'OcctCSharpBridge exposes OCCT 7.9.0 through a stable C ABI and strongly typed .NET 10 APIs. The Windows main branch provides WinForms/WPF hosts; the standalone Avalonia branch targets both Windows x64 and Linux x64.',
    viewMain:'Windows main', viewAvalonia:'Cross-platform Avalonia', architectureStack:'Branch architecture',
    mainStack:'Windows x64 · OcctNet + WinForms + WPF', avaloniaStack:'Windows x64 + Linux x64 · OcctNet + OcctNet.Avalonia', commonCore:'Strongly typed modeling, topology, exchange and Viewer semantics',
    branchEyebrow:'BRANCHES', branchTitle:'Clear responsibilities instead of mixed platform projects', openBranch:'Open branch ↗',
    mainText:'Windows source and Binary SDK producer. Public assemblies: OcctNet, OcctNet.WinForms and OcctNet.Wpf. Contract: 349/349 Native/PInvoke, 113 public .NET types, net10.0-windows.',
    avaloniaText:'Standalone cross-platform source. Only OcctNet + OcctNet.Avalonia; no sync, WinForms or WPF. Contract: 350/350, 109 public .NET types, net10.0, Windows x64 + Linux x64.',
    demoBranchText:'Windows demo consumer for the published main SDK. Contains Common, WinForms and WPF demos only; Avalonia previews and projects are intentionally absent.',
    websiteText:'Static bilingual project website. It describes both product branches without treating the Windows main SDK as the Avalonia/Linux distribution.',
    capEyebrow:'CAPABILITIES', capTitle:'One OCCT foundation, explicit platform hosts', capLead:'Core capabilities stay focused on geometry, topology, exchange, AIS and interaction while product documents and workflows remain in the consuming application.',
    f1Title:'Viewer & AIS', f1Text:'Camera, projection, display modes, material/color/transparency, transforms, text, dimensions, points, lighting and redraw batching.',
    f2Title:'Headless modeling', f2Text:'Primitives, Boolean, extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, healing and history.',
    f3Title:'Selection & interaction', f3Text:'Point/rectangle selection, detection, structured object identity, zoom, pan, rotation and world-point conversion.',
    f4Title:'Geometry & topology', f4Text:'Evaluation, curvature, projection, distance, adjacency, inertia, intersections, validation and topology references.',
    f5Title:'Meshing', f5Text:'Configurable triangulation with bulk node/triangle transfers and per-face provenance.',
    f6Title:'STEP assembly exchange', f6Text:'XDE-derived managed assembly snapshots preserve hierarchy, occurrences, transforms, visibility, colors and subshape styles.',
    f7Title:'Windows hosts', f7Text:'main keeps independent WinForms and WPF adapters on net10.0-windows.',
    f8Title:'Cross-platform Avalonia', f8Text:'avalonia exposes one OcctAvaloniaViewport API. Windows uses HWND/WNT_Window; Linux currently uses X11/XWayland XID/Xw_Window internally.',
    demoEyebrow:'WINDOWS DEMOS', demoTitle:'Two demo hosts on the demo branch', demoLead:'The demo branch consumes main/dist locally and intentionally contains only WinForms and WPF applications.',
    winformsCaption:'Classic Windows CAD-style host', wpfCaption:'Native HWND viewport with coalesced resize presentation',
    docsEyebrow:'DOCUMENTATION', docsTitle:'Use the documentation for the branch you consume', docsLead:'main and avalonia have separate contracts, platform requirements and generated API references.', mainDocs:'main · Windows docs', avaloniaDocs:'avalonia · Cross-platform docs',
    startEyebrow:'GET STARTED', startTitle:'Choose the branch by UI/platform requirement', startLead:'Use main for Windows WinForms/WPF. Use avalonia directly for Avalonia on Windows or Linux; there is no sync dependency between them.',
    copy:'Copy', copied:'Copied', licenseEyebrow:'LICENSING', licenseTitle:'GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0', licenseLead:'Commercial and proprietary applications may use the Bridge through normal runtime linking under the project exception. Distributed Bridge modifications remain subject to the Bridge license; OCCT and other third-party components keep their own licenses.', readLicense:'Read license ↗', readException:'Read exception ↗', themeLight:'Light', themeDark:'Dark'
  },
  zh: {
    navBranches:'分支', navCapabilities:'能力', navDemo:'Demo', navDocs:'文档', navLicense:'License',
    heroTitle:'面向 Windows 与跨平台 Avalonia 的清晰 OCCT Bridge',
    heroLead:'OcctCSharpBridge 通过稳定 C ABI 将 OCCT 7.9.0 接入 .NET 10。main 提供 Windows WinForms/WPF；独立 avalonia 分支同时面向 Windows x64 与 Linux x64。',
    viewMain:'Windows main', viewAvalonia:'跨平台 Avalonia', architectureStack:'分支架构',
    mainStack:'Windows x64 · OcctNet + WinForms + WPF', avaloniaStack:'Windows x64 + Linux x64 · OcctNet + OcctNet.Avalonia', commonCore:'强类型 Modeling、Topology、Exchange 与 Viewer Semantics',
    branchEyebrow:'BRANCHES', branchTitle:'按平台与 UI 职责拆分，而不是把项目混在一起', openBranch:'打开分支 ↗',
    mainText:'Windows 源码与 Binary SDK 生产分支。公开程序集为 OcctNet、OcctNet.WinForms、OcctNet.Wpf；契约 349/349、113 个公开类型、net10.0-windows。',
    avaloniaText:'独立跨平台源码，只包含 OcctNet + OcctNet.Avalonia；没有 sync、WinForms、WPF。契约 350/350、109 个公开类型、net10.0，支持 Windows x64 + Linux x64。',
    demoBranchText:'消费 main 已发布 Windows SDK 的 Demo 分支，只保留 Common、WinForms、WPF；Avalonia 项目和预览已移除。',
    websiteText:'双语静态官网，同时描述 main 与 avalonia 两条产品分支，不再把 Windows main SDK 当成 Avalonia/Linux 发布物。',
    capEyebrow:'CAPABILITIES', capTitle:'同一 OCCT 基础能力，明确的平台 Host', capLead:'Core 聚焦 Geometry、Topology、Exchange、AIS 与 Interaction，产品级 Document 与业务 Workflow 留给上层应用。',
    f1Title:'Viewer & AIS', f1Text:'Camera、Projection、Display Mode、Material/Color/Transparency、Transform、Text、Dimension、Point、Lighting 与 Redraw Batching。',
    f2Title:'Headless Modeling', f2Text:'Primitive、Boolean、Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、Shelling、Healing 与 History。',
    f3Title:'Selection & Interaction', f3Text:'点选/框选、Detection、结构化 Object Identity、Zoom、Pan、Rotate 与 World Point Conversion。',
    f4Title:'Geometry & Topology', f4Text:'Evaluation、Curvature、Projection、Distance、Adjacency、Inertia、Intersection、Validation 与 Topology Reference。',
    f5Title:'Meshing', f5Text:'可配置 Triangulation、Bulk Node/Triangle Transfer 与 per-Face Provenance。',
    f6Title:'STEP Assembly Exchange', f6Text:'基于 XDE 的 Managed Assembly Snapshot，保留 Hierarchy、Occurrence、Transform、Visibility、Color 与 Subshape Style。',
    f7Title:'Windows Hosts', f7Text:'main 在 net10.0-windows 下保留独立 WinForms 与 WPF Adapter。',
    f8Title:'跨平台 Avalonia', f8Text:'avalonia 只公开一个 OcctAvaloniaViewport。Windows 内部使用 HWND/WNT_Window；Linux 当前内部使用 X11/XWayland XID/Xw_Window。',
    demoEyebrow:'WINDOWS DEMOS', demoTitle:'demo 分支只保留两个 Windows Host', demoLead:'demo 在本地消费 main/dist，只包含 WinForms 与 WPF 应用。',
    winformsCaption:'经典 Windows CAD 风格 Host', wpfCaption:'Native HWND Viewport + Resize Coalescing',
    docsEyebrow:'DOCUMENTATION', docsTitle:'按实际使用的分支阅读文档', docsLead:'main 与 avalonia 拥有独立 Contract、平台要求和 Generated API Reference。', mainDocs:'main · Windows 文档', avaloniaDocs:'avalonia · 跨平台文档',
    startEyebrow:'GET STARTED', startTitle:'根据 UI/平台选择分支', startLead:'WinForms/WPF 使用 main；Windows/Linux Avalonia 直接使用 avalonia，两者没有 sync 依赖。',
    copy:'复制', copied:'已复制', licenseEyebrow:'LICENSING', licenseTitle:'GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0', licenseLead:'商业和闭源应用可以在项目 Exception 下通过正常 Runtime Linking 使用 Bridge；分发的 Bridge 修改仍受 Bridge License 约束，OCCT 和其它第三方组件保持各自许可证。', readLicense:'查看 License ↗', readException:'查看 Exception ↗', themeLight:'浅色', themeDark:'深色'
  }
};

const languageToggle = document.getElementById('languageToggle');
const themeToggle = document.getElementById('themeToggle');
const themeLabel = document.getElementById('themeLabel');
const copyCode = document.getElementById('copyCode');
const buildCode = document.getElementById('buildCode');
const themeMeta = document.querySelector('meta[name="theme-color"]');
let language = localStorage.getItem('occt-language');
if (language !== 'zh' && language !== 'en') language = navigator.language?.toLowerCase().startsWith('zh') ? 'zh' : 'en';

function applyLanguage(next) {
  language = next;
  const t = translations[language];
  document.documentElement.lang = language === 'zh' ? 'zh-CN' : 'en';
  document.querySelectorAll('[data-i18n]').forEach(node => { const key = node.dataset.i18n; if (t[key]) node.textContent = t[key]; });
  languageToggle.textContent = language === 'zh' ? 'EN' : '中文';
  document.querySelectorAll('img[data-src-en]').forEach(img => { img.src = language === 'zh' ? img.dataset.srcZh : img.dataset.srcEn; });
  updateThemeLabel();
  localStorage.setItem('occt-language', language);
}
function currentTheme(){ return document.documentElement.dataset.theme === 'dark' ? 'dark' : 'light'; }
function applyTheme(theme){ const value = theme === 'dark' ? 'dark' : 'light'; document.documentElement.dataset.theme = value; localStorage.setItem('occt-theme', value); themeMeta?.setAttribute('content', value === 'dark' ? '#0b1018' : '#f7f9fc'); updateThemeLabel(); }
function updateThemeLabel(){ themeLabel.textContent = currentTheme() === 'dark' ? translations[language].themeLight : translations[language].themeDark; }
languageToggle?.addEventListener('click', () => applyLanguage(language === 'zh' ? 'en' : 'zh'));
themeToggle?.addEventListener('click', () => applyTheme(currentTheme() === 'dark' ? 'light' : 'dark'));
copyCode?.addEventListener('click', async () => { try { await navigator.clipboard.writeText(buildCode.textContent); const old = copyCode.textContent; copyCode.textContent = translations[language].copied; setTimeout(() => copyCode.textContent = old, 1200); } catch {} });

const lightbox = document.getElementById('previewLightbox');
const lightboxImage = document.getElementById('previewLightboxImage');
const previewClose = document.getElementById('previewClose');
document.querySelectorAll('.preview-card img').forEach(img => img.addEventListener('click', () => { if (!lightbox || !lightboxImage) return; lightboxImage.src = img.src; lightbox.hidden = false; document.body.style.overflow = 'hidden'; }));
function closePreview(){ if (!lightbox) return; lightbox.hidden = true; document.body.style.overflow = ''; }
previewClose?.addEventListener('click', closePreview);
lightbox?.addEventListener('click', event => { if (event.target === lightbox) closePreview(); });
document.addEventListener('keydown', event => { if (event.key === 'Escape') closePreview(); });

applyTheme(currentTheme());
applyLanguage(language);
