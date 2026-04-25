import { useState, useEffect, useCallback } from 'react'
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
}

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
  })

  const loadOverview = useCallback(async () => {
    try {
      const overview = await dashboardService.getOverview()
      setState(prev => ({ ...prev, overview, isLoadingOverview: false }))
    } catch {
      setState(prev => ({ ...prev, isLoadingOverview: false }))
    }
  }, [])

  const loadVisits = useCallback(async () => {
    try {
      const visits = await dashboardService.getVisits()
      setState(prev => ({ ...prev, visits, isLoadingVisits: false, errorVisits: null }))
    } catch {
      setState(prev => ({ ...prev, isLoadingVisits: false, errorVisits: 'تعذر تحميل بيانات الغرف' }))
    }
  }, [])

  const loadPatients = useCallback(async () => {
    try {
      const patients = await dashboardService.getPatients()
      setState(prev => ({ ...prev, patients, isLoadingPatients: false, errorPatients: null }))
    } catch {
      setState(prev => ({ ...prev, isLoadingPatients: false, errorPatients: 'تعذر تحميل بيانات المرضى' }))
    }
  }, [])

  const refresh = useCallback(() => {
    setState(prev => ({
      ...prev,
      isLoadingOverview: true,
      isLoadingVisits: true,
      isLoadingPatients: true,
    }))
    loadOverview()
    loadVisits()
    loadPatients()
  }, [loadOverview, loadVisits, loadPatients])

  useEffect(() => {
    loadOverview()
    loadVisits()
    loadPatients()
  }, [loadOverview, loadVisits, loadPatients])

  return { ...state, refresh }
}