import { useState, useMemo, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Users, Clock, Calendar, RefreshCw,
  LogOut, Hospital, Bell, ClipboardList, Activity,
  CheckCircle, AlertTriangle, Stethoscope
} from 'lucide-react'
import { useNurse } from '../../../hooks/useNurse'
import type { NurseAlert } from '../../../hooks/useNurse'
import { useSignalR } from '../../../hooks/useSignalR'
// import type { QueuePatient } from '../../../services/nurseService'
import { getUserName, getRole, getOrgName, getBranchName } from '../../../utils/auth'
import { authService } from '../../../services/authService'
import ThemeLanguageToggle from '../../../components/ThemeLanguageToggle'
import './NurseDashboard.css'







export default function NurseDashboard() {
  const navigate = useNavigate()
  const userName = getUserName()
  const roleName = getRole()
  const orgName = getOrgName()
  const branchName = getBranchName()
  
  const { stats, queue, alerts, isLoading, refresh, dischargePatient } = useNurse()

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

  const handleNewNotification = useCallback(() => {
    playAlertSound()
    setLiveToast({ id: Date.now(), message: 'إشعار جديد: يوجد تحديث في البيانات' })
    refresh()
    
    setTimeout(() => {
      setLiveToast(null)
    }, 5000)
  }, [playAlertSound, refresh])

  useSignalR(useMemo(() => [
    { name: 'NewVisitAssigned', handler: handleNewNotification },
    { name: 'Notification', handler: handleNewNotification }
  ], [handleNewNotification]))

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

  if (isLoading && !stats && queue.length === 0) {
    return (
      <div className="nd-loading-screen">
        <div className="nd-spinner-lg"></div>
        <p>جارٍ تحميل لوحة التمريض...</p>
      </div>
    )
  }

  return (
    <div className="nd-wrap" dir="rtl">
      {/* ═══ Topbar ═══ */}
      <header className="nd-topbar">
        <div className="nd-topbar__brand">
          <span className="nd-topbar__logo">
            <Hospital size={24} color="var(--accent-color)" />
          </span>
          <div className="nd-topbar__org-info">
            <span className="nd-topbar__name">لوحة تحكم التمريض</span>
            <span className="nd-topbar__org-name">
              {orgName || 'MedScope'} 
              {branchName && <span className="nd-topbar__branch-name">| {branchName}</span>}
              <span className="nd-topbar__branch-name">| الفترة الحالية</span>
            </span>
          </div>
        </div>

        <div className="nd-topbar__center">
          <Calendar size={14} style={{ marginLeft: 6, verticalAlign: 'middle' }} />
          <span className="nd-topbar__date">{today}</span>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem' }}>
          <button className="nd-btn-icon" onClick={refresh} title="تحديث">
            <RefreshCw size={18} />
          </button>
          <button className="nd-btn-icon" title="تنبيهات">
            <Bell size={18} />
            {alerts.length > 0 && <span className="nd-badge-pulse"></span>}
          </button>
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

      {/* ═══ Main Grid Layout ═══ */}
      <div className="nd-dashboard-layout">
        
        {/* Left Column: Alerts */}
        <aside className="nd-sidebar-alerts">
          <div className="nd-section__header" style={{ marginBottom: '1rem' }}>
            <h2 className="nd-section__title" style={{ fontSize: '16px' }}>تنبيهات عاجلة</h2>
            <div className="nd-alert-badge-count">{alerts.length}</div>
          </div>
          
          <div className="nd-alerts-list">
            {alerts.length === 0 ? (
              <div className="nd-alerts-empty">
                <CheckCircle size={32} color="var(--success)" />
                <p>لا توجد تنبيهات حالياً</p>
              </div>
            ) : (
              alerts.map((alert: NurseAlert) => (
                <div key={alert.id} className={`nd-alert-item ${alert.type === 'late' ? 'nd-alert-item--danger' : 'nd-alert-item--warning'}`}>
                  <div className="nd-alert-item__icon">
                    {alert.type === 'late' ? <AlertTriangle size={20} /> : <Clock size={20} />}
                  </div>
                  <div className="nd-alert-item__content">
                    <div className="nd-alert-item__title">{alert.message}</div>
                    <div className="nd-alert-item__desc">مريض: {alert.patientName} (منذ {alert.time})</div>
                  </div>
                </div>
              ))
            )}
          </div>
        </aside>

        {/* Right Column: Main Content */}
        <main className="nd-main-content">
          
          {/* Top Row: Stats */}
          <div className="nd-stats-row">
            <div className="nd-stat-box">
              <div className="nd-stat-box__icon nd-bg-blue"><Users size={24} /></div>
              <div className="nd-stat-box__content">
                <span className="nd-stat-box__label">إجمالي المرضى اليوم</span>
                <span className="nd-stat-box__value">{stats?.totalPatientsToday ?? 0}</span>
              </div>
            </div>
            
            <div className="nd-stat-box nd-stat-box--danger">
              <div className="nd-stat-box__icon nd-bg-red"><Activity size={24} /></div>
              <div className="nd-stat-box__content">
                <span className="nd-stat-box__label">حالات طارئة</span>
                <span className="nd-stat-box__value">{stats?.emergencyCases ?? 0}</span>
              </div>
            </div>
            
            <div className="nd-stat-box nd-stat-box--warning">
              <div className="nd-stat-box__icon nd-bg-orange"><Bell size={24} /></div>
              <div className="nd-stat-box__content">
                <span className="nd-stat-box__label">نداءات / انتظار</span>
                <span className="nd-stat-box__value">{stats?.waitingPatients ?? 0}</span>
              </div>
            </div>
          </div>



          {/* Bottom Row: Patients List Table (Using Queue) */}
          <div className="nd-section-wrapper" style={{ marginTop: '20px', flex: 1 }}>
            <div className="nd-section__header">
              <h2 className="nd-section__title">قائمة المرضى</h2>
              <button className="nd-btn-link">عرض الكل</button>
            </div>
            
            <div className="nd-table-wrap">
              <table className="nd-table">
                <thead>
                  <tr>
                    <th>الغرفة</th>
                    <th>المريض / التعاقد</th>
                    <th>الشكوى</th>
                    <th>الملاحظات</th>
                    <th>الطبيب المعالج</th>
                    <th>الحالة الجراحية</th>
                    <th>الإجراءات</th>
                  </tr>
                </thead>
                <tbody>
                  {queue.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="nd-table__empty">لا يوجد مرضى في الجناح حالياً</td>
                    </tr>
                  ) : (
                    queue.map((q, i) => {
                      // Mocking data to match the image requirements
                      const mockRooms = ['غرفة 101', 'غرفة 102', 'غرفة 103', 'غرفة 104']
                      const mockContracts = ['تعاقد', 'نقدي', 'حالة دكتور', 'تأمين']
                      // const mockDiagnoses = ['استئصال الزائدة', 'تغيير صمام القلب', 'منظار مرارة', 'عملية قسطرة']
                      const mockStages = ['In-op', 'Post-op', 'Pre-op', 'Completed']
                      
                      const room = mockRooms[i % mockRooms.length]
                      const contract = mockContracts[i % mockContracts.length]
                      // const diagnosis = mockDiagnoses[i % mockDiagnoses.length]
                      const stage = mockStages[i % mockStages.length]

                      let stageClass = 'nd-stage-badge--upcoming'
                      if (stage === 'In-op') stageClass = 'nd-stage-badge--present'
                      else if (stage === 'Post-op') stageClass = 'nd-stage-badge--done'

                      let contractClass = 'nd-type-badge--booking'
                      if (contract === 'نقدي') contractClass = 'nd-type-badge--followup'
                      else if (contract === 'حالة دكتور') contractClass = 'nd-type-badge--emergency'

                      return (
                        <tr key={q.visitId}>
                          <td style={{ color: 'var(--accent-color)', fontWeight: 'bold' }}>{room}</td>
                          <td>
                            <div className="nd-patient-cell">
                              <div>
                                <div className="nd-patient-cell__name">{q.patientName}</div>
                                <span className={`nd-type-badge ${contractClass}`} style={{ fontSize: '10px', padding: '2px 8px', marginTop: '4px' }}>{contract}</span>
                              </div>
                            </div>
                          </td>
                          <td className="nd-table__muted" style={{ fontWeight: '600', color: 'var(--text-main)' }}>
                            {q.chiefComplaint || '—'}
                          </td>
                          <td className="nd-table__muted" style={{ fontSize: '0.85rem', color: 'var(--accent-color)', fontStyle: 'italic' }}>
                            {q.notes || '—'}
                          </td>
                          <td>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                              <Stethoscope size={16} color="var(--text-muted)" />
                              {q.doctorName}
                            </div>
                          </td>
                          <td>
                            <span className={`nd-stage-badge ${stageClass}`}>{stage}</span>
                          </td>
                          <td>
                            <div className="nd-table-actions">
                              <button 
                                className="nd-action-btn nd-action-btn--danger"
                                onClick={() => dischargePatient(q.visitId)}
                              >
                                <LogOut size={16} style={{ transform: 'rotate(180deg)' }} /> خروج
                              </button>
                              <button className="nd-action-btn nd-action-btn--primary">
                                <ClipboardList size={16} /> الملف الطبي
                              </button>
                            </div>
                          </td>
                        </tr>
                      )
                    })
                  )}
                </tbody>
              </table>
            </div>
          </div>

        </main>
      </div>

      {/* ═══ Live Toast Notification ═══ */}
      {liveToast && (
        <div className="nd-toast-notification">
          <div className="nd-toast-icon">
            <Bell size={20} />
          </div>
          <div>
            <div className="nd-toast-title">تنبيه جديد</div>
            <div className="nd-toast-desc">{liveToast.message}</div>
          </div>
        </div>
      )}
    </div>
  )
}
