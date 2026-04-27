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
import { decodeToken } from '../utils/auth'

const INITIAL_FORM: IntakeFormData = {
  step1: {},
  step2: {},
  step3: {},
  step4: {},
  step5: {},
  step6: {},
}

function getTenantId(): string {
  const token = localStorage.getItem('token') ?? localStorage.getItem('accessToken') ?? ''
  return decodeToken(token)?.orgId ?? ''
}

export function useIntake() {
  const [currentStep, setCurrentStep] = useState(1)
  const [formData, setFormData] = useState<IntakeFormData>(INITIAL_FORM)
  const [intakeId, setIntakeId] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitResult, setSubmitResult] = useState<IntakeSubmitResponse | null>(null)
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
  const goToStep = useCallback((step: number) => setCurrentStep(step), [])

  // ─── بناء payload الـ PUT /intake/{id} ───────────────────────────────────
  const buildUpdatePayload = useCallback((id: string): IntakeUpdatePayload => {
    const { step2, step3, step4, step6 } = formData

    return {
      intakeId: id,
      branchId: step3.departmentId ?? '',
      roomId: step3.roomId || null,          // ← null وليس "" لأن الـ API يتوقع Nullable<Guid>
      visitType:
        step3.visitType === 'Inpatient' ? 1
        : step3.visitType === 'Emergency' ? 2
        : 3,
      priority: step3.priority ?? '',
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
  }, [formData])

  // ─── بناء payload الـ POST /intake/submit ────────────────────────────────
  const buildSubmitPayload = useCallback((id: string): IntakeSubmitPayload => {
    const { step1, step2, step3, step4, step5, step6 } = formData
    const tenantId = getTenantId()

    return {
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
        branchId: step3.departmentId ?? '',
        visitType:
          step3.visitType === 'Inpatient' ? 1
          : step3.visitType === 'Emergency' ? 2
          : 3,
        arrivalMethod: step3.arrivalMethod ?? '',
        priority: step3.priority ?? '',
        chiefComplaint: step3.chiefComplaint ?? '',
        doctorId: step3.doctorId || null,
        roomId: step3.roomId || null,        // ← null وليس "" لتجنب خطأ Guid conversion
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
  }, [formData])

  const handleSubmit = useCallback(async (printBracelet = false) => {
    setIsSubmitting(true)
    setError(null)
    try {
      const { step1, step3 } = formData

      // ── الخطوة 1: POST /intake — يحتاج patientId + branchId ──────────────
      let id = intakeId
      if (!id) {
        const created = await intakeService.createIntake({
          patientId: step1.patientId ?? '',   // ← يُملأ من بحث المريض في step1
          branchId: step3.departmentId ?? '',
        })
        id = created.id
        setIntakeId(id)
      }

      // ── الخطوة 2: PUT /intake/{id} — تحديث بيانات الزيارة ───────────────
      await intakeService.updateIntake(id, buildUpdatePayload(id))

      // ── الخطوة 3: POST /intake/submit أو submit-and-print ────────────────
      const submitPayload = buildSubmitPayload(id)
      const result = printBracelet
        ? await intakeService.submitAndPrint(submitPayload)
        : await intakeService.submitIntake(submitPayload)

      setSubmitResult(result)
    } catch (err: unknown) {
      const axiosErr = err as any
      const serverErrors = axiosErr?.response?.data?.errors
      const serverMsg = serverErrors
        ? Object.values(serverErrors).flat().join(' | ')
        : axiosErr?.response?.data?.title ?? 'حدث خطأ أثناء الإرسال'
      setError(serverMsg)
    } finally {
      setIsSubmitting(false)
    }
  }, [intakeId, formData, buildUpdatePayload, buildSubmitPayload])

  const reset = useCallback(() => {
    setCurrentStep(1)
    setFormData(INITIAL_FORM)
    setIntakeId(null)
    setSubmitResult(null)
    setError(null)
  }, [])

  return {
    currentStep,
    formData,
    intakeId,
    isSubmitting,
    submitResult,
    error,
    departments,
    doctors,
    rooms,
    isLoadingLookups,
    updateStep,
    goNext,
    goPrev,
    goToStep,
    handleSubmit,
    reset,
  }
}