import apiClient from './apiClient'

export interface Department { id: string; name: string; branchId: string }
export interface Doctor { id: string; name: string }
export interface Room { id: string; roomNumber: string; isAvailable: boolean }
export interface IntakeCreateResponse { id: string; status: string }
export interface IntakeSubmitResponse { patientId: string; medicalNumber: string; visitId: string; message: string }

export interface IntakeFormData {
  step1: Partial<{
    fullName: string; medicalNumber: string; nationalId: string
    dateOfBirth: string; gender: 'Male' | 'Female'; nationality: string
    phone: string; email?: string; idDocumentUrl?: string
  }>
  step2: Partial<{
    emergencyContactName: string; emergencyRelation: string
    emergencyPhone: string; preferredContact: 'WhatsApp' | 'Email' | 'SMS'
  }>
  step3: Partial<{
    departmentId: string; branchId: string
    visitType: 'Inpatient' | 'Emergency' | 'Outpatient'
    doctorId: string; roomId: string; priority: 'Routine' | 'Urgent' | 'Emergency'
    arrivalMethod: 'Walk-in' | 'Ambulance'; chiefComplaint: string
  }>
  step4: Partial<{
    paymentType: 'Cash' | 'Insurance'; insuranceCompany?: string
    policyNumber?: string; coverageClass?: 'VIP' | 'A' | 'B'
  }>
  step5: Partial<{ consentToTreatment: boolean; privacyConsent: boolean; insuranceDataSharing: boolean }>
  step6: Partial<{ needsTranslator: boolean; isVip: boolean; needsMobilityAssistance: boolean; behavioralAlert: boolean }>
}

export interface IntakeUpdatePayload {
  intakeId: string; branchId: string; roomId: string | null
  visitType: number; priority: number | string; chiefComplaint: string
  emergencyContactJson: string; insuranceJson: string; flagsJson: string
}

export interface IntakeSubmitPayload {
  command: string        // ← مطلوب من الـ Backend
  intakeId: string
  tenantId: string
  personalInfo: {
    fullName: string; medicalNumber: string; nationalId: string
    dateOfBirth: string; gender: string; phone: string
    email: string; idCardFrontUrl: string
  }
  emergencyContact: { name: string; relation: string; phone: string }
  contactPreferences: { whatsApp: boolean; sms: boolean; email: boolean }
  visitInfo: {
    branchId: string; visitType: number; arrivalMethod: string
   priority: number | string; chiefComplaint: string
    doctorId: string | null; roomId: string | null
  }
  payment: { paymentType: string; company: string; policyNumber: string; class: string }
  consent: { treatment: boolean; privacy: boolean; insuranceShare: boolean }
  flags: { needsTranslator: boolean; isVip: boolean; needsAssistance: boolean; behavioralAlert: boolean }
}

export const intakeService = {
  getDepartments: async (): Promise<Department[]> => {
    try {
      const { data } = await apiClient.get('/departments')
      const items: any[] = Array.isArray(data) ? data : data.items ?? []
      return items.map((d: any) => ({ id: d.id, name: d.name, branchId: d.branchId ?? '' }))
    } catch { return [] }
  },

  getDoctors: async (): Promise<Doctor[]> => {
    try {
      const { data } = await apiClient.get('/doctors')
      const items: any[] = Array.isArray(data) ? data : data.items ?? []
      return items.map((d: any) => ({ id: d.id, name: d.name }))
    } catch { return [] }
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
    } catch { return [] }
  },

  createIntake: async (payload: { branchId: string }): Promise<IntakeCreateResponse> => {
    const { data } = await apiClient.post('/intake', payload)
    const id = typeof data === 'string' ? data : data.id ?? data
    return { id, status: 'Draft' }
  },

  updateIntake: async (id: string, payload: IntakeUpdatePayload): Promise<void> => {
    await apiClient.put(`/intake/${id}`, payload)
  },

  submitIntake: async (payload: IntakeSubmitPayload): Promise<IntakeSubmitResponse> => {
    const { data } = await apiClient.post('/intake/submit', payload)
    return data
  },

  submitAndPrint: async (payload: IntakeSubmitPayload): Promise<void> => {
  const response = await apiClient.post('/intake/submit-and-print', payload, {
    responseType: 'blob',  // ✅ مهم جداً عشان يستقبل الـ PDF
  })

  // ✅ فتح الـ PDF في tab جديد للطباعة
  const blob = new Blob([response.data], { type: 'application/pdf' })
  const url = URL.createObjectURL(blob)
  const win = window.open(url, '_blank')
  if (win) {
    win.onload = () => {
      win.print()
    }
  }
},
}