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
          background: #fff;
          border: 3px solid #4099ff;
          border-radius: 8px;
          padding: 16px;
          display: flex;
          align-items: center;
          gap: 20px;
          width: 420px;
          margin: 0 auto;
        }
        .wb-card__info {
          flex: 1;
          display: flex;
          flex-direction: column;
          gap: 8px;
        }
        .wb-card__name {
          font-size: 22px;
          font-weight: 900;
          color: #000;
          margin-bottom: 4px;
        }
        .wb-card__field {
          display: flex;
          align-items: center;
          gap: 8px;
          font-size: 14px;
        }
        .wb-card__label {
          font-weight: 700;
          color: #333;
        }
        .wb-card__value {
          font-weight: 600;
          color: #000;
        }
        .wb-card__room-value {
          font-size: 20px;
          font-weight: 900;
          color: #1a5faa;
        }
        .wb-card__qr {
          flex-shrink: 0;
        }
        .wb-card__qr img {
          width: 85px;
          height: 85px;
          border: 1px solid #eee;
        }
      `}</style>

      <div className="wb-card">
        <div className="wb-card__info">
          <div className="wb-card__name">({data.patientName})</div>
          
          <div className="wb-card__field">
            <span className="wb-card__value">{data.medicalNumber}</span>
            <span className="wb-card__label">: الرقم الطبي</span>
          </div>

          <div className="wb-card__field">
            <span className="wb-card__room-value">{data.roomNumber || '-'}</span>
            <span className="wb-card__label">: رقم الغرفة</span>
          </div>
        </div>

        <div className="wb-card__qr">
          <img
            src={`data:image/png;base64,${data.qrCode}`}
            alt="QR Code"
          />
        </div>
      </div>
    </>
  )
}