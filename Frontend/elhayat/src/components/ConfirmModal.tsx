
import Modal from './Modal'
import './ConfirmModal.css'

interface ConfirmModalProps {
  isOpen: boolean
  onClose: () => void
  onConfirm: () => void
  title: string
  message: string
  confirmText?: string
  cancelText?: string
  variant?: 'danger' | 'success' | 'info'
  isLoading?: boolean
}

export default function ConfirmModal({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = 'تأكيد',
  cancelText = 'إلغاء',
  variant = 'info',
  isLoading = false
}: ConfirmModalProps) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} maxWidth="400px">
      <div className="confirm-modal-body">
        <div className={`confirm-icon-wrap confirm-icon-wrap--${variant}`}>
          {variant === 'danger' && <span>⚠️</span>}
          {variant === 'success' && <span>✅</span>}
          {variant === 'info' && <span>ℹ️</span>}
        </div>
        <p className="confirm-message">{message}</p>
        <div className="confirm-actions">
          <button 
            className="confirm-btn confirm-btn--ghost" 
            onClick={onClose}
            disabled={isLoading}
          >
            {cancelText}
          </button>
          <button 
            className={`confirm-btn confirm-btn--${variant}`} 
            onClick={onConfirm}
            disabled={isLoading}
          >
            {isLoading ? 'جارٍ التنفيذ...' : confirmText}
          </button>
        </div>
      </div>
    </Modal>
  )
}
