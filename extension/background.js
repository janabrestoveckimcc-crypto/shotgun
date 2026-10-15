import { DEFAULTS, normalizeSettings } from './shared/settings.js';
let creating;
async function ensureOffscreen() {
  if (creating) return creating;
  creating = (async () => {
    const contexts = await chrome.runtime.getContexts({ contextTypes: ['OFFSCREEN_DOCUMENT'], documentUrls: [chrome.runtime.getURL('offscreen.html')] });
    if (!contexts.length) await chrome.offscreen.createDocument({ url: 'offscreen.html', reasons: ['AUDIO_PLAYBACK'], justification: 'Play local shotgun sound effects for keyboard presses.' });
  })().finally(() => { creating = null; });
  return creating;
}
async function updateBadge() {
  const { settings } = await chrome.storage.local.get('settings');
  await chrome.action.setBadgeText({ text: settings?.enabled ? 'ON' : '' });
  await chrome.action.setBadgeBackgroundColor({ color: '#708335' });
}
chrome.runtime.onInstalled.addListener(async () => {
  const result = await chrome.storage.local.get('settings');
  if (!result.settings) await chrome.storage.local.set({ settings: DEFAULTS });
  await updateBadge();
});
chrome.runtime.onStartup.addListener(updateBadge);
chrome.storage.onChanged.addListener((changes, area) => {
  if (area === 'local' && changes.settings) {
    updateBadge();
    if (!changes.settings.newValue?.enabled) chrome.runtime.sendMessage({ target: 'audio', type: 'stop' }).catch(() => {});
  }
});
chrome.runtime.onMessage.addListener((message, sender, respond) => {
  if (message.target !== 'background' || !['shot', 'preview', 'prepare'].includes(message.type)) return;
  if (sender.id !== chrome.runtime.id) return;
  // Preview can only come from our popup, never from a web page content script.
  if (message.type !== 'shot' && sender.url !== chrome.runtime.getURL('popup.html')) return;
  (async () => {
    const result = await chrome.storage.local.get('settings');
    const settings = normalizeSettings(result.settings);
    if (message.type === 'shot' && !settings.enabled) return { ok: true, skipped: true };
    await ensureOffscreen();
    try {
      return await chrome.runtime.sendMessage({ target: 'audio', type: message.type === 'prepare' ? 'prepare' : 'play', settings });
    } catch {
      // An idle offscreen document may have closed between lookup and messaging.
      await ensureOffscreen();
      return await chrome.runtime.sendMessage({ target: 'audio', type: message.type === 'prepare' ? 'prepare' : 'play', settings });
    }
  })().then(respond).catch(error => respond({ ok: false, error: error.message }));
  return true;
});
