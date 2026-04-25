import { Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from '../pages/Login/LoginPage'
import ReceptionDashboard from '../pages/Dashboard/Reception/ReceptionDashboard'
import AdminDashboard from '../pages/Dashboard/Admin/AdminDashboard'
import { isAuthenticated, getRole } from '../utils/auth'

function PrivateRoute({ children }: { children: React.ReactNode }) {
  if (!isAuthenticated()) return <Navigate to="/login" replace />
  return <>{children}</>
}

function DashboardRouter() {
  const role = getRole()

  // Reception (مع الـ typo في الباك اند)
  if (role === 'reception' || role === 'recption') return <ReceptionDashboard />

  // Admin (مع الـ space في الباك اند "Adm in")
  if (role === 'Admin' || role === 'Admin' || role === 'admin' || role === 'superadmin') return <AdminDashboard />

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
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  )
}