import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getUserName, } from '../../../utils/auth'
import { authService } from '../../../services/authService'
import { useUsers, useRoles, usePermissions, useAuditLogs, useBranches } from '../../../hooks/useAdmin'
import type { CreateUserDto } from '../../../services/adminService'
import './AdminDashboard.css'

type TabType = 'users' | 'roles' | 'audit' | 'branches'

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
function AddUserModal({ roles, onClose, onSubmit }: {
  roles: { id: string; name: string }[]
  onClose: () => void
  onSubmit: (dto: CreateUserDto) => Promise<void>
}) {
  const [form, setForm] = useState<CreateUserDto>({ fullName: '', email: '', userName: '', password: '' })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handle = (f: keyof CreateUserDto, v: string) => setForm(p => ({ ...p, [f]: v }))

  const submit = async () => {
    if (!form.fullName || !form.email || !form.userName || !form.password) {
      setError('جميع الحقول مطلوبة')
      return
    }
    setIsLoading(true)
    try {
      await onSubmit(form)
      onClose()
    } catch {
      setError('فشل إضافة المستخدم')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">إضافة مستخدم جديد</h2>
          <button className="ad-modal__close" onClick={onClose}>✕</button>
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
              <input className="ad-input" value={form.userName} onChange={e => handle('userName', e.target.value)} placeholder="ahmed.ali" />
            </div>
            <div className="ad-field">
              <label className="ad-label">كلمة المرور</label>
              <input className="ad-input" type="password" value={form.password} onChange={e => handle('password', e.target.value)} placeholder="••••••••" />
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

  const submit = async () => {
    if (!selectedRole) return
    setIsLoading(true)
    try {
      await onSubmit(userId, selectedRole)
      onClose()
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal ad-modal--sm">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">تعيين دور — {userName}</h2>
          <button className="ad-modal__close" onClick={onClose}>✕</button>
        </div>
        <div className="ad-modal__body">
          <div className="ad-field">
            <label className="ad-label">اختر الدور</label>
            <select className="ad-input" value={selectedRole} onChange={e => setSelectedRole(e.target.value)}>
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
function AssignPermissionsModal({ roleId, roleName, allPermissions, onClose, onSubmit }: {
  roleId: string
  roleName: string
  allPermissions: { id: string; name: string }[]
  onClose: () => void
  onSubmit: (roleId: string, permissionIds: string[]) => Promise<void>
}) {
  const [selected, setSelected] = useState<string[]>([])
  const [isLoading, setIsLoading] = useState(false)

  const toggle = (id: string) =>
    setSelected(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id])

  const submit = async () => {
    setIsLoading(true)
    try {
      await onSubmit(roleId, selected)
      onClose()
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="ad-overlay">
      <div className="ad-modal">
        <div className="ad-modal__header">
          <h2 className="ad-modal__title">تعيين صلاحيات — {roleName}</h2>
          <button className="ad-modal__close" onClick={onClose}>✕</button>
        </div>
        <div className="ad-modal__body">
          <div className="ad-permissions-grid">
            {allPermissions.map(p => (
              <label key={p.id} className={`ad-permission-item ${selected.includes(p.id) ? 'active' : ''}`}>
                <input type="checkbox" checked={selected.includes(p.id)} onChange={() => toggle(p.id)} />
                <span>{p.name}</span>
              </label>
            ))}
          </div>
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

// ===== Main Component =====
export default function AdminDashboard() {
  const navigate = useNavigate()
  const userName = getUserName()
  const [activeTab, setActiveTab] = useState<TabType>('users')
  const [search, setSearch] = useState('')
  const [showAddUser, setShowAddUser] = useState(false)
  const [assignRoleTarget, setAssignRoleTarget] = useState<{ id: string; name: string } | null>(null)
  const [assignPermsTarget, setAssignPermsTarget] = useState<{ id: string; name: string } | null>(null)
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null)

  const { users, isLoading: usersLoading, error: usersError, createUser, deleteUser, assignRole } = useUsers()
  const { roles, isLoading: rolesLoading, createRole } = useRoles()
  const { permissions, assignToRole } = usePermissions()
  const { logs, isLoading: auditLoading, error: auditError } = useAuditLogs()
  const { branches, isLoading: branchesLoading, createBranch } = useBranches()

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
          <span className="ad-topbar__logo">🏥</span>
          <span className="ad-topbar__name">MedScope</span>
        </div>
        <div className="ad-topbar__center">{today}</div>
        <div className="ad-topbar__user">
          <span className="ad-topbar__username">{userName}</span>
          <span className="ad-topbar__role-badge">Super Admin</span>
          <button className="ad-topbar__logout" onClick={() => { authService.logout(); navigate('/login') }}>خروج</button>
        </div>
      </header>

      <div className="ad-layout">
        {/* Sidebar */}
        <aside className="ad-sidebar">
          <nav className="ad-nav">
            {([
              { key: 'users', icon: '👥', label: 'المستخدمون' },
              { key: 'roles', icon: '🔐', label: 'الأدوار والصلاحيات' },
              { key: 'audit', icon: '📋', label: 'سجل المراجعة' },
              { key: 'branches', icon: '🏢', label: 'الفروع' },
            ] as { key: TabType; icon: string; label: string }[]).map(item => (
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
                        <td className="ad-muted">{u.userName || '—'}</td>
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
                <button className="ad-btn ad-btn--primary" onClick={async () => {
                  const name = prompt('اسم الدور الجديد:')
                  if (name) await createRole(name)
                }}>+ إضافة دور</button>
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
                      onClick={() => setAssignPermsTarget({ id: role.id, name: role.name })}
                    >تعيين صلاحيات</button>
                  </div>
                ))}
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
                <button className="ad-btn ad-btn--primary" onClick={async () => {
                  const name = prompt('اسم الفرع:')
                  if (name) await createBranch({ name, address: '', phone: '' })
                }}>+ إضافة فرع</button>
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
        </main>
      </div>

      {/* ===== Modals ===== */}
      {showAddUser && (
        <AddUserModal roles={roles} onClose={() => setShowAddUser(false)} onSubmit={createUser} />
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
          allPermissions={permissions}
          onClose={() => setAssignPermsTarget(null)}
          onSubmit={assignToRole}
        />
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
                setDeleteConfirm(null)
              }}>حذف نهائياً</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}