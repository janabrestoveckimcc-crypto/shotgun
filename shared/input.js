// Track events, never the text. Physical keydown + beforeinput must sound only once.
export function attachKeyboard(target, { fire, enabled = () => true, repeat = () => false, ignore = () => false }) {
  let pendingPhysical = false;
  let clearPending;
  const keydown = event => {
    if (!enabled() || ignore(event) || !event.isTrusted) return;
    // Mobile IMEs often send keyCode 229 / Unidentified for every edit.
    if (event.keyCode === 229 || event.key === 'Unidentified' || event.key === 'Process' || event.isComposing) return;
    pendingPhysical = true;
    clearTimeout(clearPending);
    clearPending = setTimeout(() => { pendingPhysical = false; }, 0);
    if (!event.repeat || repeat()) fire(event.code || event.key);
  };
  const beforeinput = event => {
    if (!enabled() || ignore(event) || !event.isTrusted) return;
    if (pendingPhysical) { pendingPhysical = false; return; }
    // A paste/autofill is not a sequence of physical key presses.
    if (/Paste|Drop|Replacement|history/i.test(event.inputType || '')) return;
    fire('Mobile');
  };
  target.addEventListener('keydown', keydown, true);
  target.addEventListener('beforeinput', beforeinput, true);
  return () => {
    clearTimeout(clearPending);
    target.removeEventListener('keydown', keydown, true);
    target.removeEventListener('beforeinput', beforeinput, true);
  };
}
