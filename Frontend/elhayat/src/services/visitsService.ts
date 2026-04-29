// ─────────────────────────────────────────────────────────────────────────────
// src/services/visitsService.ts
//
// Wraps GET /api/visits and related PATCH endpoints.
//
// Backend returns PaginatedResult<VisitListDto>:
//   {
//     items: VisitListDto[],
//     totalCount: number,
//     pageNumber: number,
//     pageSize: number
//   }
//
// VisitListDto fields (from backend):
//   id, patientName, medicalNumber, doctorName, room, branchName, status, startedAt
// ─────────────────────────────────────────────────────────────────────────────
import apiClient from './apiClient'

// ── Types ─────────────────────────────────────────────────────────────────────
export interface VisitListItem {
  id: string
  patientName: string
  medicalNumber: string
  doctorName: string
  /** Room number string */
  room: string
  branchName: string
  /** e.g. "CheckedIn" | "WaitingDoctor" | "Prepared" | "InOp" | "Completed" */
  status: string
  startedAt: string    // ISO datetime
}

export interface PaginatedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
}

export interface GetVisitsParams {
  search?: string
  status?: string
  pageNumber?: number
  pageSize?: number
}

// ── Service ───────────────────────────────────────────────────────────────────
export const visitsService = {
  /**
   * GET /api/visits
   *
   * Supports optional query params: search, status, pageNumber, pageSize.
   * Returns the raw paginated result.
   */
  getVisits: async (
    params: GetVisitsParams = {},
  ): Promise<PaginatedResult<VisitListItem>> => {
    const { data } = await apiClient.get<PaginatedResult<VisitListItem>>('/visits', {
      params: {
        search:     params.search     ?? undefined,
        status:     params.status     ?? undefined,
        pageNumber: params.pageNumber ?? 1,
        pageSize:   params.pageSize   ?? 20,
      },
    })
    return data
  },

  /**
   * PATCH /api/visits/{id}/status
   *
   * VisitStatus enum values (string names, backend uses Enum.TryParse):
   *   CheckedIn | WaitingDoctor | Prepared | InOp | OpCompleted | PostOp | Completed
   */
  updateStatus: async (visitId: string, status: string): Promise<void> => {
    await apiClient.patch(`/visits/${visitId}/status`, { visitId, status })
  },

  /**
   * PATCH /api/visits/{id}/finish — marks a visit as Completed.
   */
  finishVisit: async (visitId: string): Promise<void> => {
    await apiClient.patch(`/visits/${visitId}/finish`)
  },
}
