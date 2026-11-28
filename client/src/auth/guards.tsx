import type { ReactNode } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router'
import type { Role } from '../api/auth'
import { useAuth } from './authContext'

function Loading() {
  return (
    <p role="status" aria-live="polite">
      Loading…
    </p>
  )
}

/** Renders child routes for signed-in users; sends everyone else to the sign-in page. */
export function RequireAuth() {
  const { state } = useAuth()
  const location = useLocation()

  if (state.status === 'loading') return <Loading />

  if (state.status === 'anonymous') {
    const returnUrl = location.pathname + location.search
    const target = returnUrl === '/' ? '/sign-in' : `/sign-in?returnUrl=${encodeURIComponent(returnUrl)}`
    return <Navigate to={target} replace />
  }

  return <Outlet />
}

/** Renders its children only for users in one of `roles`. Use inside {@link RequireAuth}. */
export function RequireRole({ roles, children }: { roles: readonly Role[]; children: ReactNode }) {
  const { state } = useAuth()

  if (state.status !== 'authenticated') return <Loading />

  if (!roles.includes(state.user.role)) {
    return (
      <>
        <h1 className="mb-4 text-3xl font-bold">You do not have access to this page</h1>
        <p>This page is only available to: {roles.join(', ')}.</p>
      </>
    )
  }

  return <>{children}</>
}
