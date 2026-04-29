// ─────────────────────────────────────────────────────────────────────────────
// src/hooks/useIntake.ts
//
// FIXED BUGS:
//
// 1. Removed 'command' field from buildSubmitPayload (doesn't exist on backend)
// 2. visitType now uses parseVisitType() with correct Outpatient=1/Emergency=2/Inpatient=3
// 3. priority in submitPayload now sent as string name (Normal/Urgent/Emergency)
//    not as number or "Routine"
// 4. priority in updatePayload now uses parsePriorityInt()
// 5. arrivalMethod sent as backend enum name via parseArrivalMethod()
// 6. flags.behaviorAlert (not behavioralAlert) to match FlagsDto
// 7. submitAndPrint now calls submitIntake first (JSON) to get WristbandDto,
//    then optionally triggers the PDF print endpoint separately — the
//    /submit-and-print endpoint returns a PDF blob, not JSON
// 8. getTenantId now imported from utils/auth (single implementation)
// ─────────────────────────────────────────────────────────────────────────────
import { useState, useEffect, useCallback } from 'react'
import {
  intakeService,
  parseVisitType,
  parsePriorityInt,
  parsePriorityStr,
  parseArrivalMethod,
  type IntakeFormData,
  type IntakeSubmitPayload,
  type IntakeUpdatePayload,
  type Department,
  type Doctor,
  type Room,
  type WristbandDto,
} from '../services/intakeService'
import { getTenantId } from '../utils/auth'

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
  const [wristbandData, setWristbandData] = useState<WristbandDto | null>(null)
  const [error, setError] = useState<string | null>(null)

  const [departments, setDepartments] = useState<Department[]>([])
  const [doctors, setDoctors] = useState<Doctor[]>([])
  const [rooms, setRooms] = useState<Room[]>([])
  const [isLoadingLookups, setIsLoadingLookups] = useState(true)

  // ── Load lookups on mount ────────────────────────────────────────────────────
  useEffect(() => {
    Promise.all([
      intakeService.getDepartments(),
      intakeService.getDoctors(),
      intakeService.getRooms(),
    ])
      .then(([depts, docs, rms]) => {
        setDepartments(depts)
        setDoctors(docs)
        setRooms(rms)
      })
      .catch(() => {/* swallow — component shows empty selects */})
      .finally(() => setIsLoadingLookups(false))
  }, [])

  // ── Form state management ────────────────────────────────────────────────────
  const updateStep = useCallback(
    <K extends keyof IntakeFormData>(step: K, data: Partial<IntakeFormData[K]>) => {
      setFormData((prev) => ({ ...prev, [step]: { ...prev[step], ...data } }))
    },
    [],
  )

  const goNext = useCallback(() => setCurrentStep((s) => Math.min(s + 1, 6)), [])
  const goPrev = useCallback(() => setCurrentStep((s) => Math.max(s - 1, 1)), [])

  // ── Branch resolution ────────────────────────────────────────────────────────
  const getBranchId = useCallback((): string => {
    // Prefer branchId derived from the selected department, fall back to
    // directly entered branchId
    const dept = departments.find((d) => d.id === formData.step3.departmentId)
    return dept?.branchId ?? formData.step3.branchId ?? ''
  }, [departments, formData.step3])

  // ── Payload builders ─────────────────────────────────────────────────────────

  const buildUpdatePayload = useCallback(
    (id: string): IntakeUpdatePayload => {
      const { step2, step3, step4, step6 } = formData
      return {
        intakeId:            id,
        branchId:            getBranchId(),
        roomId:              step3.roomId || null,
        visitType:           parseVisitType(step3.visitType),
        priority:            parsePriorityInt(step3.priority),
        chiefComplaint:      step3.chiefComplaint ?? '',
        emergencyContactJson: JSON.stringify({
          name:     step2.emergencyContactName ?? '',
          relation: step2.emergencyRelation    ?? '',
          phone:    step2.emergencyPhone       ?? '',
        }),
        insuranceJson: JSON.stringify({
          paymentType:  step4.paymentType     ?? 'Cash',
          company:      step4.insuranceCompany ?? '',
          policyNumber: step4.policyNumber    ?? '',
          class:        step4.coverageClass   ?? '',
        }),
        flagsJson: JSON.stringify({
          needsTranslator:  step6.needsTranslator       ?? false,
          isVip:            step6.isVip                 ?? false,
          needsAssistance:  step6.needsMobilityAssistance ?? false,
          // FlagsDto.BehaviorAlert (NOT BehavioralAlert)
          behaviorAlert:    step6.behavioralAlert        ?? false,
        }),
      }
    },
    [formData, getBranchId],
  )

  const buildSubmitPayload = useCallback(
    (id: string): IntakeSubmitPayload => {
      const { step1, step2, step3, step4, step5, step6 } = formData
      const tenantId = getTenantId()
      const branchId = getBranchId()

      // Ensure dateOfBirth is a proper ISO string.
      // Input type="date" gives "YYYY-MM-DD"; backend needs full ISO datetime.
      const rawDob = step1.dateOfBirth ?? ''
      const dob    = rawDob && !rawDob.includes('T') ? `${rawDob}T00:00:00` : rawDob

      return {
        // NOTE: 'command' field REMOVED — backend SubmitIntakeCommand has no such property
        intakeId: id,
        tenantId,
        personalInfo: {
          fullName:      step1.fullName      ?? '',
          medicalNumber: step1.medicalNumber ?? '',
          nationalId:    step1.nationalId    ?? '',
          dateOfBirth:   dob,
          gender:        step1.gender        ?? '',
          phone:         step1.phone         ?? '',
          email:         step1.email,
          idCardFrontUrl: step1.idDocumentUrl,
        },
        emergencyContact: {
          name:     step2.emergencyContactName ?? '',
          relation: step2.emergencyRelation    ?? '',
          phone:    step2.emergencyPhone       ?? '',
        },
        contactPreferences: {
          whatsApp: step2.preferredContact === 'WhatsApp',
          sms:      step2.preferredContact === 'SMS',
          email:    step2.preferredContact === 'Email',
        },
        visitInfo: {
          branchId,
          // INTEGER enum: Outpatient=1, Emergency=2, Inpatient=3
          visitType:     parseVisitType(step3.visitType),
          // STRING name for backend Enum.TryParse: "WalkIn" | "Ambulance"
          arrivalMethod: parseArrivalMethod(step3.arrivalMethod),
          // STRING name: "Normal" | "Urgent" | "Emergency"
          priority:      parsePriorityStr(step3.priority),
          chiefComplaint: step3.chiefComplaint,
          doctorId:      step3.doctorId || null,
          roomId:        step3.roomId   || null,
        },
        payment: {
          paymentType:  step4.paymentType     ?? 'Cash',
          company:      step4.insuranceCompany,
          policyNumber: step4.policyNumber,
          class:        step4.coverageClass,
        },
        consent: {
          treatment:     step5.consentToTreatment  ?? false,
          privacy:       step5.privacyConsent       ?? false,
          insuranceShare: step5.insuranceDataSharing ?? false,
        },
        flags: {
          needsTranslator:  step6.needsTranslator        ?? false,
          isVip:            step6.isVip                  ?? false,
          needsAssistance:  step6.needsMobilityAssistance ?? false,
          // Backend FlagsDto property: BehaviorAlert (not BehavioralAlert)
          behaviorAlert:    step6.behavioralAlert         ?? false,
        },
      }
    },
    [formData, getBranchId],
  )

  // ── Submit handler ───────────────────────────────────────────────────────────
  /**
   * @param printBracelet  If true, also opens the wristband PDF in a new tab.
   *                       The JSON WristbandDto is always returned so the UI
   *                       can display the wristband component regardless.
   */
  const handleSubmit = useCallback(
    async (printBracelet = false) => {
      setIsSubmitting(true)
      setError(null)

      try {
        const branchId = getBranchId()
        if (!branchId) {
          throw new Error('Please select a department / branch first.')
        }

        // Step 1: Create draft intake
        const created  = await intakeService.createIntake({ branchId })
        const id       = created.id
        setIntakeId(id)

        // Step 2: Update draft with full form data
        await intakeService.updateIntake(id, buildUpdatePayload(id))

        // Step 3: Build submit payload (all bugs fixed)
        const payload = buildSubmitPayload(id)

        // Step 4: Submit and get WristbandDto JSON
        const wristband = await intakeService.submitIntake(payload)
        setWristbandData(wristband)

        // Step 5 (optional): Also trigger PDF print in background
        if (printBracelet) {
          // Fire-and-forget — don't block the UI on the PDF call
          intakeService.printWristband(id).catch(() => {
            // PDF print failed — data is already committed, just skip printing
          })
        }
      } catch (err: any) {
        // Extract the most useful error message from the response
        const serverErrors = err?.response?.data?.errors
        const serverMsg    = serverErrors
          ? Object.values(serverErrors).flat().join(' | ')
          : err?.response?.data?.message ??
            err?.response?.data?.title   ??
            err?.message                 ??
            'An error occurred during submission.'
        setError(serverMsg)
      } finally {
        setIsSubmitting(false)
      }
    },
    [getBranchId, buildUpdatePayload, buildSubmitPayload],
  )

  const reset = useCallback(() => {
    setCurrentStep(1)
    setFormData(INITIAL_FORM)
    setIntakeId(null)
    setWristbandData(null)
    setError(null)
  }, [])

  return {
    currentStep,
    formData,
    intakeId,
    isSubmitting,
    wristbandData,
    clearWristband: () => setWristbandData(null),
    error,
    departments,
    doctors,
    rooms,
    isLoadingLookups,
    updateStep,
    goNext,
    goPrev,
    setStep: setCurrentStep,
    handleSubmit,
    reset,
  }
}