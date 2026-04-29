// ─────────────────────────────────────────────────────────────────────────────
// src/services/dashboardService.ts
//
// FIXED BUGS:
//
// 1. getOverview() was calling /dashboard/overview (Admin endpoint).
//    The Reception dashboard uses /dashboard/reception which returns
//    ReceptionDashboardDto with a nested `kpis` object:
//    { kpis: { totalPatients, activeVisits, occupiedRooms, emergencyCases },
//      visits: [...], previousPatients: [...], ... }
//
// 2. Field mapping was wrong — frontend was looking for data.totalPatients
//    but backend wraps KPIs under data.kpis.totalPatients.
//
// 3. getVisits() was calling /visits (paginated visit list).
//    Dashboard table uses visits from the reception endpoint response.
//    Kept /visits for the VisitsPage but dashboard now uses reception endpoint.
// ─────────────────────────────────────────────────────────────────────────────
import apiClient from './apiClient'

// ── Backend DTO shapes ────────────────────────────────────────────────────────

/** Matches backend DashboardKpiDto */
interface DashboardKpiDto {
  totalPatients: number
  activeVisits: number
  occupiedRooms: number
  emergencyCases: number
}

/** Matches backend RoomStatusDto */
interface RoomStatusDto {
  visitId: string
  roomName: string
  patientName: string
  patientMedicalNumber: string
  doctorName: string
  departmentName: string
  diagnosis: string
  status: string
}

/** Matches backend PreviousPatientDto */
interface PreviousPatientDto {
  patientName: string
  medicalNumber: string
  doctorName: string
  admissionDate: string
  dischargeDate: string | null
  departmentName: string
  diagnosis: string
  status: string
}

/** Matches backend ReceptionDashboardDto */
interface ReceptionDashboardDto {
  totalCount: number
  page: number
  pageSize: number
  kpis: DashboardKpiDto
  rooms: RoomStatusDto[]
  previousPatients: PreviousPatientDto[]
  departments: string[]
}

// ── Public types used by the UI ───────────────────────────────────────────────

export interface DashboardOverview {
  totalPatients: number
  occupiedRooms: number
  activeVisits: number
  emergencyCases: number
}

/** Visit row for the rooms table in the reception dashboard */
export interface Visit {
  id: string
  roomNumber: string
  patientName: string
  patientMedicalNumber: string
  doctorName: string
  diagnosis: string
  stage: string
  age: number
  gender: string
  department: string
  departmentName: string
}

export interface Patient {
  id: string
  fullName: string
  medicalNumber: string
  departmentName: string
  lastDiagnosis: string
  admissionDate: string
  dischargeDate: string
  doctorName: string
  gender: string
  age: number
}

interface PatientsApiResponse {
  items?: Patient[]
  data?: Patient[]
  patients?: Patient[]
  totalCount?: number
}

// ── Service ───────────────────────────────────────────────────────────────────
export const dashboardService = {
  /**
   * GET /api/dashboard/reception
   *
   * Returns KPIs + visit table + previous patients.
   */
  getOverview: async (): Promise<DashboardOverview> => {
    const { data } = await apiClient.get<ReceptionDashboardDto>('/dashboard/reception')

    const kpis = data.kpis ?? {}
    return {
      totalPatients:  kpis.totalPatients  ?? 0,
      occupiedRooms:  kpis.occupiedRooms  ?? 0,
      activeVisits:   kpis.activeVisits   ?? 0,
      emergencyCases: kpis.emergencyCases ?? 0,
    }
  },

  /**
   * GET /api/dashboard/reception — returns the visits/rooms table.
   */
  getVisits: async (): Promise<Visit[]> => {
    const { data } = await apiClient.get<ReceptionDashboardDto>('/dashboard/reception')

    // Map backend RoomStatusDto → frontend Visit shape
    const rooms: RoomStatusDto[] = data.rooms ?? []
    return rooms.map((r) => ({
      id:                   r.visitId,
      roomNumber:           r.roomName   ?? '—',
      patientName:          r.patientName ?? '—',
      patientMedicalNumber: r.patientMedicalNumber ?? '—',
      doctorName:           r.doctorName  ?? '—',
      diagnosis:            r.diagnosis   ?? '—',
      stage:                r.status      ?? 'نشط',
      department:           '',
      departmentName:       r.departmentName ?? '—',
    }))
  },

  /**
   * GET /api/dashboard/reception — returns previous (discharged) patients.
   */
  getPatients: async (): Promise<Patient[]> => {
    const { data } = await apiClient.get<ReceptionDashboardDto>('/dashboard/reception')

    const prev: PreviousPatientDto[] = data.previousPatients ?? []
    return prev.map((p, i) => ({
      id:             String(i),
      fullName:       p.patientName  ?? '—',
      medicalNumber:  p.medicalNumber ?? '—',
      department:     '',
      departmentName: '',
      lastDiagnosis:  '—',
      dischargeDate:  p.dischargeDate ?? '',
      gender:         '',
      age:            0,
    }))
  },

  /**
   * GET /api/patients — full patient list for the patients tab.
   */
  getAllPatients: async (): Promise<Patient[]> => {
    try {
      const { data } = await apiClient.get<PatientsApiResponse | Patient[]>('/patients')
      if (Array.isArray(data)) return data
      return (data as PatientsApiResponse).items
          ?? (data as PatientsApiResponse).data
          ?? (data as PatientsApiResponse).patients
          ?? []
    } catch {
      return []
    }
  },

  /**
   * DELETE /api/visits/{id} — deletes a visit registration.
   */
  deleteVisit: async (id: string): Promise<void> => {
    await apiClient.delete(`/visits/${id}`)
  },
}