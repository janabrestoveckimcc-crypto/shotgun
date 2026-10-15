import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import vm from 'node:vm';
test('extension plays one shot, skips passwords, and transmits no keystroke data',async()=>{
  const listeners={}, sent=[], changes=[];
  const chrome={storage:{local:{get:async()=>({settings:{enabled:true,repeat:false}})},onChanged:{addListener:fn=>changes.push(fn)}},runtime:{sendMessage:async message=>sent.push(message)}};
  vm.runInNewContext(await readFile(new URL('../extension/content.js',import.meta.url),'utf8'),{chrome,document:{addEventListener:(type,fn)=>listeners[type]=fn},setTimeout,clearTimeout});
  await new Promise(resolve=>setImmediate(resolve));
  const event={isTrusted:true,key:'s',code:'KeyS',repeat:false,composedPath:()=>[]};
  listeners.keydown(event); listeners.beforeinput({...event,inputType:'insertText'});
  assert.equal(sent.length,1);
  assert.equal(JSON.stringify(sent[0]),'{"target":"background","type":"shot"}');
  listeners.keydown({...event,composedPath:()=>[{tagName:'INPUT',type:'password'}]});
  listeners.keydown({...event,repeat:true}); listeners.beforeinput({...event,inputType:'insertText'});
  assert.equal(sent.length,1);
  changes[0]({settings:{newValue:{enabled:false}}},'local'); listeners.keydown(event); assert.equal(sent.length,1);
});
test('extension asks only for settings, audio and declared web-page scripts',async()=>{
  const manifest=JSON.parse(await readFile(new URL('../extension/manifest.json',import.meta.url),'utf8'));
  assert.deepEqual(manifest.permissions,['storage','offscreen']);
  assert.equal(manifest.manifest_version,3);
  assert.equal(manifest.content_scripts[0].all_frames,true);
  assert.ok(!manifest.externally_connectable);
});
