import { mkdir, writeFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { resolve } from 'node:path';
import { deflateSync } from 'node:zlib';
export const root = fileURLToPath(new URL('../', import.meta.url));
// Original procedural effects: transient, low boom, noisy decay and mechanical pump.
// No third-party recordings or audio licenses required.
export function synthesize(kind, rate = 44100) {
  const length = Math.floor(rate * 0.65);
  const samples = new Float32Array(length);
  let seed = 43891, low = 0;
  const random = () => { seed = (Math.imul(seed, 1664525) + 1013904223) >>> 0; return seed / 2147483648 - 1; };
  const decay = kind === 'double' ? 0.115 : kind === 'tactical' ? 0.045 : 0.075;
  for (let i = 0; i < length; i++) {
    const t = i / rate, n = random();
    low += 0.13 * (n - low);
    const attack = Math.min(1, t / 0.0008);
    const boom = Math.sin(2 * Math.PI * (92 * t - 45 * t * t)) * Math.exp(-t / (decay * 1.25));
    let value = (n * 0.45 * Math.exp(-t / 0.012) + low * 1.8 * Math.exp(-t / decay) + boom * 0.52) * attack;
    const pumpTime = kind === 'pump' ? 0.24 : 0.19;
    if (t > pumpTime && t < pumpTime + 0.09) {
      const p = t - pumpTime;
      value += n * 0.18 * Math.exp(-p / 0.02) + Math.sin(p * 6300) * 0.07 * Math.exp(-p / 0.016);
    }
    if (kind === 'double' && t > 0.035) value += low * 0.6 * Math.exp(-(t - 0.035) / 0.08);
    samples[i] = Math.tanh(value * 1.65) * 0.84 * Math.min(1, (0.65 - t) / 0.025);
  }
  return samples;
}
function wav(samples) {
  const out = Buffer.alloc(44 + samples.length * 2);
  out.write('RIFF'); out.writeUInt32LE(out.length - 8, 4); out.write('WAVEfmt ', 8);
  out.writeUInt32LE(16, 16); out.writeUInt16LE(1, 20); out.writeUInt16LE(1, 22);
  out.writeUInt32LE(44100, 24); out.writeUInt32LE(88200, 28); out.writeUInt16LE(2, 32); out.writeUInt16LE(16, 34);
  out.write('data', 36); out.writeUInt32LE(samples.length * 2, 40);
  samples.forEach((x, i) => out.writeInt16LE(Math.round(x * 32767), 44 + i * 2));
  return out;
}
function crc32(data) { let c = -1; for (const b of data) { c ^= b; for (let j = 0; j < 8; j++) c = (c >>> 1) ^ ((c & 1) ? 0xedb88320 : 0); } return (c ^ -1) >>> 0; }
function chunk(type, data) { const b = Buffer.alloc(12 + data.length); b.writeUInt32BE(data.length); b.write(type, 4); data.copy(b, 8); b.writeUInt32BE(crc32(b.subarray(4, -4)), b.length - 4); return b; }
function icon(size) {
  const raw = Buffer.alloc((size * 4 + 1) * size);
  for (let y = 0; y < size; y++) for (let x = 0; x < size; x++) {
    const dx = x / size - .5, dy = y / size - .5, r = Math.hypot(dx, dy);
    const mark = (r > .23 && r < .27) || (Math.abs(dx) < .025 && Math.abs(dy) < .36) || (Math.abs(dy) < .025 && Math.abs(dx) < .36);
    const offset = y * (size * 4 + 1) + 1 + x * 4;
    raw.set(mark ? [30, 32, 29, 255] : [245, 255, 96, 255], offset);
  }
  const header = Buffer.alloc(13); header.writeUInt32BE(size); header.writeUInt32BE(size, 4); header[8] = 8; header[9] = 6;
  return Buffer.concat([Buffer.from([137,80,78,71,13,10,26,10]), chunk('IHDR',header), chunk('IDAT',deflateSync(raw)), chunk('IEND',Buffer.alloc(0))]);
}
export async function generateAssets() {
  await mkdir(resolve(root, 'assets/audio'), { recursive: true });
  await mkdir(resolve(root, 'assets/icons'), { recursive: true });
  for (const kind of ['pump', 'double', 'tactical']) await writeFile(resolve(root, `assets/audio/${kind}.wav`), wav(synthesize(kind)));
  for (const size of [16, 48, 128, 192, 512]) await writeFile(resolve(root, `assets/icons/icon-${size}.png`), icon(size));
}
if (process.argv[1] === fileURLToPath(import.meta.url)) { await generateAssets(); console.log('Generated original shotgun effects and icons.'); }
