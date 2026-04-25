import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { authService, type LoginCredentials } from '../services/authService'
import { useLanguage } from '../context/LanguageContext'
import { translations } from '../context/translations'
import { isAuthenticated, getRole } from '../utils/auth'

interface LoginForm {
  identifier: string
  password: string
  rememberMe: boolean
}

interface UseLoginReturn {
  form: LoginForm
  isLoading: boolean
  error: string | null
  showPassword: boolean
  successMessage: string | null
  handleChange: (field: keyof LoginForm, value: string | boolean) => void
  handleSubmit: () => Promise<void>
  togglePassword: () => void
}

export function useLogin(): UseLoginReturn {
  const navigate = useNavigate()
  const { language } = useLanguage()
  const t = translations[language]

  const [form, setForm] = useState<LoginForm>({
    identifier: '',
    password: '',
    rememberMe: false,
  })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showPassword, setShowPassword] = useState(false)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const handleChange = (field: keyof LoginForm, value: string | boolean) => {
    setForm((prev) => ({ ...prev, [field]: value }))
    if (error) setError(null)
  }

  const handleSubmit = async () => {
    if (!form.identifier.trim() || !form.password.trim()) {
      setError(t.errorEmpty)
      return
    }

    setIsLoading(true)
    setError(null)

    try {
      const credentials: LoginCredentials = {
        identifier: form.identifier,
        password: form.password,
      }

      const response = await authService.login(credentials)

      localStorage.setItem('token', response.accessToken)
      localStorage.setItem('refreshToken', response.refreshToken)
      localStorage.setItem('user', JSON.stringify({ name: response.fullName }))

      console.log('token saved:', localStorage.getItem('token')?.substring(0, 20))
      console.log('isAuth:', isAuthenticated())
      console.log('role:', getRole())

      const welcome = language === 'ar'
        ? `👋 أهلاً بك، ${response.fullName}!`
        : `👋 Welcome, ${response.fullName}!`

      setSuccessMessage(welcome)

      setTimeout(() => {
        navigate('/dashboard')
      }, 2000)

    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        ?? t.errorInvalid
      setError(message)
    } finally {
      setIsLoading(false)
    }
  }

  const togglePassword = () => setShowPassword((prev) => !prev)

  return { form, isLoading, error, showPassword, successMessage, handleChange, handleSubmit, togglePassword }
}