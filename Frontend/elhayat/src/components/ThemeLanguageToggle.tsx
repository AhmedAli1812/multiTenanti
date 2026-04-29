import { useTheme } from '../context/ThemeContext'
import { useLanguage } from '../context/LanguageContext'
import { Moon, Sun, Languages } from 'lucide-react'
import './ThemeLanguageToggle.css'

export default function ThemeLanguageToggle() {
  const { theme, toggleTheme } = useTheme()
  const { language, toggleLanguage } = useLanguage()

  return (
    <div className="settings-toggles">
      <button 
        className="settings-btn" 
        onClick={toggleTheme}
        title={theme === 'light' ? 'الوضع الداكن' : 'الوضع الفاتح'}
      >
        {theme === 'light' ? <Moon size={20} /> : <Sun size={20} />}
      </button>
      <button 
        className="settings-btn lang-btn" 
        onClick={toggleLanguage}
        title={language === 'ar' ? 'English' : 'العربية'}
      >
        <Languages size={20} />
        <span className="lang-text">{language === 'ar' ? 'EN' : 'AR'}</span>
      </button>
    </div>
  )
}
