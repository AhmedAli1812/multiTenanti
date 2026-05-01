// ─────────────────────────────────────────────────────────────────────────────
// src/services/intakeService.ts
//
// All intake-related API calls.
//
// FIXED BUGS:
//
// 1. VisitType enum mismatch (CRITICAL)
//    Frontend was: Inpatient=1, Emergency=2, default=3
//    Backend is:   Outpatient=1, Emergency=2, Inpatient=3
//    Fixed: parseVisitType now matches backend enum values.
//
// 2. PriorityLevel mismatch (CRITICAL)
//    Frontend label was "Routine" but backend enum is "Normal".
//    VisitInfoDto.Priority is a string field, so we send the string name.
//    parseArrivalMethod sends the backend string name ("WalkIn", "Ambulance").
//
// 3. ArrivalMethod mismatch (CRITICAL)
//    Frontend UI shows "Walk-in" / "Ambulance".
//    Backend ArrivalMethod enum: WalkIn=1, Ambulance=2.
//    VisitInfoDto.ArrivalMethod is a string field; we send the enum name.
//
// 4. FlagsDto field name mismatch (CRITICAL)
//    Backend: BehaviorAlert (not BehavioralAlert)
//    Fixed in IntakeSubmitPayload.flags.
//
// 5. Removed spurious 'command' field from IntakeSubmitPayload.
//    Backend SubmitIntakeCommand has no such property.
//
// 6. submitAndPrint now correctly returns WristbandDto (JSON) — the backend
//    /submit-and-print returns a PDF blob, not JSON. Two separate paths:
//    - submitIntake   → JSON WristbandDto
//    - submitAndPrint → PDF blob (opened in new tab)
// ─────────────────────────────────────────────────────────────────────────────
import apiClient from './apiClient'

// ── Lookup types ──────────────────────────────────────────────────────────────
export interface Department {
  id: string
  name: string
  branchId: string
}

export interface Doctor {
  id: string
  name: string
}

export interface Room {
  id: string
  roomNumber: string
  isAvailable: boolean
}

// ── Response types ────────────────────────────────────────────────────────────
export interface IntakeCreateResponse {
  id: string
  status: string
}

/**
 * WristbandDto — exact match to backend HMS.Application.Dtos.WristbandDto:
 *   public string PatientName
 *   public string MedicalNumber
 *   public string RoomNumber
 *   public byte[] QrCode          ← base64 string in JSON
 */
export interface WristbandDto {
  patientName: string
  medicalNumber: string
  roomNumber: string
  /** base64-encoded PNG from the backend QrCodeService */
  qrCode: string
}

// ── Form data shape (multi-step UI state) ─────────────────────────────────────
export interface IntakeFormData {
  step1: Partial<{
    fullName: string
    medicalNumber: string
    nationalId: string
    dateOfBirth: string        // ISO date string, e.g. "1990-05-20"
    gender: 'Male' | 'Female'
    nationality: string
    phone: string
    email?: string
    idDocumentUrl?: string
  }>
  step2: Partial<{
    emergencyContactName: string
    emergencyRelation: string
    emergencyPhone: string
    preferredContact: 'WhatsApp' | 'Call' | 'SMS'
  }>
  step3: Partial<{
    departmentId: string
    branchId: string
    visitType: 'Outpatient' | 'Emergency' | 'Inpatient'
    doctorId: string
    roomId: string
    /** Backend label: Normal / Urgent / Emergency */
    priority: 'Normal' | 'Urgent' | 'Emergency'
    /** UI may show "Walk-in"; store as "WalkIn" for backend */
    arrivalMethod: 'WalkIn' | 'Ambulance'
    chiefComplaint: string
    notes: string
  }>
  step4: Partial<{
    paymentType: 'Cash' | 'Insurance' | 'Referral'
    insuranceCompany?: string
    policyNumber?: string
    coverageClass?: 'VIP' | 'A' | 'B'
  }>
  step5: Partial<{
    consentToTreatment: boolean
    privacyConsent: boolean
    insuranceDataSharing: boolean
    generalAdmission: boolean
    surgicalConsent: boolean
    financialResponsibility: boolean
    bloodTransfusion: boolean
  }>
  step6: Partial<{
    needsTranslator: boolean
    isVip: boolean
    needsMobilityAssistance: boolean
    behavioralAlert: boolean
  }>
}

// ── API payload types (must match backend DTOs exactly) ───────────────────────

export interface IntakeUpdatePayload {
  intakeId: string
  branchId: string
  roomId: string | null
  visitType: number           // VisitType enum: Outpatient=1, Emergency=2, Inpatient=3
  priority: number            // PriorityLevel enum: Normal=1, Urgent=2, Emergency=3
  chiefComplaint: string
  emergencyContactJson: string
  insuranceJson: string
  flagsJson: string
}

/**
 * IntakeSubmitPayload — mirrors SubmitIntakeCommand exactly.
 *
 * Key differences from original broken payload:
 *   - No 'command' field (backend has none)
 *   - visitType is sent as INTEGER matching VisitType enum
 *   - priority is sent as STRING matching PriorityLevel name (backend uses string)
 *   - arrivalMethod is sent as STRING matching ArrivalMethod name ("WalkIn" not "Walk-in")
 *   - flags.behaviorAlert not behavioralAlert
 *   - personalInfo.dateOfBirth is ISO string → backend parses as DateTime
 */
export interface IntakeSubmitPayload {
  intakeId: string
  tenantId: string
  personalInfo: {
    fullName: string
    medicalNumber: string
    nationalId: string
    dateOfBirth: string       // ISO 8601: "1990-05-20T00:00:00"
    gender: string
    phone: string
    email?: string
    idCardFrontUrl?: string
  }
  emergencyContact: {
    name: string
    relation: string
    phone: string
  }
  contactPreferences: {
    whatsApp: boolean
    sms: boolean
    call: boolean
  }
  visitInfo: {
    branchId: string
    visitType: number         // INTEGER — backend VisitInfoDto.VisitType is enum
    arrivalMethod: string     // STRING name: "WalkIn" | "Ambulance"
    priority: string          // STRING name: "Normal" | "Urgent" | "Emergency"
    chiefComplaint?: string
    notes?: string
    doctorId?: string | null
    roomId?: string | null
  }
  payment: {
    paymentType: string
    company?: string
    policyNumber?: string
    class?: string
  }
  consent: {
    treatment: boolean
    privacy: boolean
    insuranceShare: boolean
    generalAdmission?: boolean
    surgicalConsent?: boolean
    financialResponsibility?: boolean
    bloodTransfusion?: boolean
  }
  flags: {
    needsTranslator: boolean
    isVip: boolean
    needsAssistance: boolean
    /** Backend field name is BehaviorAlert (NOT BehavioralAlert) */
    behaviorAlert: boolean
  }
}

// ── Enum mappers ──────────────────────────────────────────────────────────────

/**
 * Maps the UI visit type label to the backend VisitType enum integer.
 *
 * Backend:  Outpatient=1, Emergency=2, Inpatient=3
 * (Previous bug: Inpatient was mapped to 1 and default was 3 — reversed)
 */
export function parseVisitType(v?: string): number {
  switch (v) {
    case 'Outpatient': return 1
    case 'Emergency':  return 2
    case 'Inpatient':  return 3
    default:           return 1  // fallback: Outpatient
  }
}

/**
 * Maps the UI priority label to the backend PriorityLevel enum integer.
 *
 * Backend: Normal=1, Urgent=2, Emergency=3
 * (Previous bug: "Routine" was used instead of "Normal")
 */
export function parsePriorityInt(p?: string): number {
  switch (p) {
    case 'Normal':    return 1
    case 'Urgent':    return 2
    case 'Emergency': return 3
    default:          return 1
  }
}

/**
 * Returns the priority as the backend string name.
 * VisitInfoDto.Priority is a string field; the backend parses it via Enum.TryParse.
 */
export function parsePriorityStr(p?: string): string {
  switch (p) {
    case 'Normal':    return 'Normal'
    case 'Urgent':    return 'Urgent'
    case 'Emergency': return 'Emergency'
    default:          return 'Normal'
  }
}

/**
 * Maps UI arrival method to backend ArrivalMethod enum name.
 *
 * Backend: WalkIn=1, Ambulance=2  (VisitInfoDto.ArrivalMethod is string)
 * (Previous bug: "Walk-in" — hyphenated — was sent, Enum.TryParse would fail)
 */
export function parseArrivalMethod(a?: string): string {
  switch (a) {
    case 'WalkIn':
    case 'Walk-in':
    case 'Walk In': return 'WalkIn'
    case 'Ambulance': return 'Ambulance'
    default:          return 'WalkIn'
  }
}

// ── Service ───────────────────────────────────────────────────────────────────
export const intakeService = {
  getDepartments: async (): Promise<Department[]> => {
    try {
      const { data } = await apiClient.get('/departments')
      const items: any[] = Array.isArray(data) ? data : (data.items ?? data.data ?? [])
      return items.map((d: any) => ({
        id:       d.id,
        name:     d.name,
        branchId: d.branchId ?? '',
      }))
    } catch {
      return []
    }
  },

  getDoctors: async (): Promise<Doctor[]> => {
    try {
      const { data } = await apiClient.get('/doctors')
      const items: any[] = Array.isArray(data) ? data : (data.items ?? data.data ?? [])
      return items.map((d: any) => ({
        id:   d.id   ?? d.userId,
        name: d.name ?? d.fullName ?? '',
      }))
    } catch {
      return []
    }
  },

  getRooms: async (): Promise<Room[]> => {
    try {
      const { data } = await apiClient.get('/rooms')
      const items: any[] = Array.isArray(data) ? data : (data.items ?? data.data ?? [])
      return items.map((r: any) => ({
        id:          r.id,
        roomNumber:  r.roomNumber,
        // Backend Room.IsOccupied — available when NOT occupied
        isAvailable: r.isOccupied === false || r.isAvailable === true,
      }))
    } catch {
      return []
    }
  },

  /**
   * POST /api/intake — creates a Draft intake and returns its id.
   */
  createIntake: async (payload: { branchId: string }): Promise<IntakeCreateResponse> => {
    const { data } = await apiClient.post('/intake', payload)
    // Backend may return a bare Guid string or an object { id }
    const id = typeof data === 'string' ? data : (data.id ?? data)
    return { id, status: 'Draft' }
  },

  /**
   * PUT /api/intake/{id} — updates the draft intake with form data.
   */
  updateIntake: async (id: string, payload: IntakeUpdatePayload): Promise<void> => {
    await apiClient.put(`/intake/${id}`, payload)
  },

  /**
   * POST /api/intake/submit — submits the intake and returns WristbandDto.
   */
  submitIntake: async (payload: IntakeSubmitPayload): Promise<WristbandDto> => {
    const { data } = await apiClient.post<WristbandDto>('/intake/submit', payload)
    return data
  },

  /**
   * GET /api/intake/{id}/print-wristband — returns a PDF blob for an existing intake.
   */
  printWristband: async (id: string): Promise<void> => {
    const response = await apiClient.get(`/intake/${id}/print-wristband`, {
      responseType: 'blob',
    })
    const blob = new Blob([response.data], { type: 'application/pdf' })
    const url  = URL.createObjectURL(blob)
    const win  = window.open(url, '_blank')
    if (win) {
      win.onload = () => {
        win.print()
        // Close window after printing (optional, some browsers block this)
        // win.close()
      }
    }
    // Release the object URL after a delay
    setTimeout(() => URL.revokeObjectURL(url), 60_000)
  },

  /**
   * Deprecated: use submitIntake then printWristband instead.
   */
  submitAndPrint: async (payload: IntakeSubmitPayload): Promise<void> => {
    const response = await apiClient.post('/intake/submit-and-print', payload, {
      responseType: 'blob',
    })
    const blob = new Blob([response.data], { type: 'application/pdf' })
    const url  = URL.createObjectURL(blob)
    window.open(url, '_blank')
  },
}