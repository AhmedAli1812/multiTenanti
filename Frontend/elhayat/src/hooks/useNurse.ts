// ─────────────────────────────────────────────────────────────────────────────
// src/hooks/useNurse.ts
//
// Custom hook for the Nurse Dashboard.
// Combines stats, queue, and appointments into a single state.
// Computes alerts client-side from appointment/queue data.
// Pattern matches useDashboard.ts — useState + useEffect + useCallback.
// ─────────────────────────────────────────────────────────────────────────────
import { useState, useEffect, useCallback } from 'react'
import {
  nurseService,
  type NurseStats,
  type QueuePatient,
  type TodayAppointment,
} from '../services/nurseService'

interface NurseState {
  stats: NurseStats | null
  queue: QueuePatient[]
  appointments: TodayAppointment[]
  isLoading: boolean
  error: string | null
}

export interface NurseAlert {
  id: string
  type: 'late' | 'upcoming'
  message: string
  patientName: string
  time: string
}

export function useNurse() {
  const [state, setState] = useState<NurseState>({
    stats: null,
    queue: [],
    appointments: [],
    isLoading: true,
    error: null,
  })

  const load = useCallback(async () => {
    setState(prev => ({ ...prev, isLoading: true, error: null }))

    try {
      const [stats, queue, appointments] = await Promise.all([
        nurseService.getStats(),
        nurseService.getQueue(),
        nurseService.getTodayAppointments(),
      ])

      setState({
        stats,
        queue,
        appointments,
        isLoading: false,
        error: null,
      })
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ??
        err?.response?.data?.title ??
        err?.message ??
        'تعذر تحميل بيانات لوحة التمريض'

      setState(prev => ({
        ...prev,
        isLoading: false,
        error: msg,
      }))
    }
  }, [])

  useEffect(() => { load() }, [load])

  const refresh = useCallback(() => { load() }, [load])

  // ── Computed alerts ──────────────────────────────────────────────────────
  const alerts: NurseAlert[] = computeAlerts(state.appointments, state.queue)

  return { ...state, alerts, refresh }
}

// ── Alert computation (client-side) ──────────────────────────────────────────
function computeAlerts(appointments: TodayAppointment[], queue: QueuePatient[]): NurseAlert[] {
  const now = new Date()
  const alerts: NurseAlert[] = []

  // 1. Late patients — appointments with status "متأخر" that are NOT in the queue yet
  appointments
    .filter(a => a.status === 'متأخر' && !queue.some(q => q.visitId === a.visitId || q.patientName === a.patientName))
    .forEach(a => {
      alerts.push({
        id: `late-${a.visitId}`,
        type: 'late',
        message: `مريض متأخر عن الموعد`,
        patientName: a.patientName,
        time: formatTime(a.scheduledTime),
      })
    })

  // 2. Upcoming within 10 minutes that are NOT in the queue yet
  appointments
    .filter(a => a.status === 'قادم' && !queue.some(q => q.visitId === a.visitId || q.patientName === a.patientName))
    .forEach(a => {
      const scheduled = new Date(a.scheduledTime)
      const diffMin = (scheduled.getTime() - now.getTime()) / 60_000
      if (diffMin > 0 && diffMin <= 10) {
        alerts.push({
          id: `upcoming-${a.visitId}`,
          type: 'upcoming',
          message: `موعد قادم خلال ${Math.ceil(diffMin)} دقائق`,
          patientName: a.patientName,
          time: formatTime(a.scheduledTime),
        })
      }
    })

  return alerts
}

function formatTime(dateStr: string): string {
  try {
    return new Date(dateStr).toLocaleTimeString('ar-EG', {
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch {
    return dateStr
  }
}
