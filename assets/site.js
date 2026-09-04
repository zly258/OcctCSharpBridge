const root=document.documentElement;
const themeBtn=document.getElementById('themeToggle');
const langBtn=document.getElementById('langToggle');

const i18n={
  en:{
    navCapabilities:"Capabilities",navPlatforms:"Platforms",navSdk:"SDK",navDemo:"Demo",navLicense:"License",
    eyebrow:"Native CAD geometry infrastructure",
    heroTitle:"Industrial geometry SDK for .NET CAD/BIM software.",
    heroLead:"OcctCSharpBridge exposes Open CASCADE Technology 7.9.0 through a stable native ABI and typed .NET APIs for geometry, BRep modeling, exchange and hardware-accelerated desktop viewports.",
    quickStart:"Quick start",viewHosts:"View reference hosts",source:"Source ↗",baseline:"Current baseline",
    sideCapabilities:"01 / Capabilities",capTitle:"Low-level by design.",
    capIntro:"The website presents the Bridge as a technical SDK product: geometry kernel, data exchange, viewport and runtime contract, without marketing-style feature stacking.",
    sidePlatforms:"02 / Platforms",platformTitle:"Platform boundaries are explicit.",
    platformIntro:"Headless modeling and UI hosting are described separately instead of reducing cross-platform support to a vague Windows / Linux claim.",
    sideSdk:"03 / Developer SDK",sdkTitle:"Build once. Consume as an SDK.",
    sdkIntro:"Installation, consumption and application boundaries are presented like developer documentation.",
    sideHosts:"04 / Reference hosts",hostsTitle:"Real application surfaces.",
    hostsIntro:"Real application screenshots are shown directly at large size. No carousel. Click any screenshot to inspect the original.",
    sideLicense:"05 / License",licenseTitle:"Open-source licensing.",
    licenseIntro:"License information is exposed as a first-class section so SDK users can quickly inspect distribution and linking terms.",
    footerLeft:"OcctCSharpBridge · Industrial geometry infrastructure for .NET",
    footerRight:"LGPL 2.1 + Linking Exception"
  },
  zh:{
    navCapabilities:"核心能力",navPlatforms:"平台支持",navSdk:"SDK",navDemo:"示例",navLicense:"许可证",
    eyebrow:"原生 CAD 几何基础设施",
    heroTitle:"面向 .NET CAD/BIM 软件的工业级几何 SDK。",
    heroLead:"OcctCSharpBridge 通过稳定的原生 ABI 与强类型 .NET API 封装 Open CASCADE Technology 7.9.0，覆盖几何、BRep 建模、数据交换和硬件加速桌面视口。",
    quickStart:"快速开始",viewHosts:"查看参考应用",source:"源码 ↗",baseline:"当前基线",
    sideCapabilities:"01 / 核心能力",capTitle:"坚持低层基础设施定位。",
    capIntro:"官网按技术 SDK 产品来表达：几何内核、数据交换、视口和运行时契约，不采用营销式功能堆砌。",
    sidePlatforms:"02 / 平台支持",platformTitle:"明确的平台能力边界。",
    platformIntro:"将 Headless 建模与 UI 宿主能力分开说明，不用一句模糊的“支持 Windows / Linux”概括不同层级的能力。",
    sideSdk:"03 / Developer SDK",sdkTitle:"构建一次，作为 SDK 直接消费。",
    sdkIntro:"安装、引用和职责边界按照开发者文档方式呈现，方便应用项目直接接入。",
    sideHosts:"04 / 参考应用",hostsTitle:"真实的软件界面。",
    hostsIntro:"直接展示真实应用截图，不使用轮播；点击任意截图即可放大查看原图。",
    sideLicense:"05 / 许可证",licenseTitle:"开源许可。",
    licenseIntro:"许可证作为独立内容区展示，便于 SDK 使用者快速确认分发和链接条款。",
    footerLeft:"OcctCSharpBridge · 面向 .NET 的工业级几何基础设施",
    footerRight:"LGPL 2.1 + Linking Exception"
  }
};

function applyLang(lang){
  document.documentElement.lang=lang==='zh'?'zh-CN':'en';
  document.querySelectorAll('[data-i18n]').forEach(el=>{
    const key=el.getAttribute('data-i18n');
    if(i18n[lang] && i18n[lang][key]) el.textContent=i18n[lang][key];
  });
  localStorage.setItem('occt_site_lang',lang);
  langBtn.textContent=lang==='zh'?'EN':'中文';
}

const savedLang=localStorage.getItem('occt_site_lang');
const initialLang=savedLang==='zh'||savedLang==='en' ? savedLang :
  ((navigator.language||'').toLowerCase().startsWith('zh')?'zh':'en');
applyLang(initialLang);
langBtn.addEventListener('click',()=>{
  applyLang(document.documentElement.lang.startsWith('zh')?'en':'zh');
});
function refreshThemeLabel(){themeBtn.textContent=root.dataset.theme==='dark'?'LIGHT':'DARK'}
refreshThemeLabel();
themeBtn.addEventListener('click',()=>{
  const next=root.dataset.theme==='dark'?'light':'dark';
  root.dataset.theme=next;
  localStorage.setItem('occt_site_theme',next);
  refreshThemeLabel();
});

const dlg=document.getElementById('lightbox'),dlgImg=dlg.querySelector('img');
document.querySelectorAll('.demo-frame img').forEach(img=>{
  img.addEventListener('click',()=>{dlgImg.src=img.src;dlgImg.alt=img.alt;dlg.showModal();});
});
dlg.querySelector('.close').addEventListener('click',()=>dlg.close());
dlg.addEventListener('click',e=>{if(e.target===dlg)dlg.close();});

document.querySelectorAll('[data-copy]').forEach(btn=>{
  btn.addEventListener('click',async()=>{
    const id=btn.getAttribute('data-copy'),text=document.getElementById(id).textContent;
    try{await navigator.clipboard.writeText(text);const old=btn.textContent;btn.textContent='COPIED';setTimeout(()=>btn.textContent=old,1000)}catch{}
  });
});
