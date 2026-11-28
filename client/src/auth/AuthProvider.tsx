import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { getCurrentUser, login, logout } from '../api/auth'
import { ApiError, refreshAntiforgeryToken } from '../api/client'
import { AuthContext, type AuthContextValue, type AuthState } from './authContext'

/** Loads the signed-in user from `/api/auth/me` and exposes sign-in and sign-out. */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({ status: 'loading' })

  useEffect(() => {
    const controller = new AbortController()

    refreshAntiforgeryToken()
      .then(() => getCurrentUser(controller.signal))
      .then((user) => setState({ status: 'authenticated', user }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) return
        // 401 means "not signed in". Anything else also leaves the user at the sign-in page,
        // where a failed sign-in will show the real error.
        if (!(error instanceof ApiError) || error.status !== 401) console.error(error)
        setState({ status: 'anonymous' })
      })

    return () => controller.abort()
  }, [])

  const signIn = useCallback(async (email: string, password: string) => {
    const user = await login(email, password)
    // The antiforgery token is bound to the user, so get one for the new identity.
    await refreshAntiforgeryToken()
    setState({ status: 'authenticated', user })
    return user
  }, [])

  const signOut = useCallback(async () => {
    try {
      await logout()
    } finally {
      setState({ status: 'anonymous' })
      await refreshAntiforgeryToken().catch(() => undefined)
    }
  }, [])

  const value = useMemo<AuthContextValue>(() => ({ state, signIn, signOut }), [state, signIn, signOut])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
