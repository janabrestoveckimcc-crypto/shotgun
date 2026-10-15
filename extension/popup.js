import { normalizeSettings } from './shared/settings.js';
const $ = id => document.getElementById(id);
const result = await chrome.storage.local.get('settings');
const settings = normalizeSettings(result.settings);
function render() {
  $('power').textContent = settings.enabled ? 'Utišaj zvuk' : 'Uključi zvuk';
  $('power').setAttribute('aria-pressed', String(settings.enabled));
  $('volume').value = Math.round(settings.volume * 100); $('value').textContent = `${$('volume').value}%`;
  $('sound').value = settings.sound; $('repeat').checked = settings.repeat;
}
async function save() { await chrome.storage.local.set({ settings }); }
async function request(type) {
  const result = await chrome.runtime.sendMessage({ target: 'background', type });
  if (!result?.ok) throw new Error(result?.error || 'Zvuk nije dostupan.');
}
$('power').addEventListener('click', async () => {
  $('power').disabled = true;
  try {
    if (!settings.enabled) await request('prepare');
    settings.enabled = !settings.enabled; await save(); render();
    $('status').textContent = settings.enabled ? 'Uključeno. Osvježi ranije otvorene stranice i tipkaj.' : 'Zvuk je utišan.';
  } catch (error) { $('status').textContent = error.message; }
  finally { $('power').disabled = false; }
});
$('volume').addEventListener('input', async event => { settings.volume = Number(event.target.value) / 100; render(); await save(); });
$('sound').addEventListener('change', async event => { settings.sound = event.target.value; await save(); });
$('repeat').addEventListener('change', async event => { settings.repeat = event.target.checked; await save(); });
$('test').addEventListener('click', async () => { try { await request('preview'); $('status').textContent = 'Pucanj! Postavke su spremljene na ovom uređaju.'; } catch (error) { $('status').textContent = error.message; } });
render();
