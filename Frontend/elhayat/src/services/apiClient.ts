// ─────────────────────────────────────────────────────────────────────────────
// src/services/apiClient.ts
//
// Centralised Axios instance. All features import from here.
//
// Request interceptor:
//   1. Attaches Bearer token from localStorage
//   2. Attaches X-Tenant-Id from JWT claim (orgId) — this is the primary
//      header the backend's TenantProvider reads (Priority 2 in TenantProvider)
//
// Response interceptor:
//   401 → clear all auth state and redirect to /login
// ─────────────────────────────────────────────────────────────────────────────
import axios from 'axios'
import { getStoredToken, getTenantId, clearAuth } from '../utils/auth'

// In dev: Vite proxies /api → https://localhost:7154/api (see vite.config.ts)
// In prod: set VITE_API_BASE_URL in your .env file
const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api'

const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15_000,
})

// ── Request interceptor ───────────────────────────────────────────────────────
apiClient.interceptors.request.use((config) => {
  const token = getStoredToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`

    // X-Tenant-Id is the header the backend TenantProvider uses as Priority 2.
    // It is also read from the JWT claim (Priority 3), but we set it
    // explicitly for clarity and to support Super Admin header overrides.
    const tenantId = getTenantId()
    if (tenantId) {
      config.headers['X-Tenant-Id'] = tenantId
    }
  }
  return config
})

// ── Response interceptor ──────────────────────────────────────────────────────
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      clearAuth()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)

export default apiClient