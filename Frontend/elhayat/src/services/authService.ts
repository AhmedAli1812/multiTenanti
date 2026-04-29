// ─────────────────────────────────────────────────────────────────────────────
// src/services/authService.ts
//
// Wraps the backend /api/auth endpoints.
//
// FIXED BUGS:
//   • logout() now calls clearAuth() so orgId + user are also cleared
//   • Storage key constants are imported from utils/auth (single source)
// ─────────────────────────────────────────────────────────────────────────────
import apiClient from './apiClient'
import {
  decodeToken,
  clearAuth,
  STORAGE_KEY_TOKEN,
  STORAGE_KEY_REFRESH_TOKEN,
  STORAGE_KEY_USER,
  STORAGE_KEY_ORG_ID,
} from '../utils/auth'

// ── Request / Response shapes ─────────────────────────────────────────────────
export interface LoginCredentials {
  /** Can be email, phone, username, or nationalId — backend accepts all */
  identifier: string
  password: string
}

export interface LoginResponse {
  /** The signed JWT to attach as Bearer token */
  accessToken: string
  refreshToken: string
  /** User's display name — shown in the welcome message */
  fullName: string
  /** DeviceId returned by the backend for refresh-token binding */
  deviceId: string
}

export interface RefreshResponse {
  accessToken: string
  refreshToken: string
  fullName: string
  deviceId: string
}

// ── Service ───────────────────────────────────────────────────────────────────
export const authService = {
  /**
   * POST /api/auth/login
   *
   * On success:
   *   - Saves token under canonical key 'token'
   *   - Caches orgId from the JWT so getTenantId() has a fast path
   */
  login: async (credentials: LoginCredentials): Promise<LoginResponse> => {
    const { data } = await apiClient.post<LoginResponse>('auth/login', credentials)

    // Persist auth state
    localStorage.setItem(STORAGE_KEY_TOKEN, data.accessToken)
    localStorage.setItem(STORAGE_KEY_REFRESH_TOKEN, data.refreshToken)
    localStorage.setItem(STORAGE_KEY_USER, JSON.stringify({ name: data.fullName }))

    // Cache orgId so subsequent getTenantId() calls don't re-decode JWT
    const payload = decodeToken(data.accessToken)
    if (payload?.orgId) {
      localStorage.setItem(STORAGE_KEY_ORG_ID, payload.orgId)
    }

    return data
  },

  /**
   * POST /api/auth/refresh-token
   */
  refreshToken: async (token: string): Promise<RefreshResponse> => {
    const { data } = await apiClient.post<RefreshResponse>('/auth/refresh-token', { token })

    // Update persisted token on successful refresh
    localStorage.setItem(STORAGE_KEY_TOKEN, data.accessToken)
    localStorage.setItem(STORAGE_KEY_REFRESH_TOKEN, data.refreshToken)

    const payload = decodeToken(data.accessToken)
    if (payload?.orgId) {
      localStorage.setItem(STORAGE_KEY_ORG_ID, payload.orgId)
    }

    return data
  },

  /**
   * POST /api/auth/logout  (requires auth)
   *
   * Always clears local state even if the server call fails.
   */
  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/auth/logout')
    } finally {
      clearAuth()
    }
  },
}