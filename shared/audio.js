import { normalizeSettings } from './settings.js';

export class ShotgunAudio {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
    this.context = null;
    this.buffers = new Map();
    this.loading = null;
    this.active = new Set();
  }
  async unlock() {
    if (!this.context || this.context.state === 'closed') {
      const AudioCtor = globalThis.AudioContext || globalThis.webkitAudioContext;
      if (!AudioCtor) throw new Error('Ovaj preglednik ne podržava Web Audio.');
      this.context = new AudioCtor({ latencyHint: 'interactive' });
      this.master = this.context.createGain();
      this.compressor = this.context.createDynamicsCompressor();
      this.compressor.threshold.value = -10;
      this.compressor.knee.value = 10;
      this.compressor.ratio.value = 8;
      this.compressor.attack.value = 0.002;
      this.compressor.release.value = 0.09;
      this.master.connect(this.compressor).connect(this.context.destination);
    }
    // Resume synchronously within the initiating user gesture (especially Safari).
    const resume = this.context.resume();
    if (!this.loading) {
      this.loading = Promise.all(['pump', 'double', 'tactical'].map(async name => {
        const response = await fetch(new URL(`${name}.wav`, this.baseUrl));
        if (!response.ok) throw new Error(`Zvuk nije dostupan (${response.status}).`);
        this.buffers.set(name, await this.context.decodeAudioData(await response.arrayBuffer()));
      })).catch(error => { this.loading = null; throw error; });
    }
    await Promise.all([resume, this.loading]);
    if (this.context.state !== 'running') throw new Error('Dodirni Uključi zvuk za aktivaciju audija.');
  }
  play(options = {}) {
    const settings = normalizeSettings(options);
    if (!this.context || this.context.state !== 'running' || !this.buffers.has(settings.sound)) return false;
    if (this.active.size >= 20) this.active.values().next().value.stop();
    this.master.gain.setTargetAtTime(settings.volume, this.context.currentTime, 0.008);
    const source = this.context.createBufferSource();
    source.buffer = this.buffers.get(settings.sound);
    source.playbackRate.value = 0.98 + Math.random() * 0.04;
    source.connect(this.master);
    this.active.add(source);
    source.onended = () => { this.active.delete(source); source.disconnect(); };
    source.start();
    return true;
  }
  stop() {
    for (const source of this.active) { try { source.stop(); } catch {} }
    this.active.clear();
  }
}
