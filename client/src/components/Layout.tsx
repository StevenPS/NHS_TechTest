import { NavLink, Outlet, useNavigate } from 'react-router'
import { useAuth } from '../auth/authContext'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  [
    'inline-block px-3 py-2 text-white underline-offset-4 hover:underline',
    isActive ? 'font-semibold underline' : '',
  ].join(' ')

export function Layout() {
  const { state, signOut } = useAuth()
  const navigate = useNavigate()

  async function handleSignOut() {
    try {
      await signOut()
    } finally {
      navigate('/sign-in', { replace: true })
    }
  }

  return (
    <div className="flex min-h-screen flex-col bg-slate-50 text-slate-900">
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:absolute focus:left-2 focus:top-2 focus:z-10 focus:bg-nhs-yellow focus:px-3 focus:py-2 focus:text-slate-900"
      >
        Skip to main content
      </a>

      <header className="bg-nhs-blue text-white">
        <div className="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-4 px-4 py-3">
          <NavLink to="/" className="text-xl font-bold tracking-tight text-white no-underline">
            MPFT Clinic Space
          </NavLink>
          {state.status === 'authenticated' && (
            <div className="flex flex-wrap items-center gap-4">
              {/* Role-specific links (My requests, All requests, ...) arrive with the pages in Steps 3 to 5. */}
              <nav aria-label="Main">
                <ul className="flex gap-2">
                  <li>
                    <NavLink to="/" end className={navLinkClass}>
                      Home
                    </NavLink>
                  </li>
                </ul>
              </nav>
              <p className="text-sm">
                <span className="sr-only">Signed in as </span>
                <span className="font-semibold">{state.user.fullName}</span> ({state.user.role})
              </p>
              <button
                type="button"
                onClick={handleSignOut}
                className="rounded border-2 border-white px-3 py-1 font-semibold text-white hover:bg-white hover:text-nhs-blue"
              >
                Sign out
              </button>
            </div>
          )}
        </div>
      </header>

      <main id="main" tabIndex={-1} className="mx-auto w-full max-w-5xl flex-1 px-4 py-8 focus:outline-none">
        <Outlet />
      </main>

      <footer className="border-t border-slate-200 bg-white">
        <p className="mx-auto max-w-5xl px-4 py-4 text-sm text-slate-600">
          Midlands Partnership University NHS Foundation Trust: clinical space requests
        </p>
      </footer>
    </div>
  )
}
