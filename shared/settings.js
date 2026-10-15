export const DEFAULTS = Object.freeze({ enabled: false, volume: 0.35, sound: 'pump', repeat: false });
export function normalizeSettings(value = {}) {
  return {
    enabled: value.enabled === true,
    volume: Number.isFinite(value.volume) ? Math.max(0, Math.min(1, value.volume)) : DEFAULTS.volume,
    sound: ['pump', 'double', 'tactical'].includes(value.sound) ? value.sound : DEFAULTS.sound,
    repeat: value.repeat === true,
  };
}
