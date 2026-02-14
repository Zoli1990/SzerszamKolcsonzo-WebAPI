// ============================================================================
// src/services/pushService.js - Push Notification Service
// ============================================================================

import api from './api'

class PushService {
  constructor() {
    this.swRegistration = null
    this.subscription = null
    this.vapidPublicKey = null
  }

  // ============================================================================
  // VAPID KEY
  // ============================================================================
  
  async getVapidPublicKey() {
    if (this.vapidPublicKey) return this.vapidPublicKey
    
    try {
      const response = await api.get('/push/vapid-public-key')
      this.vapidPublicKey = response.data.publicKey
      return this.vapidPublicKey
    } catch (error) {
      console.error('❌ VAPID key lekérése sikertelen:', error)
      return null
    }
  }

  // ============================================================================
  // SERVICE WORKER
  // ============================================================================
  
  async registerServiceWorker() {
    if (!('serviceWorker' in navigator)) {
      console.warn('⚠️ Service Worker nem támogatott')
      return null
    }

    try {
      this.swRegistration = await navigator.serviceWorker.register('/sw.js', {
        scope: '/'
      })
      
      console.log('✅ Service Worker regisztrálva')
      
      // Frissítés figyelése
      this.swRegistration.addEventListener('updatefound', () => {
        const newWorker = this.swRegistration.installing
        newWorker.addEventListener('statechange', () => {
          if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
            window.dispatchEvent(new CustomEvent('sw-update-available'))
          }
        })
      })
      
      return this.swRegistration
    } catch (error) {
      console.error('❌ Service Worker regisztráció sikertelen:', error)
      return null
    }
  }

  async updateServiceWorker() {
    if (this.swRegistration?.waiting) {
      this.swRegistration.waiting.postMessage({ type: 'SKIP_WAITING' })
      window.location.reload()
    }
  }

  // ============================================================================
  // PUSH NOTIFICATION
  // ============================================================================
  
  isPushSupported() {
    return 'PushManager' in window && 'serviceWorker' in navigator
  }

  isNotificationSupported() {
    return 'Notification' in window
  }

  getPermissionStatus() {
    if (!this.isNotificationSupported()) return 'unsupported'
    return Notification.permission
  }

  async requestPermission() {
    if (!this.isNotificationSupported()) return 'unsupported'
    
    try {
      const permission = await Notification.requestPermission()
      console.log('🔔 Notification permission:', permission)
      return permission
    } catch (error) {
      console.error('❌ Permission request failed:', error)
      return 'denied'
    }
  }

  async subscribe(userEmail = null) {
    if (!this.isPushSupported()) {
      throw new Error('Push notifications not supported')
    }

    const permission = await this.requestPermission()
    if (permission !== 'granted') {
      throw new Error('Notification permission denied')
    }

    if (!this.swRegistration) {
      this.swRegistration = await navigator.serviceWorker.ready
    }

    const vapidKey = await this.getVapidPublicKey()
    if (!vapidKey) {
      throw new Error('VAPID key not available')
    }

    try {
      let subscription = await this.swRegistration.pushManager.getSubscription()
      
      if (!subscription) {
        subscription = await this.swRegistration.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey: this.urlBase64ToUint8Array(vapidKey)
        })
        console.log('✅ Új subscription létrehozva')
      }
      
      this.subscription = subscription
      await this.sendSubscriptionToServer(subscription, userEmail)
      
      return subscription
    } catch (error) {
      console.error('❌ Subscribe failed:', error)
      throw error
    }
  }

  async sendSubscriptionToServer(subscription, userEmail = null) {
    const subscriptionData = {
      endpoint: subscription.endpoint,
      p256dh: this.arrayBufferToBase64(subscription.getKey('p256dh')),
      auth: this.arrayBufferToBase64(subscription.getKey('auth')),
      email: userEmail
    }

    try {
      const response = await api.post('/push/subscribe', subscriptionData)
      console.log('✅ Subscription mentve:', response.data)
      return response.data
    } catch (error) {
      console.error('❌ Subscription mentés sikertelen:', error)
      throw error
    }
  }

  async unsubscribe() {
    if (!this.subscription) {
      this.subscription = await this.swRegistration?.pushManager.getSubscription()
    }

    if (!this.subscription) return true

    try {
      await api.post('/push/unsubscribe', {
        endpoint: this.subscription.endpoint
      })
    } catch (error) {
      console.warn('⚠️ Backend unsubscribe failed:', error)
    }

    try {
      const result = await this.subscription.unsubscribe()
      this.subscription = null
      return result
    } catch (error) {
      console.error('❌ Unsubscribe failed:', error)
      throw error
    }
  }

  async getSubscriptionStatus() {
    if (!this.swRegistration) {
      this.swRegistration = await navigator.serviceWorker.ready
    }

    const subscription = await this.swRegistration.pushManager.getSubscription()
    return {
      isSubscribed: !!subscription,
      subscription: subscription,
      permission: this.getPermissionStatus()
    }
  }

  // ============================================================================
  // HELPERS
  // ============================================================================
  
  urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4)
    const base64 = (base64String + padding)
      .replace(/-/g, '+')
      .replace(/_/g, '/')
    
    const rawData = window.atob(base64)
    const outputArray = new Uint8Array(rawData.length)
    
    for (let i = 0; i < rawData.length; ++i) {
      outputArray[i] = rawData.charCodeAt(i)
    }
    
    return outputArray
  }

  arrayBufferToBase64(buffer) {
    const bytes = new Uint8Array(buffer)
    let binary = ''
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i])
    }
    return window.btoa(binary)
  }

  async showLocalNotification(title, options = {}) {
    if (!this.isNotificationSupported()) return
    if (this.getPermissionStatus() !== 'granted') return

    if (this.swRegistration) {
      return this.swRegistration.showNotification(title, {
        icon: '/icons/icon-192x192.png',
        badge: '/icons/icon-72x72.png',
        vibrate: [100, 50, 100],
        ...options
      })
    }
  }
}

export const pushService = new PushService()
export default pushService
