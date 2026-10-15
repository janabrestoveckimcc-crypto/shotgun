import { ShotgunAudio } from './shared/audio.js';
const audio = new ShotgunAudio(new URL('./assets/audio/', import.meta.url));
const button = document.getElementById('sound-test'), status = document.getElementById('preview-status');
let busy = false;
button.addEventListener('click', async () => {
  if (busy) return;
  busy = true; button.disabled = true;
  try {
    await audio.unlock(); audio.play({ sound:'pump', volume:.35 });
    document.body.classList.add('firing'); setTimeout(() => document.body.classList.remove('firing'),160);
    status.textContent = 'Ovo je zvuk ekstenzije. Za tipkanje na web-stranicama prvo je instaliraj i uključi.';
  } catch(error) { status.textContent = `Zvuk nije pokrenut: ${error.message}`; }
  finally { busy = false; button.disabled = false; }
});
