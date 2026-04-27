import { Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from '../pages/Login/LoginPage'
import ReceptionDashboard from '../pages/Dashboard/Reception/Receptiondashboard'
import AdminDashboard from '../pages/Dashboard/Admin/AdminDashboard'
import PatientIntakeFlow from '../pages/Intake/PatientIntakeFlow'
import { isAuthenticated, getRole } from '../utils/auth'

function PrivateRoute({ children }: { children: React.ReactNode }) {
  if (!isAuthenticated()) return <Navigate to="/login" replace />
  return <>{children}</>
}

function DashboardRouter() {
  const role = getRole()
if (role === 'Receptionist' || role === 'Reception') return <ReceptionDashboard />
  if (['Super Admin', 'Admin', 'admin', 'superadmin'].includes(role)) return <AdminDashboard />
  return <Navigate to="/login" replace />
}

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/dashboard"
        element={
          <PrivateRoute>
            <DashboardRouter />
          </PrivateRoute>
        }
      />
      <Route
        path="/patients/new"
        element={
          <PrivateRoute>
            <PatientIntakeFlow />
          </PrivateRoute>
        }
      />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  )
}