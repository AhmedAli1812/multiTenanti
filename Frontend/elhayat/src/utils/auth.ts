// ─────────────────────────────────────────────────────────────────────────────
// src/utils/auth.ts
//
// Single source of truth for all JWT decoding and auth checks.
// ─────────────────────────────────────────────────────────────────────────────
import { decodeJwt } from 'jose'

const ROLE_CLAIM  = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAME_CLAIM  = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

// ─── Storage key constants (one place to change if needed) ──────────────────
export const STORAGE_KEY_TOKEN         = 'token'
export const STORAGE_KEY_REFRESH_TOKEN = 'refreshToken'
export const STORAGE_KEY_USER          = 'user'
export const STORAGE_KEY_ORG_ID        = 'orgId'

// ─── JWT payload shape ────────────────────────────────────────────────────────
export interface JwtPayload {
  sub: string
  name: string
  /** Tenant GUID — emitted as "orgId" claim by the .NET JwtService */
  orgId: string
  orgName?: string
  branchId?: string
  branchName?: string
  roles: string[]
  permission: string[]
  exp: number
}

// ─────────────────────────────────────────────────────────────────────────────
// decodeToken — parses the JWT and normalises claims.
//
// The .NET JwtService emits roles under the long WS-Federation claim URI.
// We unpack them into a plain string array.
// ─────────────────────────────────────────────────────────────────────────────
export function decodeToken(token: string): JwtPayload | null {
  try {
    const decoded  = decodeJwt(token)
    const rawRole  = decoded[ROLE_CLAIM]
    const roles    = Array.isArray(rawRole) ? rawRole : rawRole ? [rawRole as string] : []
    return {
      sub:        decoded.sub ?? '',
      name:       (decoded[NAME_CLAIM] as string) ?? '',
      orgId:      (decoded['orgId'] as string) ?? '',
      orgName:    (decoded['orgName'] as string) ?? '',
      branchId:   (decoded['branchId'] as string) ?? '',
      branchName: (decoded['branchName'] as string) ?? '',
      roles,
      permission: (decoded['permission'] as string[]) ?? [],
      exp:        decoded.exp ?? 0,
    }
  } catch {
    return null
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// getStoredToken — canonical token retriever (prefer 'token' key)
// ─────────────────────────────────────────────────────────────────────────────
export function getStoredToken(): string | null {
  return localStorage.getItem(STORAGE_KEY_TOKEN)
}

// ─────────────────────────────────────────────────────────────────────────────
// getTenantId — reads orgId from the JWT claim.
//
// The claim key priority matches the backend CurrentUser.cs:
//   orgId → tenantId → tenant_id → TenantId → tid
// ─────────────────────────────────────────────────────────────────────────────
export function getTenantId(): string {
  // Fast path: already cached in localStorage from login
  const cached = localStorage.getItem(STORAGE_KEY_ORG_ID)
  if (cached) return cached

  const token = getStoredToken()
  if (!token) return ''
  try {
    const d = decodeJwt(token)
    return (
      (d['orgId']     as string) ??
      (d['tenantId']  as string) ??
      (d['tenant_id'] as string) ??
      (d['TenantId']  as string) ??
      (d['tid']       as string) ??
      ''
    )
  } catch {
    return ''
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// getRole — returns the first non-Patient role.
// ─────────────────────────────────────────────────────────────────────────────
export function getRole(): string {
  const token = getStoredToken()
  if (!token) return ''
  const roles = decodeToken(token)?.roles ?? []
  return roles.find(r => r !== 'Patient') ?? roles[0] ?? ''
}

export function isSuperAdmin(): boolean {
  const token = getStoredToken()
  if (!token) return false
  const roles = decodeToken(token)?.roles ?? []
  return roles.some(r => ['SuperAdmin', 'Super Admin'].includes(r))
}

export function getUserName(): string {
  const token = getStoredToken()
  if (!token) return ''
  return decodeToken(token)?.name ?? ''
}

export function getOrgName(): string {
  const token = getStoredToken()
  if (!token) return ''
  return decodeToken(token)?.orgName ?? ''
}

export function getBranchName(): string {
  const token = getStoredToken()
  if (!token) return ''
  return decodeToken(token)?.branchName ?? ''
}

// ─────────────────────────────────────────────────────────────────────────────
// isAuthenticated — checks token existence + expiry.
// ─────────────────────────────────────────────────────────────────────────────
export function isAuthenticated(): boolean {
  const token = getStoredToken()
  if (!token) return false
  const payload = decodeToken(token)
  if (!payload) return false
  return payload.exp * 1000 > Date.now()
}

// ─────────────────────────────────────────────────────────────────────────────
// clearAuth — wipes all auth-related keys from localStorage.
// Call this on logout OR on 401.
// ─────────────────────────────────────────────────────────────────────────────
export function clearAuth(): void {
  localStorage.removeItem(STORAGE_KEY_TOKEN)
  localStorage.removeItem(STORAGE_KEY_REFRESH_TOKEN)
  localStorage.removeItem(STORAGE_KEY_USER)
  localStorage.removeItem(STORAGE_KEY_ORG_ID)
}