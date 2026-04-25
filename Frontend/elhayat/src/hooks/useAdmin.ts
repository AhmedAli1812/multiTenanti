import { useState, useEffect, useCallback } from 'react'
import {
  adminService,
  type User, type Role, type Permission, type AuditLog, type Branch, type CreateUserDto
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

export function useBranches() {
  const [branches, setBranches] = useState<Branch[]>([])
  const [isLoading, setIsLoading] = useState(true)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await adminService.getBranches()
      setBranches(data)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  const createBranch = async (dto: Omit<Branch, 'id'>) => {
    await adminService.createBranch(dto)
    await load()
  }

  return { branches, isLoading, refresh: load, createBranch }
}