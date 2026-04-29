// ─────────────────────────────────────────────────────────────────────────────
// src/pages/Visits/VisitsPage.tsx
//
// Displays paginated list of visits from GET /api/visits.
// Supports search (patient name / medical number) and status filter.
//
// Backend response shape: PaginatedResult<VisitListDto>
//   items[]   → id, patientName, medicalNumber, doctorName, room, branchName, status, startedAt
// ─────────────────────────────────────────────────────────────────────────────
import { useRef } from 'react'
import { useVisits } from '../../hooks/useVisits'
import './VisitsPage.css'

// ── Status badge helper ───────────────────────────────────────────────────────
const STATUS_CLASS: Record<string, string> = {
  CheckedIn:     'visit-status--checkedin',
  WaitingDoctor: 'visit-status--waitingdoctor',
  Prepared:      'visit-status--prepared',
  InOp:          'visit-status--inop',
  OpCompleted:   'visit-status--completed',
  PostOp:        'visit-status--prepared',
  Completed:     'visit-status--completed',
}

const STATUS_LABEL: Record<string, string> = {
  CheckedIn:     'Checked In',
  WaitingDoctor: 'Waiting Doctor',
  Prepared:      'Prepared',
  InOp:          'In Operation',
  OpCompleted:   'Op Completed',
  PostOp:        'Post-Op',
  Completed:     'Completed',
}

function StatusBadge({ status }: { status: string }) {
  const cls   = STATUS_CLASS[status]   ?? 'visit-status--default'
  const label = STATUS_LABEL[status]   ?? status
  return <span className={`visit-status ${cls}`}>{label}</span>
}

// ── Skeleton rows ─────────────────────────────────────────────────────────────
function SkeletonRows() {
  return (
    <>
      {Array.from({ length: 8 }).map((_, i) => (
        <tr key={i} className="visits-skeleton">
          {Array.from({ length: 7 }).map((_, j) => (
            <td key={j}>
              <span
                className="visits-skeleton-cell"
                style={{ width: `${60 + Math.random() * 80}px` }}
              />
            </td>
          ))}
        </tr>
      ))}
    </>
  )
}

// ── Main component ────────────────────────────────────────────────────────────
export default function VisitsPage() {
  const {
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
    refresh,
    finishVisit,
  } = useVisits()

  // Debounce search to avoid firing on every keystroke
  const searchTimer = useRef<ReturnType<typeof setTimeout> | null>(null)

  const handleSearchChange = (value: string) => {
    if (searchTimer.current) clearTimeout(searchTimer.current)
    searchTimer.current = setTimeout(() => {
      setSearch(value)
      setPageNumber(1)
    }, 400)
  }

  const totalPages = Math.ceil(totalCount / pageSize)

  return (
    <main className="visits-page">
      {/* ── Header ─────────────────────────────────────────────────────── */}
      <div className="visits-page__header">
        <h1 className="visits-page__title">🏥 Active Visits</h1>
        <span style={{ color: '#64748b', fontSize: '0.9rem' }}>
          {totalCount} visit{totalCount !== 1 ? 's' : ''} found
        </span>
      </div>

      {/* ── Controls ───────────────────────────────────────────────────── */}
      <div className="visits-controls">
        <input
          id="visits-search"
          className="visits-search"
          type="search"
          placeholder="Search patient name or medical number…"
          defaultValue={search}
          onChange={(e) => handleSearchChange(e.target.value)}
        />

        <select
          id="visits-status-filter"
          className="visits-filter"
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPageNumber(1) }}
        >
          <option value="">All Statuses</option>
          <option value="CheckedIn">Checked In</option>
          <option value="WaitingDoctor">Waiting Doctor</option>
          <option value="Prepared">Prepared</option>
          <option value="InOp">In Operation</option>
          <option value="OpCompleted">Op Completed</option>
          <option value="PostOp">Post-Op</option>
          <option value="Completed">Completed</option>
        </select>

        <button
          id="visits-refresh-btn"
          className="visits-refresh-btn"
          onClick={refresh}
          disabled={isLoading}
        >
          {isLoading ? '↻ Refreshing…' : '↻ Refresh'}
        </button>
      </div>

      {/* ── Error ──────────────────────────────────────────────────────── */}
      {error && (
        <div className="visits-error" role="alert">
          ⚠️ {error}
        </div>
      )}

      {/* ── Table ──────────────────────────────────────────────────────── */}
      <div className="visits-table-wrap">
        <table className="visits-table" role="table">
          <thead>
            <tr>
              <th>#</th>
              <th>Patient</th>
              <th>Medical #</th>
              <th>Doctor</th>
              <th>Room</th>
              <th>Status</th>
              <th>Started</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <SkeletonRows />
            ) : visits.length === 0 ? (
              <tr>
                <td colSpan={8}>
                  <div className="visits-empty">No visits found.</div>
                </td>
              </tr>
            ) : (
              visits.map((visit, idx) => (
                <tr key={visit.id}>
                  <td style={{ color: '#64748b' }}>
                    {(pageNumber - 1) * pageSize + idx + 1}
                  </td>
                  <td style={{ fontWeight: 600 }}>{visit.patientName}</td>
                  <td style={{ fontFamily: 'monospace', color: '#60a5fa' }}>
                    {visit.medicalNumber}
                  </td>
                  <td>{visit.doctorName}</td>
                  <td>
                    <span style={{
                      background: 'rgba(167,139,250,0.12)',
                      color: '#a78bfa',
                      padding: '0.2rem 0.6rem',
                      borderRadius: '6px',
                      fontSize: '0.82rem',
                      fontWeight: 600,
                    }}>
                      {visit.room || '—'}
                    </span>
                  </td>
                  <td>
                    <StatusBadge status={visit.status} />
                  </td>
                  <td style={{ color: '#64748b', fontSize: '0.85rem' }}>
                    {new Date(visit.startedAt).toLocaleString(undefined, {
                      dateStyle: 'short',
                      timeStyle: 'short',
                    })}
                  </td>
                  <td>
                    {visit.status !== 'Completed' && (
                      <button
                        id={`visit-finish-${visit.id}`}
                        className="visit-finish-btn"
                        onClick={() => finishVisit(visit.id)}
                        title="Mark as Completed"
                      >
                        ✓ Finish
                      </button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* ── Pagination ─────────────────────────────────────────────────── */}
      {totalPages > 1 && (
        <div className="visits-pagination" role="navigation" aria-label="Pagination">
          <button
            id="visits-prev-btn"
            onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
            disabled={pageNumber <= 1 || isLoading}
          >
            ← Prev
          </button>
          <span className="visits-pagination__info">
            Page {pageNumber} of {totalPages}
          </span>
          <button
            id="visits-next-btn"
            onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
            disabled={pageNumber >= totalPages || isLoading}
          >
            Next →
          </button>
        </div>
      )}
    </main>
  )
}
