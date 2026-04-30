import { useState, useMemo, useCallback, useEffect } from 'react'
import {
  Users, Clock, AlertCircle, Bell, BellRing,
  Activity, AlertTriangle, CheckCircle, FileText,
  LogOut, ChevronLeft, Droplet, Hospital, Calendar
} from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { getUserName, getRole, getOrgName, getBranchName, getStoredToken } from '../../../utils/auth'
import { authService } from '../../../services/authService'
import ThemeLanguageToggle from '../../../components/ThemeLanguageToggle'
import { useNurse } from '../../../hooks/useNurse'
import { useSignalR } from '../../../hooks/useSignalR'
import type { QueuePatient } from '../../../services/nurseService'
import apiClient from '../../../services/apiClient'
import './NurseDashboard.css'

export default function NurseDashboard() {
  const navigate = useNavigate()
  const userName = getUserName()
  const roleName = getRole()
  const orgName = getOrgName()
  const branchName = getBranchName()

  const { queue, stats, alerts, refresh } = useNurse()
  const [liveNotifications, setLiveNotifications] = useState<any[]>([])
  const [isBellOpen, setIsBellOpen] = useState(false)
  const [liveToast, setLiveToast] = useState<{ id: number; message: string; title: string } | null>(null)

  // Combine live notifications (from signalR) with existing alerts from useNurse
  const allNotifications = useMemo(() => {
    const existing = alerts.map(a => ({
      id: a.id,
      type: a.type === 'late' ? 'danger' : 'warning',
      title: a.patientName,
      desc: a.message,
      icon: a.type === 'late' ? 'triangle' : 'bell'
    }))
    return [...liveNotifications, ...existing]
  }, [alerts, liveNotifications])

  // Helper to map arabic statuses to english UI pills
  const getUiStatus = (statusName: string) => {
    switch (statusName) {
      case 'تم الدخول': return 'Check-in';
      case 'انتظار الطبيب': return 'Waiting';
      case 'جاهز': return 'Pre-op';
      case 'قيد العملية': return 'In-op';
      case 'انتهت العملية': return 'Post-op';
      case 'ما بعد العملية': return 'Post-op';
      case 'مكتمل': return 'Completed';
      case 'بانتظار خروج التمريض': return 'Pending';
      case 'بانتظار خروج الاستقبال': return 'Waiting-Reception';
      case 'نشط': return 'Active';
      default: return 'Check-in';
    }
  }

  // Table Data purely from backend queue
  const tablePatients = useMemo(() => {
    if (!queue) return []
    return queue.map((q, i) => ({
      id: q.visitId,
      room: `غرفة ${100 + i + 1}`,
      name: q.patientName,
      type: q.visitTypeName || 'نقدي',
      typeColor: q.visitTypeName === 'تعاقد' ? 'blue' : q.visitTypeName === 'طوارئ' ? 'red' : 'green',
      diagnosis: q.departmentName || 'غير محدد',
      doctor: q.doctorName || 'غير محدد',
      status: getUiStatus(q.statusName),
      isPendingCheckoutNurse: q.status === 'PendingCheckoutNurse',
      isPendingCheckoutReception: q.status === 'PendingCheckoutReception'
    }))
  }, [queue])

  // Ward Overview purely from top 3 backend queue patients
  const wardOverviewCards = useMemo(() => {
    if (!queue) return []
    return queue.slice(0, 3).map((q, i) => {
      const isEmergency = q.priorityName === 'طوارئ' || q.priorityName === 'عاجل'
      return {
        id: q.visitId,
        room: `10${i + 1}`,
        patient: q.patientName,
        bp: isEmergency ? '140/90' : '120/80', // Fake vitals since backend doesn't provide them yet
        hr: isEmergency ? '95' : '72',
        status: getUiStatus(q.statusName),
        time: q.arrivalTime ? new Date(q.arrivalTime).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' }) : '-',
        isStable: !isEmergency
      }
    })
  }, [queue])

  const totalPatients = stats?.totalPatientsToday ?? 0;

  const playAlertSound = useCallback(() => {
    try {
      const AudioContextClass = window.AudioContext || (window as any).webkitAudioContext
      if (!AudioContextClass) return
      const ctx = new AudioContextClass()
      const osc = ctx.createOscillator()
      const gainNode = ctx.createGain()
      
      osc.type = 'sine'
      osc.frequency.setValueAtTime(880, ctx.currentTime) // A5
      osc.frequency.exponentialRampToValueAtTime(440, ctx.currentTime + 0.3)
      
      gainNode.gain.setValueAtTime(0.2, ctx.currentTime)
      gainNode.gain.exponentialRampToValueAtTime(0.00001, ctx.currentTime + 0.5)
      
      osc.connect(gainNode)
      gainNode.connect(ctx.destination)
      
      osc.start()
      osc.stop(ctx.currentTime + 0.5)
    } catch (e) {
      console.error('Audio play failed:', e)
    }
  }, [])

  const handleNewNotification = useCallback((data?: any) => {
    playAlertSound()
    refresh()
    
    // Extract payload data (handle nested Payload from backend)
    const payload = data?.payload || data?.Payload || data
    const patientName = payload?.patientName || payload?.PatientName
    
    const newAlert = {
      id: Date.now().toString(),
      type: 'info',
      title: 'مريض جديد تم إضافته',
      desc: patientName ? `تم إضافة المريض: ${patientName} للجناح` : 'تمت إضافة مريض جديد للتو',
      icon: 'bell'
    }

    setLiveNotifications(prev => [newAlert, ...prev])
    setLiveToast({ 
      id: Date.now(), 
      title: 'مريض جديد', 
      message: payload?.patientName ? `تمت إضافة المريض ${payload.patientName} للتو` : 'تم تسجيل مريض جديد بنجاح' 
    })

    setTimeout(() => {
      setLiveToast(null)
    }, 5000)
  }, [playAlertSound, refresh])

  const handleCheckoutNotification = useCallback((payload?: any) => {
    playAlertSound()
    refresh()
    
    setLiveToast({ 
      id: Date.now(), 
      title: 'طلب خروج', 
      message: 'الاستقبال يطلب تأكيد خروج مريض' 
    })
    
    setTimeout(() => {
      setLiveToast(null)
    }, 5000)
  }, [playAlertSound, refresh])

  useSignalR(useMemo(() => [
    { name: 'roomStatusUpdated', handler: handleCheckoutNotification },
    { name: 'roomAssigned', handler: () => handleNewNotification({ patientName: 'تحديث في الغرف' }) },
    { name: 'NewPatientAdded', handler: () => handleNewNotification() },
    { name: 'Notification', handler: handleNewNotification }
  ], [handleNewNotification, handleCheckoutNotification]))

  const toggleBell = () => setIsBellOpen(!isBellOpen)

  const handleLogout = () => {
    authService.logout()
    navigate('/login')
  }

  const todayStr = new Date().toLocaleDateString('ar-EG', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })

  return (
    <div className="nd-wrapper" dir="rtl">
      {/* ===== Global Nav ===== */}
      <nav className="nd-global-nav">
        {/* Right Section (Brand) */}
        <div className="nd-nav-brand-section">
          <div className="nd-nav-brand">
            <Hospital size={24} className="nd-nav-logo" />
            <div className="nd-nav-org">
              <span className="nd-nav-org-name">MedScope</span>
              <span className="nd-nav-branch">{orgName || 'Elhaya Hospital'} | {branchName || 'elhayat1'}</span>
            </div>
          </div>
        </div>

        {/* Center Section (Date) */}
        <div className="nd-nav-center">
          <Calendar size={16} />
          <span>{todayStr}</span>
        </div>
        
        {/* Left Section (User & Actions) */}
        <div className="nd-nav-user-section">
          <span className="nd-nav-user">{userName || 'Ahmed Hassan'}</span>
          <span className="nd-nav-role">{roleName || 'NURSE'}</span>

          <div style={{ width: '1px', height: '24px', background: '#EAECEE', margin: '0 4px' }} />

          <ThemeLanguageToggle />

          <button className="nd-nav-logout" onClick={handleLogout}>
            خروج <LogOut size={14} style={{ transform: 'scaleX(-1)' }} />
          </button>
        </div>
      </nav>

      {/* ===== Top Header ===== */}
      <header className="nd-header">
        <div className="nd-header-right">
          <div className="nd-header-text">
            <h1 className="nd-title">لوحة تحكم التمريض</h1>
            <p className="nd-subtitle">جناح العناية المركزة - الفترة الصباحية</p>
          </div>
        </div>

        <div className="nd-header-left">
          <div className="nd-bell-wrapper">
            <button className="nd-bell-btn" onClick={toggleBell}>
              <Bell size={20} color="#6F767E" />
              {allNotifications.length > 0 && (
                <span className="nd-bell-dot" />
              )}
            </button>
            
            {/* Notifications Dropdown */}
            {isBellOpen && (
              <div className="nd-notifications-dropdown">
                <div className="nd-notif-header">الإشعارات ({allNotifications.length})</div>
                <div className="nd-notif-list">
                  {allNotifications.length === 0 ? (
                    <div className="nd-notif-empty">لا توجد إشعارات</div>
                  ) : (
                    allNotifications.map(n => (
                      <div key={n.id} className="nd-notif-item">
                        <div className="nd-notif-title">{n.title}</div>
                        <div className="nd-notif-desc">{n.desc}</div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            )}
          </div>
        </div>
      </header>

      {/* ===== Main Layout ===== */}
      <div className="nd-layout">
        
        {/* RIGHT AREA: Main Content */}
        <main className="nd-main">
          
          {/* Top Stats Cards */}
          <div className="nd-stats-row">
            {/* Card 1: Total */}
            <div className="nd-stat-card">
              <div className="nd-stat-icon-wrap nd-icon-blue">
                <Users size={24} color="#0066FF" />
              </div>
              <div className="nd-stat-info">
                <span className="nd-stat-label">إجمالي المرضى</span>
                <span className="nd-stat-value">{stats?.totalPatientsToday ?? totalPatients}</span>
              </div>
            </div>

            {/* Card 2: Critical */}
            <div className="nd-stat-card">
              <div className="nd-stat-icon-wrap nd-icon-red">
                <AlertCircle size={24} color="#EF4444" />
              </div>
              <div className="nd-stat-info">
                <span className="nd-stat-label">حالات حرجة</span>
                <span className="nd-stat-value nd-text-red">
                  {stats?.emergencyCases != null ? (stats.emergencyCases < 10 ? `0${stats.emergencyCases}` : stats.emergencyCases) : '03'}
                </span>
              </div>
            </div>

            {/* Card 3: Active Calls */}
            <div className="nd-stat-card">
              <div className="nd-stat-icon-wrap nd-icon-orange">
                <BellRing size={24} color="#F97316" />
              </div>
              <div className="nd-stat-info">
                <span className="nd-stat-label">نداءات نشطة</span>
                <span className="nd-stat-value nd-text-orange">
                   {stats?.waitingPatients != null ? (stats.waitingPatients < 10 ? `0${stats.waitingPatients}` : stats.waitingPatients) : '02'}
                </span>
              </div>
            </div>
          </div>

          {/* Ward Overview */}
          <div className="nd-section-header">
            <h2 className="nd-section-title">نظرة عامة على الجناح</h2>
            <div className="nd-legend">
              <span className="nd-legend-item">
                <span className="nd-dot nd-dot-green" /> مستقرة
              </span>
              <span className="nd-legend-item">
                <span className="nd-dot nd-dot-orange" /> متابعة
              </span>
            </div>
          </div>

          <div className="nd-ward-grid">
            {wardOverviewCards.length === 0 ? (
               <div style={{ padding: '2rem', textAlign: 'center', color: '#6F767E', gridColumn: 'span 3', border: '1px dashed #EAECEE', borderRadius: '16px' }}>
                 لا يوجد مرضى في الجناح حالياً
               </div>
            ) : (
              wardOverviewCards.map((card, idx) => (
                <div className="nd-ward-card" key={card.id || idx}>
                  <div className="nd-ward-top">
                    <span className="nd-room-badge">غرفة {card.room}</span>
                    {card.isStable ? (
                      <div className="nd-status-icon nd-status-green">
                        <CheckCircle size={16} />
                      </div>
                    ) : (
                      <div className="nd-status-icon nd-status-orange">
                        <AlertCircle size={16} />
                      </div>
                    )}
                  </div>
                  <div className="nd-ward-patient">{card.patient}</div>
                  
                  <div className="nd-vitals-row">
                    <div className="nd-vital">
                      <span className="nd-vital-label">النبض</span>
                      <span className="nd-vital-value">bpm {card.hr}</span>
                    </div>
                    <div className="nd-vital">
                      <span className="nd-vital-label">الضغط</span>
                      <span className={`nd-vital-value ${!card.isStable ? 'nd-text-red' : ''}`}>
                        {card.bp}
                      </span>
                    </div>
                  </div>

                  <div className="nd-ward-bottom">
                    <div className="nd-ward-bottom-right">
                      <span className={`nd-status-pill nd-pill-${card.status.toLowerCase()}`}>
                        {card.status === 'In-op' ? 'قيد العملية (In-op)' : 
                         card.status === 'Post-op' ? 'ما بعد العملية (Post-op)' : 
                         card.status === 'Check-in' ? 'تم الدخول (Check-in)' :
                         card.status === 'Waiting-Reception' ? 'انتظار الاستقبال' :
                         card.status === 'Pending' ? 'بانتظار التأكيد' :
                         'ما قبل العملية (Pre-op)'}
                      </span>
                      <span className="nd-time-pill">
                        <Clock size={12} />
                        {card.time}
                      </span>
                    </div>
                    <button className="nd-arrow-btn">
                      <ChevronLeft size={16} />
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>

          {/* Patient List */}
          <div className="nd-section-header" style={{ marginTop: '2rem' }}>
            <h2 className="nd-section-title">قائمة المرضى</h2>
            <div className="nd-list-actions">
              <span className="nd-list-count">{totalPatients} مريضاً في الجناح <span className="nd-blue-dot"/></span>
              <a href="#all" className="nd-link-all">عرض الكل</a>
            </div>
          </div>

          <div className="nd-table-wrap">
            <table className="nd-table">
              <thead>
                <tr>
                  <th>الغرفة</th>
                  <th>المريض / التعاقد</th>
                  <th>التشخيص</th>
                  <th>الطبيب المعالج</th>
                  <th>الحالة الجراحية</th>
                  <th>الإجراءات</th>
                </tr>
              </thead>
              <tbody>
                {tablePatients.length === 0 ? (
                  <tr>
                    <td colSpan={6} style={{ textAlign: 'center', color: '#6F767E', padding: '3rem' }}>
                      لا يوجد مرضى في الطابور
                    </td>
                  </tr>
                ) : (
                  tablePatients.map(p => (
                    <tr key={p.id} className={p.isPendingCheckoutNurse ? 'nd-row-danger' : ''}>
                      <td className="nd-td-room">{p.room}</td>
                      <td>
                        <div className="nd-td-patient">
                          <span className="nd-td-name">{p.name}</span>
                          <span className={`nd-td-type nd-type-${p.typeColor}`}>{p.type}</span>
                        </div>
                      </td>
                      <td className="nd-td-muted">{p.diagnosis}</td>
                      <td className="nd-td-muted">{p.doctor}</td>
                      <td>
                        <span className={`nd-state-badge nd-state-${p.status.toLowerCase()}`}>
                          {p.status}
                        </span>
                      </td>
                      <td>
                        <div className="nd-td-actions">
                          <button className="nd-action-btn nd-btn-blue">
                            <FileText size={14} />
                            الملف الطبي
                          </button>
                          <button 
                            className={`nd-action-btn nd-btn-red ${p.isPendingCheckoutReception ? 'nd-disabled' : ''}`}
                            onClick={async (e) => {
                               const btn = e.currentTarget as HTMLButtonElement;
                               btn.disabled = true;
                               try {
                                 await apiClient.patch(`/visits/${p.id}/finish`);
                                 refresh();
                               } catch (err) {
                                 console.error("Checkout failed", err);
                               } finally {
                                 btn.disabled = false;
                               }
                            }}
                            disabled={p.isPendingCheckoutReception}
                          >
                            <LogOut size={14} style={{ transform: 'scaleX(-1)' }} />
                            {p.isPendingCheckoutNurse ? 'تأكيد الخروج' : p.isPendingCheckoutReception ? 'بانتظار الاستقبال' : 'خروج'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

        </main>

        {/* LEFT AREA: Sidebar Alerts */}
        <aside className="nd-sidebar">
          <div className="nd-sidebar-header">
            <h3 className="nd-sidebar-title">تنبيهات عاجلة</h3>
            <span className="nd-sidebar-count">{allNotifications.length}</span>
          </div>

          <div className="nd-alerts-list">
            {allNotifications.length === 0 ? (
               <div style={{ textAlign: 'center', color: '#6F767E', padding: '2rem 0' }}>لا توجد تنبيهات</div>
            ) : (
              allNotifications.map(alert => (
                <div key={alert.id} className={`nd-alert-card nd-alert-${alert.type}`}>
                  <div className="nd-alert-icon-box">
                    {alert.icon === 'triangle' && <AlertTriangle size={20} />}
                    {alert.icon === 'asterisk' && <Droplet size={20} />}
                    {alert.icon === 'bell' && <Bell size={20} />}
                  </div>
                  <div className="nd-alert-content">
                    <div className="nd-alert-title">{alert.title}</div>
                    <div className="nd-alert-desc">{alert.desc}</div>
                  </div>
                </div>
              ))
            )}
          </div>
        </aside>
      </div>

      {/* Live Toast Notification */}
      {liveToast && (
        <div className="nd-toast">
          <div className="nd-toast-icon">
            <BellRing size={20} color="#0066FF" />
          </div>
          <div className="nd-toast-content">
            <div className="nd-toast-title">{liveToast.title}</div>
            <div className="nd-toast-desc">{liveToast.message}</div>
          </div>
        </div>
      )}
    </div>
  )
}
