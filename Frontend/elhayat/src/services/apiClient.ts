import axios from 'axios'
import { decodeToken } from '../utils/auth'

const apiClient = axios.create({
  baseURL: 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
})

function getToken(): string | null {
  return localStorage.getItem('accessToken') ?? localStorage.getItem('token')
}

apiClient.interceptors.request.use((config) => {
  const token = getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
    const payload = decodeToken(token)
    if (payload?.orgId) {
      config.headers['X-Tenant-Id'] = payload.orgId
    }
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken')
      localStorage.removeItem('token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default apiClient