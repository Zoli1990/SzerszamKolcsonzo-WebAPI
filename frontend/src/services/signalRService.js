// ============================================================================
// src/services/signalRService.js — SignalR kliens
// ============================================================================

import * as signalR from '@microsoft/signalr'

const API_BASE = import.meta.env.VITE_API_BASE_URL
// Az API URL-ből levágjuk az /api végződést a hub URL-hez
const HUB_URL = API_BASE.replace(/\/api\/?$/, '') + '/hubs/eszkoz'

let connection = null
let reconnectAttempts = 0
const MAX_RECONNECT = 5

/**
 * SignalR kapcsolat indítása + eseményfigyelés
 * @param {Function} onStatuszValtozas - callback: ({ eszkozId, eszkozNev, ujStatusz, esemeny }) => {}
 */
export function startSignalR(onStatuszValtozas) {
  if (connection) {
    console.log('[SignalR] Már csatlakozva')
    return
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      withCredentials: false,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build()

  // ═══════════════════════════════════════════════════════════════════
  // ESEMÉNYEK
  // ═══════════════════════════════════════════════════════════════════

  // Eszköz státusz változás
  connection.on('EszkozStatuszValtozas', (data) => {
    console.log('[SignalR] Státusz változás:', data)
    if (onStatuszValtozas) {
      onStatuszValtozas(data)
    }
  })

  // Kapcsolat események
  connection.onreconnecting((error) => {
    console.warn('[SignalR] Újracsatlakozás...', error)
  })

  connection.onreconnected((connectionId) => {
    console.log('[SignalR] Újracsatlakozva:', connectionId)
    reconnectAttempts = 0
  })

  connection.onclose((error) => {
    console.warn('[SignalR] Kapcsolat lezárva:', error)
    connection = null

    // Manuális újrapróbálkozás ha az automatikus elfogyott
    if (reconnectAttempts < MAX_RECONNECT) {
      reconnectAttempts++
      setTimeout(() => startSignalR(onStatuszValtozas), 5000 * reconnectAttempts)
    }
  })

  // ═══════════════════════════════════════════════════════════════════
  // INDÍTÁS
  // ═══════════════════════════════════════════════════════════════════
  connection
    .start()
    .then(() => {
      console.log('[SignalR] Csatlakozva:', HUB_URL)
      reconnectAttempts = 0
    })
    .catch((err) => {
      console.error('[SignalR] Csatlakozás sikertelen:', err)
      connection = null
    })
}

/**
 * SignalR kapcsolat leállítása
 */
export function stopSignalR() {
  if (connection) {
    connection.stop()
    connection = null
    console.log('[SignalR] Leállítva')
  }
}

/**
 * Kapcsolat állapota
 */
export function isConnected() {
  return connection?.state === signalR.HubConnectionState.Connected
}
