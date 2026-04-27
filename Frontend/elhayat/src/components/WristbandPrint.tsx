import { useEffect, useRef } from 'react'

interface WristbandData {
  patientName: string
  medicalNumber: string
  roomNumber: string
  qrCode: string // base64
}

interface WristbandPrintProps {
  data: WristbandData
  onClose: () => void
}

export default function WristbandPrint({ data, onClose }: WristbandPrintProps) {
  const printRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    // فتح print dialog تلقائياً
    const timer = setTimeout(() => {
      window.print()
    }, 500)
    return () => clearTimeout(timer)
  }, [])

  return (
    <>
      {/* ===== Print Styles ===== */}
      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700;900&display=swap');

        @media print {
          body * { visibility: hidden !important; }
          #wristband-print, #wristband-print * { visibility: visible !important; }
          #wristband-print {
            position: fixed !important;
            top: 0 !important;
            left: 0 !important;
            width: 100vw !important;
            height: 100vh !important;
            display: flex !important;
            align-items: center !important;
            justify-content: center !important;
            background: white !important;
          }
        }

        @media screen {
          .wristband-overlay {
            position: fixed;
            inset: 0;
            background: rgba(0,0,0,0.6);
            backdrop-filter: blur(4px);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 9999;
            font-family: 'Cairo', sans-serif;
          }
          .wristband-modal {
            background: #fff;
            border-radius: 16px;
            padding: 2rem;
            max-width: 520px;
            width: 90%;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            direction: rtl;
          }
          .wristband-modal__title {
            font-size: 18px;
            font-weight: 700;
            color: #1a3a5c;
            margin-bottom: 1.5rem;
            text-align: center;
          }
          .wristband-modal__actions {
            display: flex;
            gap: 10px;
            margin-top: 1.5rem;
          }
          .wristband-modal__btn {
            flex: 1;
            padding: 10px;
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            border: none;
            font-family: 'Cairo', sans-serif;
          }
          .wristband-modal__btn--print {
            background: #1a5faa;
            color: #fff;
          }
          .wristband-modal__btn--close {
            background: #f1f3f4;
            color: #5f6368;
          }
        }
      `}</style>

      {/* Screen Overlay */}
      <div className="wristband-overlay">
        <div className="wristband-modal">
          <div className="wristband-modal__title">🖨️ معاينة طباعة سوار المريض</div>

          {/* الـ Wristband نفسه */}
          <div id="wristband-print" ref={printRef}>
            <WristbandCard data={data} />
          </div>

          <div className="wristband-modal__actions">
            <button className="wristband-modal__btn wristband-modal__btn--print" onClick={() => window.print()}>
              🖨️ طباعة السوار
            </button>
            <button className="wristband-modal__btn wristband-modal__btn--close" onClick={onClose}>
              تخطي وإغلاق
            </button>
          </div>
        </div>
      </div>
    </>
  )
}

function WristbandCard({ data }: { data: WristbandData }) {
  return (
    <>
      <style>{`
        .wb-card {
          font-family: 'Cairo', sans-serif;
          direction: rtl;
          background: linear-gradient(135deg, #e8f4fd 0%, #d0e8f7 100%);
          border: 2px dashed #5ba3d9;
          border-radius: 12px;
          padding: 14px 16px;
          display: flex;
          align-items: center;
          gap: 14px;
          width: 380px;
          margin: 0 auto;
          box-shadow: 0 2px 12px rgba(26,95,170,0.12);
        }
        .wb-card__info {
          flex: 1;
          display: flex;
          flex-direction: column;
          gap: 6px;
        }
        .wb-card__hospital {
          font-size: 10px;
          color: #5ba3d9;
          font-weight: 600;
          letter-spacing: 0.5px;
          margin-bottom: 2px;
        }
        .wb-card__name {
          font-size: 20px;
          font-weight: 900;
          color: #1a3a5c;
          line-height: 1.2;
        }
        .wb-card__row {
          display: flex;
          gap: 16px;
          align-items: center;
        }
        .wb-card__field {
          display: flex;
          flex-direction: column;
        }
        .wb-card__label {
          font-size: 9px;
          color: #5ba3d9;
          font-weight: 600;
          text-transform: uppercase;
          letter-spacing: 0.3px;
        }
        .wb-card__value {
          font-size: 13px;
          font-weight: 700;
          color: #1a3a5c;
        }
        .wb-card__room-value {
          font-size: 20px;
          font-weight: 900;
          color: #1a5faa;
        }
        .wb-card__divider {
          width: 1px;
          height: 30px;
          background: #5ba3d9;
          opacity: 0.3;
        }
        .wb-card__qr {
          display: flex;
          flex-direction: column;
          align-items: center;
          gap: 4px;
          flex-shrink: 0;
        }
        .wb-card__qr img {
          width: 72px;
          height: 72px;
          border-radius: 6px;
          border: 1px solid #c0d8ee;
        }
        .wb-card__qr-label {
          font-size: 9px;
          color: #5ba3d9;
          font-weight: 600;
        }
      `}</style>

      <div className="wb-card">
        <div className="wb-card__info">
          <div className="wb-card__hospital">🏥 MedScope — نظام إدارة المستشفى</div>
          <div className="wb-card__name">{data.patientName}</div>
          <div className="wb-card__row">
            <div className="wb-card__field">
              <span className="wb-card__label">الاسم الكامل</span>
              <span className="wb-card__value">{data.patientName}</span>
            </div>
            <div className="wb-card__divider" />
            <div className="wb-card__field">
              <span className="wb-card__label">رقم الملف الطبي</span>
              <span className="wb-card__value">{data.medicalNumber}</span>
            </div>
            {data.roomNumber && data.roomNumber !== '-' && (
              <>
                <div className="wb-card__divider" />
                <div className="wb-card__field">
                  <span className="wb-card__label">رقم الغرفة</span>
                  <span className="wb-card__room-value">{data.roomNumber}</span>
                </div>
              </>
            )}
          </div>
        </div>

        <div className="wb-card__qr">
          <img
            src={`data:image/png;base64,${data.qrCode}`}
            alt="QR Code"
          />
          <span className="wb-card__qr-label">{data.medicalNumber}</span>
        </div>
      </div>
    </>
  )
}