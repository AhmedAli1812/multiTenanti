import { useState, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import { useDashboard } from '../../../hooks/useDashboard'
import { getUserName } from '../../../utils/auth'
import { authService } from '../../../services/authService'
import type { Visit, Patient } from '../../../services/dashboardService'
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

const STAGE_CLASS: Record<string, string> = {
  'ما قبل العملية': 'stage-pre',
  'قيد العملية': 'stage-in',
  'ما بعد العملية': 'stage-post',
}
function stageClass(stage: string) {
  for (const key of Object.keys(STAGE_CLASS)) {
    if (stage?.includes(key.split(' ').pop()!)) return STAGE_CLASS[key]
  }
  return 'stage-active'
}

export default function ReceptionDashboard() {
  const navigate = useNavigate()
  const userName = getUserName()
  const { overview, visits, patients, isLoadingVisits, isLoadingPatients, errorVisits, errorPatients, refresh } = useDashboard()

  const [activeTab, setActiveTab] = useState<TabType>('rooms')
  const [deptFilter, setDeptFilter] = useState('all')
  const [patientSearch, setPatientSearch] = useState('')
  const [roomsPage, setRoomsPage] = useState(1)
  const [patientsPage, setPatientsPage] = useState(1)

  const filteredVisits = useMemo(() => {
    if (deptFilter === 'all') return visits
    return visits.filter(v => (v.department || v.departmentName || '').includes(deptFilter))
  }, [visits, deptFilter])

  const filteredPatients = useMemo(() => {
    if (!patientSearch) return patients
    const s = patientSearch.toLowerCase()
    return patients.filter(p =>
      (p.fullName || '').toLowerCase().includes(s) ||
      String(p.medicalNumber || '').toLowerCase().includes(s)
    )
  }, [patients, patientSearch])

  const visitsPage = filteredVisits.slice((roomsPage - 1) * PAGE_SIZE, roomsPage * PAGE_SIZE)
  const patientsPageData = filteredPatients.slice((patientsPage - 1) * PAGE_SIZE, patientsPage * PAGE_SIZE)

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
          <span className="rd-topbar__logo">🏥</span>
          <span className="rd-topbar__name">MedScope</span>
        </div>
        <div className="rd-topbar__center">
          <span className="rd-topbar__date">{today}</span>
        </div>
        <div className="rd-topbar__user">
          <span className="rd-topbar__username">{userName}</span>
          <span className="rd-topbar__role-badge">استقبال</span>
          <button className="rd-topbar__logout" onClick={handleLogout}>خروج</button>
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
              <span className="rd-nav__icon">🗂</span>
              جدول الإشغال
            </button>
            <button
              className={`rd-nav__item ${activeTab === 'patients' ? 'active' : ''}`}
              onClick={() => setActiveTab('patients')}
            >
              <span className="rd-nav__icon">👥</span>
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
              <span className="rd-stat__label">إجمالي المرضى</span>
              <span className="rd-stat__value">{overview?.totalPatients ?? '—'}</span>
            </div>
            <div className="rd-stat">
              <span className="rd-stat__label">الغرف المشغولة</span>
              <span className="rd-stat__value">{overview?.occupiedRooms ?? '—'}</span>
            </div>
            <div className="rd-stat">
              <span className="rd-stat__label">الزيارات النشطة</span>
              <span className="rd-stat__value">{overview?.activeVisits ?? '—'}</span>
            </div>
            <div className="rd-stat rd-stat--danger">
              <span className="rd-stat__label">حالات الطوارئ</span>
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
                  <button className="rd-btn rd-btn--ghost" onClick={refresh}>تحديث</button>
                  <button className="rd-btn rd-btn--primary" onClick={() => navigate('/patients/new')}>
                    + إدخال مريض جديد
                  </button>
                </div>
              </div>

              <div className="rd-filters">
                <span className="rd-filters__label">قسم:</span>
                {['all', 'القلب', 'العظام', 'العناية المركزة', 'الأطفال'].map(d => (
                  <button
                    key={d}
                    className={`rd-filter-btn ${deptFilter === d ? 'active' : ''}`}
                    onClick={() => { setDeptFilter(d); setRoomsPage(1) }}
                  >
                    {d === 'all' ? 'الكل' : d}
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
                        <tr key={v.id}>
                          <td><span className={`rd-room-badge ${isICU ? 'rd-room-badge--icu' : ''}`}>{room}</span></td>
                          <td>
                            <div className="rd-patient-cell">
                              <span className="rd-avatar">{initials(v.patientName ?? '؟')}</span>
                              <div>
                                <div className="rd-patient-cell__name">{v.patientName ?? '—'}</div>
                                {v.patientMedicalNumber && <div className="rd-patient-cell__id">ID: #{v.patientMedicalNumber}</div>}
                              </div>
                            </div>
                          </td>
                          <td className="rd-table__muted">د. {(v.doctorName ?? '—').replace(/^د\.\s*/, '')}</td>
                          <td className="rd-table__muted">{v.diagnosis ?? '—'}</td>
                          <td><span className={`rd-stage-badge ${stageClass(stage)}`}>{stage}</span></td>
                          <td><button className="rd-link-btn">عرض</button></td>
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
                  <button className="rd-btn rd-btn--ghost rd-btn--icon">⬇ تصدير القائمة</button>
                  <button className="rd-btn rd-btn--primary" onClick={() => navigate('/patients/new')}>
                    + إضافة مريض جديد
                  </button>
                </div>
              </div>

              <div className="rd-search-bar">
                <div className="rd-search-bar__fields">
                  <input
                    className="rd-input"
                    type="text"
                    placeholder="ابحث بالاسم أو الرقم الطبي..."
                    value={patientSearch}
                    onChange={e => { setPatientSearch(e.target.value); setPatientsPage(1) }}
                  />
                  <select className="rd-input">
                    <option>جميع الأقسام</option>
                    <option>القلب</option>
                    <option>العظام</option>
                    <option>الباطنة</option>
                    <option>العناية المركزة</option>
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
                        <td><span className={`rd-dept-badge ${deptClass(p.departmentName ?? p.department ?? '')}`}>{p.departmentName ?? p.department ?? '—'}</span></td>
                        <td className="rd-table__muted">{p.lastDiagnosis ?? '—'}</td>
                        <td className="rd-table__muted">—</td>
                        <td className="rd-table__muted">{formatDate(p.dischargeDate)}</td>
                        <td className="rd-table__muted">—</td>
                        <td><span className="rd-stage-badge stage-post">تم الخروج</span></td>
                        <td><button className="rd-icon-btn">⋮</button></td>
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
    </div>
  )
}