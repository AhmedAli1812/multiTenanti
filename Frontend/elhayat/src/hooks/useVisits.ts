// ─────────────────────────────────────────────────────────────────────────────
// src/hooks/useVisits.ts
// ─────────────────────────────────────────────────────────────────────────────
import { useState, useEffect, useCallback } from 'react'
import { visitsService, type VisitListItem, type GetVisitsParams } from '../services/visitsService'

export function useVisits(initialParams: GetVisitsParams = {}) {
  const [visits, setVisits]           = useState<VisitListItem[]>([])
  const [totalCount, setTotalCount]   = useState(0)
  const [pageNumber, setPageNumber]   = useState(initialParams.pageNumber ?? 1)
  const [pageSize]                    = useState(initialParams.pageSize   ?? 20)
  const [search, setSearch]           = useState(initialParams.search     ?? '')
  const [statusFilter, setStatusFilter] = useState(initialParams.status  ?? '')
  const [isLoading, setIsLoading]     = useState(false)
  const [error, setError]             = useState<string | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const result = await visitsService.getVisits({
        search:     search     || undefined,
        status:     statusFilter || undefined,
        pageNumber,
        pageSize,
      })
      setVisits(result.items ?? [])
      setTotalCount(result.totalCount ?? 0)
    } catch (err: any) {
      setError(err?.response?.data?.message ?? err?.message ?? 'Failed to load visits')
    } finally {
      setIsLoading(false)
    }
  }, [search, statusFilter, pageNumber, pageSize])

  useEffect(() => { load() }, [load])

  const finishVisit = async (visitId: string) => {
    try {
      await visitsService.finishVisit(visitId)
      await load()
    } catch (err: any) {
      setError(err?.response?.data?.message ?? 'Failed to finish visit')
    }
  }

  return {
    visits,
    totalCount,
    pageNumber,
    pageSize,
    search,
    statusFilter,
    isLoading,
    error,
    setSearch,
    setStatusFilter,
    setPageNumber,
    refresh: load,
    finishVisit,
  }
}
