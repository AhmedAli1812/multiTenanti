// ─────────────────────────────────────────────────────────────────────────────
// src/hooks/useDashboard.ts
//
// FIXED: Previously called dashboardService.getOverview(), .getVisits(),
// .getPatients() in 3 separate requests — all hitting the same backend
// endpoint /dashboard/reception. This caused 3x unnecessary HTTP calls.
//
// Now: single API call via dashboardService.getOverview() which is also
// exposed as the unified load function. getVisits() and getPatients() each
// also call /dashboard/reception internally but those are now unified via
// a single fetchAll() that calls the endpoint once and derives all 3 data sets.
// ─────────────────────────────────────────────────────────────────────────────
import { useState, useEffect, useCallback } from 'react'
import apiClient from '../services/apiClient'
import { dashboardService, type DashboardOverview, type Visit, type Patient } from '../services/dashboardService'

interface DashboardState {
  overview: DashboardOverview | null
  visits: Visit[]
  patients: Patient[]
  isLoadingOverview: boolean
  isLoadingVisits: boolean
  isLoadingPatients: boolean
  errorVisits: string | null
  errorPatients: string | null
  availableDepartments: string[]
}

// ── Direct API call (single request, derive everything) ───────────────────────
async function fetchReceptionDashboard() {
  const { data } = await apiClient.get('/dashboard/reception')
  return data
}

function mapOverview(data: any): DashboardOverview {
  const kpis = data?.kpis ?? {}
  return {
    totalPatients:  kpis.totalPatients  ?? 0,
    occupiedRooms:  kpis.occupiedRooms  ?? 0,
    activeVisits:   kpis.activeVisits   ?? 0,
    emergencyCases: kpis.emergencyCases ?? 0,
  }
}

function mapVisits(data: any): Visit[] {
  const rooms: any[] = data?.rooms ?? []
  return rooms.map((r: any) => ({
    id:                   r.visitId,
    roomNumber:           r.roomName    ?? '—',
    patientName:          r.patientName ?? '—',
    patientMedicalNumber: r.patientMedicalNumber ?? '',
    doctorName:           r.doctorName  ?? '—',
    diagnosis:            r.diagnosis   ?? '—',
    stage:                r.status      ?? 'نشط',
    age:                  r.age         ?? 0,
    gender:               r.gender === 'Male' ? 'ذكر' : (r.gender === 'Female' ? 'أنثى' : r.gender),
    department:           '',
    departmentName:       r.departmentName ?? '',
  }))
}

function mapPatients(data: any): Patient[] {
  const prev: any[] = data?.previousPatients ?? []
  return prev.map((p: any, i: number) => ({
    id:             String(i),
    fullName:       p.patientName    ?? '—',
    medicalNumber:  p.medicalNumber ?? '—',
    departmentName: p.departmentName ?? '—',
    lastDiagnosis:  p.diagnosis      ?? '—',
    admissionDate:  p.admissionDate  ?? '',
    dischargeDate:  p.dischargeDate  ?? '',
    doctorName:     p.doctorName     ?? '—',
    gender:         p.gender === 'Male' ? 'ذكر' : (p.gender === 'Female' ? 'أنثى' : p.gender),
    age:            p.age            ?? 0,
  }))
}

// ── Hook ──────────────────────────────────────────────────────────────────────
export function useDashboard() {
  const [state, setState] = useState<DashboardState>({
    overview: null,
    visits: [],
    patients: [],
    isLoadingOverview: true,
    isLoadingVisits: true,
    isLoadingPatients: true,
    errorVisits: null,
    errorPatients: null,
    availableDepartments: [],
  })

  const load = useCallback(async () => {
    setState(prev => ({
      ...prev,
      isLoadingOverview: true,
      isLoadingVisits: true,
      isLoadingPatients: true,
      errorVisits: null,
      errorPatients: null,
    }))

    try {
      const data = await fetchReceptionDashboard()

      setState({
        overview:           mapOverview(data),
        visits:             mapVisits(data),
        patients:           mapPatients(data),
        isLoadingOverview:  false,
        isLoadingVisits:    false,
        isLoadingPatients:  false,
        errorVisits:        null,
        errorPatients:      null,
        availableDepartments: data?.departments ?? [],
      })
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ??
        err?.response?.data?.title   ??
        err?.message                 ??
        'تعذر تحميل بيانات لوحة التحكم'

      setState(prev => ({
        ...prev,
        isLoadingOverview:  false,
        isLoadingVisits:    false,
        isLoadingPatients:  false,
        errorVisits:        msg,
        errorPatients:      msg,
      }))
    }
  }, [])

  const deleteVisit = useCallback(async (id: string) => {
    try {
      await dashboardService.deleteVisit(id)
      await load()
    } catch (err: any) {
      console.error('Delete error:', err)
      throw err
    }
  }, [load])

  const finishVisit = useCallback(async (id: string) => {
    try {
      await apiClient.patch(`/visits/${id}/finish`)
      await load()
    } catch (err: any) {
      console.error('Finish error:', err)
      throw err
    }
  }, [load])

  useEffect(() => { load() }, [load])

  const refresh = useCallback(() => { load() }, [load])

  return { ...state, refresh, deleteVisit, finishVisit }
}