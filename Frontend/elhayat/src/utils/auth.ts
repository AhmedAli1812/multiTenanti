import { decodeJwt } from 'jose'

export interface JwtPayload {
  sub: string
  name: string
  orgId: string
  role: string
  permission: string[]
  exp: number
}

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'

export function decodeToken(token: string): JwtPayload | null {
  try {
    const decoded = decodeJwt(token)
    return {
      sub: decoded.sub ?? '',
      name: (decoded[NAME_CLAIM] as string) ?? '',
      orgId: (decoded.orgId as string) ?? '',
      role: (decoded[ROLE_CLAIM] as string) ?? '',
      permission: (decoded.permission as string[]) ?? [],
      exp: decoded.exp ?? 0,
    }
  } catch {
    return null
  }
}

export function getRole(): string {
  const token = localStorage.getItem('token')
  if (!token) return ''
  return decodeToken(token)?.role ?? ''
}

export function getUserName(): string {
  const token = localStorage.getItem('token')
  if (!token) return ''
  return decodeToken(token)?.name ?? ''
}

export function isAuthenticated(): boolean {
  const token = localStorage.getItem('token')
  if (!token) return false
  const payload = decodeToken(token)
  if (!payload) return false
  return payload.exp * 1000 > Date.now()
}