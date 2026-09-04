(function(){
  const saved=localStorage.getItem('occt_site_theme');
  const theme=saved==='dark'||saved==='light' ? saved :
    (matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
  document.documentElement.dataset.theme=theme;
})();
