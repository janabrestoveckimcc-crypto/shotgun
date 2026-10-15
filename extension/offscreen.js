import { ShotgunAudio } from './shared/audio.js';
const audio = new ShotgunAudio(new URL('./assets/audio/', import.meta.url));
let epoch = 0;
chrome.runtime.onMessage.addListener((message, sender, respond) => {
  if (message.target !== 'audio' || sender.id !== chrome.runtime.id) return;
  if (message.type === 'stop') { epoch++; audio.stop(); respond({ ok: true }); return; }
  const current = epoch;
  (async () => {
    await audio.unlock();
    if (message.type === 'play' && current === epoch) audio.play(message.settings);
    return { ok: true };
  })().then(respond).catch(error => respond({ ok: false, error: error.message }));
  return true;
});
