import { useState, useEffect, useCallback } from 'react'
import {
  intakeService,
  type IntakeFormData,
  type IntakeSubmitPayload,
  type IntakeUpdatePayload,
  type Department,
  type Doctor,
  type Room,
  type IntakeSubmitResponse,
} from '../services/intakeService'
import { decodeJwt } from 'jose'

const INITIAL_FORM: IntakeFormData = {
  step1: {},
  step2: {},
  step3: {},
  step4: {},
  step5: {},
  step6: {},
}

export interface WristbandData {
  patientName: string
  medicalNumber: string
  roomNumber: string
  qrCode: string
}

function getTenantId(): string {
  const token = localStorage.getItem('token') ?? localStorage.getItem('accessToken') ?? ''
  if (!token) return ''
  try {
    const decoded = decodeJwt(token)
    return (
      (decoded['tenantId'] as string) ??
      (decoded['TenantId'] as string) ??
      (decoded['orgId'] as string) ??
      (decoded['tenant_id'] as string) ??
      (decoded['http://schemas.microsoft.com/identity/claims/tenantid'] as string) ??
      ''
    )
  } catch {
    return ''
  }
}

function parsePriority(p?: string): number {
  if (p === 'Routine') return 1
  if (p === 'Urgent') return 2
  if (p === 'Emergency') return 3
  return 1
}

function parseVisitType(v?: string): number {
  if (v === 'Inpatient') return 1
  if (v === 'Emergency') return 2
  return 3
}

export function useIntake() {
  const [currentStep, setCurrentStep] = useState(1)
  const [formData, setFormData] = useState<IntakeFormData>(INITIAL_FORM)
  const [intakeId, setIntakeId] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitResult, setSubmitResult] = useState<IntakeSubmitResponse | null>(null)
  const [wristbandData, setWristbandData] = useState<WristbandData | null>(null)  // ✅ جديد
  const [error, setError] = useState<string | null>(null)

  const [departments, setDepartments] = useState<Department[]>([])
  const [doctors, setDoctors] = useState<Doctor[]>([])
  const [rooms, setRooms] = useState<Room[]>([])
  const [isLoadingLookups, setIsLoadingLookups] = useState(true)

  useEffect(() => {
    Promise.all([
      intakeService.getDepartments(),
      intakeService.getDoctors(),
      intakeService.getRooms(),
    ]).then(([depts, docs, rms]) => {
      setDepartments(depts)
      setDoctors(docs)
      setRooms(rms)
      setIsLoadingLookups(false)
    }).catch(() => setIsLoadingLookups(false))
  }, [])

  const updateStep = useCallback(<K extends keyof IntakeFormData>(
    step: K,
    data: Partial<IntakeFormData[K]>
  ) => {
    setFormData(prev => ({
      ...prev,
      [step]: { ...prev[step], ...data },
    }))
  }, [])

  const goNext = useCallback(() => setCurrentStep(s => Math.min(s + 1, 6)), [])
  const goPrev = useCallback(() => setCurrentStep(s => Math.max(s - 1, 1)), [])

  const getBranchId = useCallback((): string => {
    const dept = departments.find(d => d.id === formData.step3.departmentId)
    return dept?.branchId ?? formData.step3.branchId ?? ''
  }, [departments, formData.step3])

  const buildUpdatePayload = useCallback((id: string): IntakeUpdatePayload => {
    const { step2, step3, step4, step6 } = formData
    return {
      intakeId: id,
      branchId: getBranchId(),
      roomId: step3.roomId || null,
      visitType: parseVisitType(step3.visitType),
      priority: parsePriority(step3.priority),
      chiefComplaint: step3.chiefComplaint ?? '',
      emergencyContactJson: JSON.stringify({
        name: step2.emergencyContactName ?? '',
        relation: step2.emergencyRelation ?? '',
        phone: step2.emergencyPhone ?? '',
      }),
      insuranceJson: JSON.stringify({
        paymentType: step4.paymentType ?? 'Cash',
        company: step4.insuranceCompany ?? '',
        policyNumber: step4.policyNumber ?? '',
        class: step4.coverageClass ?? '',
      }),
      flagsJson: JSON.stringify({
        needsTranslator: step6.needsTranslator ?? false,
        isVip: step6.isVip ?? false,
        needsAssistance: step6.needsMobilityAssistance ?? false,
        behavioralAlert: step6.behavioralAlert ?? false,
      }),
    }
  }, [formData, getBranchId])

  const buildSubmitPayload = useCallback((id: string): IntakeSubmitPayload => {
    const { step1, step2, step3, step4, step5, step6 } = formData
    const tenantId = getTenantId()
    const branchId = getBranchId()

    return {
      command: 'SubmitIntake',
      intakeId: id,
      tenantId,
      personalInfo: {
        fullName: step1.fullName ?? '',
        medicalNumber: step1.medicalNumber ?? '',
        nationalId: step1.nationalId ?? '',
        dateOfBirth: step1.dateOfBirth ?? '',
        gender: step1.gender ?? '',
        phone: step1.phone ?? '',
        email: step1.email ?? '',
        idCardFrontUrl: step1.idDocumentUrl ?? '',
      },
      emergencyContact: {
        name: step2.emergencyContactName ?? '',
        relation: step2.emergencyRelation ?? '',
        phone: step2.emergencyPhone ?? '',
      },
      contactPreferences: {
        whatsApp: step2.preferredContact === 'WhatsApp',
        sms: step2.preferredContact === 'SMS',
        email: step2.preferredContact === 'Email',
      },
      visitInfo: {
        branchId,
        visitType: parseVisitType(step3.visitType),
        arrivalMethod: step3.arrivalMethod ?? '',
        priority: step3.priority ?? 'Routine',
        chiefComplaint: step3.chiefComplaint ?? '',
        doctorId: step3.doctorId || null,
        roomId: step3.roomId || null,
      },
      payment: {
        paymentType: step4.paymentType ?? 'Cash',
        company: step4.insuranceCompany ?? '',
        policyNumber: step4.policyNumber ?? '',
        class: step4.coverageClass ?? '',
      },
      consent: {
        treatment: step5.consentToTreatment ?? false,
        privacy: step5.privacyConsent ?? false,
        insuranceShare: step5.insuranceDataSharing ?? false,
      },
      flags: {
        needsTranslator: step6.needsTranslator ?? false,
        isVip: step6.isVip ?? false,
        needsAssistance: step6.needsMobilityAssistance ?? false,
        behavioralAlert: step6.behavioralAlert ?? false,
      },
    }
  }, [formData, getBranchId])

  const handleSubmit = useCallback(async (printBracelet = false) => {
    setIsSubmitting(true)
    setError(null)
    try {
      const branchId = getBranchId()
      if (!branchId) throw new Error('لازم تختار القسم الأول')

      const created = await intakeService.createIntake({ branchId })
      const id = created.id
      setIntakeId(id)

      await intakeService.updateIntake(id, buildUpdatePayload(id))

      const payload = buildSubmitPayload(id)

      if (printBracelet) {
        // ✅ بيجيب الـ wristband data ويعرض الـ component
        const data = await intakeService.submitAndPrint(payload)
        setWristbandData({
          patientName: data.patientName ?? '',
          medicalNumber: data.medicalNumber ?? '',
          roomNumber: data.roomNumber ?? '-',
          qrCode: data.qrCode ?? '',
        })
        setSubmitResult({ patientId: '', medicalNumber: data.medicalNumber ?? '', visitId: '', message: 'تم التسجيل' })
      } else {
        const result = await intakeService.submitIntake(payload)
        setSubmitResult(result)
      }

    } catch (err: any) {
      const serverErrors = err?.response?.data?.errors
      const serverMsg = serverErrors
        ? Object.values(serverErrors).flat().join(' | ')
        : err?.response?.data?.message ?? err?.message ?? 'حدث خطأ أثناء الإرسال'
      setError(serverMsg)
    } finally {
      setIsSubmitting(false)
    }
  }, [getBranchId, buildUpdatePayload, buildSubmitPayload])

  const reset = useCallback(() => {
    setCurrentStep(1)
    setFormData(INITIAL_FORM)
    setIntakeId(null)
    setSubmitResult(null)
    setWristbandData(null)
    setError(null)
  }, [])

  return {
    currentStep,
    formData,
    intakeId,
    isSubmitting,
    submitResult,
    wristbandData,          // ✅ جديد
    clearWristband: () => setWristbandData(null),  // ✅ جديد
    error,
    departments,
    doctors,
    rooms,
    isLoadingLookups,
    updateStep,
    goNext,
    goPrev,
    handleSubmit,
    reset,
  }
}