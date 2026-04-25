import apiClient from './apiClient'

// ===== Types =====

export interface User {
  id: string
  fullName: string
  email: string
  userName: string
  role: string
  isActive: boolean
  createdAt: string
}

export interface CreateUserDto {
  fullName: string
  email: string
  userName: string
  password: string
}

export interface Role {
  id: string
  name: string
  permissions: string[]
}

export interface Permission {
  id: string
  name: string
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

export interface User {
  id: string
  fullName: string
  email: string
  userName: string
  roles: string[]      // ← كان role: string
  isActive: boolean
  createdAt: string
}

// ===== Users =====

export const adminService = {
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
  await apiClient.post(`/users/${userId}/roles`, { roleIds: [roleId] })
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
    await apiClient.post('/permissions/assign-to-role', { roleId, permissionIds })
  },

  // Audit
  getAuditLogs: async (): Promise<AuditLog[]> => {
    const { data } = await apiClient.get('/audit')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  // Branches
  getBranches: async (): Promise<Branch[]> => {
    const { data } = await apiClient.get('/branches')
    if (Array.isArray(data)) return data
    return data.items ?? data.data ?? []
  },

  createBranch: async (dto: Omit<Branch, 'id'>): Promise<Branch> => {
    const { data } = await apiClient.post('/branches', dto)
    return data
  },
}