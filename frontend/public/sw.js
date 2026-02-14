// ============================================================================
// Service Worker - Szerszámkölcsönző PWA
// ============================================================================

const CACHE_NAME = 'szerszamkolcsonzo-v1';
const OFFLINE_URL = '/offline.html';

// Statikus fájlok cache-elésre
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/offline.html',
  '/manifest.json',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png'
];

// ============================================================================
// INSTALL - Service Worker telepítése
// ============================================================================
addEventListener

// ============================================================================
// ACTIVATE - Régi cache törlése
// ============================================================================
self.addEventListener('activate', (event) => {
  console.log('[SW] Activating Service Worker...');
  
  event.waitUntil(
    caches.keys()
      .then((cacheNames) => {
        return Promise.all(
          cacheNames
            .filter((name) => name !== CACHE_NAME)
            .map((name) => {
              console.log('[SW] Deleting old cache:', name);
              return caches.delete(name);
            })
        );
      })
      .then(() => {
        console.log('[SW] Activation complete');
        return self.clients.claim(); // Azonnal átvegye az irányítást
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
  console.log('[SW] Push received:', event);

  let data = {
    title: 'Szerszámkölcsönző',
    body: 'Új értesítés érkezett!',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/icon-72x72.png',
    tag: 'general',
    data: { url: '/' }
  };

  // JSON parse, ha van payload
  if (event.data) {
    try {
      const payload = event.data.json();
      data = { ...data, ...payload };
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
    requireInteraction: data.requireInteraction || false,
    data: data.data || { url: '/' },
    actions: data.actions || []
  };

  event.waitUntil(
    self.registration.showNotification(data.title, options)
  );
});

// ============================================================================
// NOTIFICATION CLICK - Értesítésre kattintás
// ============================================================================
self.addEventListener('notificationclick', (event) => {
  console.log('[SW] Notification clicked:', event);

  event.notification.close();

  const urlToOpen = event.notification.data?.url || '/';

  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true })
      .then((windowClients) => {
        // Ha már nyitva van az alkalmazás, fókuszáljunk rá
        for (const client of windowClients) {
          if (client.url.includes(self.location.origin) && 'focus' in client) {
            client.navigate(urlToOpen);
            return client.focus();
          }
        }
        // Ha nincs nyitva, nyissunk új ablakot
        if (clients.openWindow) {
          return clients.openWindow(urlToOpen);
        }
      })
  );
});

// ============================================================================
// MESSAGE - Kommunikáció az alkalmazással
// ============================================================================
self.addEventListener('message', (event) => {
  console.log('[SW] Message received:', event.data);

  if (event.data && event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }

  if (event.data && event.data.type === 'GET_VERSION') {
    event.ports[0].postMessage({ version: CACHE_NAME });
  }
});

console.log('[SW] Service Worker loaded');
