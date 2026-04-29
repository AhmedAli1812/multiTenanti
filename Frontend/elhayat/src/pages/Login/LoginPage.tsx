import { Link } from 'react-router-dom'
import { useLogin } from '../../hooks/useLogin'
import { 
  Hospital, Mail, Lock, Eye, EyeOff, User, 
  ChevronLeft, Moon, Sun, Globe, CheckCircle2, AlertCircle
} from 'lucide-react'
import './LoginPage.css'
import { useTheme } from '../../context/ThemeContext'
import { useLanguage } from '../../context/LanguageContext'
import { translations } from '../../context/translations'

export default function LoginPage() {
  const { form, isLoading, error, successMessage, showPassword, handleChange, handleSubmit, togglePassword } =
    useLogin()

  const { theme, toggleTheme } = useTheme()
  const { language, toggleLanguage } = useLanguage()
  const t = translations[language]

  return (
    <div className="login-page" dir={language === 'ar' ? 'rtl' : 'ltr'}>
      {/* Controls */}
      <div className="login-controls">
        <button className="login-controls__btn" onClick={toggleLanguage} title={language === 'ar' ? 'English' : 'العربية'}>
          <Globe size={16} />
          <span>{language === 'ar' ? 'EN' : 'AR'}</span>
        </button>
        <button className="login-controls__btn" onClick={toggleTheme} title={theme === 'light' ? 'Dark Mode' : 'Light Mode'}>
          {theme === 'light' ? <Moon size={16} /> : <Sun size={16} />}
        </button>
      </div>

      <div className="login-panel login-panel--hero">
        <div className="login-hero__brand">
          <span className="login-hero__brand-text">MedScope</span>
          <Hospital size={32} color="#fff" strokeWidth={2.5} />
        </div>
        <div className="login-hero__badge">{t.badge}</div>
        <h2 className="login-hero__headline">{t.headline}</h2>
        <p className="login-hero__sub">{t.sub}</p>
        <div className="login-hero__trust">
          <div className="login-hero__avatars">
            <span className="login-hero__avatar"><User size={16} /></span>
            <span className="login-hero__avatar"><User size={16} /></span>
          </div>
          <div className="login-hero__trust-text">
            <strong>{t.trusted}</strong>
            <span>{t.trustedSub}</span>
          </div>
          <span className="login-hero__count">500+</span>
        </div>
      </div>

      <div className="login-panel login-panel--form">
        <div className="login-form-wrapper">
          <h1 className="login-form__title">{t.welcome}</h1>
          <p className="login-form__subtitle">{t.subtitle}</p>

          <div className="login-form">
            <div className="login-field">
              <label className="login-field__label">{t.username}</label>
              <div className="login-field__input-wrap">
                <input
                  className="login-field__input"
                  type="text"
                  placeholder={t.username}
                  value={form.identifier}
                  onChange={(e) => handleChange('identifier', e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSubmit()}
                  disabled={isLoading}
                />
                <Mail className="login-field__icon" size={18} />
              </div>
            </div>

            <div className="login-field">
              <label className="login-field__label">{t.password}</label>
              <div className="login-field__input-wrap">
                <input
                  className="login-field__input"
                  type={showPassword ? 'text' : 'password'}
                  placeholder={t.password}
                  value={form.password}
                  onChange={(e) => handleChange('password', e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSubmit()}
                  disabled={isLoading}
                />
                <button type="button" className="login-field__toggle" onClick={togglePassword}>
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
                <Lock className="login-field__icon login-field__icon--lock" size={18} />
              </div>
            </div>

            <div className="login-form__row">
              <label className="login-checkbox">
                <input
                  type="checkbox"
                  checked={form.rememberMe}
                  onChange={(e) => handleChange('rememberMe', e.target.checked)}
                />
                <span>{t.remember}</span>
              </label>
              <Link to="/forgot-password" className="login-form__forgot">
                {t.forgot}
              </Link>
            </div>

            {/* ✅ Success Message */}
            {successMessage && (
              <div className="login-form__success" role="status">
                <CheckCircle2 size={18} />
                <span>{successMessage}</span>
              </div>
            )}

            {/* ❌ Error Message */}
            {error && (
              <div className="login-form__error" role="alert">
                <AlertCircle size={18} />
                <span>{t.errorInvalid}</span>
              </div>
            )}

            <button
              className="login-form__submit"
              onClick={handleSubmit}
              disabled={isLoading || !!successMessage}
            >
              {isLoading ? (
                <span className="login-form__spinner" />
              ) : successMessage ? (
                <CheckCircle2 size={24} />
              ) : (
                <>
                  <span>{t.submit}</span>
                  {language === 'ar' ? <ChevronLeft size={20} /> : <ChevronLeft size={20} style={{ transform: 'rotate(180deg)' }} />}
                </>
              )}
            </button>

            <p className="login-form__register">
              {t.noAccount}{' '}
              <a href="mailto:admin@ether.com" className="login-form__contact">
                {t.contact}
              </a>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}