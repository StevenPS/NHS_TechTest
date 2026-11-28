import { useEffect, useState } from 'react'
import { ApiError } from '../api/client'
import { getHealth } from '../api/health'
import { useAuth } from '../auth/authContext'

type HealthState =
  | { kind: 'loading' }
  | { kind: 'ok'; database: string }
  | { kind: 'error'; message: string }

export function HomePage() {
  const { state } = useAuth()
  const [health, setHealth] = useState<HealthState>({ kind: 'loading' })

  useEffect(() => {
    const controller = new AbortController()

    getHealth(controller.signal)
      .then((result) => setHealth({ kind: 'ok', database: result.database }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) return
        const message = error instanceof ApiError ? error.message : 'The API could not be reached.'
        setHealth({ kind: 'error', message })
      })

    return () => controller.abort()
  }, [])

  // Rendered under <RequireAuth>, so the user is always known here.
  if (state.status !== 'authenticated') return null
  const { user } = state

  return (
    <>
      <h1 className="mb-4 text-3xl font-bold">Welcome, {user.fullName}</h1>
      <p className="mb-6 max-w-prose">
        {user.role === 'Admin'
          ? 'As an administrator you can review clinical space requests from all staff, and accept or reject them.'
          : 'You can request clinical space and track the status of your requests.'}
      </p>

      <section aria-labelledby="system-status-heading" className="rounded border border-slate-200 bg-white p-4">
        <h2 id="system-status-heading" className="mb-2 text-lg font-semibold">
          System status
        </h2>
        <p aria-live="polite" role="status">
          {health.kind === 'loading' && <span>Checking API…</span>}
          {health.kind === 'ok' && (
            <span className="font-medium text-nhs-green">API OK (db {health.database})</span>
          )}
          {health.kind === 'error' && (
            <span className="font-medium text-nhs-red">API unavailable: {health.message}</span>
          )}
        </p>
      </section>
    </>
  )
}
