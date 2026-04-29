// ─────────────────────────────────────────────────────────────────────────────
// src/hooks/useLogin.ts
//
// FIXED:
//   - All localStorage writes moved into authService.login() so there's
//     a single owner of auth persistence.
//   - getRole/isAuthenticated used for redirect guard after login.
// ─────────────────────────────────────────────────────────────────────────────
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
        identifier: form.identifier.trim(),
        password: form.password,
      }

      // authService.login() handles all localStorage persistence internally
      const response = await authService.login(credentials)

      const welcome =
        language === 'ar'
          ? `👋 أهلاً بك، ${response.fullName}!`
          : `👋 Welcome, ${response.fullName}!`

      setSuccessMessage(welcome)

      // Short delay so the user sees the success message, then redirect
      setTimeout(() => {
        // Verify auth state and pick the right dashboard
        if (isAuthenticated()) {
          const role = getRole()
          if (role === 'Receptionist' || role === 'Reception') {
            navigate('/dashboard')
          } else {
            navigate('/dashboard')
          }
        }
      }, 1500)
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string; title?: string } } }
      const message =
        axiosErr?.response?.data?.message ??
        axiosErr?.response?.data?.title ??
        t.errorInvalid
      setError(message)
    } finally {
      setIsLoading(false)
    }
  }

  const togglePassword = () => setShowPassword((prev) => !prev)

  return {
    form,
    isLoading,
    error,
    showPassword,
    successMessage,
    handleChange,
    handleSubmit,
    togglePassword,
  }
}