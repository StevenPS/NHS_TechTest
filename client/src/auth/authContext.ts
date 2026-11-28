import { createContext, useContext } from 'react'
import type { CurrentUser } from '../api/auth'

export type AuthState =
  | { status: 'loading' }
  | { status: 'anonymous' }
  | { status: 'authenticated'; user: CurrentUser }

export interface AuthContextValue {
  state: AuthState
  /** Signs in and returns the user. Throws ApiError (401 for bad credentials). */
  signIn: (email: string, password: string) => Promise<CurrentUser>
  signOut: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext)
  if (!value) throw new Error('useAuth must be used inside <AuthProvider>')
  return value
}
