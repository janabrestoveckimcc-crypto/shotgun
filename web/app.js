import { ShotgunAudio } from './shared/audio.js';
import { normalizeSettings } from './shared/settings.js';
import { attachKeyboard } from './shared/input.js';
const $ = id => document.getElementById(id);
let saved = {};
try { saved = JSON.parse(localStorage.getItem('shotgun-settings') || '{}'); } catch {}
const settings = { ...normalizeSettings(saved), enabled: false };
const audio = new ShotgunAudio(new URL('./assets/audio/', import.meta.url));
let shots = 0, shotTimes = [], flashTimeout, busy = false;
function save() { try { localStorage.setItem('shotgun-settings', JSON.stringify({ ...settings, enabled: false })); } catch {} }
function renderPower() {
  document.body.classList.toggle('armed', settings.enabled);
  $('power').setAttribute('aria-pressed', String(settings.enabled));
  $('power-label').textContent = settings.enabled ? 'Utišaj zvuk' : 'Uključi zvuk';
  $('status').textContent = settings.enabled ? 'ZVUK UKLJUČEN' : 'ZVUK ISKLJUČEN';
  $('typing-hint').textContent = settings.enabled ? 'Tipkaj ovdje ili isprobaj tipke ispod.' : 'Uključi zvuk pa počni tipkati.';
}
async function enable() {
  await audio.unlock();
  settings.enabled = true;
  $('audio-message').textContent = 'Spreman. Svaka tipka ima svoj prasak.';
  renderPower();
}
function disable() { settings.enabled = false; audio.stop(); renderPower(); $('audio-message').textContent = 'Zvuk je utišan. Tekst ostaje samo ovdje.'; }
function reportError(error) { disable(); $('audio-message').textContent = `Zvuk nije pokrenut: ${error.message}`; }
function fire(code) {
  if (!settings.enabled || !audio.play(settings)) return;
  shots++; shotTimes.push(performance.now());
  $('shots').textContent = String(shots).padStart(3, '0');
  $('feedback').textContent = ['BOOM. TAKO SE TIPKA.', 'MAKE SOME NOISE.', 'JEDNA TIPKA. JEDAN PRASAK.'][shots % 3];
  document.body.classList.add('firing'); clearTimeout(flashTimeout);
  flashTimeout = setTimeout(() => document.body.classList.remove('firing'), 100);
  const key = document.querySelector(`[data-code="${CSS.escape(code)}"]`);
  if (key) { key.classList.add('hot'); setTimeout(() => key.classList.remove('hot'), 120); }
}
$('power').addEventListener('click', async () => {
  if (busy) return;
  if (settings.enabled) return disable();
  busy = true; $('power').disabled = true;
  try { await enable(); } catch (error) { reportError(error); }
  finally { busy = false; $('power').disabled = false; }
});
$('test').addEventListener('click', async () => { try { await enable(); fire('Space'); } catch (error) { reportError(error); } });
$('volume').value = Math.round(settings.volume * 100);
$('volume-value').textContent = `${$('volume').value}%`;
$('volume').addEventListener('input', event => { settings.volume = Number(event.target.value) / 100; $('volume-value').textContent = `${event.target.value}%`; save(); });
document.querySelector(`input[name=sound][value=${settings.sound}]`).checked = true;
for (const radio of document.querySelectorAll('input[name=sound]')) radio.addEventListener('change', () => { settings.sound = radio.value; save(); if (settings.enabled) fire('Space'); });
$('repeat').checked = settings.repeat;
$('repeat').addEventListener('change', event => { settings.repeat = event.target.checked; save(); });
$('clear').addEventListener('click', () => { $('typing').value = ''; $('typing').focus(); });
document.addEventListener('keydown', event => { if (event.key === 'Escape') disable(); }, true);
attachKeyboard(document, { fire, enabled: () => settings.enabled, repeat: () => settings.repeat, ignore: event => event.key === 'Escape' || event.target.closest?.('.controls, .keyboard, #install, .topbar, footer') });
// Buttons work with touch, mouse and assistive technologies, without summoning a mobile keyboard.
const rows = [
  [['Esc','Escape'],...Array.from('1234567890', c => [c,`Digit${c}`]),['⌫','Backspace','wide']],
  [['Tab','Tab','wide'],...Array.from('QWERTYUIOP', c => [c,`Key${c}`])],
  [['Caps','CapsLock','wide'],...Array.from('ASDFGHJKL', c => [c,`Key${c}`]),['↵','Enter','wide']],
  [['Shift','ShiftLeft','wide'],...Array.from('ZXCVBNM', c => [c,`Key${c}`]),[',','Comma'],['.','Period'],['Shift','ShiftRight','wide']],
  [['Ctrl','ControlLeft','wide'],['Alt','AltLeft'],['S P A C E','Space','space'],['Alt','AltRight'],['←','ArrowLeft'],['→','ArrowRight']]
];
for (const row of rows) {
  const element = document.createElement('div'); element.className = 'key-row';
  for (const [label, code, width] of row) {
    const key = document.createElement('button'); key.type = 'button'; key.className = `key ${width || ''} ${code === 'Enter' ? 'accent' : ''}`;
    key.dataset.code = code; key.textContent = label; key.setAttribute('aria-label', code === 'Space' ? 'Razmak' : label);
    key.addEventListener('pointerdown', event => event.preventDefault());
    key.addEventListener('click', async event => {
      if (code === 'Escape') return disable();
      try {
        if (!settings.enabled) await enable();
        fire(code);
        const field = $('typing'), start = field.selectionStart, end = field.selectionEnd;
        if (code === 'Backspace') field.setRangeText('', start === end ? Math.max(0, start - 1) : start, end, 'end');
        else if (code === 'ArrowLeft' || code === 'ArrowRight') { const position = Math.max(0, Math.min(field.value.length, start + (code === 'ArrowLeft' ? -1 : 1))); field.setSelectionRange(position, position); }
        else if (label.length === 1 || ['Space','Enter','Tab'].includes(code)) field.setRangeText(code === 'Space' ? ' ' : code === 'Enter' ? '\n' : code === 'Tab' ? '\t' : label.toLowerCase(), start, end, 'end');
      } catch (error) { reportError(error); }
    });
    element.append(key);
  }
  $('keyboard').append(element);
}
setInterval(() => { const now = performance.now(); shotTimes = shotTimes.filter(time => now - time < 1000); $('speed').textContent = shotTimes.length.toFixed(1); }, 250);
document.addEventListener('visibilitychange', () => { if (document.hidden) disable(); });
// Resume after an OS interruption on the next intentional touch, without ever recording text.
document.addEventListener('pointerdown', () => { if (settings.enabled && audio.context?.state !== 'running') audio.unlock().catch(reportError); }, { passive: true });
if ('serviceWorker' in navigator) navigator.serviceWorker.register('./sw.js').catch(() => {});
renderPower();
