import test from 'node:test';
import assert from 'node:assert/strict';
import { attachKeyboard } from '../shared/input.js';
import { normalizeSettings } from '../shared/settings.js';
import { synthesize } from '../scripts/generate-assets.mjs';
class Target {
  listeners = new Map();
  addEventListener(type, listener) { this.listeners.set(type,listener); }
  removeEventListener(type) { this.listeners.delete(type); }
  emit(type, values={}) { this.listeners.get(type)?.({ isTrusted:true, key:'a', code:'KeyA', repeat:false, ...values }); }
}
test('one physical press plus beforeinput plays exactly one shot', () => {
  const target = new Target(), shots=[];
  const detach = attachKeyboard(target,{ fire:code=>shots.push(code) });
  for (let i=0;i<100;i++) { target.emit('keydown'); target.emit('beforeinput',{inputType:'insertText'}); }
  assert.equal(shots.length,100); detach(); assert.equal(target.listeners.size,0);
});
test('holding a key is silent unless repeat is enabled', () => {
  const target=new Target(); let shots=0, repeat=false;
  const detach=attachKeyboard(target,{ fire:()=>shots++, repeat:()=>repeat });
  target.emit('keydown'); target.emit('beforeinput');
  target.emit('keydown',{repeat:true}); target.emit('beforeinput');
  assert.equal(shots,1);
  repeat=true; target.emit('keydown',{repeat:true}); target.emit('beforeinput');
  assert.equal(shots,2); detach();
});
test('Android IME keyCode 229 falls back to one shot per input event', () => {
  const target=new Target(); let shots=0;
  const detach=attachKeyboard(target,{fire:()=>shots++});
  target.emit('keydown',{keyCode:229,key:'Unidentified'});
  target.emit('beforeinput',{inputType:'insertCompositionText',isComposing:true});
  assert.equal(shots,1); detach();
});
test('iOS input-only edits fire; paste, disabled and untrusted events do not', () => {
  const target=new Target(); let shots=0, enabled=true;
  const detach=attachKeyboard(target,{fire:()=>shots++, enabled:()=>enabled});
  target.emit('beforeinput',{inputType:'insertText'}); target.emit('beforeinput',{inputType:'deleteContentBackward'});
  target.emit('beforeinput',{inputType:'insertFromPaste'}); target.emit('keydown',{isTrusted:false});
  enabled=false; target.emit('keydown'); target.emit('beforeinput');
  assert.equal(shots,2); detach();
});
test('settings reject invalid sounds, volumes and accidental enabling',()=>{
  assert.deepEqual(normalizeSettings({enabled:'true',sound:'bad',volume:Infinity}),{enabled:false,sound:'pump',volume:.35,repeat:false});
  assert.equal(normalizeSettings({volume:2}).volume,1);
  assert.equal(normalizeSettings({volume:-2}).volume,0);
});
test('original effects are non-silent, finite, peak limited and decay to silence',()=>{
  for(const sound of ['pump','double','tactical']){
    const samples=synthesize(sound);
    assert.equal(samples.length,28665);
    assert.ok(samples.every(Number.isFinite));
    assert.ok(samples.some(value=>Math.abs(value)>.25));
    assert.ok(samples.every(value=>Math.abs(value)<.85));
    assert.ok(Math.abs(samples.at(-1))<.001);
  }
});
