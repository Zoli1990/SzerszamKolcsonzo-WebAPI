// ============================================================================
// Service Worker - Admin PWA (HASH MODE ROUTING)
// ============================================================================

const CACHE_NAME = 'admin-pwa-v1';
const OFFLINE_URL = '/offline.html';

// Statikus fájlok cache-elésre
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/offline.html',
  '/manifest-admin.json',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png'
];

// ============================================================================
// INSTALL - Service Worker telepítése
// ============================================================================
self.addEventListener('install', (event) => {
  console.log('[Admin SW] Installing Service Worker...');
  
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => {
        console.log('[Admin SW] Caching static assets');
        return cache.addAll(STATIC_ASSETS);
      })
      .then(() => {
        console.log('[Admin SW] Install complete');
        return self.skipWaiting();
      })
  );
});

// ============================================================================
// ACTIVATE - Régi cache törlése
// ============================================================================
self.addEventListener('activate', (event) => {
  console.log('[Admin SW] Activating Service Worker...');
  
  event.waitUntil(
    caches.keys()
      .then((cacheNames) => {
        return Promise.all(
          cacheNames
            .filter((name) => name !== CACHE_NAME)
            .map((name) => {
              console.log('[Admin SW] Deleting old cache:', name);
              return caches.delete(name);
            })
        );
      })
      .then(() => {
        console.log('[Admin SW] Activation complete');
        return self.clients.claim();
      })
  );
});

// ============================================================================
// FETCH - Network first, cache fallback stratégia
// ============================================================================
self.addEventListener('fetch', (event) => {
  // Csak GET kéréseket cache-elünk
  if (event.request.method !== 'GET') {
    return;
  }

  // API kéréseket nem cache-elünk
  if (event.request.url.includes('/api/')) {
    return;
  }

  event.respondWith(
    fetch(event.request)
      .then((response) => {
        // Sikeres válasz - cache-eljük
        if (response.status === 200) {
          const responseClone = response.clone();
          caches.open(CACHE_NAME).then((cache) => {
            cache.put(event.request, responseClone);
          });
        }
        return response;
      })
      .catch(async () => {
        // Offline - cache-ből próbálunk
        const cachedResponse = await caches.match(event.request);
        
        if (cachedResponse) {
          return cachedResponse;
        }

        // Ha navigációs kérés és nincs cache, offline oldal
        if (event.request.mode === 'navigate') {
          const offlinePage = await caches.match(OFFLINE_URL);
          if (offlinePage) {
            return offlinePage;
          }
        }

        // Utolsó mentsvár
        return new Response('Offline - tartalom nem elérhető', {
          status: 503,
          statusText: 'Service Unavailable'
        });
      })
  );
});

// ============================================================================
// PUSH - Push értesítések kezelése
// ============================================================================
self.addEventListener('push', (event) => {
  console.log('[Admin SW] Push received:', event);

  let data = {
    title: 'Szerszámkölcsönző - Admin',
    body: 'Új értesítés érkezett!',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/icon-72x72.png',
    tag: 'general',
    data: { url: '/#/admin-pwa' }
  };

  // JSON parse, ha van payload
  if (event.data) {
    try {
      const payload = event.data.json();
      data = { ...data, ...payload };
      console.log('[Admin SW] Push payload:', payload);
    } catch (e) {
      data.body = event.data.text();
    }
  }

  const options = {
    body: data.body,
    icon: data.icon || '/icons/icon-192x192.png',
    badge: data.badge || '/icons/icon-72x72.png',
    tag: data.tag || 'notification',
    vibrate: [200, 100, 200],
    requireInteraction: data.requireInteraction || true,
    data: data.data || { url: '/#/admin-pwa' },
    actions: data.actions || []
  };

  event.waitUntil(
    self.registration.showNotification(data.title, options)
  );
});

// ============================================================================
// NOTIFICATION CLICK - Értesítésre kattintás (HASH MODE)
// ============================================================================
self.addEventListener('notificationclick', (event) => {
  console.log('[Admin SW] Notification clicked:', event);

  event.notification.close();

  // Hash mode URL (pl: https://domain.com/#/admin-pwa?tab=foglalasok)
  const urlToOpen = event.notification.data?.url || '/#/admin-pwa';
  
  // FONTOS: Teljes URL kell hash mode-nál
  const fullUrl = new URL(urlToOpen, self.location.origin).href;

  console.log('[Admin SW] Opening URL:', fullUrl);

  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true })
      .then((windowClients) => {
        // Ha már nyitva van az alkalmazás, fókuszáljunk rá
        for (const client of windowClients) {
          if (client.url.includes(self.location.origin) && 'focus' in client) {
            console.log('[Admin SW] Navigating existing window to:', fullUrl);
            client.navigate(fullUrl);
            return client.focus();
          }
        }
        // Ha nincs nyitva, nyissunk új ablakot
        if (clients.openWindow) {
          console.log('[Admin SW] Opening new window:', fullUrl);
          return clients.openWindow(fullUrl);
        }
      })
  );
});

// ============================================================================
// MESSAGE - Kommunikáció az alkalmazással
// ============================================================================
self.addEventListener('message', (event) => {
  console.log('[Admin SW] Message received:', event.data);

  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }

  if (event.data && event.data.type === 'GET_VERSION') {
    event.ports[0].postMessage({ version: CACHE_NAME });
  }
});

console.log('[Admin SW] Service Worker loaded');