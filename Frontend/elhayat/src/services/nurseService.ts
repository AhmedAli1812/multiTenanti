// ─────────────────────────────────────────────────────────────────────────────
// src/services/nurseService.ts
//
// Service layer for the Nurse Dashboard endpoints.
// Calls /api/nurse/* which is served by NurseController.
// Pattern matches dashboardService.ts exactly.
// ─────────────────────────────────────────────────────────────────────────────
import apiClient from './apiClient'

// ── Backend DTO shapes ────────────────────────────────────────────────────────

export interface NurseStats {
  totalPatientsToday: number
  waitingPatients: number
  upcomingAppointments: number
  emergencyCases: number
}

export interface QueuePatient {
  visitId: string
  patientName: string
  nationalId: string
  arrivalTime: string
  visitTypeName: string
  statusName: string
  status: string
  doctorName: string
  departmentName: string
  priorityName: string
  queueNumber: number
}

export interface TodayAppointment {
  visitId: string
  patientName: string
  doctorName: string
  departmentName: string
  scheduledTime: string
  queueNumber: number
  status: string
  visitTypeName: string
}

// ── Service ───────────────────────────────────────────────────────────────────

export const nurseService = {
  /**
   * GET /api/nurse/dashboard-stats
   */
  getStats: async (): Promise<NurseStats> => {
    const { data } = await apiClient.get<NurseStats>('/nurse/dashboard-stats')
    return {
      totalPatientsToday:   data.totalPatientsToday   ?? 0,
      waitingPatients:      data.waitingPatients      ?? 0,
      upcomingAppointments: data.upcomingAppointments ?? 0,
      emergencyCases:       data.emergencyCases       ?? 0,
    }
  },

  /**
   * GET /api/nurse/queue
   */
  getQueue: async (): Promise<QueuePatient[]> => {
    const { data } = await apiClient.get<QueuePatient[]>('/nurse/queue')
    if (!Array.isArray(data)) return []
    return data
  },

  /**
   * GET /api/nurse/appointments/today
   */
  getTodayAppointments: async (): Promise<TodayAppointment[]> => {
    const { data } = await apiClient.get<TodayAppointment[]>('/nurse/appointments/today')
    if (!Array.isArray(data)) return []
    return data
  },
}
