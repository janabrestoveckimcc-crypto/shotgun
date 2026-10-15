const CACHE = 'shotgun-keyboard-v2';
const FILES = ['./','./index.html','./site.css','./landing.js','./demo.html','./style.css','./app.js','./shared/audio.js','./shared/settings.js','./shared/input.js','./manifest.webmanifest','./assets/audio/pump.wav','./assets/audio/double.wav','./assets/audio/tactical.wav','./assets/icons/icon-48.png','./assets/icons/icon-192.png','./assets/icons/icon-512.png'];
self.addEventListener('install', event => { event.waitUntil(caches.open(CACHE).then(cache => cache.addAll(FILES)).then(() => self.skipWaiting())); });
self.addEventListener('activate', event => { event.waitUntil(caches.keys().then(keys => Promise.all(keys.filter(key => key.startsWith('shotgun-keyboard-') && key !== CACHE).map(key => caches.delete(key)))).then(() => self.clients.claim())); });
self.addEventListener('fetch', event => {
  if (event.request.method !== 'GET' || new URL(event.request.url).origin !== self.location.origin || new URL(event.request.url).pathname.includes('/downloads/')) return;
  event.respondWith(fetch(event.request).then(response => {
    if (response.ok) { const copy = response.clone(); caches.open(CACHE).then(cache => cache.put(event.request, copy)); }
    return response;
  }).catch(() => caches.match(event.request).then(cached => cached || (event.request.mode === 'navigate' ? caches.match('./index.html') : Response.error()))));
});
