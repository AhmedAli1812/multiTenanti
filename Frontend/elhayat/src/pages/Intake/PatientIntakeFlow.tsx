import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { 
  User, Phone, Mail, UploadCloud, AlertCircle, MessageSquare, Hospital, 
  UserCheck, DoorOpen, Check, CreditCard, Coins, ShieldCheck, CheckCircle2, 
  Globe, Star, Accessibility, Printer, ChevronLeft, ChevronRight, X
} from 'lucide-react'
import { useIntake } from '../../hooks/useIntake'
import SearchableSelect from '../../components/SearchableSelect'
import WristbandPrint from '../../components/WristbandPrint'
import './PatientIntakeFlow.css'

const STEPS = [
  { num: 1, label: 'بيانات الهوية' },
  { num: 2, label: 'التواصل / الطوارئ' },
  { num: 3, label: 'الزيارة' },
  { num: 4, label: 'التأمين' },
  { num: 5, label: 'الجوانب القانونية' },
  { num: 6, label: 'المؤشرات التشغيلية' },
]

function Stepper({ current, onStepClick, isValid }: { 
  current: number; 
  onStepClick: (n: number) => void;
  isValid: (n: number) => boolean;
}) {
  return (
    <div className="pif-stepper">
      {STEPS.map((s, idx, arr) => {
        const isDone = isValid(s.num);
        return (
          <button 
            key={s.num} 
            type="button"
            className={`pif-stepper__item ${current === s.num ? 'active' : ''}`}
            onClick={() => onStepClick(s.num)}
          >
            <div className={`pif-stepper__circle ${current === s.num ? 'active' : isDone ? 'done' : ''}`}>
              {isDone ? <Check size={14} /> : s.num}
            </div>
            <span className={`pif-stepper__label ${current === s.num ? 'active' : ''}`}>{s.label}</span>
            {idx < arr.length - 1 && <div className={`pif-stepper__line ${isDone ? 'done' : ''}`} />}
          </button>
        );
      })}
    </div>
  )
}

// ─── Step 1 ───────────────────────────────────────────────────────────────────
function Step1({ data, onChange }: {
  data: ReturnType<typeof useIntake>['formData']['step1']
  onChange: (d: typeof data) => void
}) {
  return (
    <div className="pif-step-content">
      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon"><User size={20} /></span>
          <h3>المعلومات الشخصية</h3>
        </div>
        <div className="pif-grid pif-grid--2">
          <div className="pif-field">
            <label>الاسم الكامل (بالعربية) <span className="pif-required">*</span></label>
            <input className="pif-input" placeholder="أحمد محمد علي"
              value={data.fullName ?? ''}
              onChange={e => onChange({ ...data, fullName: e.target.value })} />
          </div>
          <div className="pif-field">
            <label>الرقم الطبي <span className="pif-required">*</span></label>
            <input className="pif-input" placeholder="MED-XXXXXX"
              value={data.medicalNumber ?? ''}
              onChange={e => onChange({ ...data, medicalNumber: e.target.value })} />
          </div>
          <div className="pif-field">
            <label>رقم الهوية / الإقامة <span className="pif-required">*</span></label>
            <input className="pif-input" placeholder="1XXXXXXXXX"
              value={data.nationalId ?? ''}
              onChange={e => onChange({ ...data, nationalId: e.target.value })} />
          </div>
          <div className="pif-field">
            <label>تاريخ الميلاد <span className="pif-required">*</span></label>
            <input className="pif-input" type="date"
              value={data.dateOfBirth ?? ''}
              onChange={e => onChange({ ...data, dateOfBirth: e.target.value })} />
          </div>
          <div className="pif-field">
            <label>الجنسية</label>
            <select className="pif-input" value={data.nationality ?? 'مصري'}
              onChange={e => onChange({ ...data, nationality: e.target.value })}>
              <option>مصري</option>
              <option>سعودي</option>
              <option>إماراتي</option>
              <option>أردني</option>
              <option>أخرى</option>
            </select>
          </div>
          <div className="pif-field pif-field--full">
            <label>الجنس <span className="pif-required">*</span></label>
            <div className="pif-radio-group">
              {(['Male', 'Female'] as const).map(g => (
                <label key={g} className={`pif-radio-card ${data.gender === g ? 'selected' : ''}`}>
                  <input type="radio" name="gender" value={g} checked={data.gender === g}
                    onChange={() => onChange({ ...data, gender: g })} />
                  {g === 'Male' ? 'ذكر' : 'أنثى'}
                </label>
              ))}
            </div>
          </div>
        </div>
      </div>

      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon"><Phone size={20} /></span>
          <h3>معلومات التواصل</h3>
        </div>
        <div className="pif-grid pif-grid--2">
          <div className="pif-field">
            <label>رقم الجوال <span className="pif-required">*</span></label>
            <div className="pif-input-prefix">
              <span>+20</span>
              <input className="pif-input" placeholder="1XXXXXXXX"
                value={data.phone ?? ''}
                onChange={e => onChange({ ...data, phone: e.target.value })} />
            </div>
          </div>
          <div className="pif-field">
            <label>البريد الإلكتروني (اختياري)</label>
            <input className="pif-input" type="email" placeholder="example@gmail.com"
              value={data.email ?? ''}
              onChange={e => onChange({ ...data, email: e.target.value })} />
          </div>
        </div>
      </div>

      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon"><UserCheck size={20} /></span>
          <h3>التحقق من الهوية</h3>
        </div>
        <div className="pif-upload-zone">
          <span className="pif-upload-zone__icon"><UploadCloud size={32} /></span>
          <p>اسحب الملف أو اضغط للرفع</p>
          <span className="pif-upload-zone__hint">يدعم صيغ JPG، PNG، PDF (بحد أقصى 5MB)</span>
          <input type="file" accept=".jpg,.jpeg,.png,.pdf" className="pif-upload-zone__input"
            onChange={e => {
              const file = e.target.files?.[0]
              if (file) onChange({ ...data, idDocumentUrl: file.name })
            }} />
        </div>
        {data.idDocumentUrl && (
          <div className="pif-upload-preview">
            <span>📄 {data.idDocumentUrl}</span>
            <button className="pif-upload-preview__remove"
              onClick={() => onChange({ ...data, idDocumentUrl: undefined })}><X size={16} /></button>
          </div>
        )}
      </div>
    </div>
  )
}

// ─── Step 2 ───────────────────────────────────────────────────────────────────
function Step2({ data, onChange }: {
  data: ReturnType<typeof useIntake>['formData']['step2']
  onChange: (d: typeof data) => void
}) {
  return (
    <div className="pif-step-content">
      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon pif-card__icon--red"><AlertCircle size={20} /></span>
          <h3>جهة اتصال الطوارئ <span className="pif-required">إلزامي</span></h3>
        </div>
        <div className="pif-grid pif-grid--3">
          <div className="pif-field">
            <label>اسم الشخص المختص</label>
            <input className="pif-input" placeholder="الاسم الثلاثي"
              value={data.emergencyContactName ?? ''}
              onChange={e => onChange({ ...data, emergencyContactName: e.target.value })} />
          </div>
          <div className="pif-field">
            <label>صلة القرابة</label>
            <select className="pif-input" value={data.emergencyRelation ?? ''}
              onChange={e => onChange({ ...data, emergencyRelation: e.target.value })}>
              <option value="">اختر</option>
              <option>أب / أم</option>
              <option>زوج / زوجة</option>
              <option>أخ / أخت</option>
              <option>ابن / ابنة</option>
              <option>صديق</option>
            </select>
          </div>
          <div className="pif-field">
            <label>رقم الجوال</label>
            <input className="pif-input" placeholder="5XXXXXXXX"
              value={data.emergencyPhone ?? ''}
              onChange={e => onChange({ ...data, emergencyPhone: e.target.value })} />
          </div>
        </div>
      </div>

      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon"><MessageSquare size={20} /></span>
          <h3>تفضيلات التواصل</h3>
        </div>
        <div className="pif-radio-group">
          {[
            { id: 'WhatsApp', label: 'واتساب (WhatsApp)', icon: <MessageSquare size={18} /> },
            { id: 'Email', label: 'البريد الإلكتروني', icon: <Mail size={18} /> },
            { id: 'SMS', label: 'رسائل قصيرة (SMS)', icon: <Phone size={18} /> },
          ].map((opt) => (
            <label key={opt.id} className={`pif-radio-card ${data.preferredContact === opt.id ? 'selected' : ''}`}>
              <input 
                type="radio" 
                name="preferredContact" 
                value={opt.id} 
                checked={data.preferredContact === opt.id} 
                onChange={() => onChange({ ...data, preferredContact: opt.id as any })} 
              />
              <span className="pif-radio-card__icon">{opt.icon}</span>
              <span className="pif-radio-card__label">{opt.label}</span>
              {data.preferredContact === opt.id && <Check className="pif-radio-card__check" size={14} />}
            </label>
          ))}
        </div>
      </div>
    </div>
  )
}

// ─── Step 3 ───────────────────────────────────────────────────────────────────
function Step3({ data, onChange, departments, doctors, rooms }: {
  data: ReturnType<typeof useIntake>['formData']['step3']
  onChange: (d: typeof data) => void
  departments: ReturnType<typeof useIntake>['departments']
  doctors: ReturnType<typeof useIntake>['doctors']
  rooms: ReturnType<typeof useIntake>['rooms']
}) {
  return (
    <div className="pif-step-content">
      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon"><Hospital size={20} /></span>
          <h3>معلومات الحجز</h3>
        </div>
        <div className="pif-grid pif-grid--2">
          <div className="pif-field">
            <SearchableSelect
              label="القسم"
              required
              placeholder="اختر القسم"
              value={data.departmentId ?? ''}
              options={departments.map(d => ({ id: d.id, label: d.name }))}
              onChange={val => onChange({ ...data, departmentId: val })}
              icon={<Hospital size={16} />}
            />
          </div>
          <div className="pif-field">
            <label>نوع الزيارة</label>
            <div className="pif-segmented">
              {([
                { value: 'Inpatient', label: 'داخلي' },
                { value: 'Emergency', label: 'طوارئ' },
                { value: 'Outpatient', label: 'حاضنات' },
              ] as const).map(t => (
                <button key={t.value}
                  className={`pif-segmented__btn ${data.visitType === t.value ? 'active' : ''}`}
                  onClick={() => onChange({ ...data, visitType: t.value })}>
                  {t.label}
                </button>
              ))}
            </div>
          </div>
          {data.visitType === 'Inpatient' && (
            <>
              <div className="pif-field">
                <SearchableSelect
                  label="الدكتور"
                  placeholder="اختر الدكتور"
                  value={data.doctorId ?? ''}
                  options={doctors.map(d => ({ id: d.id, label: `د. ${d.name}` }))}
                  onChange={val => onChange({ ...data, doctorId: val })}
                  icon={<UserCheck size={16} />}
                />
              </div>
              <div className="pif-field">
                <SearchableSelect
                  label="الغرفة"
                  placeholder="اختر الغرفة"
                  value={data.roomId ?? ''}
                  options={rooms.filter(r => r.isAvailable).map(r => ({ id: r.id, label: `غرفة ${r.roomNumber}` }))}
                  onChange={val => onChange({ ...data, roomId: val })}
                  icon={<DoorOpen size={16} />}
                />
              </div>
            </>
          )}
        </div>

        <div className="pif-field pif-field--full" style={{ marginTop: '1.5rem' }}>
          <label>مستوى الأولوية</label>
          <div className="pif-priority-group">
            {([
              { value: 'Normal', label: 'اعتيادي', icon: <CheckCircle2 size={14} />, cls: 'routine' },
              { value: 'Urgent', label: 'عاجل', icon: <AlertCircle size={14} />, cls: 'urgent' },
              { value: 'Emergency', label: 'طارئ للغاية', icon: <AlertCircle size={14} />, cls: 'emergency' },
            ] as const).map(p => (
              <button key={p.value}
                type="button"
                className={`pif-priority-btn pif-priority-btn--${p.cls} ${data.priority === p.value ? 'active' : ''}`}
                onClick={() => onChange({ ...data, priority: p.value })}>
                <span>{p.icon}</span> {p.label}
              </button>
            ))}
          </div>
        </div>

        <div className="pif-field pif-field--full" style={{ marginTop: '1.5rem' }}>
          <label>وسيلة الوصول</label>
          <div className="pif-radio-group">
            {[
              { id: 'WalkIn', label: 'سيراً (Walk-in)', icon: <UserCheck size={18} /> },
              { id: 'Ambulance', label: 'إسعاف (Ambulance)', icon: <Accessibility size={18} /> },
            ].map((opt) => (
              <label key={opt.id} className={`pif-radio-card ${data.arrivalMethod === opt.id ? 'selected' : ''}`}>
                <input 
                  type="radio" 
                  name="arrivalMethod" 
                  value={opt.id} 
                  checked={data.arrivalMethod === opt.id} 
                  onChange={() => onChange({ ...data, arrivalMethod: opt.id as any })} 
                />
                <span className="pif-radio-card__icon">{opt.icon}</span>
                <span className="pif-radio-card__label">{opt.label}</span>
                {data.arrivalMethod === opt.id && <Check className="pif-radio-card__check" size={14} />}
              </label>
            ))}
          </div>
        </div>

        <div className="pif-field pif-field--full" style={{ marginTop: '1.5rem' }}>
          <label>الشكوى الرئيسية</label>
          <textarea className="pif-input pif-textarea"
            placeholder="اكتب الشكوى الرئيسية للمريض هنا..." rows={4}
            value={data.chiefComplaint ?? ''}
            onChange={e => onChange({ ...data, chiefComplaint: e.target.value })} />
        </div>
      </div>
    </div>
  )
}

// ─── Step 4 ───────────────────────────────────────────────────────────────────
function Step4({ data, onChange }: {
  data: ReturnType<typeof useIntake>['formData']['step4']
  onChange: (d: typeof data) => void
}) {
  return (
    <div className="pif-step-content">
      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon"><CreditCard size={20} /></span>
          <h3>نوع السداد</h3>
        </div>
        <div className="pif-payment-group">
          {[
            { id: 'Cash', label: 'نقدي (Cash)', sub: 'دفع ذاتي مباشر', icon: <Coins size={24} />, reset: true },
            { id: 'Insurance', label: 'تأمين صحي', sub: 'شركات التأمين المتعاقدة', icon: <ShieldCheck size={24} />, reset: false },
            { id: 'Referral', label: 'تحويل طبي', sub: 'حالات تحويل الأطباء', icon: <UserCheck size={24} />, reset: true },
          ].map((opt) => (
            <label key={opt.id} className={`pif-payment-card ${data.paymentType === opt.id ? 'selected' : ''}`}>
              <input 
                type="radio" 
                name="paymentType" 
                checked={data.paymentType === opt.id}
                onChange={() => onChange({ 
                  ...data, 
                  paymentType: opt.id as any, 
                  ...(opt.reset ? { insuranceCompany: undefined, policyNumber: undefined, coverageClass: undefined } : {}) 
                })} 
              />
              <span className="pif-payment-card__icon">{opt.icon}</span>
              <strong>{opt.label}</strong>
              <small>{opt.sub}</small>
              {data.paymentType === opt.id && <CheckCircle2 className="pif-payment-card__check" size={16} />}
            </label>
          ))}
        </div>
      </div>

      {data.paymentType === 'Insurance' && (
        <div className="pif-card pif-card--animate">
          <div className="pif-card__header">
            <span className="pif-card__icon"><CreditCard size={20} /></span>
            <h3>تفاصيل الوثيقة</h3>
          </div>
          <div className="pif-grid pif-grid--2">
            <div className="pif-field">
              <label>شركة التأمين</label>
              <select className="pif-input" value={data.insuranceCompany ?? ''}
                onChange={e => onChange({ ...data, insuranceCompany: e.target.value })}>
                <option value="">اختر الشركة</option>
                <option>شركة بوبا العربية</option>
                <option>تكافل الراجحي</option>
                <option>ميدغلف</option>
                <option>أخرى</option>
              </select>
            </div>
            <div className="pif-field">
              <label>رقم البوليصة</label>
              <input className="pif-input" placeholder="POL-XXXXXXXX"
                value={data.policyNumber ?? ''}
                onChange={e => onChange({ ...data, policyNumber: e.target.value })} />
            </div>
          </div>
          <div className="pif-field pif-field--full" style={{ marginTop: '1.5rem' }}>
            <label>فئة التغطية</label>
            <div className="pif-coverage-group">
              {(['VIP', 'A', 'B'] as const).map(cls => (
                <button key={cls}
                  type="button"
                  className={`pif-coverage-btn ${data.coverageClass === cls ? 'active' : ''}`}
                  onClick={() => onChange({ ...data, coverageClass: cls })}>
                  {cls === 'VIP' && <Star size={14} style={{ marginLeft: 6 }} />} فئة {cls}
                </button>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// ─── Step 5 ───────────────────────────────────────────────────────────────────
function Step5({ data, onChange }: {
  data: ReturnType<typeof useIntake>['formData']['step5']
  onChange: (d: typeof data) => void
}) {
  const consents = [
    { key: 'consentToTreatment' as const, title: 'الموافقة على العلاج', desc: 'أقر بموافقتي على تلقي الرعاية الطبية والفحوصات اللازمة تحت إشراف الفريق الطبي المختص.' },
    { key: 'privacyConsent' as const, title: 'خصوصية البيانات (HIPAA/GDPR)', desc: 'أوافق على سياسة معالجة البيانات الشخصية والطبية وفقاً للمعايير العالمية لحماية الخصوصية.' },
    { key: 'insuranceDataSharing' as const, title: 'مشاركة بيانات التأمين', desc: 'أفوض المنشأة بمشاركة البيانات الضرورية مع جهة التأمين الصحي لتغطية تكاليف العلاج.' },
  ]

  return (
    <div className="pif-step-content">
      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon"><CheckCircle2 size={20} /></span>
          <h3>إقرارات المريض</h3>
        </div>
        <div className="pif-consents">
          {consents.map(c => (
            <label key={c.key} className={`pif-consent-item ${data[c.key] ? 'checked' : ''}`}>
              <input type="checkbox" checked={data[c.key] ?? false}
                onChange={e => onChange({ ...data, [c.key]: e.target.checked })} />
              <div className="pif-consent-item__checkbox">{data[c.key] ? <Check size={14} /> : ''}</div>
              <div>
                <strong>{c.title}</strong>
                <p>{c.desc}</p>
              </div>
            </label>
          ))}
        </div>
      </div>
    </div>
  )
}

// ─── Step 6 ───────────────────────────────────────────────────────────────────
function Step6({ data, onChange }: {
  data: ReturnType<typeof useIntake>['formData']['step6']
  onChange: (d: typeof data) => void
}) {
  const flags = [
    { key: 'needsTranslator' as const, label: 'مترجم لغة', desc: 'المريض بحاجة لمترجم (الإنجليزية / الأردية).', icon: <Globe size={20} />, color: 'blue' },
    { key: 'isVip' as const, label: 'مريض VIP', desc: 'توفير رعاية متميزة وخصوصية إضافية للمريض.', icon: <Star size={20} />, color: 'amber' },
    { key: 'needsMobilityAssistance' as const, label: 'مساعدة حركية', desc: 'المريض يحتاج لكرسي متحرك أو مساعدة في التنقل.', icon: <Accessibility size={20} />, color: 'orange' },
    { key: 'behavioralAlert' as const, label: 'تنبيه سلوكي', desc: 'تاريخ من السلوك العدواني أو عدم الامتثال.', icon: <AlertCircle size={20} />, color: 'red' },
  ]

  return (
    <div className="pif-step-content">
      <div className="pif-flags-grid">
        {flags.map(f => (
          <div key={f.key} className={`pif-flag-card pif-flag-card--${f.color} ${data[f.key] ? 'active' : ''}`}>
            <div className="pif-flag-card__top">
              <label className="pif-toggle">
                <input type="checkbox" checked={data[f.key] ?? false}
                  onChange={e => onChange({ ...data, [f.key]: e.target.checked })} />
                <span className="pif-toggle__slider" />
              </label>
              <span className={`pif-flag-card__icon pif-flag-card__icon--${f.color}`}>{f.icon}</span>
            </div>
            <strong>{f.label}</strong>
            <p>{f.desc}</p>
          </div>
        ))}
      </div>
    </div>
  )
}

// ─── Success Screen ───────────────────────────────────────────────────────────
function SuccessScreen({ result, onReset }: {
  result: NonNullable<ReturnType<typeof useIntake>['wristbandData']>
  onReset: () => void
}) {
  const navigate = useNavigate()
  return (
    <div className="pif-success">
      <div className="pif-success__icon"><CheckCircle2 size={64} /></div>
      <h2>تم تسجيل المريض بنجاح</h2>
      <div className="pif-success__info">
        <div><span>الرقم الطبي</span><strong>{result.medicalNumber}</strong></div>
        <div><span>رقم الغرفة</span><strong>{result.roomNumber ?? '—'}</strong></div>
      </div>
      <div className="pif-success__actions">
        <button className="pif-btn pif-btn--primary" onClick={() => navigate('/dashboard')}>العودة للداشبورد</button>
        <button className="pif-btn pif-btn--ghost" onClick={onReset}>تسجيل مريض جديد</button>
      </div>
    </div>
  )
}

// ─── Main ─────────────────────────────────────────────────────────────────────
export default function PatientIntakeFlow({ onClose }: { onClose?: () => void }) {
  const navigate = useNavigate()
  const [showPrintModal, setShowPrintModal] = useState(false)
  const {
    currentStep, formData, isSubmitting, wristbandData, error,
    departments, doctors, rooms, isLoadingLookups,
    updateStep, goNext, goPrev, setStep, handleSubmit, reset,
  } = useIntake()

  const handleFinalSubmit = async (print: boolean) => {
    await handleSubmit(print)
    if (print) setShowPrintModal(true)
  }

  if (wristbandData && !showPrintModal) {
    return <SuccessScreen result={wristbandData} onReset={() => { reset(); onClose?.(); }} />
  }

  const isStepValid = (step: number) => {
    if (step === 1) return !!(formData.step1.fullName && formData.step1.medicalNumber && formData.step1.nationalId && formData.step1.phone)
    if (step === 2) return !!(formData.step2.emergencyContactName && formData.step2.emergencyPhone)
    if (step === 3) return !!(formData.step3.departmentId)
    if (step === 5) return !!(formData.step5.consentToTreatment && formData.step5.privacyConsent)
    return true
  }

  const canGoNext = () => isStepValid(currentStep)

  const isModal = !!onClose

  return (
    <div className={`pif-wrap ${isModal ? 'pif-wrap--modal' : ''}`} dir="rtl">
      {!isModal && (
        <header className="pif-header">
          <button className="pif-header__back" onClick={() => navigate('/dashboard')}>
            <ChevronRight size={18} /> نظام إدارة المرضى
          </button>
        </header>
      )}

      <div className="pif-body">
        {!isModal && (
          <div className="pif-title">
            <h1>تسجيل المريض</h1>
            <p>الرجاء إدخال البيانات الشخصية للمريض بدقة لضمان تكامل السجل الطبي.</p>
          </div>
        )}

        <Stepper current={currentStep} onStepClick={setStep} isValid={isStepValid} />

        {isLoadingLookups && currentStep === 3 ? (
          <div className="pif-loading">جارٍ تحميل البيانات...</div>
        ) : (
          <>
            {currentStep === 1 && <Step1 data={formData.step1} onChange={d => updateStep('step1', d)} />}
            {currentStep === 2 && <Step2 data={formData.step2} onChange={d => updateStep('step2', d)} />}
            {currentStep === 3 && <Step3 data={formData.step3} onChange={d => updateStep('step3', d)} departments={departments} doctors={doctors} rooms={rooms} />}
            {currentStep === 4 && <Step4 data={formData.step4} onChange={d => updateStep('step4', d)} />}
            {currentStep === 5 && <Step5 data={formData.step5} onChange={d => updateStep('step5', d)} />}
            {currentStep === 6 && <Step6 data={formData.step6} onChange={d => updateStep('step6', d)} />}
          </>
        )}

        {error && <div className="pif-error">{error}</div>}

      </div>

      <div className="pif-nav">
        <button className="pif-btn pif-btn--ghost" onClick={() => isModal ? onClose() : navigate('/dashboard')} disabled={isSubmitting}>
          إلغاء
        </button>
        <div className="pif-nav__right">
          {currentStep > 1 && (
            <button className="pif-btn pif-btn--outline" onClick={goPrev} disabled={isSubmitting}>
              <ChevronRight size={18} /> السابق
            </button>
          )}
          {currentStep < 6 ? (
            <button className="pif-btn pif-btn--primary" onClick={goNext} disabled={!canGoNext()}>
              التالي <ChevronLeft size={18} />
            </button>
          ) : (
            <div className="pif-submit-group">
              <button className="pif-btn pif-btn--outline" onClick={() => handleFinalSubmit(false)} disabled={isSubmitting}>
                {isSubmitting ? '...' : 'حفظ بدون طباعة'}
              </button>
              <button className="pif-btn pif-btn--primary" onClick={() => handleFinalSubmit(true)} disabled={isSubmitting}>
                {isSubmitting ? 'جارٍ الإرسال...' : <><Printer size={18} /> إرسال وطباعة السوار</>}
              </button>
            </div>
          )}
        </div>
      </div>

      {showPrintModal && wristbandData && (
        <WristbandPrint
          data={wristbandData}
          onClose={() => { setShowPrintModal(false); onClose?.(); }}
        />
      )}
    </div>
  )
}