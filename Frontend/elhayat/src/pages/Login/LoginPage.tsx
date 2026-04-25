import { Link } from 'react-router-dom'
import { useLogin } from '../../hooks/useLogin'
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
        <button className="login-controls__btn" onClick={toggleLanguage}>
          {language === 'ar' ? 'EN' : 'عربي'}
        </button>
        <button className="login-controls__btn" onClick={toggleTheme}>
          {theme === 'light' ? '🌙' : '☀️'}
        </button>
      </div>

      <div className="login-panel login-panel--hero">
        <div className="login-hero__brand">
          <span className="login-hero__brand-text">MedScope</span>
          <span className="login-hero__brand-icon">🏥</span>
        </div>
        <div className="login-hero__badge">{t.badge}</div>
        <h2 className="login-hero__headline">{t.headline}</h2>
        <p className="login-hero__sub">{t.sub}</p>
        <div className="login-hero__trust">
          <div className="login-hero__avatars">
            <span className="login-hero__avatar">👨‍⚕️</span>
            <span className="login-hero__avatar">👩‍⚕️</span>
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
                <span className="login-field__icon">✉️</span>
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
                  {showPassword ? '🙈' : '👁️'}
                </button>
                <span className="login-field__icon login-field__icon--lock">🔒</span>
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
                {successMessage}
              </div>
            )}

            {/* ❌ Error Message */}
            {error && (
              <div className="login-form__error" role="alert">
                ⚠️ {t.errorInvalid}
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
                <span>✅</span>
              ) : (
                <>
                  <span>{t.submit}</span>
                  <span>←</span>
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