import { useNavigate } from 'react-router-dom'
import { useIntake } from '../../hooks/useIntake'
import './PatientIntakeFlow.css'

const STEPS = [
  { num: 1, label: 'بيانات الهوية' },
  { num: 2, label: 'التواصل / الطوارئ' },
  { num: 3, label: 'الزيارة' },
  { num: 4, label: 'التأمين' },
  { num: 5, label: 'الجوانب القانونية' },
  { num: 6, label: 'المؤشرات التشغيلية' },
]

function Stepper({ current }: { current: number }) {
  return (
    <div className="pif-stepper">
      {[...STEPS].reverse().map((s, idx, arr) => (
        <div key={s.num} className="pif-stepper__item">
          <div className={`pif-stepper__circle ${current === s.num ? 'active' : current > s.num ? 'done' : ''}`}>
            {current > s.num ? '✓' : s.num}
          </div>
          <span className={`pif-stepper__label ${current === s.num ? 'active' : ''}`}>{s.label}</span>
          {idx < arr.length - 1 && <div className={`pif-stepper__line ${current > s.num ? 'done' : ''}`} />}
        </div>
      ))}
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
          <span className="pif-card__icon">👤</span>
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
                  {g === 'Male' ? '🧔 ذكر' : '👩 أنثى'}
                </label>
              ))}
            </div>
          </div>
        </div>
      </div>

      <div className="pif-card">
        <div className="pif-card__header">
          <span className="pif-card__icon">📞</span>
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
          <span className="pif-card__icon">🪪</span>
          <h3>التحقق من الهوية</h3>
        </div>
        <div className="pif-upload-zone">
          <span className="pif-upload-zone__icon">☁️</span>
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
              onClick={() => onChange({ ...data, idDocumentUrl: undefined })}>✕</button>
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
          <span className="pif-card__icon pif-card__icon--red">🚨</span>
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
          <span className="pif-card__icon">💬</span>
          <h3>تفضيلات التواصل</h3>
        </div>
        <div className="pif-contact-options">
          {([
            { value: 'WhatsApp', label: 'واتساب (WhatsApp)', icon: '📱' },
            { value: 'Email', label: 'البريد الإلكتروني', icon: '📧' },
            { value: 'SMS', label: 'رسائل قصيرة (SMS)', icon: '💬' },
          ] as const).map(opt => (
            <label key={opt.value} className={`pif-contact-option ${data.preferredContact === opt.value ? 'selected' : ''}`}>
              <input type="radio" name="contact" value={opt.value}
                checked={data.preferredContact === opt.value}
                onChange={() => onChange({ ...data, preferredContact: opt.value })} />
              <span>{opt.icon} {opt.label}</span>
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
          <span className="pif-card__icon">🏥</span>
          <h3>معلومات الحجز</h3>
        </div>
        <div className="pif-grid pif-grid--2">
          <div className="pif-field">
            <label>القسم <span className="pif-required">*</span></label>
            <select className="pif-input" value={data.departmentId ?? ''}
              onChange={e => onChange({ ...data, departmentId: e.target.value })}>
              <option value="">اختر القسم</option>
              {departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
            </select>
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
          <div className="pif-field">
            <label>الدكتور</label>
            <select className="pif-input" value={data.doctorId ?? ''}
              onChange={e => onChange({ ...data, doctorId: e.target.value })}>
              <option value="">اختر الدكتور</option>
              {doctors.map(d => <option key={d.id} value={d.id}>د. {d.name}</option>)}
            </select>
          </div>
          <div className="pif-field">
            <label>الغرفة</label>
            <select className="pif-input" value={data.roomId ?? ''}
              onChange={e => onChange({ ...data, roomId: e.target.value })}>
              <option value="">اختر الغرفة</option>
              {rooms.filter(r => r.isAvailable).map(r => <option key={r.id} value={r.id}>{r.roomNumber}</option>)}
            </select>
          </div>
        </div>

        <div className="pif-field pif-field--full" style={{ marginTop: '1.5rem' }}>
          <label>مستوى الأولوية</label>
          <div className="pif-priority-group">
            {([
              { value: 'Routine', label: 'روتيني', icon: '✓', cls: 'routine' },
              { value: 'Urgent', label: 'عاجل', icon: '⚠', cls: 'urgent' },
              { value: 'Emergency', label: 'طارئ', icon: '!', cls: 'emergency' },
            ] as const).map(p => (
              <button key={p.value}
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
            {([
              { value: 'Walk-in', label: '🚶 مشياً (تلقائي)' },
              { value: 'Ambulance', label: '🚑 إسعاف' },
            ] as const).map(a => (
              <label key={a.value} className={`pif-radio-card ${data.arrivalMethod === a.value ? 'selected' : ''}`}>
                <input type="radio" name="arrival" value={a.value}
                  checked={data.arrivalMethod === a.value}
                  onChange={() => onChange({ ...data, arrivalMethod: a.value })} />
                {a.label}
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
          <span className="pif-card__icon">💳</span>
          <h3>نوع السداد</h3>
        </div>
        <div className="pif-payment-group">
          <label className={`pif-payment-card ${data.paymentType === 'Cash' ? 'selected' : ''}`}>
            <input type="radio" name="payment" checked={data.paymentType === 'Cash'}
              onChange={() => onChange({ ...data, paymentType: 'Cash', insuranceCompany: undefined, policyNumber: undefined, coverageClass: undefined })} />
            <span className="pif-payment-card__icon">🪙</span>
            <strong>نقدي (Cash)</strong>
            <small>دفع ذاتي مباشر</small>
          </label>
          <label className={`pif-payment-card ${data.paymentType === 'Insurance' ? 'selected' : ''}`}>
            <input type="radio" name="payment" checked={data.paymentType === 'Insurance'}
              onChange={() => onChange({ ...data, paymentType: 'Insurance' })} />
            <span className="pif-payment-card__icon">🛡️</span>
            <strong>تأمين صحي</strong>
            <small>شركات التأمين المتعاقدة</small>
          </label>
        </div>
      </div>

      {data.paymentType === 'Insurance' && (
        <div className="pif-card pif-card--animate">
          <div className="pif-card__header">
            <span className="pif-card__icon">📄</span>
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
                  className={`pif-coverage-btn ${data.coverageClass === cls ? 'active' : ''}`}
                  onClick={() => onChange({ ...data, coverageClass: cls })}>
                  {cls === 'VIP' && '⭐ '}فئة {cls}
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
          <span className="pif-card__icon">✅</span>
          <h3>إقرارات المريض</h3>
        </div>
        <div className="pif-consents">
          {consents.map(c => (
            <label key={c.key} className={`pif-consent-item ${data[c.key] ? 'checked' : ''}`}>
              <input type="checkbox" checked={data[c.key] ?? false}
                onChange={e => onChange({ ...data, [c.key]: e.target.checked })} />
              <div className="pif-consent-item__checkbox">{data[c.key] ? '✓' : ''}</div>
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
    { key: 'needsTranslator' as const, label: 'مترجم لغة', desc: 'المريض بحاجة لمترجم (الإنجليزية / الأردية).', icon: '🌐', color: 'blue' },
    { key: 'isVip' as const, label: 'مريض VIP', desc: 'توفير رعاية متميزة وخصوصية إضافية للمريض.', icon: '⭐', color: 'amber' },
    { key: 'needsMobilityAssistance' as const, label: 'مساعدة حركية', desc: 'المريض يحتاج لكرسي متحرك أو مساعدة في التنقل.', icon: '♿', color: 'orange' },
    { key: 'behavioralAlert' as const, label: 'تنبيه سلوكي', desc: 'تاريخ من السلوك العدواني أو عدم الامتثال.', icon: '⚠', color: 'red' },
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
  result: NonNullable<ReturnType<typeof useIntake>['submitResult']>
  onReset: () => void
}) {
  const navigate = useNavigate()
  return (
    <div className="pif-success">
      <div className="pif-success__icon">✅</div>
      <h2>تم تسجيل المريض بنجاح</h2>
      <div className="pif-success__info">
        <div><span>الرقم الطبي</span><strong>{result.medicalNumber}</strong></div>
        <div><span>رقم الزيارة</span><strong>{result.visitId}</strong></div>
      </div>
      <div className="pif-success__actions">
        <button className="pif-btn pif-btn--primary" onClick={() => navigate('/dashboard')}>العودة للداشبورد</button>
        <button className="pif-btn pif-btn--ghost" onClick={onReset}>تسجيل مريض جديد</button>
      </div>
    </div>
  )
}

// ─── Main ─────────────────────────────────────────────────────────────────────
export default function PatientIntakeFlow() {
  const navigate = useNavigate()
  const {
    currentStep, formData, isSubmitting, submitResult, error,
    departments, doctors, rooms, isLoadingLookups,
    updateStep, goNext, goPrev, handleSubmit, reset,
  } = useIntake()

  if (submitResult) return <SuccessScreen result={submitResult} onReset={reset} />

  const canGoNext = () => {
    if (currentStep === 1) return !!(formData.step1.fullName && formData.step1.medicalNumber && formData.step1.nationalId && formData.step1.phone)
    if (currentStep === 2) return !!(formData.step2.emergencyContactName && formData.step2.emergencyPhone)
    if (currentStep === 3) return !!(formData.step3.departmentId)
    if (currentStep === 5) return !!(formData.step5.consentToTreatment && formData.step5.privacyConsent)
    return true
  }

  return (
    <div className="pif-wrap" dir="rtl">
      <header className="pif-header">
        <button className="pif-header__back" onClick={() => navigate('/dashboard')}>
          ← نظام إدارة المرضى
        </button>
      </header>

      <div className="pif-body">
        <div className="pif-title">
          <h1>تسجيل المريض</h1>
          <p>الرجاء إدخال البيانات الشخصية للمريض بدقة لضمان تكامل السجل الطبي.</p>
        </div>

        <Stepper current={currentStep} />

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

        <div className="pif-nav">
          <button className="pif-btn pif-btn--ghost" onClick={() => navigate('/dashboard')} disabled={isSubmitting}>
            إلغاء
          </button>
          <div className="pif-nav__right">
            {currentStep > 1 && (
              <button className="pif-btn pif-btn--outline" onClick={goPrev} disabled={isSubmitting}>
                ← السابق
              </button>
            )}
            {currentStep < 6 ? (
              <button className="pif-btn pif-btn--primary" onClick={goNext} disabled={!canGoNext()}>
                التالي →
              </button>
            ) : (
              <div className="pif-submit-group">
                <button className="pif-btn pif-btn--outline" onClick={() => handleSubmit(false)} disabled={isSubmitting}>
                  {isSubmitting ? '...' : 'حفظ بدون طباعة'}
                </button>
                <button className="pif-btn pif-btn--primary" onClick={() => handleSubmit(true)} disabled={isSubmitting}>
                  {isSubmitting ? 'جارٍ الإرسال...' : '🖨 إرسال وطباعة السوار'}
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}