import apiClient from './apiClient'

export interface LoginCredentials {
  identifier: string
  password: string
}

export interface LoginResponse {
  accessToken: string       // ✅ كان token
  refreshToken: string
  fullName: string          // ✅ بدل user object
  deviceId: string
}

export const authService = {
  login: async (credentials: LoginCredentials): Promise<LoginResponse> => {
    const { data } = await apiClient.post<LoginResponse>('/auth/login', credentials)
    return data
  },

  refreshToken: async (token: string): Promise<LoginResponse> => {
    const { data } = await apiClient.post<LoginResponse>('/auth/refresh-token', { token })
    return data
  },

  logout: (): void => {
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
  },
}