import apiClient from './apiClient'

// ─── Types ───────────────────────────────────────────────────────────────────

export interface IntakeStep1 {
  fullName: string
  medicalNumber: string       // ← جديد
  nationalId: string
  dateOfBirth: string
  gender: 'Male' | 'Female'
  nationality: string
  phone: string
  email?: string
  idDocumentUrl?: string
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

  createIntake: async (payload: object): Promise<IntakeCreateResponse> => {
    const { data } = await apiClient.post('/intake', payload)
    return data
  },

  updateIntake: async (id: string, payload: object): Promise<void> => {
    await apiClient.put(`/intake/${id}`, payload)
  },

  submitIntake: async (payload: object): Promise<IntakeSubmitResponse> => {
    const { data } = await apiClient.post('/intake/submit', payload)
    return data
  },

  submitAndPrint: async (payload: object): Promise<IntakeSubmitResponse> => {
    const { data } = await apiClient.post('/intake/submit-and-print', payload)
    return data
  },
}