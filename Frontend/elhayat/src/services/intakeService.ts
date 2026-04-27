import apiClient from './apiClient'

// ─── Types ───────────────────────────────────────────────────────────────────

export interface IntakeStep1 {
  fullName: string
  medicalNumber: string
  nationalId: string
  dateOfBirth: string
  gender: 'Male' | 'Female'
  nationality: string
  phone: string
  email?: string
  idDocumentUrl?: string
  patientId?: string        // ← مطلوب لإنشاء الـ intake
}

export interface IntakeStep2 {
  emergencyContactName: string
  emergencyRelation: string
  emergencyPhone: string
  preferredContact: 'WhatsApp' | 'Email' | 'SMS'
}

export interface IntakeStep3 {
  departmentId: string
  visitType: 'Inpatient' | 'Emergency' | 'Outpatient'
  doctorId: string
  roomId: string
  priority: 'Routine' | 'Urgent' | 'Emergency'
  arrivalMethod: 'Walk-in' | 'Ambulance'
  chiefComplaint: string
}

export interface IntakeStep4 {
  paymentType: 'Cash' | 'Insurance'
  insuranceCompany?: string
  policyNumber?: string
  coverageClass?: 'VIP' | 'A' | 'B'
}

export interface IntakeStep5 {
  consentToTreatment: boolean
  privacyConsent: boolean
  insuranceDataSharing: boolean
}

export interface IntakeStep6 {
  needsTranslator: boolean
  isVip: boolean
  needsMobilityAssistance: boolean
  behavioralAlert: boolean
}

export interface IntakeFormData {
  step1: Partial<IntakeStep1>
  step2: Partial<IntakeStep2>
  step3: Partial<IntakeStep3>
  step4: Partial<IntakeStep4>
  step5: Partial<IntakeStep5>
  step6: Partial<IntakeStep6>
}

export interface IntakeCreateResponse {
  id: string
  status: string
}

export interface IntakeSubmitResponse {
  patientId: string
  medicalNumber: string
  visitId: string
  message: string
}

export interface Department {
  id: string
  name: string
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

// ─── Payload Types ────────────────────────────────────────────────────────────

export interface IntakeCreatePayload {
  patientId: string
  branchId: string
}

export interface IntakeUpdatePayload {
  intakeId: string
  branchId: string
  roomId: string | null
  visitType: number
  priority: string
  chiefComplaint: string
  emergencyContactJson: string
  insuranceJson: string
  flagsJson: string
}

export interface IntakeSubmitPayload {
  intakeId: string
  tenantId: string
  personalInfo: {
    fullName: string
    medicalNumber: string
    nationalId: string
    dateOfBirth: string
    gender: string
    phone: string
    email: string
    idCardFrontUrl: string
  }
  emergencyContact: {
    name: string
    relation: string
    phone: string
  }
  contactPreferences: {
    whatsApp: boolean
    sms: boolean
    email: boolean
  }
  visitInfo: {
    branchId: string
    visitType: number
    arrivalMethod: string
    priority: string
    chiefComplaint: string
    doctorId: string | null
    roomId: string | null
  }
  payment: {
    paymentType: string
    company: string
    policyNumber: string
    class: string
  }
  consent: {
    treatment: boolean
    privacy: boolean
    insuranceShare: boolean
  }
  flags: {
    needsTranslator: boolean
    isVip: boolean
    needsAssistance: boolean
    behavioralAlert: boolean
  }
}

// ─── Service ─────────────────────────────────────────────────────────────────

export const intakeService = {
  getDepartments: async (): Promise<Department[]> => {
    try {
      const { data } = await apiClient.get('/departments')
      const items: any[] = Array.isArray(data) ? data : data.items ?? []
      return items.map((d: any) => ({ id: d.id, name: d.name }))
    } catch {
      return []
    }
  },

  getDoctors: async (): Promise<Doctor[]> => {
    try {
      const { data } = await apiClient.get('/doctors')
      const items: any[] = Array.isArray(data) ? data : data.items ?? []
      return items.map((d: any) => ({ id: d.id, name: d.name }))
    } catch {
      return []
    }
  },

  getRooms: async (): Promise<Room[]> => {
    try {
      const { data } = await apiClient.get('/rooms')
      const items: any[] = Array.isArray(data) ? data : data.items ?? []
      return items.map((r: any) => ({
        id: r.id,
        roomNumber: r.roomNumber,
        isAvailable: r.isOccupied === false,
      }))
    } catch {
      return []
    }
  },

  // POST /api/intake — يحتاج patientId + branchId فقط
  createIntake: async (payload: IntakeCreatePayload): Promise<IntakeCreateResponse> => {
    const { data } = await apiClient.post('/intake', payload)
    return data
  },

  // PUT /api/intake/{id} — تحديث بيانات الزيارة الكاملة
  updateIntake: async (id: string, payload: IntakeUpdatePayload): Promise<void> => {
    await apiClient.put(`/intake/${id}`, payload)
  },

  // POST /api/intake/submit
  submitIntake: async (payload: IntakeSubmitPayload): Promise<IntakeSubmitResponse> => {
    const { data } = await apiClient.post('/intake/submit', payload)
    return data
  },

  // POST /api/intake/submit-and-print
  submitAndPrint: async (payload: IntakeSubmitPayload): Promise<IntakeSubmitResponse> => {
    const { data } = await apiClient.post('/intake/submit-and-print', payload)
    return data
  },
}