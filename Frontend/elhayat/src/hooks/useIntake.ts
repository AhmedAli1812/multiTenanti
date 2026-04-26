import { useState, useCallback, useEffect } from 'react'
import {
  intakeService,
  type IntakeFormData,
  type IntakeStep1,
  type IntakeStep2,
  type IntakeStep3,
  type IntakeStep4,
  type IntakeStep5,
  type IntakeStep6,
  type Department,
  type Doctor,
  type Room,
  type IntakeSubmitResponse,
} from '../services/intakeService'

const INITIAL_FORM: IntakeFormData = {
  step1: {},
  step2: {},
  step3: {},
  step4: {},
  step5: {},
  step6: {},
}

export function useIntake() {
  const [currentStep, setCurrentStep] = useState(1)
  const [formData, setFormData] = useState<IntakeFormData>(INITIAL_FORM)
  const [intakeId, setIntakeId] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitResult, setSubmitResult] = useState<IntakeSubmitResponse | null>(null)
  const [error, setError] = useState<string | null>(null)

  // Lookup data
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

  // بناء payload مجمع من كل الـ steps
  const buildPayload = useCallback(() => {
    const { step1, step2, step3, step4, step5, step6 } = formData
    return {
      // Step 1
      fullName: step1.fullName,
      nationalId: step1.nationalId,
      dateOfBirth: step1.dateOfBirth,
      gender: step1.gender,
      nationality: step1.nationality,
      phone: step1.phone,
      email: step1.email,
      idDocumentUrl: step1.idDocumentUrl,
      // Step 2
      emergencyContactName: step2.emergencyContactName,
      emergencyRelation: step2.emergencyRelation,
      emergencyPhone: step2.emergencyPhone,
      preferredContact: step2.preferredContact,
      // Step 3
      departmentId: step3.departmentId,
      visitType: step3.visitType,
      doctorId: step3.doctorId,
      roomId: step3.roomId,
      priority: step3.priority,
      arrivalMethod: step3.arrivalMethod,
      chiefComplaint: step3.chiefComplaint,
      // Step 4
      paymentType: step4.paymentType,
      insuranceCompany: step4.insuranceCompany,
      policyNumber: step4.policyNumber,
      coverageClass: step4.coverageClass,
      // Step 5
      consentToTreatment: step5.consentToTreatment,
      privacyConsent: step5.privacyConsent,
      insuranceDataSharing: step5.insuranceDataSharing,
      // Step 6
      needsTranslator: step6.needsTranslator,
      isVip: step6.isVip,
      needsMobilityAssistance: step6.needsMobilityAssistance,
      behavioralAlert: step6.behavioralAlert,
    }
  }, [formData])

  const handleSubmit = useCallback(async (printBracelet = false) => {
    setIsSubmitting(true)
    setError(null)
    try {
      const payload = buildPayload()
      const result = printBracelet
        ? await intakeService.submitAndPrint(payload)
        : await intakeService.submitIntake(payload)
      setSubmitResult(result)
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'حدث خطأ أثناء الإرسال'
      setError(msg)
    } finally {
      setIsSubmitting(false)
    }
  }, [buildPayload])

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