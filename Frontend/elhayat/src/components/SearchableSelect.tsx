import React, { useState, useRef, useEffect, useMemo } from 'react'
import { ChevronDown, Check } from 'lucide-react'
import './SearchableSelect.css'

interface Option {
  id: string
  label: string
  [key: string]: any
}

interface SearchableSelectProps {
  options: Option[]
  value: string
  onChange: (value: string) => void
  placeholder: string
  label?: string
  required?: boolean
  disabled?: boolean
  icon?: React.ReactNode
}

export default function SearchableSelect({
  options,
  value,
  onChange,
  placeholder,
  label,
  required,
  disabled,
  icon
}: SearchableSelectProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const containerRef = useRef<HTMLDivElement>(null)

  // Find the selected option's label for display
  const selectedOption = useMemo(() => 
    options.find(opt => opt.id === value),
  [options, value])

  // Filter options based on search term
  const filteredOptions = useMemo(() => {
    if (!searchTerm) return options
    const lowerSearch = searchTerm.toLowerCase()
    return options.filter(opt => 
      opt.label.toLowerCase().includes(lowerSearch)
    )
  }, [options, searchTerm])

  // Handle click outside to close
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false)
        setSearchTerm('')
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const handleSelect = (id: string) => {
    onChange(id)
    setIsOpen(false)
    setSearchTerm('')
  }

  return (
    <div className={`ss-container ${disabled ? 'disabled' : ''}`} ref={containerRef}>
      {label && (
        <label className="ss-label">
          {label} {required && <span className="pif-required">*</span>}
        </label>
      )}
      
      <div className={`ss-input-wrap ${isOpen ? 'active' : ''}`} onClick={() => !disabled && setIsOpen(!isOpen)}>
        {icon && <span className="ss-icon">{icon}</span>}
        
        <div className="ss-display">
          {isOpen ? (
            <input
              autoFocus
              className="ss-search-input"
              placeholder="ابحث هنا..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              onClick={(e) => e.stopPropagation()}
            />
          ) : (
            <span className={`ss-value ${!selectedOption ? 'placeholder' : ''}`}>
              {selectedOption ? selectedOption.label : placeholder}
            </span>
          )}
        </div>
        
        <span className={`ss-arrow ${isOpen ? 'up' : ''}`}>
          <ChevronDown size={18} />
        </span>
      </div>

      {isOpen && !disabled && (
        <div className="ss-dropdown">
          {filteredOptions.length > 0 ? (
            filteredOptions.map((opt) => (
              <div
                key={opt.id}
                className={`ss-option ${opt.id === value ? 'selected' : ''}`}
                onClick={() => handleSelect(opt.id)}
              >
                {opt.label}
                {opt.id === value && <span className="ss-check"><Check size={14} /></span>}
              </div>
            ))
          ) : (
            <div className="ss-no-results">لا توجد نتائج</div>
          )}
        </div>
      )}
    </div>
  )
}
