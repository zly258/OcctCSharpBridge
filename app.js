const translations = {
  en: {
    navBranches:'Branches', navCapabilities:'Capabilities', navDemo:'Demo', navDocs:'Docs', navLicense:'License',
    heroTitle:'A clean OCCT bridge for Windows and cross-platform Avalonia applications',
    heroLead:'OcctCSharpBridge exposes OCCT 7.9.0 through a stable C ABI and strongly typed .NET 10 APIs. The Windows main branch provides WinForms/WPF hosts; the standalone Avalonia branch targets both Windows x64 and Linux x64.',
    viewMain:'Windows main', viewAvalonia:'Cross-platform Avalonia', architectureStack:'Branch architecture',
    mainStack:'Windows x64 · OcctNet + WinForms + WPF', avaloniaStack:'Windows x64 + Linux x64 · OcctNet + OcctNet.Avalonia', commonCore:'Strongly typed modeling, topology, exchange and Viewer semantics',
    branchEyebrow:'BRANCHES', branchTitle:'Clear responsibilities instead of mixed platform projects', openBranch:'Open branch ↗',
    mainText:'Windows source and Binary SDK producer. Public assemblies: OcctNet, OcctNet.WinForms and OcctNet.Wpf. Contract: 419/419 Native/PInvoke, 139 public .NET types, net10.0-windows.',
    avaloniaText:'Standalone cross-platform source. OcctNet + OcctNet.Avalonia with one API for Windows x64 and Linux x64. Contract: 420/420 Native/PInvoke, 135 public .NET types, net10.0.',
    demoBranchText:'Windows demonstration branch for the published main SDK, focused on the WinForms and WPF hosts. Cross-platform Avalonia demonstrations are maintained independently on the avalonia branch.',
    websiteText:'Static bilingual project website. It presents the Windows demo family and cross-platform Avalonia without treating the Windows main SDK as the Avalonia/Linux distribution.',
    capEyebrow:'CAPABILITIES', capTitle:'One OCCT foundation, explicit platform hosts', capLead:'Core capabilities stay focused on geometry, topology, exchange, AIS and interaction while product documents and workflows remain in the consuming application.',
    f1Title:'Viewer & AIS', f1Text:'Camera, projection, display modes, material/color/transparency, transforms, text, dimensions, points, lighting and redraw batching.',
    f2Title:'Headless modeling', f2Text:'Primitives, Boolean, extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, healing and history.',
    f3Title:'Selection & interaction', f3Text:'Point/rectangle selection, detection, structured object identity, zoom, pan, rotation and world-point conversion.',
    f4Title:'Geometry & topology', f4Text:'Evaluation, curvature, projection, distance, adjacency, inertia, intersections, validation and topology references.',
    f5Title:'Meshing', f5Text:'Configurable triangulation with bulk node/triangle transfers and per-face provenance.',
    f6Title:'STEP assembly exchange', f6Text:'XDE-derived managed assembly snapshots preserve hierarchy, occurrences, transforms, visibility, colors and subshape styles.',
    f7Title:'Windows hosts', f7Text:'main keeps independent WinForms and WPF adapters on net10.0-windows.',
    f8Title:'Cross-platform Avalonia', f8Text:'avalonia exposes one OcctAvaloniaViewport API. Windows uses HWND/WNT_Window; Linux uses the X11/XWayland XID/Xw_Window backend with native-child pointer input and coalesced motion processing.',
    demoEyebrow:'RUNNING PREVIEWS', demoTitle:'Windows demo and cross-platform Avalonia', demoLead:'WinForms/WPF are maintained on demo; Avalonia is maintained on the dedicated avalonia branch for Windows and Linux. Click any preview to inspect the original image.',
    winformsCaption:'Classic Windows CAD-style host', wpfCaption:'Native HWND viewport with coalesced resize presentation', avaloniaWinCaption:'Avalonia CAD host on Windows x64', avaloniaLinuxCaption:'Avalonia CAD host on Linux x64',
    docsEyebrow:'DOCUMENTATION', docsTitle:'Use the documentation for the branch you consume', docsLead:'main and avalonia have separate contracts, platform requirements and generated API references.', mainDocs:'main · Windows docs', avaloniaDocs:'avalonia · Cross-platform docs',
    startEyebrow:'GET STARTED', startTitle:'Choose the branch by UI/platform requirement', startLead:'Use main for Windows WinForms/WPF. Use avalonia directly for Avalonia on Windows or Linux; there is no sync dependency between them.',
    copy:'Copy', copied:'Copied',
    licenseEyebrow:'LICENSING', licenseTitle:'Clear licensing for bridge use and distribution', licenseLead:'OcctCSharpBridge uses GNU LGPL 2.1 with the OcctCSharpBridge Exception 1.0. The project exception permits normal runtime linking from commercial and proprietary applications while Bridge modifications remain subject to the Bridge license.',
    licenseCardTitle:'Bridge license', licenseCardText:'Review the GNU LGPL 2.1 terms that govern OcctCSharpBridge itself and redistributed modifications.', exceptionCardTitle:'Linking exception', exceptionCardText:'Review the project exception covering normal .NET references, dynamic linking, P/Invoke and equivalent runtime linking.',
    readLicense:'Read license ↗', readException:'Read exception ↗', footerLead:'OCCT 7.9.0 bridge for .NET 10 · Windows and Linux', themeLight:'Light', themeDark:'Dark'
  },
  zh: {
    navBranches:'分支', navCapabilities:'能力', navDemo:'案例', navDocs:'文档', navLicense:'许可',
    heroTitle:'面向 Windows 与跨平台 Avalonia 的清晰 OCCT Bridge', heroLead:'OcctCSharpBridge 通过稳定 C ABI 将 OCCT 7.9.0 接入 .NET 10。main 提供 Windows WinForms/WPF；独立 avalonia 分支同时面向 Windows x64 与 Linux x64。',
    viewMain:'Windows main', viewAvalonia:'跨平台 Avalonia', architectureStack:'分支架构', mainStack:'Windows x64 · OcctNet + WinForms + WPF', avaloniaStack:'Windows x64 + Linux x64 · OcctNet + OcctNet.Avalonia', commonCore:'强类型 Modeling、Topology、Exchange 与 Viewer Semantics',
    branchEyebrow:'分支', branchTitle:'按平台与 UI 职责拆分，而不是把项目混在一起', openBranch:'打开分支 ↗',
    mainText:'Windows 源码与 Binary SDK 生产分支。公开程序集为 OcctNet、OcctNet.WinForms、OcctNet.Wpf；契约 419/419、139 个公开类型、net10.0-windows。', avaloniaText:'独立跨平台源码，使用 OcctNet + OcctNet.Avalonia，通过同一套 API 支持 Windows x64 + Linux x64；契约 420/420、135 个公开类型、net10.0。',
    demoBranchText:'消费 main 已发布 Windows SDK 的案例分支，重点展示 WinForms 与 WPF Host；跨平台 Avalonia 案例独立维护在 avalonia 分支。', websiteText:'双语静态官网，统一展示 Windows 案例与跨平台 Avalonia，不把 Windows main SDK 当成 Avalonia/Linux 发布物。',
    capEyebrow:'能力', capTitle:'同一 OCCT 基础能力，明确的平台 Host', capLead:'Core 聚焦 Geometry、Topology、Exchange、AIS 与 Interaction，产品级 Document 与业务 Workflow 留给上层应用。',
    f1Title:'Viewer & AIS', f1Text:'Camera、Projection、Display Mode、Material/Color/Transparency、Transform、Text、Dimension、Point、Lighting 与 Redraw Batching。', f2Title:'Headless Modeling', f2Text:'Primitive、Boolean、Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、Shelling、Healing 与 History。', f3Title:'Selection & Interaction', f3Text:'点选/框选、Detection、结构化 Object Identity、Zoom、Pan、Rotate 与 World Point Conversion。', f4Title:'Geometry & Topology', f4Text:'Evaluation、Curvature、Projection、Distance、Adjacency、Inertia、Intersection、Validation 与 Topology Reference。', f5Title:'Meshing', f5Text:'可配置 Triangulation、Bulk Node/Triangle Transfer 与 per-Face Provenance。', f6Title:'STEP Assembly Exchange', f6Text:'基于 XDE 的 Managed Assembly Snapshot，保留 Hierarchy、Occurrence、Transform、Visibility、Color 与 Subshape Style。', f7Title:'Windows Hosts', f7Text:'main 在 net10.0-windows 下保留独立 WinForms 与 WPF Adapter。', f8Title:'跨平台 Avalonia', f8Text:'avalonia 只公开一个 OcctAvaloniaViewport。Windows 内部使用 HWND/WNT_Window；Linux 使用 X11/XWayland XID/Xw_Window，并通过原生子窗口输入和 Motion 合并处理鼠标交互。',
    demoEyebrow:'案例预览', demoTitle:'Windows 案例与跨平台 Avalonia', demoLead:'WinForms/WPF 维护在 demo 分支；Avalonia 独立维护在 avalonia 分支，并同时支持 Windows 与 Linux。点击任意预览图可查看原始大图。', winformsCaption:'经典 Windows CAD 风格 Host', wpfCaption:'Native HWND Viewport + Resize Coalescing', avaloniaWinCaption:'Windows x64 Avalonia CAD Host', avaloniaLinuxCaption:'Linux x64 Avalonia CAD Host',
    docsEyebrow:'文档', docsTitle:'按实际使用的分支阅读文档', docsLead:'main 与 avalonia 拥有独立 Contract、平台要求和 Generated API Reference。', mainDocs:'main · Windows 文档', avaloniaDocs:'avalonia · 跨平台文档', startEyebrow:'开始使用', startTitle:'根据 UI/平台选择分支', startLead:'WinForms/WPF 使用 main；Windows/Linux Avalonia 直接使用 avalonia，两者没有 sync 依赖。', copy:'复制', copied:'已复制',
    licenseEyebrow:'许可', licenseTitle:'清晰区分 Bridge 许可与应用链接边界', licenseLead:'OcctCSharpBridge 采用 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0。项目 Exception 允许商业及闭源应用通过正常运行时链接方式使用 Bridge；对 Bridge 本身的修改和再分发仍需遵循 Bridge 许可。', licenseCardTitle:'Bridge 许可', licenseCardText:'查看适用于 OcctCSharpBridge 本身及其再分发修改版本的 GNU LGPL 2.1 条款。', exceptionCardTitle:'链接例外', exceptionCardText:'查看项目对 .NET Assembly Reference、Dynamic Linking、P/Invoke 等正常运行时链接方式的例外说明。', readLicense:'查看许可 ↗', readException:'查看例外 ↗', footerLead:'面向 .NET 10 的 OCCT 7.9.0 Bridge · Windows 与 Linux', themeLight:'浅色', themeDark:'深色'
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
function applyLanguage(next) { language = next; const t = translations[language]; document.documentElement.lang = language === 'zh' ? 'zh-CN' : 'en'; document.querySelectorAll('[data-i18n]').forEach(node => { const key = node.dataset.i18n; if (t[key]) node.textContent = t[key]; }); languageToggle.textContent = language === 'zh' ? 'EN' : '中文'; document.querySelectorAll('img[data-src-en]').forEach(img => { img.src = language === 'zh' ? img.dataset.srcZh : img.dataset.srcEn; }); updateThemeLabel(); localStorage.setItem('occt-language', language); }
function currentTheme(){ return document.documentElement.dataset.theme === 'dark' ? 'dark' : 'light'; }
function applyTheme(theme){ const value = theme === 'dark' ? 'dark' : 'light'; document.documentElement.dataset.theme = value; localStorage.setItem('occt-theme', value); themeMeta?.setAttribute('content', value === 'dark' ? '#0b1018' : '#f7f9fc'); updateThemeLabel(); }
function updateThemeLabel(){ themeLabel.textContent = currentTheme() === 'dark' ? translations[language].themeLight : translations[language].themeDark; }
languageToggle?.addEventListener('click', () => applyLanguage(language === 'zh' ? 'en' : 'zh')); themeToggle?.addEventListener('click', () => applyTheme(currentTheme() === 'dark' ? 'light' : 'dark'));
copyCode?.addEventListener('click', async () => { try { await navigator.clipboard.writeText(buildCode.textContent); const old = copyCode.textContent; copyCode.textContent = translations[language].copied; setTimeout(() => copyCode.textContent = old, 1200); } catch {} });
const lightbox = document.getElementById('previewLightbox'); const lightboxImage = document.getElementById('previewLightboxImage'); const previewClose = document.getElementById('previewClose');
document.querySelectorAll('.preview-card img').forEach(img => img.addEventListener('click', () => { if (!lightbox || !lightboxImage) return; lightboxImage.src = img.src; lightboxImage.alt = img.alt; lightbox.hidden = false; document.body.style.overflow = 'hidden'; }));
function closePreview(){ if (!lightbox) return; lightbox.hidden = true; document.body.style.overflow = ''; }
previewClose?.addEventListener('click', closePreview); lightbox?.addEventListener('click', event => { if (event.target === lightbox) closePreview(); }); document.addEventListener('keydown', event => { if (event.key === 'Escape') closePreview(); });
applyTheme(currentTheme()); applyLanguage(language);
