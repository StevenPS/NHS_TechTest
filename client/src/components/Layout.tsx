import { NavLink, Outlet } from 'react-router'

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  [
    'inline-block px-3 py-2 text-white underline-offset-4 hover:underline',
    isActive ? 'font-semibold underline' : '',
  ].join(' ')

export function Layout() {
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
          <nav aria-label="Main">
            <ul className="flex gap-2">
              <li>
                <NavLink to="/" end className={navLinkClass}>
                  Home
                </NavLink>
              </li>
            </ul>
          </nav>
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
