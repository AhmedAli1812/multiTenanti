import apiClient from './apiClient'

export interface DashboardOverview {
  totalPatients: number
  occupiedRooms: number
  activeVisits: number
  emergencyCases: number
}

export interface Visit {
  id: string
  roomNumber: string
  patientName: string
  patientMedicalNumber: string
  doctorName: string
  diagnosis: string
  stage: string
  department: string
  departmentName: string
}

export interface Patient {
  id: string
  fullName: string
  medicalNumber: string
  department: string
  departmentName: string
  lastDiagnosis: string
  dischargeDate: string
  gender: string
  age: number
}

export interface PatientsResponse {
  items?: Patient[]
  data?: Patient[]
  patients?: Patient[]
  totalCount?: number
}

export interface VisitsResponse {
  items?: Visit[]
  data?: Visit[]
  visits?: Visit[]
  totalCount?: number
}

export const dashboardService = {
  getOverview: async (): Promise<DashboardOverview> => {
    const { data } = await apiClient.get('/dashboard/overview')
    return {
      totalPatients: data.totalPatients ?? data.TotalPatients ?? 0,
      occupiedRooms: data.occupiedRooms ?? data.OccupiedRooms ?? 0,
      activeVisits: data.activeVisits ?? data.ActiveVisits ?? data.totalVisits ?? 0,
      emergencyCases: data.emergencyCases ?? data.EmergencyCases ?? 0,
    }
  },

  getVisits: async (): Promise<Visit[]> => {
    const { data } = await apiClient.get<VisitsResponse | Visit[]>('/visits')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? data.visits ?? []
  },

  getPatients: async (): Promise<Patient[]> => {
    const { data } = await apiClient.get<PatientsResponse | Patient[]>('/patients')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? data.patients ?? []
  },
}