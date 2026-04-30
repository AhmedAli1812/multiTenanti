import { useState, useMemo, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Users, Clock, Calendar, AlertCircle, RefreshCw,
  LogOut, Hospital, Bell, ClipboardList, Activity,
  CheckCircle, AlertTriangle
} from 'lucide-react'
import { useNurse } from '../../../hooks/useNurse'
import type { NurseAlert } from '../../../hooks/useNurse'
import { useSignalR } from '../../../hooks/useSignalR'
import type { QueuePatient, TodayAppointment } from '../../../services/nurseService'
import { getUserName, getRole, getOrgName, getBranchName } from '../../../utils/auth'
import { authService } from '../../../services/authService'
import ThemeLanguageToggle from '../../../components/ThemeLanguageToggle'
import './NurseDashboard.css'

const PAGE_SIZE = 8

type TabType = 'queue' | 'appointments' | 'alerts'

function initials(name: string) {
  const parts = name.trim().split(' ')
  return parts.length >= 2 ? parts[0][0] + parts[1][0] : parts[0]?.[0] ?? '؟'
}

function formatTime(dateStr: string) {
  if (!dateStr) return '—'
  try {
    return new Date(dateStr).toLocaleTimeString('ar-EG', {
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch {
    return dateStr
  }
}

function getPriorityClass(name: string) {
  if (name === 'طوارئ') return 'nd-priority-badge--emergency'
  if (name === 'عاجل') return 'nd-priority-badge--urgent'
  return 'nd-priority-badge--normal'
}

function getTypeClass(name: string) {
  if (name === 'طارئ') return 'nd-type-badge--emergency'
  if (name === 'راجع') return 'nd-type-badge--followup'
  return 'nd-type-badge--booking'
}

function getAppointmentStatusClass(status: string) {
  if (status === 'مكتمل') return 'nd-stage-badge--done'
  if (status === 'حاضر') return 'nd-stage-badge--present'
  if (status === 'متأخر') return 'nd-stage-badge--late'
  return 'nd-stage-badge--upcoming'
}

export default function NurseDashboard() {
  const navigate = useNavigate()
  const userName = getUserName()
  const roleName = getRole()
  const orgName = getOrgName()
  const branchName = getBranchName()
  const { stats, queue, appointments, alerts, isLoading, error, refresh } = useNurse()

  const [activeTab, setActiveTab] = useState<TabType>('queue')
  const [queuePage, setQueuePage] = useState(1)
  const [priorityFilter, setPriorityFilter] = useState('all')
  const [liveToast, setLiveToast] = useState<{ id: number; message: string } | null>(null)

  const playAlertSound = useCallback(() => {
    try {
      const AudioContextClass = window.AudioContext || (window as any).webkitAudioContext
      if (!AudioContextClass) return
      const ctx = new AudioContextClass()
      const osc = ctx.createOscillator()
      const gainNode = ctx.createGain()
      
      osc.type = 'sine'
      osc.frequency.setValueAtTime(880, ctx.currentTime) // A5 note
      
      gainNode.gain.setValueAtTime(0.1, ctx.currentTime)
      gainNode.gain.exponentialRampToValueAtTime(0.00001, ctx.currentTime + 0.5)
      
      osc.connect(gainNode)
      gainNode.connect(ctx.destination)
      
      osc.start()
      osc.stop(ctx.currentTime + 0.5)
    } catch (e) {
      console.error('Audio play failed:', e)
    }
  }, [])

  const handleNewNotification = useCallback((payload?: any) => {
    playAlertSound()
    setLiveToast({ id: Date.now(), message: 'إشعار جديد: يوجد تحديث في الطابور أو المواعيد' })
    refresh()
    
    setTimeout(() => {
      setLiveToast(null)
    }, 5000)
  }, [playAlertSound, refresh])

  useSignalR(useMemo(() => [
    { name: 'NewVisitAssigned', handler: handleNewNotification },
    { name: 'Notification', handler: handleNewNotification }
  ], [handleNewNotification]))

  // ── Queue filtering ─────────────────────────────────────────────────────
  const filteredQueue = useMemo(() => {
    if (priorityFilter === 'all') return queue
    return queue.filter(q => q.priorityName === priorityFilter)
  }, [queue, priorityFilter])

  const queuePageData = filteredQueue.slice(
    (queuePage - 1) * PAGE_SIZE,
    queuePage * PAGE_SIZE
  )

  function handleLogout() {
    authService.logout()
    navigate('/login')
  }

  const today = new Date().toLocaleDateString('ar-EG', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })

  return (
    <div className="nd-wrap" dir="rtl">
      {/* ═══ Topbar ═══ */}
      <header className="nd-topbar">
        <div className="nd-topbar__brand">
          <span className="nd-topbar__logo">
            <Hospital size={24} color="var(--accent-color)" />
          </span>
          <div className="nd-topbar__org-info">
            <span className="nd-topbar__name">MedScope</span>
            {orgName && (
              <span className="nd-topbar__org-name">
                {orgName}
                {branchName && (
                  <span className="nd-topbar__branch-name">| {branchName}</span>
                )}
              </span>
            )}
          </div>
        </div>

        <div className="nd-topbar__center">
          <Calendar size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
          <span className="nd-topbar__date">{today}</span>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem' }}>
          <ThemeLanguageToggle />
          <div className="nd-topbar__user">
            <span className="nd-topbar__username">{userName}</span>
            <span className="nd-topbar__role-badge">{roleName || 'تمريض'}</span>
            <button className="nd-topbar__logout" onClick={handleLogout}>
              <LogOut size={14} style={{ marginLeft: 4 }} />
              خروج
            </button>
          </div>
        </div>
      </header>

      <div className="nd-content">
        {/* ═══ Sidebar ═══ */}
        <aside className="nd-sidebar">
          <nav className="nd-nav">
            <button
              className={`nd-nav__item ${activeTab === 'queue' ? 'active' : ''}`}
              onClick={() => setActiveTab('queue')}
            >
              <ClipboardList size={18} />
              طابور المرضى
            </button>
            <button
              className={`nd-nav__item ${activeTab === 'appointments' ? 'active' : ''}`}
              onClick={() => setActiveTab('appointments')}
            >
              <Calendar size={18} />
              المواعيد
            </button>
            <button
              className={`nd-nav__item ${activeTab === 'alerts' ? 'active' : ''}`}
              onClick={() => setActiveTab('alerts')}
            >
              <Bell size={18} />
              التنبيهات
              {alerts.length > 0 && (
                <span className="nd-nav__badge">{alerts.length}</span>
              )}
            </button>
          </nav>
          <div className="nd-sidebar__footer">
            <button className="nd-btn nd-btn--ghost nd-btn--sm" onClick={handleLogout}>
              تسجيل الخروج
            </button>
          </div>
        </aside>

        {/* ═══ Main Content ═══ */}
        <main className="nd-main">
          {/* Stats Cards */}
          <div className="nd-stats">
            <div className="nd-stat">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="nd-stat__label">إجمالي المرضى اليوم</span>
                <Users size={20} color="var(--accent-color)" />
              </div>
              <span className="nd-stat__value">{stats?.totalPatientsToday ?? '—'}</span>
            </div>
            <div className="nd-stat nd-stat--warning">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="nd-stat__label">المرضى في الانتظار</span>
                <Clock size={20} color="var(--warning)" />
              </div>
              <span className="nd-stat__value">{stats?.waitingPatients ?? '—'}</span>
            </div>
            <div className="nd-stat">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="nd-stat__label">المواعيد القادمة</span>
                <Activity size={20} color="var(--accent-color)" />
              </div>
              <span className="nd-stat__value">{stats?.upcomingAppointments ?? '—'}</span>
            </div>
            <div className="nd-stat nd-stat--danger">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="nd-stat__label">الحالات الطارئة</span>
                <AlertCircle size={20} color="#d93025" />
              </div>
              <span className="nd-stat__value">{stats?.emergencyCases ?? '—'}</span>
            </div>
          </div>

          {/* ═══ Tab: Queue ═══ */}
          {activeTab === 'queue' && (
            <div className="nd-section">
              <div className="nd-section__header">
                <div>
                  <h1 className="nd-section__title">طابور المرضى</h1>
                  <p className="nd-section__sub">
                    قائمة المرضى المنتظرين اليوم مع الأولوية والحالة
                  </p>
                </div>
                <div className="nd-section__actions">
                  <button className="nd-btn nd-btn--ghost" onClick={refresh}>
                    <RefreshCw size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    تحديث
                  </button>
                </div>
              </div>

              {/* Priority Filter */}
              <div className="nd-filters">
                <span className="nd-filters__label">الأولوية:</span>
                {['all', 'طوارئ', 'عاجل', 'عادي'].map(f => (
                  <button
                    key={f}
                    className={`nd-filter-btn ${priorityFilter === f ? 'active' : ''}`}
                    onClick={() => { setPriorityFilter(f); setQueuePage(1) }}
                  >
                    {f === 'all' ? 'الكل' : f}
                  </button>
                ))}
              </div>

              <div className="nd-table-wrap">
                <table className="nd-table">
                  <thead>
                    <tr>
                      <th>#</th>
                      <th>اسم المريض</th>
                      <th>رقم الهوية</th>
                      <th>وقت الوصول</th>
                      <th>النوع</th>
                      <th>الحالة</th>
                      <th>الطبيب</th>
                      <th>القسم</th>
                      <th>الأولوية</th>
                    </tr>
                  </thead>
                  <tbody>
                    {isLoading ? (
                      <tr>
                        <td colSpan={9} className="nd-table__empty">
                          <span className="nd-spinner" /> جارٍ التحميل...
                        </td>
                      </tr>
                    ) : error ? (
                      <tr>
                        <td colSpan={9} className="nd-table__error">{error}</td>
                      </tr>
                    ) : queuePageData.length === 0 ? (
                      <tr>
                        <td colSpan={9} className="nd-table__empty">
                          لا يوجد مرضى في الطابور
                        </td>
                      </tr>
                    ) : (
                      queuePageData.map((q: QueuePatient) => (
                        <tr key={q.visitId}>
                          <td>
                            <span className="nd-queue-number">{q.queueNumber}</span>
                          </td>
                          <td>
                            <div className="nd-patient-cell">
                              <span className="nd-avatar">
                                {initials(q.patientName)}
                              </span>
                              <div className="nd-patient-cell__name">
                                {q.patientName}
                              </div>
                            </div>
                          </td>
                          <td className="nd-table__muted">{q.nationalId || '—'}</td>
                          <td className="nd-table__muted">{formatTime(q.arrivalTime)}</td>
                          <td>
                            <span className={`nd-type-badge ${getTypeClass(q.visitTypeName)}`}>
                              {q.visitTypeName}
                            </span>
                          </td>
                          <td className="nd-table__muted">{q.statusName}</td>
                          <td className="nd-table__muted">{q.doctorName}</td>
                          <td className="nd-table__muted">{q.departmentName}</td>
                          <td>
                            <span className={`nd-priority-badge ${getPriorityClass(q.priorityName)}`}>
                              {q.priorityName}
                            </span>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>

                {filteredQueue.length > PAGE_SIZE && (
                  <div className="nd-pagination">
                    <span className="nd-pagination__info">
                      عرض {queuePageData.length} من أصل {filteredQueue.length} مريض
                    </span>
                    <div className="nd-pagination__btns">
                      {Array.from(
                        { length: Math.ceil(filteredQueue.length / PAGE_SIZE) },
                        (_, i) => (
                          <button
                            key={i}
                            className={`nd-page-btn ${queuePage === i + 1 ? 'active' : ''}`}
                            onClick={() => setQueuePage(i + 1)}
                          >
                            {i + 1}
                          </button>
                        )
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>
          )}

          {/* ═══ Tab: Appointments ═══ */}
          {activeTab === 'appointments' && (
            <div className="nd-section">
              <div className="nd-section__header">
                <div>
                  <h1 className="nd-section__title">مواعيد اليوم</h1>
                  <p className="nd-section__sub">
                    قائمة جميع مواعيد اليوم مع الحالة الحالية
                  </p>
                </div>
                <div className="nd-section__actions">
                  <button className="nd-btn nd-btn--ghost" onClick={refresh}>
                    <RefreshCw size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    تحديث
                  </button>
                </div>
              </div>

              {isLoading ? (
                <div className="nd-loading-center">
                  <span className="nd-spinner" />
                  <span>جارٍ التحميل...</span>
                </div>
              ) : error ? (
                <div className="nd-table__error" style={{ padding: '2rem', textAlign: 'center' }}>
                  {error}
                </div>
              ) : appointments.length === 0 ? (
                <div className="nd-table__empty" style={{ padding: '3rem', textAlign: 'center' }}>
                  لا توجد مواعيد اليوم
                </div>
              ) : (
                <div className="nd-appointments">
                  {appointments.map((a: TodayAppointment) => (
                    <div className="nd-appt-card" key={a.visitId}>
                      <div className="nd-appt-card__time">
                        <span className="nd-appt-card__time-value">
                          {formatTime(a.scheduledTime)}
                        </span>
                        <span className="nd-appt-card__time-label">
                          {a.visitTypeName}
                        </span>
                      </div>
                      <div className="nd-appt-card__divider" />
                      <div className="nd-appt-card__info">
                        <div className="nd-appt-card__patient">{a.patientName}</div>
                        <div className="nd-appt-card__doctor">
                          {a.doctorName} · {a.departmentName}
                        </div>
                      </div>
                      <div className="nd-appt-card__status">
                        <span
                          className={`nd-stage-badge ${getAppointmentStatusClass(a.status)}`}
                        >
                          {a.status}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {/* ═══ Tab: Alerts ═══ */}
          {activeTab === 'alerts' && (
            <div className="nd-section">
              <div className="nd-section__header">
                <div>
                  <h1 className="nd-section__title">تنبيهات عاجلة</h1>
                  <p className="nd-section__sub">
                    تنبيهات فورية للتمريض — مرضى متأخرون ومواعيد قادمة
                  </p>
                </div>
                <div className="nd-section__actions">
                  <button className="nd-btn nd-btn--ghost" onClick={refresh}>
                    <RefreshCw size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
                    تحديث
                  </button>
                </div>
              </div>

              {isLoading ? (
                <div className="nd-loading-center">
                  <span className="nd-spinner" />
                  <span>جارٍ التحميل...</span>
                </div>
              ) : alerts.length === 0 ? (
                <div className="nd-alerts-empty">
                  <div className="nd-alerts-empty__icon">
                    <CheckCircle size={28} />
                  </div>
                  <div className="nd-alerts-empty__text">
                    لا توجد تنبيهات عاجلة حالياً
                  </div>
                </div>
              ) : (
                <div className="nd-alerts">
                  {alerts.map((alert: NurseAlert) => (
                    <div
                      key={alert.id}
                      className={`nd-alert-card ${
                        alert.type === 'late'
                          ? 'nd-alert-card--late'
                          : 'nd-alert-card--upcoming'
                      }`}
                    >
                      <div
                        className={`nd-alert-icon ${
                          alert.type === 'late'
                            ? 'nd-alert-icon--late'
                            : 'nd-alert-icon--upcoming'
                        }`}
                      >
                        {alert.type === 'late' ? (
                          <AlertTriangle size={20} />
                        ) : (
                          <Clock size={20} />
                        )}
                      </div>
                      <div className="nd-alert-body">
                        <div className="nd-alert-body__title">
                          {alert.patientName}
                        </div>
                        <div className="nd-alert-body__desc">
                          {alert.message}
                        </div>
                        <div className="nd-alert-body__time">
                          الموعد: {alert.time}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </main>
      </div>

      {/* ═══ Live Toast Notification ═══ */}
      {liveToast && (
        <div style={{
          position: 'fixed',
          bottom: '30px',
          left: '30px',
          background: 'var(--bg-surface)',
          border: '1px solid var(--accent-color)',
          boxShadow: 'var(--shadow-lg)',
          padding: '16px 24px',
          borderRadius: '12px',
          display: 'flex',
          alignItems: 'center',
          gap: '12px',
          zIndex: 9999,
          animation: 'nd-slide-up 0.3s ease-out forwards'
        }}>
          <div style={{ background: 'var(--accent-soft)', color: 'var(--accent-color)', padding: '8px', borderRadius: '50%' }}>
            <Bell size={20} />
          </div>
          <div>
            <div style={{ fontWeight: 700, fontSize: '14px', color: 'var(--text-main)' }}>تنبيه جديد</div>
            <div style={{ fontSize: '13px', color: 'var(--text-muted)' }}>{liveToast.message}</div>
          </div>
        </div>
      )}
      <style>{`
        @keyframes nd-slide-up {
          from { opacity: 0; transform: translateY(20px); }
          to { opacity: 1; transform: translateY(0); }
        }
      `}</style>
    </div>
  )
}
