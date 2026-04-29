import apiClient from './apiClient'

// ===== Types =====

export interface User {
  id: string
  fullName: string
  email: string
  username: string
  roles: string[]
  isActive: boolean
  createdAt: string
}

export interface CreateUserDto {
  fullName: string
  email: string
  username: string
  password: string
  tenantId?: string
  branchId?: string
}

export interface Role {
  id: string
  name: string
  permissions: string[]
}

export interface Permission {
  id: string
  code: string
  module: string
  action: string
}

export interface AuditLog {
  id: string
  userId: string
  userName: string
  action: string
  entityName: string
  entityId: string
  createdAt: string
  ipAddress: string
}

export interface Branch {
  id: string
  name: string
  address: string
  phone: string
}

export interface Tenant {
  id: string
  name: string
  code: string
  isActive: boolean
  createdAt: string
}

export interface CreateTenantDto {
  name: string
  code: string
}

export interface Room {
  id: string
  roomNumber: string
  capacity: number
  isOccupied: boolean
  floorName: string
  branchName: string
}

export interface CreateRoomDto {
  roomNumber: string
  capacity: number
  floorId: string
  branchId: string
  tenantId?: string
}

export interface Floor {
  id: string
  name: string
  branchId: string
}

export interface CreateFloorDto {
  name: string
  number: number
  branchId: string
  tenantId?: string
}

export interface Department {
  id: string
  name: string
  branchId: string
}

export interface CreateDepartmentDto {
  name: string
  branchId: string
  tenantId?: string
}

export interface DashboardStats {
  summary: {
    totalCases: number
    growthPercentage: number
    cashCases: number
    cashGrowth: number
    contractCases: number
    contractGrowth: number
    referralCases: number
    referralGrowth: number
  }
  monthlyChart: Array<{ month: string; count: number }>
  payerDistribution: Array<{ name: string; value: number; color: string }>
  topDoctors: Array<{
    doctorName: string
    imageUrl: string
    totalCases: number
    cashCases: number
    contractCases: number
    lastMonthCases: number
    diff: number
    status: 'Increased' | 'Decreased' | 'Unchanged'
  }>
  specialties: Array<{ specialty: string; count: number }>
}

// ===== Users =====

export const adminService = {
  // Stats
  getStats: async (params?: { tenantId?: string; branchId?: string }): Promise<DashboardStats> => {
    const { data } = await apiClient.get('/admin/stats', { params })
    return data
  },

  // Users
  getUsers: async (): Promise<User[]> => {
    const { data } = await apiClient.get('/users')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  createUser: async (dto: CreateUserDto): Promise<User> => {
    const { data } = await apiClient.post('/users', dto)
    return data
  },

  deleteUser: async (id: string): Promise<void> => {
    await apiClient.delete(`/users/${id}`)
  },

  assignRole: async (userId: string, roleId: string): Promise<void> => {
    await apiClient.post(`/users/${userId}/roles`, { RoleIds: [roleId] })
  },

  // Roles
  getRoles: async (): Promise<Role[]> => {
    const { data } = await apiClient.get('/roles')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  createRole: async (name: string): Promise<Role> => {
    const { data } = await apiClient.post('/roles', { name })
    return data
  },

  // Permissions
  getPermissions: async (): Promise<Permission[]> => {
    const { data } = await apiClient.get('/permissions')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  assignPermissionToRole: async (roleId: string, permissionIds: string[]): Promise<void> => {
    await apiClient.post('/permissions/assign-to-role', { RoleId: roleId, PermissionIds: permissionIds })
  },

  // Audit
  getAuditLogs: async (): Promise<AuditLog[]> => {
    const { data } = await apiClient.get('/audit')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  // Branches
  getBranches: async (tenantId?: string): Promise<Branch[]> => {
    const { data } = await apiClient.get('/branches', { params: { tenantId } })
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  createBranch: async (dto: Omit<Branch, 'id'>): Promise<Branch> => {
    const { data } = await apiClient.post('/branches', dto)
    return data
  },

  // Tenants
  getTenants: async (): Promise<Tenant[]> => {
    const { data } = await apiClient.get('/tenants')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  createTenant: async (dto: CreateTenantDto): Promise<Tenant> => {
    const { data } = await apiClient.post('/tenants', dto)
    return data
  },

  // Rooms
  getRooms: async (params?: { branchId?: string; tenantId?: string }): Promise<Room[]> => {
    const { data } = await apiClient.get('/rooms', { params })
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  createRoom: async (dto: CreateRoomDto): Promise<void> => {
    await apiClient.post('/rooms', dto)
  },

  // Floors
  getFloors: async (branchId: string): Promise<Floor[]> => {
    const { data } = await apiClient.get('/floors/by-branch', { params: { branchId } })
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  createFloor: async (dto: CreateFloorDto): Promise<void> => {
    await apiClient.post('/floors', dto)
  },

  // Departments
  getDepartments: async (branchId: string): Promise<Department[]> => {
    const { data } = await apiClient.get('/departments', { params: { branchId } })
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  createDepartment: async (dto: CreateDepartmentDto): Promise<void> => {
    await apiClient.post('/departments', dto)
  }
}