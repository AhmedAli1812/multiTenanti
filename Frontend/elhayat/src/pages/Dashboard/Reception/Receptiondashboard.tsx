import { useState, useMemo, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { 
  Bed, Users, Activity, AlertCircle, RefreshCw, Plus, 
  CheckCircle, Eye, Trash2, LogOut, Calendar, Search,
  Hospital, Download, MoreVertical
} from 'lucide-react'
import { useDashboard } from '../../../hooks/useDashboard'
import { getUserName, getRole, getOrgName, getBranchName } from '../../../utils/auth'
import { authService } from '../../../services/authService'
import { useSignalR } from '../../../hooks/useSignalR'
import type { Visit, Patient } from '../../../services/dashboardService'
import Modal from '../../../components/Modal'
import ConfirmModal from '../../../components/ConfirmModal'
import PatientIntakeFlow from '../../Intake/PatientIntakeFlow'
import ThemeLanguageToggle from '../../../components/ThemeLanguageToggle'
import './ReceptionDashboard.css'

const PAGE_SIZE = 5

type TabType = 'rooms' | 'patients'

function initials(name: string) {
  const parts = name.trim().split(' ')
  return parts.length >= 2 ? parts[0][0] + parts[1][0] : parts[0][0]
}

function formatDate(d: string) {
  if (!d) return '—'
  try { return new Date(d).toLocaleDateString('ar-EG') } catch { return d }
}

const DEPT_COLORS: Record<string, string> = {
  'القلب': 'dept-red',
  'العظام': 'dept-amber',
  'العناية المركزة': 'dept-red',
  'الأطفال': 'dept-green',
  'الباطنة': 'dept-blue',
  'الحراحة': 'dept-purple',
}
function deptClass(dept: string) {
  return DEPT_COLORS[dept] ?? 'dept-blue'
}

const STATUS_MAP: Record<string, { label: string, class: string }> = {
  'CheckedIn': { label: 'تم الدخول', class: 'stage-active' },
  'WaitingDoctor': { label: 'انتظار الطبيب', class: 'stage-pre' },
  'Prepared': { label: 'جاهز', class: 'stage-pre' },
  'InOp': { label: 'قيد العملية', class: 'stage-in' },
  'OpCompleted': { label: 'انتهت العملية', class: 'stage-post' },
  'PostOp': { label: 'ما بعد العملية', class: 'stage-post' },
  'Completed': { label: 'تم الانتهاء', class: 'stage-post' },
  'PendingCheckoutNurse': { label: 'بانتظار خروج التمريض', class: 'stage-pre' },
  'PendingCheckoutReception': { label: 'تأكيد خروج الاستقبال', class: 'stage-in' },
}

function getStageInfo(stage: string) {
  return STATUS_MAP[stage] ?? { label: stage ?? 'نشط', class: 'stage-active' }
}

export default function ReceptionDashboard() {
  const navigate = useNavigate()
  const userName = getUserName()
  const roleName = getRole()
  const orgName = getOrgName()
  const branchName = getBranchName()
  const { 
    overview, visits, patients, isLoadingVisits, isLoadingPatients, 
    errorVisits, errorPatients, refresh, deleteVisit, finishVisit,
    availableDepartments 
  } = useDashboard()

  const [activeTab, setActiveTab] = useState<TabType>('rooms')
  const [deptFilter, setDeptFilter] = useState('all')
  const [patientSearch, setPatientSearch] = useState('')
  const [deptSearch, setDeptSearch] = useState('')
  const [roomsPage, setRoomsPage] = useState(1)
  const [patientsPage, setPatientsPage] = useState(1)
  const [isIntakeOpen, setIsIntakeOpen] = useState(false)
  const [confirmModal, setConfirmModal] = useState<{
    isOpen: boolean,
    type: 'delete' | 'finish',
    id: string,
    isLoading: boolean
  }>({ isOpen: false, type: 'finish', id: '', isLoading: false })
  
  const [liveToast, setLiveToast] = useState<{ title: string, message: string } | null>(null)

  const handleCheckoutNotification = useCallback(() => {
    refresh()
    setLiveToast({ 
      title: 'طلب خروج', 
      message: 'التمريض يطلب تأكيد خروج مريض' 
    })
    setTimeout(() => setLiveToast(null), 5000)
  }, [refresh])

  const filteredVisits = useMemo(() => {
    if (deptFilter === 'all') return visits
    return visits.filter(v => 
      (v.departmentName || '').toLowerCase() === deptFilter.toLowerCase() ||
      (v.department || '').toLowerCase() === deptFilter.toLowerCase()
    )
  }, [visits, deptFilter])

  const filteredPatients = useMemo(() => {
    let filtered = patients
    if (patientSearch) {
      const s = patientSearch.toLowerCase()
      filtered = filtered.filter(p =>
        (p.fullName || '').toLowerCase().includes(s) ||
        String(p.medicalNumber || '').toLowerCase().includes(s)
      )
    }
    if (deptSearch) {
      filtered = filtered.filter(p => 
        (p.departmentName || '').toLowerCase() === deptSearch.toLowerCase()
      )
    }
    return filtered
  }, [patients, patientSearch, deptSearch])

  const visitsPage = filteredVisits.slice((roomsPage - 1) * PAGE_SIZE, roomsPage * PAGE_SIZE)
  const patientsPageData = filteredPatients.slice((patientsPage - 1) * PAGE_SIZE, patientsPage * PAGE_SIZE)

  useSignalR(useMemo(() => [
    { name: 'roomStatusUpdated', handler: handleCheckoutNotification },
    { name: 'visitCreated', handler: () => refresh() },
  ], [refresh, handleCheckoutNotification]))

  function handleLogout() {
    authService.logout()
    navigate('/login')
  }

  const today = new Date().toLocaleDateString('ar-EG', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })

  return (
    <div className="rd-wrap" dir="rtl">
      {/* Topbar */}
      <header className="rd-topbar">
        <div className="rd-topbar__brand">
          <span className="rd-topbar__logo"><Hospital size={24} color="var(--accent-color)" /></span>
          <div className="rd-topbar__org-info">
            <span className="rd-topbar__name">MedScope</span>
            {orgName && <span className="rd-topbar__org-name">{orgName} {branchName && <span className="rd-topbar__branch-name">| {branchName}</span>}</span>}
          </div>
        </div>
        <div className="rd-topbar__center">
          <Calendar size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
          <span className="rd-topbar__date">{today}</span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem' }}>
          <ThemeLanguageToggle />
          <div className="rd-topbar__user">
            <span className="rd-topbar__username">{userName}</span>
            <span className="rd-topbar__role-badge">{roleName || 'استقبال'}</span>
            <button className="rd-topbar__logout" onClick={handleLogout}>
              <LogOut size={14} style={{ marginLeft: 4 }} />
              خروج
            </button>
          </div>
        </div>
      </header>

      <div className="rd-content">
        {/* Sidebar */}
        <aside className="rd-sidebar">
          <nav className="rd-nav">
            <button
              className={`rd-nav__item ${activeTab === 'rooms' ? 'active' : ''}`}
              onClick={() => setActiveTab('rooms')}
            >
              <Bed size={18} />
              جدول الإشغال
            </button>
            <button
              className={`rd-nav__item ${activeTab === 'patients' ? 'active' : ''}`}
              onClick={() => setActiveTab('patients')}
            >
              <Users size={18} />
              المرضى السابقين
            </button>
          </nav>
          <div className="rd-sidebar__footer">
            <button className="rd-btn rd-btn--ghost rd-btn--sm" onClick={handleLogout}>
              تسجيل الخروج
            </button>
          </div>
        </aside>

        {/* Main */}
        <main className="rd-main">
          {/* Stats */}
          <div className="rd-stats">
            <div className="rd-stat">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="rd-stat__label">إجمالي المرضى</span>
                <Users size={20} color="var(--accent-color)" />
              </div>
              <span className="rd-stat__value">{overview?.totalPatients ?? '—'}</span>
            </div>
            <div className="rd-stat">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="rd-stat__label">الغرف المشغولة</span>
                <Bed size={20} color="var(--accent-color)" />
              </div>
              <span className="rd-stat__value">{overview?.occupiedRooms ?? '—'}</span>
            </div>
            <div className="rd-stat">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="rd-stat__label">الزيارات النشطة</span>
                <Activity size={20} color="var(--accent-color)" />
              </div>
              <span className="rd-stat__value">{overview?.activeVisits ?? '—'}</span>
            </div>
            <div className="rd-stat rd-stat--danger">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="rd-stat__label">حالات الطوارئ</span>
                <AlertCircle size={20} color="#d93025" />
              </div>
              <span className="rd-stat__value">{overview?.emergencyCases ?? '—'}</span>
            </div>
          </div>

          {/* Tab: Rooms */}
          {activeTab === 'rooms' && (
            <div className="rd-section">
              <div className="rd-section__header">
                <div>
                  <h1 className="rd-section__title">جدول إشغال الغرف التفصيلي</h1>
                  <p className="rd-section__sub">نظرة شاملة ومحدّثة على حالة الغرف والمرضى في المستشفى</p>
                </div>
                <div className="rd-section__actions">
                  <button className="rd-btn rd-btn--ghost" onClick={refresh}>
                    <RefreshCw size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    تحديث
                  </button>
                  <button className="rd-btn rd-btn--primary" onClick={() => setIsIntakeOpen(true)}>
                    <Plus size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    إدخال مريض جديد
                  </button>
                </div>
              </div>

              <div className="rd-filters">
                <span className="rd-filters__label">قسم:</span>
                <button
                  className={`rd-filter-btn ${deptFilter === 'all' ? 'active' : ''}`}
                  onClick={() => { setDeptFilter('all'); setRoomsPage(1) }}
                >
                  الكل
                </button>
                {availableDepartments.map(d => (
                  <button
                    key={d}
                    className={`rd-filter-btn ${deptFilter === d ? 'active' : ''}`}
                    onClick={() => { setDeptFilter(d); setRoomsPage(1) }}
                  >
                    {d}
                  </button>
                ))}
              </div>

              <div className="rd-table-wrap">
                <table className="rd-table">
                  <thead>
                    <tr>
                      <th>الغرفة</th>
                      <th>بيانات المريض</th>
                      <th>الطبيب المعالج</th>
                      <th>التشخيص</th>
                      <th>المرحلة</th>
                      <th>الإجراءات</th>
                    </tr>
                  </thead>
                  <tbody>
                    {isLoadingVisits ? (
                      <tr><td colSpan={6} className="rd-table__empty"><span className="rd-spinner" /> جارٍ التحميل...</td></tr>
                    ) : errorVisits ? (
                      <tr><td colSpan={6} className="rd-table__error">{errorVisits}</td></tr>
                    ) : visitsPage.length === 0 ? (
                      <tr><td colSpan={6} className="rd-table__empty">لا توجد بيانات</td></tr>
                    ) : visitsPage.map((v: Visit) => {
                      const room = v.roomNumber ?? '—'
                      const isICU = String(room).includes('ICU') || (v.departmentName || v.department || '').includes('عناية')
                      const stage = v.stage ?? 'نشط'
                      return (
                        <tr key={v.id} className={stage === 'PendingCheckoutReception' ? 'rd-row-danger' : ''}>
                          <td><span className={`rd-room-badge ${isICU ? 'rd-room-badge--icu' : ''}`}>{room}</span></td>
                          <td>
                            <div className="rd-patient-cell">
                              <span className="rd-avatar">{initials(v.patientName ?? '؟')}</span>
                              <div>
                                <div className="rd-patient-cell__name">{v.patientName ?? '—'}</div>
                                <div className="rd-patient-cell__id">
                                  {v.gender} · {v.age} عاماً
                                  {v.patientMedicalNumber && ` · ID: #${v.patientMedicalNumber}`}
                                </div>
                              </div>
                            </div>
                          </td>
                          <td className="rd-table__muted">
                            {(v.doctorName ?? '—').replace(/^د\.\s*/, '').replace(/\s*د\.$/, '')}
                          </td>
                          <td className="rd-table__muted">{v.diagnosis ?? '—'}</td>
                          <td>
                            <span className={`rd-stage-badge ${getStageInfo(stage).class}`}>
                              {getStageInfo(stage).label}
                            </span>
                          </td>
                          <td>
                            <div className="rd-table-actions">
                              <button 
                                className={`rd-action-btn rd-action-btn--success ${stage === 'PendingCheckoutReception' ? 'rd-action-btn--pulse' : ''}`}
                                title={stage === 'PendingCheckoutReception' ? 'تأكيد خروج المريض' : 'إنهاء الزيارة'}
                                onClick={() => setConfirmModal({ isOpen: true, type: 'finish', id: v.id, isLoading: false })}
                              >
                                <CheckCircle size={16} />
                              </button>
                              <button className="rd-action-btn" title="عرض">
                                <Eye size={16} />
                              </button>
                              <button 
                                className="rd-action-btn rd-action-btn--danger" 
                                title="مسح"
                                onClick={() => setConfirmModal({ isOpen: true, type: 'delete', id: v.id, isLoading: false })}
                              >
                                <Trash2 size={16} />
                              </button>
                            </div>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
                <div className="rd-pagination">
                  <span className="rd-pagination__info">
                    إظهار {visitsPage.length} من أصل {filteredVisits.length} غرفة
                  </span>
                  <div className="rd-pagination__btns">
                    {Array.from({ length: Math.ceil(filteredVisits.length / PAGE_SIZE) }, (_, i) => (
                      <button
                        key={i}
                        className={`rd-page-btn ${roomsPage === i + 1 ? 'active' : ''}`}
                        onClick={() => setRoomsPage(i + 1)}
                      >{i + 1}</button>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Tab: Previous Patients */}
          {activeTab === 'patients' && (
            <div className="rd-section">
              <div className="rd-section__header">
                <div>
                  <h1 className="rd-section__title">المرضى السابقين</h1>
                </div>
                <div className="rd-section__actions">
                  <button className="rd-btn rd-btn--ghost rd-btn--icon"><Download size={14} style={{ marginLeft: 6 }} /> تصدير القائمة</button>
                  <button className="rd-btn rd-btn--primary" onClick={() => setIsIntakeOpen(true)}>
                    <Plus size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    إضافة مريض جديد
                  </button>
                </div>
              </div>

              <div className="rd-search-bar">
                <div className="rd-search-bar__fields">
                  <div style={{ position: 'relative', flex: 1 }}>
                    <Search size={16} style={{ position: 'absolute', right: 10, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                    <input
                      className="rd-input"
                      style={{ paddingRight: 34 }}
                      type="text"
                      placeholder="ابحث بالاسم أو الرقم الطبي..."
                      value={patientSearch}
                      onChange={e => { setPatientSearch(e.target.value); setPatientsPage(1) }}
                    />
                  </div>
                  <select 
                    className="rd-input"
                    value={deptSearch}
                    onChange={e => { setDeptSearch(e.target.value); setPatientsPage(1) }}
                  >
                    <option value="">جميع الأقسام</option>
                    {availableDepartments.map(d => (
                      <option key={d} value={d}>{d}</option>
                    ))}
                  </select>
                  <input className="rd-input" type="date" />
                </div>
                <button className="rd-btn rd-btn--ghost">تطبيق الفلترة</button>
              </div>

              <div className="rd-table-wrap">
                <table className="rd-table">
                  <thead>
                    <tr>
                      <th style={{ width: 40 }}></th>
                      <th>اسم المريض</th>
                      <th>الرقم الطبي</th>
                      <th>القسم</th>
                      <th>التشخيص</th>
                      <th>تاريخ الدخول</th>
                      <th>تاريخ الخروج</th>
                      <th>اسم الدكتور</th>
                      <th>الحالة</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {isLoadingPatients ? (
                      <tr><td colSpan={10} className="rd-table__empty"><span className="rd-spinner" /> جارٍ التحميل...</td></tr>
                    ) : errorPatients ? (
                      <tr><td colSpan={10} className="rd-table__error">{errorPatients}</td></tr>
                    ) : patientsPageData.length === 0 ? (
                      <tr><td colSpan={10} className="rd-table__empty">لا توجد بيانات</td></tr>
                    ) : patientsPageData.map((p: Patient) => (
                      <tr key={p.id}>
                        <td><span className="rd-avatar rd-avatar--sm">{initials(p.fullName ?? '؟')}</span></td>
                        <td>
                          <div className="rd-patient-cell__name">{p.fullName ?? '—'}</div>
                          <div className="rd-patient-cell__id">{p.gender} · {p.age} عاماً</div>
                        </td>
                        <td className="rd-table__muted">#{p.medicalNumber ?? '—'}</td>
                        <td><span className={`rd-dept-badge ${deptClass(p.departmentName ?? '')}`}>{p.departmentName ?? '—'}</span></td>
                        <td className="rd-table__muted">{p.lastDiagnosis ?? '—'}</td>
                        <td className="rd-table__muted">{formatDate(p.admissionDate)}</td>
                        <td className="rd-table__muted">{formatDate(p.dischargeDate)}</td>
                        <td className="rd-table__muted">{p.doctorName ?? '—'}</td>
                        <td><span className="rd-stage-badge stage-post">تم الخروج</span></td>
                        <td><button className="rd-icon-btn"><MoreVertical size={16} /></button></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                <div className="rd-pagination">
                  <span className="rd-pagination__info">
                    عرض {patientsPageData.length} من أصل {filteredPatients.length} سجل مريض سابق
                  </span>
                  <div className="rd-pagination__btns">
                    {Array.from({ length: Math.ceil(filteredPatients.length / PAGE_SIZE) }, (_, i) => (
                      <button
                        key={i}
                        className={`rd-page-btn ${patientsPage === i + 1 ? 'active' : ''}`}
                        onClick={() => setPatientsPage(i + 1)}
                      >{i + 1}</button>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}
        </main>
      </div>

      {/* Intake Modal */}
      <Modal 
        isOpen={isIntakeOpen} 
        onClose={() => { setIsIntakeOpen(false); refresh(); }}
        title="تسجيل مريض جديد"
        maxWidth="1000px"
      >
        <PatientIntakeFlow onClose={() => { setIsIntakeOpen(false); refresh(); }} />
      </Modal>

      {/* Confirmation Modal */}
      <ConfirmModal
        isOpen={confirmModal.isOpen}
        onClose={() => setConfirmModal(prev => ({ ...prev, isOpen: false }))}
        onConfirm={async () => {
          setConfirmModal(prev => ({ ...prev, isLoading: true }))
          try {
            if (confirmModal.type === 'delete') await deleteVisit(confirmModal.id)
            else await finishVisit(confirmModal.id)
            setConfirmModal(prev => ({ ...prev, isOpen: false }))
          } catch {
            setConfirmModal(prev => ({ ...prev, isLoading: false }))
          }
        }}
        isLoading={confirmModal.isLoading}
        title={confirmModal.type === 'delete' ? 'تأكيد الحذف' : 'إنهاء الزيارة'}
        message={confirmModal.type === 'delete' 
          ? 'هل أنت متأكد من مسح هذا التسجيل نهائياً؟ لا يمكن التراجع عن هذا الإجراء.' 
          : 'هل تريد إنهاء هذه الزيارة ونقل بيانات المريض إلى السجل السابق؟ سيتم إخلاء الغرفة أيضاً.'
        }
        variant={confirmModal.type === 'delete' ? 'danger' : 'success'}
        confirmText={confirmModal.type === 'delete' ? 'حذف نهائي' : 'إنهاء الآن'}
      />
      {/* Live Toast Notification */}
      {liveToast && (
        <div className="rd-toast">
          <div className="rd-toast-icon">
            <CheckCircle size={20} color="#10b981" />
          </div>
          <div className="rd-toast-content">
            <div className="rd-toast-title">{liveToast.title}</div>
            <div className="rd-toast-desc">{liveToast.message}</div>
          </div>
        </div>
      )}
    </div>
  )
}