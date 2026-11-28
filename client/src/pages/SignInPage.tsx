import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Navigate, useNavigate, useSearchParams } from 'react-router'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/authContext'
import { safeReturnUrl } from '../auth/returnUrl'

type Field = 'email' | 'password'
type FieldErrors = Partial<Record<Field, string>>

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validate(email: string, password: string): FieldErrors {
  const errors: FieldErrors = {}
  if (!email.trim()) errors.email = 'Enter your email address'
  else if (!EMAIL_PATTERN.test(email.trim())) errors.email = 'Enter an email address in the correct format, like name@example.nhs.uk'
  if (!password) errors.password = 'Enter your password'
  return errors
}

const inputClass = (hasError: boolean) =>
  [
    'block w-full max-w-md rounded border-2 px-3 py-2',
    hasError ? 'border-nhs-red' : 'border-slate-600',
  ].join(' ')

export function SignInPage() {
  const { state, signIn } = useAuth()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const returnUrl = safeReturnUrl(searchParams.get('returnUrl'))

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<string>()
  const [submitting, setSubmitting] = useState(false)
  const summaryRef = useRef<HTMLDivElement>(null)

  const hasErrors = formError !== undefined || Object.keys(fieldErrors).length > 0

  // Move focus to the error summary so screen reader and keyboard users hear what went wrong.
  useEffect(() => {
    if (hasErrors) summaryRef.current?.focus()
  }, [hasErrors, fieldErrors, formError])

  useEffect(() => {
    document.title = `${hasErrors ? 'Error: ' : ''}Sign in - MPFT Clinic Space`
  }, [hasErrors])

  if (state.status === 'authenticated' && !submitting) {
    return <Navigate to={returnUrl} replace />
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError(undefined)

    const errors = validate(email, password)
    setFieldErrors(errors)
    if (Object.keys(errors).length > 0) return

    setSubmitting(true)
    try {
      await signIn(email.trim(), password)
      navigate(returnUrl, { replace: true })
    } catch (error) {
      setPassword('')
      if (error instanceof ApiError && error.status === 401) {
        setFormError(error.problem?.detail ?? 'Invalid email or password.')
      } else if (error instanceof ApiError && error.status === 400 && error.problem?.errors) {
        const serverErrors = error.problem.errors
        setFieldErrors({
          email: serverErrors.Email?.[0],
          password: serverErrors.Password?.[0],
        })
      } else {
        setFormError('Sign-in is unavailable right now. Try again in a moment.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  const fieldErrorList = (Object.entries(fieldErrors) as [Field, string | undefined][]).filter(
    (entry): entry is [Field, string] => entry[1] !== undefined,
  )

  return (
    <div className="max-w-xl">
      {hasErrors && (
        <div
          ref={summaryRef}
          tabIndex={-1}
          role="alert"
          aria-labelledby="error-summary-title"
          className="mb-6 border-4 border-nhs-red bg-white p-4 focus:outline-none"
        >
          <h2 id="error-summary-title" className="mb-2 text-xl font-bold">
            There is a problem
          </h2>
          <ul className="list-none space-y-1">
            {formError && <li className="font-semibold text-nhs-red">{formError}</li>}
            {fieldErrorList.map(([field, message]) => (
              <li key={field}>
                <a href={`#${field}`} className="font-semibold text-nhs-red underline">
                  {message}
                </a>
              </li>
            ))}
          </ul>
        </div>
      )}

      <h1 className="mb-2 text-3xl font-bold">Sign in</h1>
      <p className="mb-6">Sign in with your NHS email address and password.</p>

      <form noValidate onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label htmlFor="email" className="mb-1 block font-semibold">
            Email address
          </label>
          {fieldErrors.email && (
            <p id="email-error" className="mb-1 font-semibold text-nhs-red">
              <span className="sr-only">Error:</span> {fieldErrors.email}
            </p>
          )}
          <input
            id="email"
            name="email"
            type="email"
            autoComplete="username"
            spellCheck={false}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            aria-invalid={fieldErrors.email ? true : undefined}
            aria-describedby={fieldErrors.email ? 'email-error' : undefined}
            className={inputClass(fieldErrors.email !== undefined)}
          />
        </div>

        <div>
          <label htmlFor="password" className="mb-1 block font-semibold">
            Password
          </label>
          {fieldErrors.password && (
            <p id="password-error" className="mb-1 font-semibold text-nhs-red">
              <span className="sr-only">Error:</span> {fieldErrors.password}
            </p>
          )}
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            aria-invalid={fieldErrors.password ? true : undefined}
            aria-describedby={fieldErrors.password ? 'password-error' : undefined}
            className={inputClass(fieldErrors.password !== undefined)}
          />
        </div>

        <button
          type="submit"
          disabled={submitting}
          className="rounded bg-nhs-green px-5 py-2 font-semibold text-white shadow-[0_4px_0_#00401e] hover:bg-[#00662f] disabled:opacity-60"
        >
          {submitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </div>
  )
}
