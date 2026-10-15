(() => {
  let settings = { enabled: false, repeat: false };
  chrome.storage.local.get('settings').then(result => { if (result.settings) settings = result.settings; }).catch(() => {});
  chrome.storage.onChanged.addListener((changes, area) => { if (area === 'local' && changes.settings) settings = changes.settings.newValue || { enabled: false }; });
  let physical = false, timer;
  function ignored(event) {
    // Only inspect the control type, never its value or the pressed character.
    return event.composedPath().some(node => node?.tagName === 'INPUT' && node.type === 'password');
  }
  function shot() {
    // No key name, text, URL or other page data leaves the content script.
    chrome.runtime.sendMessage({ target: 'background', type: 'shot' }).catch(() => {});
  }
  document.addEventListener('keydown', event => {
    if (!settings.enabled || !event.isTrusted || ignored(event)) return;
    if (event.keyCode === 229 || event.key === 'Unidentified' || event.key === 'Process' || event.isComposing) return;
    physical = true; clearTimeout(timer); timer = setTimeout(() => { physical = false; }, 0);
    if (!event.repeat || settings.repeat) shot();
  }, true);
  document.addEventListener('beforeinput', event => {
    if (!settings.enabled || !event.isTrusted || ignored(event)) return;
    if (physical) { physical = false; return; }
    if (/Paste|Drop|Replacement|history/i.test(event.inputType || '')) return;
    shot();
  }, true);
})();
