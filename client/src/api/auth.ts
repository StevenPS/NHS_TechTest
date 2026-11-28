import { getJson, sendJson } from './client'

/** Role names, as stored by the API. */
export type Role = 'Admin' | 'Requester'

export interface CurrentUser {
  fullName: string
  email: string
  role: Role
}

export const login = (email: string, password: string) =>
  sendJson<CurrentUser>('POST', '/api/auth/login', { email, password })

export const logout = () => sendJson('POST', '/api/auth/logout')

export const getCurrentUser = (signal?: AbortSignal) => getJson<CurrentUser>('/api/auth/me', signal)
