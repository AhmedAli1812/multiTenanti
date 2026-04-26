import { decodeJwt } from 'jose'

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

export interface JwtPayload {
  sub: string
  name: string
  orgId: string
  roles: string[]   // ← array مش string
  permission: string[]
  exp: number
}

export function decodeToken(token: string): JwtPayload | null {
  try {
    const decoded = decodeJwt(token)
    const rawRole = decoded[ROLE_CLAIM]
    const roles = Array.isArray(rawRole) ? rawRole : rawRole ? [rawRole as string] : []
    return {
      sub: decoded.sub ?? '',
      name: (decoded[NAME_CLAIM] as string) ?? '',
      orgId: (decoded.orgId as string) ?? '',
      roles,
      permission: (decoded.permission as string[]) ?? [],
      exp: decoded.exp ?? 0,
    }
  } catch {
    return null
  }
}

function getToken(): string | null {
return localStorage.getItem('token') ?? localStorage.getItem('accessToken')
}

export function getRole(): string {
  const token = getToken()
  if (!token) return ''
  const roles = decodeToken(token)?.roles ?? []
  // بيرجع أول role مش Patient
  return roles.find(r => r !== 'Patient') ?? roles[0] ?? ''
}

export function getUserName(): string {
  const token = getToken()
  if (!token) return ''
  return decodeToken(token)?.name ?? ''
}

export function isAuthenticated(): boolean {
  const token = getToken()
  if (!token) return false
  const payload = decodeToken(token)
  if (!payload) return false
  return payload.exp * 1000 > Date.now()
}