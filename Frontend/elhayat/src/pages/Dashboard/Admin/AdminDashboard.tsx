import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { 
  LayoutDashboard, Users, ShieldCheck, Bed, ClipboardList, Building2, Globe, 
  Plus, X, LogOut, Hospital, Calendar
} from 'lucide-react'
import { getUserName, getRole, getOrgName, getBranchName, isSuperAdmin } from '../../../utils/auth'
import { authService } from '../../../services/authService'
import { useUsers, useRoles, usePermissions, useAuditLogs, useBranches, useTenants, useRooms, useFloors, useDepartments, useStats } from '../../../hooks/useAdmin'
import type { CreateUserDto, CreateRoomDto, CreateFloorDto, CreateDepartmentDto, CreateTenantDto } from '../../../services/adminService'
import ThemeLanguageToggle from '../../../components/ThemeLanguageToggle'
import './AdminDashboard.css'

type TabType = 'dashboard' | 'users' | 'roles' | 'audit' | 'branches' | 'tenants' | 'rooms'

function initials(name: string) {
  if (!name) return '؟'
  const parts = name.trim().split(' ')
  return parts.length >= 2 ? parts[0][0] + parts[1][0] : parts[0][0]
}

function formatDate(d: string) {
  if (!d) return '—'
  try { return new Date(d).toLocaleDateString('ar-EG') } catch { return d }
}

// ===== Modal: Add User =====
function AddUserModal({ onClose, onSubmit }: {
  onClose: () => void
  onSubmit: (dto: CreateUserDto) => Promise<void>
}) {
  const superAdmin = isSuperAdmin()
  const { tenants } = useTenants()
  const { branches, loadForTenant } = useBranches()
  
  const [form, setForm] = useState<CreateUserDto>({ 
    fullName: '', 
    email: '', 
    username: '', 
    password: '',
    tenantId: '',
    branchId: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handle = (f: keyof CreateUserDto, v: string) => {
    setForm(p => ({ ...p, [f]: v }))
    
    // If tenant changed, reload branches
    if (f === 'tenantId' && v) {
      loadForTenant(v)
    }
  }

  const submit = async () => {
    if (!form.fullName || !form.email || !form.username || !form.password) {
      setError('جميع الحقول المطلوبة (الاسم، البريد، اسم المستخدم، كلمة المرور)')
      return
    }

    if (superAdmin && !form.tenantId) {
      setError('يجب اختيار المستأجر (Tenant)')
      return
    }

    setIsLoading(true)
    try {
      await onSubmit(form)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل إضافة المستخدم')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">إضافة مستخدم جديد</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error">{error}</div>}
          <div className="ad-form-grid">
            <div className="ad-field">
              <label className="ad-label">الاسم الكامل</label>
              <input className="ad-input" value={form.fullName} onChange={e => handle('fullName', e.target.value)} placeholder="أحمد محمد علي" />
            </div>
            <div className="ad-field">
              <label className="ad-label">البريد الإلكتروني</label>
              <input className="ad-input" type="email" value={form.email} onChange={e => handle('email', e.target.value)} placeholder="example@hospital.com" />
            </div>
            <div className="ad-field">
              <label className="ad-label">اسم المستخدم</label>
              <input className="ad-input" value={form.username} onChange={e => handle('username', e.target.value)} placeholder="ahmed.ali" />
            </div>
            <div className="ad-field">
              <label className="ad-label">كلمة المرور</label>
              <input className="ad-input" type="password" value={form.password} onChange={e => handle('password', e.target.value)} placeholder="••••••••" />
            </div>

            {/* 🔥 Tenant Selection (Super Admin Only) */}
            {superAdmin && (
              <div className="ad-field">
                <label className="ad-label">المستأجر (Tenant)</label>
                <select className="ad-input" value={form.tenantId} onChange={e => handle('tenantId', e.target.value)}>
                  <option value="">— اختر المستأجر —</option>
                  {tenants.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
                </select>
              </div>
            )}

            {/* 🔥 Branch Selection */}
            <div className="ad-field">
              <label className="ad-label">الفرع (Branch)</label>
              <select className="ad-input" value={form.branchId} onChange={e => handle('branchId', e.target.value)} disabled={superAdmin && !form.tenantId}>
                <option value="">— اختر الفرع —</option>
                {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
              </select>
            </div>

          </div>
        </div>
        <div className="ad-modal__footer">
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading}>
            {isLoading ? <span className="ad-spinner" /> : 'إضافة المستخدم'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Modal: Assign Role =====
function AssignRoleModal({ userId, userName, roles, onClose, onSubmit }: {
  userId: string
  userName: string
  roles: { id: string; name: string }[]
  onClose: () => void
  onSubmit: (userId: string, roleId: string) => Promise<void>
}) {
  const [selectedRole, setSelectedRole] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const submit = async () => {
    if (!selectedRole) return
    setIsLoading(true)
    setError(null)
    try {
      await onSubmit(userId, selectedRole)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل تعيين الدور')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal ad-modal--sm">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">تعيين دور — {userName}</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error" style={{ marginBottom: '1rem' }}>{error}</div>}
          <div className="ad-field">
            <label className="ad-label" htmlFor="role-select">اختر الدور</label>
            <select id="role-select" className="ad-input" value={selectedRole} onChange={e => setSelectedRole(e.target.value)}>
              <option value="">— اختر —</option>
              {roles.map(r => <option key={r.id} value={r.id}>{r.name}</option>)}
            </select>
          </div>
        </div>
        <div className="ad-modal__footer">
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading || !selectedRole}>
            {isLoading ? <span className="ad-spinner" /> : 'تعيين'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Modal: Assign Permissions =====
function AssignPermissionsModal({ roleId, roleName, initialPermissions, allPermissions, onClose, onSubmit }: {
  roleId: string
  roleName: string
  initialPermissions: string[]
  allPermissions: { id: string; code: string }[]
  onClose: () => void
  onSubmit: (roleId: string, permissionIds: string[]) => Promise<void>
}) {
  const [selected, setSelected] = useState<string[]>(() => 
    allPermissions.filter(p => initialPermissions.includes(p.code)).map(p => p.id)
  )
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const toggle = (id: string) =>
    setSelected(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id])

  const submit = async () => {
    setIsLoading(true)
    setError(null)
    try {
      await onSubmit(roleId, selected)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل حفظ الصلاحيات')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">تعيين صلاحيات — {roleName}</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error" style={{ marginBottom: '1rem' }}>{error}</div>}
          {allPermissions.length === 0 ? (
            <div className="ad-table__empty">لا توجد صلاحيات متاحة</div>
          ) : (
            <div className="ad-permissions-grid">
              {allPermissions.map(p => (
                <label key={p.id} className={`ad-permission-item ${selected.includes(p.id) ? 'active' : ''}`}>
                  <input type="checkbox" checked={selected.includes(p.id)} onChange={() => toggle(p.id)} />
                  <span>{p.code}</span>
                </label>
              ))}
            </div>
          )}
        </div>
        <div className="ad-modal__footer">
          <span className="ad-modal__count">{selected.length} صلاحية محددة</span>
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading}>
            {isLoading ? <span className="ad-spinner" /> : 'حفظ الصلاحيات'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Modal: Add Role =====
function AddRoleModal({ onClose, onSubmit }: {
  onClose: () => void
  onSubmit: (name: string) => Promise<void>
}) {
  const [name, setName] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const submit = async () => {
    if (!name) return
    setIsLoading(true)
    setError(null)
    try {
      await onSubmit(name)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل إضافة الدور')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal ad-modal--sm">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">إضافة دور جديد</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error" style={{ marginBottom: '1rem' }}>{error}</div>}
          <div className="ad-field">
            <label className="ad-label">اسم الدور</label>
            <input 
              className="ad-input" 
              value={name} 
              onChange={e => setName(e.target.value)} 
              placeholder="مثلاً: مدير النظام" 
              autoFocus
            />
          </div>
        </div>
        <div className="ad-modal__footer">
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading || !name}>
            {isLoading ? <span className="ad-spinner" /> : 'إضافة'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Modal: Add Branch =====
function AddBranchModal({ onClose, onSubmit }: {
  onClose: () => void
  onSubmit: (dto: { name: string, address: string, phone: string }) => Promise<void>
}) {
  const [form, setForm] = useState({ name: '', address: '', phone: '' })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handle = (f: string, v: string) => setForm(p => ({ ...p, [f]: v }))

  const submit = async () => {
    if (!form.name) return
    setIsLoading(true)
    setError(null)
    try {
      await onSubmit(form)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل إضافة الفرع')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal ad-modal--sm">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">إضافة فرع جديد</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error" style={{ marginBottom: '1rem' }}>{error}</div>}
          <div className="ad-form-grid">
            <div className="ad-field">
              <label className="ad-label">اسم الفرع</label>
              <input className="ad-input" value={form.name} onChange={e => handle('name', e.target.value)} placeholder="فرع القاهرة" autoFocus />
            </div>
            <div className="ad-field">
              <label className="ad-label">العنوان</label>
              <input className="ad-input" value={form.address} onChange={e => handle('address', e.target.value)} placeholder="شارع التحرير" />
            </div>
            <div className="ad-field">
              <label className="ad-label">رقم الهاتف</label>
              <input className="ad-input" value={form.phone} onChange={e => handle('phone', e.target.value)} placeholder="010..." />
            </div>
          </div>
        </div>
        <div className="ad-modal__footer">
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading || !form.name}>
            {isLoading ? <span className="ad-spinner" /> : 'إضافة'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Modal: Add Room =====
function AddRoomModal({ onClose, onSubmit }: {
  onClose: () => void
  onSubmit: (dto: CreateRoomDto) => Promise<void>
}) {
  const superAdmin = isSuperAdmin()
  const { tenants } = useTenants()
  const { branches, loadForTenant } = useBranches()
  const { floors, loadForBranch } = useFloors()

  const [form, setForm] = useState<CreateRoomDto>({
    roomNumber: '',
    name: '',
    type: 1, // Default: Clinic
    capacity: 1,
    floorId: '',
    branchId: '',
    tenantId: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handle = (f: keyof CreateRoomDto, v: any) => {
    setForm(p => ({ ...p, [f]: v }))

    if (f === 'tenantId' && v) {
      loadForTenant(v)
      setForm(p => ({ ...p, branchId: '', floorId: '' }))
    }
    if (f === 'branchId' && v) {
      loadForBranch(v)
      setForm(p => ({ ...p, floorId: '' }))
    }
  }

  const submit = async () => {
    if (!form.roomNumber || !form.name || !form.floorId || !form.branchId) {
      setError('جميع الحقول مطلوبة')
      return
    }
    if (superAdmin && !form.tenantId) {
      setError('يجب اختيار المستأجر')
      return
    }

    setIsLoading(true)
    try {
      await onSubmit(form)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل إضافة الغرفة')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal ad-modal--sm">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">إضافة غرفة جديدة</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error" style={{ marginBottom: '1rem' }}>{error}</div>}
          <div className="ad-form-grid">
            <div className="ad-field">
              <label className="ad-label">رقم الغرفة</label>
              <input className="ad-input" value={form.roomNumber} onChange={e => handle('roomNumber', e.target.value)} placeholder="101" autoFocus />
            </div>
            <div className="ad-field">
              <label className="ad-label">اسم الغرفة (للعرض)</label>
              <input className="ad-input" value={form.name} onChange={e => handle('name', e.target.value)} placeholder="غرفة الكشف 1" />
            </div>
            <div className="ad-field">
              <label className="ad-label">نوع الغرفة</label>
              <select className="ad-input" value={form.type} onChange={e => handle('type', parseInt(e.target.value))}>
                <option value={1}>عيادة (Clinic)</option>
                <option value={2}>طوارئ (ER)</option>
                <option value={3}>رعاية مركزة (ICU)</option>
                <option value={4}>عمليات (Operation)</option>
              </select>
            </div>
            <div className="ad-field">
              <label className="ad-label">السعة (عدد الأسرة)</label>
              <input className="ad-input" type="number" min="1" value={form.capacity} onChange={e => handle('capacity', parseInt(e.target.value))} />
            </div>

            {superAdmin && (
              <div className="ad-field">
                <label className="ad-label">المستأجر</label>
                <select className="ad-input" value={form.tenantId} onChange={e => handle('tenantId', e.target.value)}>
                  <option value="">— اختر —</option>
                  {tenants.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
                </select>
              </div>
            )}

            <div className="ad-field">
              <label className="ad-label">الفرع</label>
              <select className="ad-input" value={form.branchId} onChange={e => handle('branchId', e.target.value)} disabled={superAdmin && !form.tenantId}>
                <option value="">— اختر —</option>
                {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
              </select>
            </div>

            <div className="ad-field">
              <label className="ad-label">الطابق</label>
              <select className="ad-input" value={form.floorId} onChange={e => handle('floorId', e.target.value)} disabled={!form.branchId}>
                <option value="">— اختر —</option>
                {floors.map(f => <option key={f.id} value={f.id}>{f.name}</option>)}
              </select>
            </div>
          </div>
        </div>
        <div className="ad-modal__footer">
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading}>
            {isLoading ? <span className="ad-spinner" /> : 'إضافة'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Modal: Add Floor =====
function AddFloorModal({ onClose, onSubmit }: {
  onClose: () => void
  onSubmit: (dto: CreateFloorDto) => Promise<void>
}) {
  const superAdmin = isSuperAdmin()
  const { tenants } = useTenants()
  const { branches, loadForTenant } = useBranches()

  const [form, setForm] = useState<CreateFloorDto>({
    name: '',
    number: 0,
    branchId: '',
    tenantId: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handle = (f: keyof CreateFloorDto, v: any) => {
    setForm(p => ({ ...p, [f]: v }))
    if (f === 'tenantId' && v) {
      loadForTenant(v)
      setForm(p => ({ ...p, branchId: '' }))
    }
  }

  const submit = async () => {
    if (!form.name || !form.branchId) {
      setError('جميع الحقول مطلوبة')
      return
    }
    setIsLoading(true)
    try {
      await onSubmit(form)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل إضافة الطابق')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal ad-modal--sm">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">إضافة طابق جديد</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error" style={{ marginBottom: '1rem' }}>{error}</div>}
          <div className="ad-form-grid">
            <div className="ad-field">
              <label className="ad-label">اسم الطابق</label>
              <input className="ad-input" value={form.name} onChange={e => handle('name', e.target.value)} placeholder="الطابق الأول" autoFocus />
            </div>
            <div className="ad-field">
              <label className="ad-label">رقم الطابق</label>
              <input className="ad-input" type="number" value={form.number} onChange={e => handle('number', parseInt(e.target.value))} />
            </div>

            {superAdmin && (
              <div className="ad-field">
                <label className="ad-label">المستأجر</label>
                <select className="ad-input" value={form.tenantId} onChange={e => handle('tenantId', e.target.value)}>
                  <option value="">— اختر —</option>
                  {tenants.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
                </select>
              </div>
            )}

            <div className="ad-field">
              <label className="ad-label">الفرع</label>
              <select className="ad-input" value={form.branchId} onChange={e => handle('branchId', e.target.value)} disabled={superAdmin && !form.tenantId}>
                <option value="">— اختر —</option>
                {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
              </select>
            </div>
          </div>
        </div>
        <div className="ad-modal__footer">
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading}>
            {isLoading ? <span className="ad-spinner" /> : 'إضافة'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Modal: Add Department =====
function AddDepartmentModal({ onClose, onSubmit }: {
  onClose: () => void
  onSubmit: (dto: CreateDepartmentDto) => Promise<void>
}) {
  const superAdmin = isSuperAdmin()
  const { tenants } = useTenants()
  const { branches, loadForTenant } = useBranches()

  const [form, setForm] = useState<CreateDepartmentDto>({
    name: '',
    branchId: '',
    tenantId: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handle = (f: keyof CreateDepartmentDto, v: any) => {
    setForm(p => ({ ...p, [f]: v }))
    if (f === 'tenantId' && v) {
      loadForTenant(v)
      setForm(p => ({ ...p, branchId: '' }))
    }
  }

  const submit = async () => {
    if (!form.name || !form.branchId) {
      setError('جميع الحقول مطلوبة')
      return
    }
    setIsLoading(true)
    try {
      await onSubmit(form)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل إضافة القسم')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal ad-modal--sm">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">إضافة قسم جديد</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error" style={{ marginBottom: '1rem' }}>{error}</div>}
          <div className="ad-form-grid">
            <div className="ad-field">
              <label className="ad-label">اسم القسم</label>
              <input className="ad-input" value={form.name} onChange={e => handle('name', e.target.value)} placeholder="قسم الطوارئ" autoFocus />
            </div>

            {superAdmin && (
              <div className="ad-field">
                <label className="ad-label">المستأجر</label>
                <select className="ad-input" value={form.tenantId} onChange={e => handle('tenantId', e.target.value)}>
                  <option value="">— اختر —</option>
                  {tenants.map(t => <option key={t.id} value={t.id}>{t.name}</option>)}
                </select>
              </div>
            )}

            <div className="ad-field">
              <label className="ad-label">الفرع</label>
              <select className="ad-input" value={form.branchId} onChange={e => handle('branchId', e.target.value)} disabled={superAdmin && !form.tenantId}>
                <option value="">— اختر —</option>
                {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
              </select>
            </div>
          </div>
        </div>
        <div className="ad-modal__footer">
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading}>
            {isLoading ? <span className="ad-spinner" /> : 'إضافة'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Modal: Add Tenant =====
function AddTenantModal({ onClose, onSubmit }: {
  onClose: () => void
  onSubmit: (dto: CreateTenantDto) => Promise<void>
}) {
  const [form, setForm] = useState<CreateTenantDto>({ name: '', code: '' })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handle = (f: keyof CreateTenantDto, v: string) => setForm(p => ({ ...p, [f]: v }))

  const submit = async () => {
    if (!form.name || !form.code) {
      setError('جميع الحقول مطلوبة (الاسم، الكود)')
      return
    }
    setIsLoading(true)
    try {
      await onSubmit(form)
      onClose()
    } catch (err: any) {
      setError(err.response?.data?.message || 'فشل إضافة المستأجر')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal ad-modal--sm">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">إضافة مستأجر جديد</h2>
          <button className="ad-modal__close" onClick={onClose}><X size={20} /></button>
        </div>
        <div className="ad-modal__body">
          {error && <div className="ad-error">{error}</div>}
          <div className="ad-form-grid">
            <div className="ad-field">
              <label className="ad-label">اسم المنظمة / المستشفى</label>
              <input className="ad-input" value={form.name} onChange={e => handle('name', e.target.value)} placeholder="مثلاً: مستشفى الحياة" autoFocus />
            </div>
            <div className="ad-field">
              <label className="ad-label">كود النظام (Code)</label>
              <input className="ad-input" value={form.code} onChange={e => handle('code', e.target.value)} placeholder="elhayat" />
              <small style={{ color: '#64748b', marginTop: 4, display: 'block' }}>يستخدم هذا الكود في عملية تسجيل الدخول</small>
            </div>
          </div>
        </div>
        <div className="ad-modal__footer">
          <button className="ad-btn ad-btn--ghost" onClick={onClose}>إلغاء</button>
          <button className="ad-btn ad-btn--primary" onClick={submit} disabled={isLoading}>
            {isLoading ? <span className="ad-spinner" /> : 'إضافة المستأجر'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ===== Main Component =====
export default function AdminDashboard() {
  const navigate = useNavigate()
  const userName = getUserName()
  const roleName = getRole()
  const orgName = getOrgName()
  const branchName = getBranchName()
  const superAdmin = isSuperAdmin()
  const [activeTab, setActiveTab] = useState<TabType>('dashboard')
  const [search, setSearch] = useState('')
  const [showAddUser, setShowAddUser] = useState(false)
  const [showAddRole, setShowAddRole] = useState(false)
  const [showAddBranch, setShowAddBranch] = useState(false)
  const [showAddRoom, setShowAddRoom] = useState(false)
  const [showAddFloor, setShowAddFloor] = useState(false)
  const [showAddDept, setShowAddDept] = useState(false)
  const [showAddTenant, setShowAddTenant] = useState(false)
  const [assignRoleTarget, setAssignRoleTarget] = useState<{ id: string; name: string } | null>(null)
  const [assignPermsTarget, setAssignPermsTarget] = useState<{ id: string; name: string; permissions: string[] } | null>(null)
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null)

  const { users, isLoading: usersLoading, error: usersError, createUser, deleteUser, assignRole } = useUsers()
  const { roles, isLoading: rolesLoading, refresh: rolesRefresh, createRole } = useRoles()
  const { permissions, assignToRole } = usePermissions()
  const { logs, isLoading: auditLoading, error: auditError } = useAuditLogs()
  const { branches, isLoading: branchesLoading, createBranch } = useBranches()
  const { tenants, isLoading: tenantsLoading, error: tenantsError, createTenant } = useTenants()
  const { rooms, isLoading: roomsLoading, createRoom } = useRooms()
  const { createFloor } = useFloors()
  const { createDepartment } = useDepartments()
  const { stats, isLoading: statsLoading } = useStats()

  const filteredUsers = users.filter(u =>
    (u.fullName || '').toLowerCase().includes(search.toLowerCase()) ||
    (u.email || '').toLowerCase().includes(search.toLowerCase())
  )

  const today = new Date().toLocaleDateString('ar-EG', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })

  return (
    <div className="ad-wrap" dir="rtl">
      {/* Topbar */}
      <header className="ad-topbar">
        <div className="ad-topbar__brand">
          <Hospital size={24} className="ad-topbar__logo" color="var(--accent-color)" />
          <div className="ad-topbar__org-info">
            <span className="ad-topbar__name">MedScope</span>
            {orgName && <span className="ad-topbar__org-name">{orgName} {branchName && <span className="ad-topbar__branch-name">| {branchName}</span>}</span>}
          </div>
        </div>
        <div className="ad-topbar__center">
          <Calendar size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
          {today}
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem' }}>
          <ThemeLanguageToggle />
          <div className="ad-topbar__user">
            <span className="ad-topbar__username">{userName}</span>
            <span className="ad-topbar__role-badge">{roleName || 'Super Admin'}</span>
            <button className="ad-topbar__logout" onClick={() => { authService.logout(); navigate('/login') }}>
              <LogOut size={14} style={{ marginLeft: 4 }} />
              خروج
            </button>
          </div>
        </div>
      </header>

      <div className="ad-layout">
        {/* Sidebar */}
        <aside className="ad-sidebar">
          <nav className="ad-nav">
            {([
              { key: 'dashboard', icon: <LayoutDashboard size={18} />, label: 'الإحصائيات' },
              { key: 'users', icon: <Users size={18} />, label: 'المستخدمون' },
              { key: 'roles', icon: <ShieldCheck size={18} />, label: 'الأدوار والصلاحيات' },
              { key: 'rooms', icon: <Bed size={18} />, label: 'الغرف' },
              { key: 'audit', icon: <ClipboardList size={18} />, label: 'سجل المراجعة' },
              { key: 'branches', icon: <Building2 size={18} />, label: 'الفروع' },
              ...(superAdmin ? [{ key: 'tenants', icon: <Globe size={18} />, label: 'المستأجرون (Tenants)' }] as any : [])
            ] as { key: TabType; icon: React.ReactNode; label: string }[]).map(item => (
              <button
                key={item.key}
                className={`ad-nav__item ${activeTab === item.key ? 'active' : ''}`}
                onClick={() => setActiveTab(item.key)}
              >
                <span>{item.icon}</span>
                {item.label}
              </button>
            ))}
          </nav>
        </aside>

        {/* Main */}
        <main className="ad-main">

          {/* ===== Dashboard Tab ===== */}
          {activeTab === 'dashboard' && (
            <div className="ad-section">
              <div className="ad-section__header">
                <div>
                  <h1 className="ad-section__title">لوحة الإحصائيات</h1>
                  <p className="ad-section__sub">ملخص عام لأداء المستشفى هذا الشهر</p>
                </div>
              </div>

              {statsLoading || !stats ? (
                <div className="ad-table__empty"><span className="ad-spinner" /> جارٍ التحميل...</div>
              ) : (
                <div className="ad-dashboard-grid">
                  {/* Summary Cards */}
                  <div className="ad-stats-cards">
                    <div className="ad-stat-card">
                      <div className="ad-stat-card__icon" style={{ backgroundColor: '#f0f9ff', color: '#0ea5e9' }}>📊</div>
                      <div className="ad-stat-card__info">
                        <span className="ad-stat-card__label">إجمالي الحالات هذا الشهر</span>
                        <div className="ad-stat-card__row">
                          <span className="ad-stat-card__value">{stats.summary.totalCases}</span>
                          <span className={`ad-stat-card__growth ${stats.summary.growthPercentage >= 0 ? 'up' : 'down'}`}>
                            {stats.summary.growthPercentage >= 0 ? '↑' : '↓'} {Math.abs(stats.summary.growthPercentage)}%
                          </span>
                        </div>
                      </div>
                    </div>
                    <div className="ad-stat-card">
                      <div className="ad-stat-card__icon" style={{ backgroundColor: '#f0fdf4', color: '#22c55e' }}>💵</div>
                      <div className="ad-stat-card__info">
                        <span className="ad-stat-card__label">الحالات النقدية</span>
                        <div className="ad-stat-card__row">
                          <span className="ad-stat-card__value">{stats.summary.cashCases}</span>
                          <span className={`ad-stat-card__growth ${stats.summary.cashGrowth >= 0 ? 'up' : 'down'}`}>
                            {stats.summary.cashGrowth >= 0 ? '↑' : '↓'} {Math.abs(stats.summary.cashGrowth)}%
                          </span>
                        </div>
                      </div>
                    </div>
                    <div className="ad-stat-card">
                      <div className="ad-stat-card__icon" style={{ backgroundColor: '#fffbeb', color: '#eab308' }}>📋</div>
                      <div className="ad-stat-card__info">
                        <span className="ad-stat-card__label">حالات العقود</span>
                        <div className="ad-stat-card__row">
                          <span className="ad-stat-card__value">{stats.summary.contractCases}</span>
                          <span className={`ad-stat-card__growth ${stats.summary.contractGrowth >= 0 ? 'up' : 'down'}`}>
                            {stats.summary.contractGrowth >= 0 ? '↑' : '↓'} {Math.abs(stats.summary.contractGrowth)}%
                          </span>
                        </div>
                      </div>
                    </div>
                    <div className="ad-stat-card">
                      <div className="ad-stat-card__icon" style={{ backgroundColor: '#eff6ff', color: '#3b82f6' }}>👨‍⚕️</div>
                      <div className="ad-stat-card__info">
                        <span className="ad-stat-card__label">حالات تحويل الأطباء</span>
                        <div className="ad-stat-card__row">
                          <span className="ad-stat-card__value">{stats.summary.referralCases}</span>
                          <span className={`ad-stat-card__growth ${stats.summary.referralGrowth >= 0 ? 'up' : 'down'}`}>
                            {stats.summary.referralGrowth >= 0 ? '↑' : '↓'} {Math.abs(stats.summary.referralGrowth)}%
                          </span>
                        </div>
                      </div>
                    </div>
                    <div className="ad-stat-card">
                      <div className="ad-stat-card__icon" style={{ 
                        backgroundColor: stats.summary.growthPercentage >= 0 ? '#f0fdf4' : '#fef2f2', 
                        color: stats.summary.growthPercentage >= 0 ? '#22c55e' : '#ef4444' 
                      }}>
                        {stats.summary.growthPercentage >= 0 ? '📈' : '📉'}
                      </div>
                      <div className="ad-stat-card__info">
                        <span className="ad-stat-card__label">معدل النمو الشهري</span>
                        <div className="ad-stat-card__row">
                          <span className="ad-stat-card__value" style={{ color: stats.summary.growthPercentage >= 0 ? '#22c55e' : '#ef4444' }}>
                            {stats.summary.growthPercentage >= 0 ? '+' : ''}{stats.summary.growthPercentage}%
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div className="ad-dashboard-main">
                    {/* Charts Row */}
                    <div className="ad-charts-row">
                      <div className="ad-chart-container ad-card">
                        <h3 className="ad-card-title">توزيع الحالات حسب الدفع</h3>
                        <div className="ad-pie-chart-wrap">
                          <svg viewBox="0 0 100 100" className="ad-pie-chart">
                            {(() => {
                              let currentAngle = 0
                              return stats.payerDistribution.map((item, i) => {
                                const percentage = (item.value / (stats.summary.totalCases || 1)) * 360
                                if (percentage === 0) return null
                                
                                // Special case: Full circle (360 degrees)
                                if (percentage >= 359.9) {
                                  return <circle key={i} cx="50" cy="50" r="40" fill={item.color} />
                                }

                                const x1 = 50 + 40 * Math.cos((currentAngle * Math.PI) / 180)
                                const y1 = 50 + 40 * Math.sin((currentAngle * Math.PI) / 180)
                                currentAngle += percentage
                                const x2 = 50 + 40 * Math.cos((currentAngle * Math.PI) / 180)
                                const y2 = 50 + 40 * Math.sin((currentAngle * Math.PI) / 180)
                                const largeArc = percentage > 180 ? 1 : 0
                                return (
                                  <path
                                    key={i}
                                    d={`M 50 50 L ${x1} ${y1} A 40 40 0 ${largeArc} 1 ${x2} ${y2} Z`}
                                    fill={item.color}
                                  />
                                )
                              })
                            })()}
                          </svg>
                          <div className="ad-pie-legend">
                            {stats.payerDistribution.map((item, i) => (
                              <div key={i} className="ad-legend-item">
                                <span className="ad-legend-color" style={{ backgroundColor: item.color }}></span>
                                <span className="ad-legend-label">{item.name} ({item.value})</span>
                              </div>
                            ))}
                          </div>
                        </div>
                      </div>

                      <div className="ad-chart-container ad-card">
                        <h3 className="ad-card-title">عدد الحالات - آخر 6 أشهر</h3>
                        <div className="ad-bar-chart-wrap">
                          <div className="ad-bar-chart">
                            {stats.monthlyChart.map((item, i) => {
                              const max = Math.max(...stats.monthlyChart.map(m => m.count), 1)
                              const height = (item.count / max) * 100
                              return (
                                <div key={i} className="ad-bar-col">
                                  <div className="ad-bar" style={{ height: `${height}%` }}>
                                    <span className="ad-bar-tooltip">{item.count}</span>
                                  </div>
                                  <span className="ad-bar-label">{item.month}</span>
                                </div>
                              )
                            })}
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Doctor Analytics Sections */}
                    <div className="ad-dashboard-row-3" style={{ marginTop: '1.5rem' }}>
                      {/* Top 10 Doctors Section */}
                      <div className="ad-card">
                        <div className="ad-card-header">
                          <h3 className="ad-card-title">أفضل 10 أطباء</h3>
                          <p className="ad-card-sub">ترتيب الأطباء بناءً على إجمالي الحالات هذا الشهر</p>
                        </div>
                        <table className="ad-mini-table">
                          <thead>
                            <tr>
                              <th style={{ textAlign: 'right' }}>اسم الطبيب</th>
                              <th style={{ textAlign: 'center' }}>الحالات</th>
                              <th style={{ textAlign: 'left' }}>الترتيب</th>
                            </tr>
                          </thead>
                          <tbody>
                            {stats.topDoctors.map((doc, i) => (
                              <tr key={i}>
                                <td>
                                  <div className="ad-cell-doctor" style={{ justifyContent: 'flex-start' }}>
                                    {doc.imageUrl ? (
                                      <img src={doc.imageUrl} className="ad-doctor-avatar" alt="" />
                                    ) : (
                                      <div className="ad-avatar ad-avatar--xs" style={{ width: 24, height: 24, fontSize: 10 }}>
                                        {doc.doctorName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                                      </div>
                                    )}
                                    <span className="ad-muted" style={{ fontSize: 13 }}>{doc.doctorName}</span>
                                  </div>
                                </td>
                                <td style={{ textAlign: 'center', fontWeight: 600 }}>{doc.totalCases.toLocaleString()}</td>
                                <td>
                                  <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
                                    <div className={`ad-rank ad-rank--${i < 3 ? i + 1 : 'other'}`}>
                                      {i + 1}
                                    </div>
                                  </div>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>

                      {/* Doctors Monthly Comparison Table */}
                      <div className="ad-card">
                        <div className="ad-card-header">
                          <h3 className="ad-card-title">مقارنة الأداء الشهري</h3>
                        </div>
                        <table className="ad-mini-table">
                          <thead>
                            <tr>
                              <th style={{ textAlign: 'right' }}>اسم الطبيب</th>
                              <th style={{ textAlign: 'center' }}>الشهر السابق</th>
                              <th style={{ textAlign: 'center' }}>الشهر الحالي</th>
                              <th style={{ textAlign: 'center' }}>الفرق</th>
                              <th style={{ textAlign: 'left' }}>الحالة</th>
                            </tr>
                          </thead>
                          <tbody>
                            {stats.topDoctors.slice(0, 5).map((doc, i) => (
                              <tr key={i}>
                                <td style={{ textAlign: 'right' }}>{doc.doctorName}</td>
                                <td style={{ textAlign: 'center' }}>{doc.lastMonthCases}</td>
                                <td style={{ textAlign: 'center' }}>{doc.totalCases}</td>
                                <td style={{ textAlign: 'center', color: doc.diff >= 0 ? '#22c55e' : '#ef4444', fontWeight: 600 }}>
                                  {doc.diff >= 0 ? '+' : ''}{doc.diff}
                                </td>
                                <td>
                                  <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
                                    <span className={`ad-status-pill ${doc.status.toLowerCase()}`} style={{ fontSize: 9 }}>
                                      {doc.status === 'Increased' ? 'زاد' : doc.status === 'Decreased' ? 'نقص' : 'ثابت'}
                                    </span>
                                  </div>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>

                      {/* Doctors Performance Table */}
                      <div className="ad-card">
                        <div className="ad-card-header">
                          <h3 className="ad-card-title">تحليل نوعية الحالات</h3>
                        </div>
                        <table className="ad-mini-table">
                          <thead>
                            <tr>
                              <th style={{ textAlign: 'right' }}>الطبيب</th>
                              <th style={{ textAlign: 'center' }}>نقدي</th>
                              <th style={{ textAlign: 'center' }}>عقود</th>
                              <th style={{ textAlign: 'left' }}>الإجمالي</th>
                            </tr>
                          </thead>
                          <tbody>
                            {stats.topDoctors.slice(0, 5).map((doc, i) => (
                              <tr key={i} style={{ backgroundColor: i % 2 === 1 ? '#fcfcfc' : 'transparent' }}>
                                <td>
                                  <div className="ad-cell-doctor" style={{ justifyContent: 'flex-start' }}>
                                    <div className="ad-avatar ad-avatar--xs" style={{ width: 24, height: 24, fontSize: 10, background: '#f1f5f9', color: '#64748b' }}>
                                      {doc.doctorName.split(' ').map(n => n[0]).join('').slice(0, 2)}
                                    </div>
                                    <span style={{ fontSize: 13 }}>{doc.doctorName}</span>
                                  </div>
                                </td>
                                <td style={{ textAlign: 'center' }}>{doc.cashCases}</td>
                                <td style={{ textAlign: 'center' }}>{doc.contractCases}</td>
                                <td style={{ textAlign: 'left', fontWeight: 700, color: '#1a5faa' }}>{doc.totalCases}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>

                    <div className="ad-card" style={{ marginTop: '1.5rem' }}>
                      <h3 className="ad-card-title">تحليل التخصصات</h3>
                      
                      <div className="ad-specialty-summary">
                        {(() => {
                          const sorted = [...stats.specialties].sort((a, b) => b.count - a.count);
                          const highest = sorted[0];
                          const lowest = sorted[sorted.length - 1];
                          return (
                            <>
                              <div className="ad-specialty-box ad-specialty-box--highest">
                                <span className="ad-specialty-box__label">أعلى تخصص هذا الشهر</span>
                                <span className="ad-specialty-box__value">{highest?.specialty || '—'}</span>
                              </div>
                              <div className="ad-specialty-box ad-specialty-box--lowest">
                                <span className="ad-specialty-box__label">أدنى تخصص هذا الشهر</span>
                                <span className="ad-specialty-box__value">{lowest?.specialty || '—'}</span>
                              </div>
                            </>
                          );
                        })()}
                      </div>

                      <div className="ad-specialty-list">
                        {stats.specialties.map((s, i) => (
                          <div key={i} className="ad-specialty-item">
                            <span className="ad-specialty-name">{s.specialty}</span>
                            <div className="ad-specialty-bar-wrap">
                              <div className="ad-specialty-bar" style={{ width: `${(s.count / (stats.summary.totalCases || 1)) * 100}%` }}></div>
                            </div>
                            <span className="ad-specialty-count">{s.count}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* ===== Users Tab ===== */}
          {activeTab === 'users' && (
            <div className="ad-section">
              <div className="ad-section__header">
                <div>
                  <h1 className="ad-section__title">إدارة المستخدمين</h1>
                  <p className="ad-section__sub">إضافة وحذف وتعيين أدوار المستخدمين</p>
                </div>
                <button className="ad-btn ad-btn--primary" onClick={() => setShowAddUser(true)}>
                  + إضافة مستخدم
                </button>
              </div>

              <div className="ad-toolbar">
                <input
                  className="ad-input ad-input--search"
                  placeholder="ابحث بالاسم أو البريد..."
                  value={search}
                  onChange={e => setSearch(e.target.value)}
                />
                <span className="ad-count">{filteredUsers.length} مستخدم</span>
              </div>

              <div className="ad-table-wrap">
                <table className="ad-table">
                  <thead>
                    <tr>
                      <th style={{ width: 40 }}></th>
                      <th>المستخدم</th>
                      <th>اسم المستخدم</th>
                      <th>الأدوار</th>
                      <th>تاريخ الإنشاء</th>
                      <th>الإجراءات</th>
                    </tr>
                  </thead>
                  <tbody>
                    {usersLoading ? (
                      <tr><td colSpan={6} className="ad-table__empty"><span className="ad-spinner" /> جارٍ التحميل...</td></tr>
                    ) : usersError ? (
                      <tr><td colSpan={6} className="ad-table__error">{usersError}</td></tr>
                    ) : filteredUsers.length === 0 ? (
                      <tr><td colSpan={6} className="ad-table__empty">لا يوجد مستخدمون</td></tr>
                    ) : filteredUsers.map(u => (
                      <tr key={u.id}>
                        <td><span className="ad-avatar">{initials(u.fullName ?? '؟')}</span></td>
                        <td>
                          <div className="ad-user-cell__name">{u.fullName}</div>
                          <div className="ad-user-cell__email">{u.email}</div>
                        </td>
                        <td className="ad-muted">{u.username || '—'}</td>
                        {/* ✅ الأدوار */}
                        <td>
                          <div className="ad-roles-cell">
                            {(u.roles ?? []).length > 0
                              ? (u.roles ?? []).map((r: string) => (
                                <span key={r} className="ad-role-badge">{r}</span>
                              ))
                              : <span className="ad-muted">—</span>
                            }
                          </div>
                        </td>
                        <td className="ad-muted">{formatDate(u.createdAt)}</td>
                        <td>
                          <div className="ad-actions">
                            <button
                              className="ad-action-btn ad-action-btn--blue"
                              onClick={() => setAssignRoleTarget({ id: u.id, name: u.fullName })}
                            >+ دور</button>
                            <button
                              className="ad-action-btn ad-action-btn--red"
                              onClick={() => setDeleteConfirm(u.id)}
                            >حذف</button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* ===== Roles Tab ===== */}
          {activeTab === 'roles' && (
            <div className="ad-section">
              <div className="ad-section__header">
                <div>
                  <h1 className="ad-section__title">الأدوار والصلاحيات</h1>
                  <p className="ad-section__sub">إدارة الأدوار وتعيين الصلاحيات لكل دور</p>
                </div>
                <button className="ad-btn ad-btn--primary" onClick={() => setShowAddRole(true)}>+ إضافة دور</button>
              </div>

              <div className="ad-roles-grid">
                {rolesLoading ? (
                  <div className="ad-table__empty"><span className="ad-spinner" /> جارٍ التحميل...</div>
                ) : roles.length === 0 ? (
                  <div className="ad-table__empty">لا توجد أدوار</div>
                ) : roles.map(role => (
                  <div key={role.id} className="ad-role-card">
                    <div className="ad-role-card__header">
                      <span className="ad-role-card__icon">🔐</span>
                      <div>
                        <div className="ad-role-card__name">{role.name}</div>
                        <div className="ad-role-card__count">{role.permissions?.length ?? 0} صلاحية</div>
                      </div>
                    </div>
                    <div className="ad-role-card__perms">
                      {(role.permissions ?? []).slice(0, 4).map(p => (
                        <span key={p} className="ad-perm-tag">{p}</span>
                      ))}
                      {(role.permissions?.length ?? 0) > 4 && (
                        <span className="ad-perm-tag ad-perm-tag--more">+{(role.permissions?.length ?? 0) - 4}</span>
                      )}
                    </div>
                    <button
                      className="ad-btn ad-btn--ghost ad-btn--sm ad-btn--full"
                      onClick={() => setAssignPermsTarget({ id: role.id, name: role.name, permissions: role.permissions ?? [] })}
                    >تعيين صلاحيات</button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* ===== Rooms Tab ===== */}
          {activeTab === 'rooms' && (
            <div className="ad-section">
              <div className="ad-section__header">
                <div>
                  <h1 className="ad-section__title">إدارة الغرف</h1>
                  <p className="ad-section__sub">عرض وإضافة غرف التنويم والإقامة</p>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <button className="ad-btn ad-btn--ghost" onClick={() => setShowAddDept(true)}>
                    <Plus size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    إضافة قسم
                  </button>
                  <button className="ad-btn ad-btn--ghost" onClick={() => setShowAddFloor(true)}>
                    <Plus size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    إضافة طابق
                  </button>
                  <button className="ad-btn ad-btn--primary" onClick={() => setShowAddRoom(true)}>
                    <Plus size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    إضافة غرفة
                  </button>
                </div>
              </div>

              <div className="ad-table-wrap">
                <table className="ad-table">
                  <thead>
                    <tr>
                      <th>رقم الغرفة</th>
                      <th>الفرع</th>
                      <th>الطابق</th>
                      <th>السعة</th>
                      <th>الحالة</th>
                    </tr>
                  </thead>
                  <tbody>
                    {roomsLoading ? (
                      <tr><td colSpan={5} className="ad-table__empty"><span className="ad-spinner" /> جارٍ التحميل...</td></tr>
                    ) : rooms.length === 0 ? (
                      <tr><td colSpan={5} className="ad-table__empty">لا توجد غرف</td></tr>
                    ) : rooms.map(room => (
                      <tr key={room.id}>
                        <td><div className="ad-user-cell__name">{room.roomNumber}</div></td>
                        <td className="ad-muted">{room.branchName}</td>
                        <td className="ad-muted">{room.floorName}</td>
                        <td>{room.capacity} سرير</td>
                        <td>
                          <span className={`ad-status-badge ${room.isOccupied ? 'inactive' : 'active'}`}>
                            {room.isOccupied ? 'مشغولة' : 'متاحة'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* ===== Audit Tab ===== */}
          {activeTab === 'audit' && (
            <div className="ad-section">
              <div className="ad-section__header">
                <div>
                  <h1 className="ad-section__title">سجل المراجعة</h1>
                  <p className="ad-section__sub">جميع العمليات المنفذة على النظام</p>
                </div>
              </div>
              <div className="ad-table-wrap">
                <table className="ad-table">
                  <thead>
                    <tr>
                      <th>المستخدم</th>
                      <th>العملية</th>
                      <th>الكيان</th>
                      <th>IP</th>
                      <th>التاريخ</th>
                    </tr>
                  </thead>
                  <tbody>
                    {auditLoading ? (
                      <tr><td colSpan={5} className="ad-table__empty"><span className="ad-spinner" /> جارٍ التحميل...</td></tr>
                    ) : auditError ? (
                      <tr><td colSpan={5} className="ad-table__error">{auditError}</td></tr>
                    ) : logs.length === 0 ? (
                      <tr><td colSpan={5} className="ad-table__empty">لا توجد سجلات</td></tr>
                    ) : logs.map(log => (
                      <tr key={log.id}>
                        <td>
                          <div className="ad-user-cell__name">{log.userName || '—'}</div>
                        </td>
                        <td><span className="ad-action-tag">{log.action || '—'}</span></td>
                        <td className="ad-muted">{log.entityName || '—'}</td>
                        <td className="ad-muted">{log.ipAddress || '—'}</td>
                        <td className="ad-muted">{formatDate(log.createdAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}

          {/* ===== Branches Tab ===== */}
          {activeTab === 'branches' && (
            <div className="ad-section">
              <div className="ad-section__header">
                <div>
                  <h1 className="ad-section__title">الفروع</h1>
                  <p className="ad-section__sub">إدارة فروع المستشفى</p>
                </div>
                <button className="ad-btn ad-btn--primary" onClick={() => setShowAddBranch(true)}>+ إضافة فرع</button>
              </div>
              <div className="ad-branches-grid">
                {branchesLoading ? (
                  <div className="ad-table__empty"><span className="ad-spinner" /></div>
                ) : branches.length === 0 ? (
                  <div className="ad-table__empty">لا توجد فروع</div>
                ) : branches.map(b => (
                  <div key={b.id} className="ad-branch-card">
                    <span className="ad-branch-card__icon">🏢</span>
                    <div className="ad-branch-card__name">{b.name}</div>
                    {b.address && <div className="ad-branch-card__info">{b.address}</div>}
                    {b.phone && <div className="ad-branch-card__info">📞 {b.phone}</div>}
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* ===== Tenants Tab ===== */}
          {activeTab === 'tenants' && (
            <div className="ad-section">
              <div className="ad-section__header">
                <div>
                  <h1 className="ad-section__title">إدارة المستأجرين (Tenants)</h1>
                  <p className="ad-section__sub">عرض قائمة جميع المستشفيات والمنظمات المسجلة في النظام</p>
                </div>
                <button className="ad-btn ad-btn--primary" onClick={() => setShowAddTenant(true)}>
                  + إضافة مستأجر
                </button>
              </div>

              <div className="ad-table-wrap">
                <table className="ad-table">
                  <thead>
                    <tr>
                      <th>اسم المنظمة</th>
                      <th>الكود (Code)</th>
                      <th>الحالة</th>
                      <th>تاريخ التسجيل</th>
                    </tr>
                  </thead>
                  <tbody>
                    {tenantsLoading ? (
                      <tr><td colSpan={4} className="ad-table__empty"><span className="ad-spinner" /> جارٍ التحميل...</td></tr>
                    ) : tenantsError ? (
                      <tr><td colSpan={4} className="ad-table__error">{tenantsError}</td></tr>
                    ) : tenants.length === 0 ? (
                      <tr><td colSpan={4} className="ad-table__empty">لا يوجد مستأجرون</td></tr>
                    ) : tenants.map(t => (
                      <tr key={t.id}>
                        <td>
                          <div className="ad-user-cell__name">{t.name}</div>
                        </td>
                        <td><code className="ad-code-badge">{t.code}</code></td>
                        <td>
                          <span className={`ad-status-badge ${t.isActive ? 'active' : 'inactive'}`}>
                            {t.isActive ? 'نشط' : 'متوقف'}
                          </span>
                        </td>
                        <td className="ad-muted">{formatDate(t.createdAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </main>
      </div>

      {/* ===== Modals ===== */}
      {showAddUser && (
        <AddUserModal onClose={() => setShowAddUser(false)} onSubmit={createUser} />
      )}

      {assignRoleTarget && (
        <AssignRoleModal
          userId={assignRoleTarget.id}
          userName={assignRoleTarget.name}
          roles={roles}
          onClose={() => setAssignRoleTarget(null)}
          onSubmit={assignRole}
        />
      )}

      {assignPermsTarget && (
        <AssignPermissionsModal
          roleId={assignPermsTarget.id}
          roleName={assignPermsTarget.name}
          initialPermissions={assignPermsTarget.permissions}
          allPermissions={permissions}
          onClose={() => setAssignPermsTarget(null)}
          onSubmit={async (rid, pids) => {
            await assignToRole(rid, pids)
            await rolesRefresh()
          }}
        />
      )}

      {showAddRole && (
        <AddRoleModal onClose={() => setShowAddRole(false)} onSubmit={createRole} />
      )}

      {showAddBranch && (
        <AddBranchModal onClose={() => setShowAddBranch(false)} onSubmit={createBranch} />
      )}

      {showAddRoom && (
        <AddRoomModal onClose={() => setShowAddRoom(false)} onSubmit={createRoom} />
      )}

      {showAddFloor && (
        <AddFloorModal onClose={() => setShowAddFloor(false)} onSubmit={createFloor} />
      )}

      {showAddDept && (
        <AddDepartmentModal onClose={() => setShowAddDept(false)} onSubmit={createDepartment} />
      )}

      {showAddTenant && (
        <AddTenantModal onClose={() => setShowAddTenant(false)} onSubmit={createTenant} />
      )}

      {deleteConfirm && (
        <div className="ad-overlay">
          <div className="ad-modal ad-modal--sm">
            <div className="ad-modal__header">
              <h2 className="ad-modal__title">تأكيد الحذف</h2>
              <button className="ad-modal__close" onClick={() => setDeleteConfirm(null)}>✕</button>
            </div>
            <div className="ad-modal__body">
              <p className="ad-confirm-text">هل أنت متأكد من حذف هذا المستخدم؟ لا يمكن التراجع عن هذا الإجراء.</p>
            </div>
            <div className="ad-modal__footer">
              <button className="ad-btn ad-btn--ghost" onClick={() => setDeleteConfirm(null)}>إلغاء</button>
              <button className="ad-btn ad-btn--danger" onClick={async () => {
                await deleteUser(deleteConfirm)
                await rolesRefresh() // Refresh roles too just in case
                setDeleteConfirm(null)
              }}>حذف نهائياً</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}