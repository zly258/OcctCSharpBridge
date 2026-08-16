const translations = {
  en: {
    navBranches:'Branches', navCapabilities:'Capabilities', navDemo:'Demo', navDocs:'Docs', navLicense:'License',
    heroTitle:'One Bridge SDK, one unified Demo for Windows and Linux',
    heroLead:'OcctCSharpBridge exposes OCCT 7.9.0 through an ABI5-only C interface and strongly typed .NET 10 APIs. main is the single SDK source; demo provides WinForms, WPF and Avalonia on Windows x64 and Avalonia on Linux x64.',
    viewMain:'View SDK', viewDemo:'View Demo', architectureStack:'Repository architecture',
    mainStack:'main / main-dev · Bridge SDK source', demoStack:'demo / demo-dev · Windows 3 hosts · Linux Avalonia', commonCore:'Bridge 3.0.0-preview.1 · ABI 5 only · OCCT 7.9.0 · .NET SDK 10.0.303',
    branchEyebrow:'BRANCHES', branchTitle:'SDK, Demo and website have explicit responsibilities', openBranch:'Open branch ↗',
    mainText:'The sole Bridge SDK source. Windows Binary SDK contains Core, WinForms, WPF and Avalonia adapters; Linux Binary SDK contains Core and Avalonia. ABI5-only, .NET SDK 10.0.303 and C# 14.',
    demoBranchText:'The single application-consumer branch. Windows x64 runs WinForms, WPF and Avalonia; Linux x64 runs Avalonia only. Demo consumes validated Binary SDKs and does not vendor Bridge implementation source.',
    websiteText:'Static bilingual project website. It presents the current Bridge 3 contract, unified Demo platform matrix and canonical screenshots from demo.',
    capEyebrow:'CAPABILITIES', capTitle:'Strongly typed OCCT capabilities with explicit UI adapters', capLead:'Core stays focused on geometry, topology, exchange, AIS, modeling and interaction while UI integration is isolated in platform adapters.',
    f1Title:'Viewer & AIS', f1Text:'Camera, projection, display modes, material/color/transparency, transforms, text, dimensions, points, lighting and redraw batching.',
    f2Title:'Headless modeling', f2Text:'Primitives, Boolean operations, extrude, revolve, sweep, loft, fillet, chamfer, offset, shelling, healing and history.',
    f3Title:'Selection & interaction', f3Text:'Point/rectangle selection, detection, structured object identity, zoom, pan, rotation and world-point conversion.',
    f4Title:'Geometry & topology', f4Text:'Evaluation, curvature, projection, distance, adjacency, inertia, intersections, validation and topology references.',
    f5Title:'Meshing', f5Text:'Configurable triangulation with bulk node/triangle transfers and per-face provenance.',
    f6Title:'STEP assembly exchange', f6Text:'XDE-derived managed assembly snapshots preserve hierarchy, occurrences, transforms, visibility, colors and subshape styles.',
    f7Title:'Windows adapters', f7Text:'WinForms, WPF and Avalonia adapters are produced by main and consumed by the three Windows Demo hosts.',
    f8Title:'Linux Avalonia', f8Text:'Linux uses the same Core + Avalonia managed API with the X11/XWayland Viewer backend. WinForms and WPF remain Windows-only.',
    demoEyebrow:'RUNNING PREVIEWS', demoTitle:'One Demo branch, four canonical previews', demoLead:'Windows provides WinForms, WPF and Avalonia; Linux provides Avalonia only. All screenshots are maintained in the formal demo branch.',
    winformsCaption:'Windows x64 · WinForms CAD host', wpfCaption:'Windows x64 · WPF CAD host', avaloniaWinCaption:'Windows x64 · Avalonia CAD host', avaloniaLinuxCaption:'Linux x64 · Avalonia CAD host',
    docsEyebrow:'DOCUMENTATION', docsTitle:'SDK and Demo documentation follow the same architecture', docsLead:'Use main for SDK contracts and implementation guidance; use demo for Windows/Linux consumer workflows and platform-specific build, run and publish commands.', mainDocs:'main · SDK documentation', demoDocs:'demo · Consumer documentation', contractDocs:'Bridge 3 contract',
    startEyebrow:'GET STARTED', startTitle:'Build the SDK once, consume it from demo', startLead:'main produces validated Binary SDKs. demo synchronizes them by source commit and manifest hash before building the platform hosts.',
    copy:'Copy', copied:'Copied',
    licenseEyebrow:'LICENSING', licenseTitle:'Clear licensing for bridge use and distribution', licenseLead:'OcctCSharpBridge uses GNU LGPL 2.1 with the OcctCSharpBridge Exception 1.0. The exception permits normal runtime linking from commercial and proprietary applications while Bridge modifications remain subject to the Bridge license.',
    licenseCardTitle:'Bridge license', licenseCardText:'Review the GNU LGPL 2.1 terms that govern OcctCSharpBridge itself and redistributed modifications.', exceptionCardTitle:'Linking exception', exceptionCardText:'Review the project exception covering normal .NET references, dynamic linking, P/Invoke and equivalent runtime linking.',
    readLicense:'Read license ↗', readException:'Read exception ↗', footerLead:'Bridge 3 · OCCT 7.9.0 · .NET 10 · Windows and Linux', themeLight:'Light', themeDark:'Dark'
  },
  zh: {
    navBranches:'分支', navCapabilities:'能力', navDemo:'案例', navDocs:'文档', navLicense:'许可',
    heroTitle:'一套 Bridge SDK，一套统一 Windows/Linux Demo',
    heroLead:'OcctCSharpBridge 通过 ABI5-only C 接口将 OCCT 7.9.0 接入 .NET 10。main 是唯一 SDK 源；demo 在 Windows x64 提供 WinForms、WPF、Avalonia，在 Linux x64 仅提供 Avalonia。',
    viewMain:'查看 SDK', viewDemo:'查看 Demo', architectureStack:'仓库架构',
    mainStack:'main / main-dev · Bridge SDK 源码', demoStack:'demo / demo-dev · Windows 三 Host · Linux Avalonia', commonCore:'Bridge 3.0.0-preview.1 · ABI 5 only · OCCT 7.9.0 · .NET SDK 10.0.303',
    branchEyebrow:'分支', branchTitle:'SDK、Demo、官网职责清晰分离', openBranch:'打开分支 ↗',
    mainText:'唯一 Bridge SDK 源。Windows Binary SDK 包含 Core、WinForms、WPF、Avalonia Adapter；Linux Binary SDK 包含 Core 与 Avalonia。ABI5-only，固定 .NET SDK 10.0.303 与 C# 14。',
    demoBranchText:'唯一应用 Consumer 分支。Windows x64 提供 WinForms、WPF、Avalonia；Linux x64 仅提供 Avalonia。Demo 只消费经过校验的 Binary SDK，不复制 Bridge 实现源码。',
    websiteText:'双语静态官网，展示当前 Bridge 3 Contract、统一 Demo 平台矩阵，并统一引用 demo 分支中的正式截图。',
    capEyebrow:'能力', capTitle:'强类型 OCCT 能力与明确的 UI Adapter 边界', capLead:'Core 聚焦 Geometry、Topology、Exchange、AIS、Modeling 与 Interaction，UI 集成由独立平台 Adapter 承担。',
    f1Title:'Viewer & AIS', f1Text:'Camera、Projection、Display Mode、Material/Color/Transparency、Transform、Text、Dimension、Point、Lighting 与 Redraw Batching。',
    f2Title:'Headless Modeling', f2Text:'Primitive、Boolean、Extrude、Revolve、Sweep、Loft、Fillet、Chamfer、Offset、Shelling、Healing 与 History。',
    f3Title:'Selection & Interaction', f3Text:'点选/框选、Detection、结构化 Object Identity、Zoom、Pan、Rotate 与 World Point Conversion。',
    f4Title:'Geometry & Topology', f4Text:'Evaluation、Curvature、Projection、Distance、Adjacency、Inertia、Intersection、Validation 与 Topology Reference。',
    f5Title:'Meshing', f5Text:'可配置 Triangulation、Bulk Node/Triangle Transfer 与 per-Face Provenance。',
    f6Title:'STEP Assembly Exchange', f6Text:'基于 XDE 的 Managed Assembly Snapshot，保留 Hierarchy、Occurrence、Transform、Visibility、Color 与 Subshape Style。',
    f7Title:'Windows Adapter', f7Text:'main 同时产出 WinForms、WPF、Avalonia Adapter，由 Windows 三个 Demo Host 消费。',
    f8Title:'Linux Avalonia', f8Text:'Linux 使用同一套 Core + Avalonia Managed API 和 X11/XWayland Viewer Backend；WinForms/WPF 保持 Windows-only。',
    demoEyebrow:'案例预览', demoTitle:'一个 Demo 分支，四组正式预览', demoLead:'Windows 提供 WinForms、WPF、Avalonia；Linux 仅提供 Avalonia。所有正式截图均维护在 demo 分支。',
    winformsCaption:'Windows x64 · WinForms CAD Host', wpfCaption:'Windows x64 · WPF CAD Host', avaloniaWinCaption:'Windows x64 · Avalonia CAD Host', avaloniaLinuxCaption:'Linux x64 · Avalonia CAD Host',
    docsEyebrow:'文档', docsTitle:'SDK 与 Demo 文档遵循同一套架构', docsLead:'main 提供 SDK Contract 与实现说明；demo 提供 Windows/Linux Consumer 的同步、构建、运行和发布流程。', mainDocs:'main · SDK 文档', demoDocs:'demo · Consumer 文档', contractDocs:'Bridge 3 Contract',
    startEyebrow:'开始使用', startTitle:'main 生成 SDK，demo 统一消费', startLead:'main 生成经过验证的 Binary SDK；demo 按 sourceCommit 和 Manifest Hash 同步后再构建对应平台 Host。',
    copy:'复制', copied:'已复制',
    licenseEyebrow:'许可', licenseTitle:'清晰区分 Bridge 许可与应用链接边界', licenseLead:'OcctCSharpBridge 采用 GNU LGPL 2.1 + OcctCSharpBridge Exception 1.0。项目 Exception 允许商业及闭源应用通过正常运行时链接方式使用 Bridge；对 Bridge 本身的修改和再分发仍需遵循 Bridge 许可。', licenseCardTitle:'Bridge 许可', licenseCardText:'查看适用于 OcctCSharpBridge 本身及其再分发修改版本的 GNU LGPL 2.1 条款。', exceptionCardTitle:'链接例外', exceptionCardText:'查看项目对 .NET Assembly Reference、Dynamic Linking、P/Invoke 等正常运行时链接方式的例外说明。', readLicense:'查看许可 ↗', readException:'查看例外 ↗', footerLead:'Bridge 3 · OCCT 7.9.0 · .NET 10 · Windows 与 Linux', themeLight:'浅色', themeDark:'深色'
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
