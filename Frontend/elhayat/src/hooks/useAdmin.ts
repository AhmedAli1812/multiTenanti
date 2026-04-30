import { useState, useEffect, useCallback } from 'react'
import {
  adminService,
  type User, type Role, type Permission, type AuditLog, type Branch, type CreateUserDto, type Tenant, type CreateTenantDto,
  type Room, type CreateRoomDto, type Floor, type CreateFloorDto, type Department, type CreateDepartmentDto,
  type DashboardStats
} from '../services/adminService'

export function useUsers() {
  const [users, setUsers] = useState<User[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await adminService.getUsers()
      setUsers(data)
      setError(null)
    } catch {
      setError('تعذر تحميل المستخدمين')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const createUser = async (dto: CreateUserDto) => {
    await adminService.createUser(dto)
    await load()
  }

  const deleteUser = async (id: string) => {
    await adminService.deleteUser(id)
    setUsers(prev => prev.filter(u => u.id !== id))
  }

  const assignRole = async (userId: string, roleId: string) => {
    await adminService.assignRole(userId, roleId)
    await load()
  }

  return { users, isLoading, error, refresh: load, createUser, deleteUser, assignRole }
}

export function useRoles() {
  const [roles, setRoles] = useState<Role[]>([])
  const [isLoading, setIsLoading] = useState(true)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await adminService.getRoles()
      setRoles(data)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const createRole = async (name: string) => {
    await adminService.createRole(name)
    await load()
  }

  return { roles, isLoading, refresh: load, createRole }
}

export function usePermissions() {
  const [permissions, setPermissions] = useState<Permission[]>([])
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    adminService.getPermissions()
      .then(setPermissions)
      .finally(() => setIsLoading(false))
  }, [])

  const assignToRole = async (roleId: string, permissionIds: string[]) => {
    await adminService.assignPermissionToRole(roleId, permissionIds)
  }

  return { permissions, isLoading, assignToRole }
}

export function useAuditLogs() {
  const [logs, setLogs] = useState<AuditLog[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await adminService.getAuditLogs()
      setLogs(data)
      setError(null)
    } catch {
      setError('تعذر تحميل سجل المراجعة')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  return { logs, isLoading, error, refresh: load }
}

export function useBranches(initialTenantId?: string) {
  const [branches, setBranches] = useState<Branch[]>([])
  const [isLoading, setIsLoading] = useState(true)

  const load = useCallback(async (tId?: string) => {
    setIsLoading(true)
    try {
      const data = await adminService.getBranches(tId)
      setBranches(data)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { load(initialTenantId) }, [load, initialTenantId])

  const createBranch = async (dto: Omit<Branch, 'id'>) => {
    await adminService.createBranch(dto)
    await load(initialTenantId)
  }

  return { branches, isLoading, refresh: () => load(initialTenantId), loadForTenant: load, createBranch }
}

export function useTenants() {
  const [tenants, setTenants] = useState<Tenant[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await adminService.getTenants()
      setTenants(data)
      setError(null)
    } catch {
      setError('تعذر تحميل المستأجرين')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const createTenant = async (dto: { name: string; code: string }) => {
    await adminService.createTenant(dto)
    await load()
  }

  return { tenants, isLoading, error, refresh: load, createTenant }
}

export function useRooms(initialParams?: { branchId?: string; tenantId?: string }) {
  const [rooms, setRooms] = useState<Room[]>([])
  const [isLoading, setIsLoading] = useState(true)

  const load = useCallback(async (params?: { branchId?: string; tenantId?: string }) => {
    setIsLoading(true)
    try {
      const data = await adminService.getRooms(params)
      setRooms(data)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { load(initialParams) }, [load, initialParams])

  const createRoom = async (dto: CreateRoomDto) => {
    await adminService.createRoom(dto)
    await load(initialParams)
  }

  return { rooms, isLoading, refresh: () => load(initialParams), loadWithParams: load, createRoom }
}

export function useFloors() {
  const [floors, setFloors] = useState<Floor[]>([])
  const [isLoading, setIsLoading] = useState(false)

  const load = useCallback(async (branchId: string) => {
    if (!branchId) return
    setIsLoading(true)
    try {
      const data = await adminService.getFloors(branchId)
      setFloors(data)
    } finally {
      setIsLoading(false)
    }
  }, [])

  const createFloor = async (dto: CreateFloorDto) => {
    await adminService.createFloor(dto)
    if (dto.branchId) load(dto.branchId)
  }

  return { floors, isLoading, loadForBranch: load, createFloor }
}

export function useDepartments() {
  const [departments, setDepartments] = useState<Department[]>([])
  const [isLoading, setIsLoading] = useState(false)

  const load = useCallback(async (branchId: string) => {
    if (!branchId) return
    setIsLoading(true)
    try {
      const data = await adminService.getDepartments(branchId)
      setDepartments(data)
    } finally {
      setIsLoading(false)
    }
  }, [])

  const createDepartment = async (dto: CreateDepartmentDto) => {
    await adminService.createDepartment(dto)
    if (dto.branchId) load(dto.branchId)
  }

  return { departments, isLoading, loadForBranch: load, createDepartment }
}

export function useStats(initialParams?: { tenantId?: string; branchId?: string }) {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const load = useCallback(async (params?: { tenantId?: string; branchId?: string }) => {
    setIsLoading(true)
    try {
      const data = await adminService.getStats(params)
      setStats(data)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { load(initialParams) }, [load, initialParams])

  return { stats, isLoading, refresh: () => load(initialParams), loadWithParams: load }
}